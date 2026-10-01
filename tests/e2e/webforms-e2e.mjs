// End-to-end check of the WebForms sample in a real browser, using real key events
// and mouse clicks through the Chrome DevTools Protocol. No npm packages needed (Node 22+).
//
// Normally started by run-webforms-e2e.ps1, which builds the sample and starts
// IIS Express and Chrome. Usage: node webforms-e2e.mjs <baseUrl> <chromeDebugPort>
const [baseUrl, port] = process.argv.slice(2);
if (!baseUrl || !port) {
  console.error("Usage: node webforms-e2e.mjs <baseUrl> <chromeDebugPort>");
  process.exit(2);
}
const results = [];
let failed = 0;

// Never hang a CI job: report what was checked so far and fail.
function abort(reason) {
  console.log(results.join("\n"));
  console.error(`\nABORTED: ${reason}`);
  process.exit(1);
}
setTimeout(() => abort("timed out after 120 seconds"), 120_000).unref();
process.on("unhandledRejection", (e) => abort(e && e.stack ? e.stack : String(e)));
process.on("uncaughtException", (e) => abort(e && e.stack ? e.stack : String(e)));
function check(name, actual, expected) {
  const ok = JSON.stringify(actual) === JSON.stringify(expected);
  if (!ok) failed++;
  results.push(`${ok ? "PASS" : "FAIL"}  ${name}${ok ? "" : `\n        expected ${JSON.stringify(expected)}\n        actual   ${JSON.stringify(actual)}`}`);
}

const target = await (await fetch(`http://localhost:${port}/json/new?about:blank`, { method: "PUT" })).json();
const ws = new WebSocket(target.webSocketDebuggerUrl);
await new Promise((r) => (ws.onopen = r));
let id = 0;
const pending = new Map();
const listeners = [];
ws.onmessage = (m) => {
  const msg = JSON.parse(m.data);
  if (msg.id && pending.has(msg.id)) { pending.get(msg.id)(msg); pending.delete(msg.id); }
  else listeners.forEach((l) => l(msg));
};
const send = (method, params = {}) =>
  new Promise((resolve, reject) => {
    const i = ++id;
    pending.set(i, (msg) => (msg.error ? reject(new Error(method + ": " + msg.error.message)) : resolve(msg.result)));
    ws.send(JSON.stringify({ id: i, method, params }));
  });
const waitFor = (event) =>
  new Promise((resolve) => {
    const l = (msg) => { if (msg.method === event) { listeners.splice(listeners.indexOf(l), 1); resolve(msg.params); } };
    listeners.push(l);
  });
const evaluate = async (expression) => {
  const r = await send("Runtime.evaluate", { expression, returnByValue: true });
  if (r.exceptionDetails) throw new Error(r.exceptionDetails.text + " in " + expression);
  return r.result.value;
};

await send("Page.enable");
await send("Runtime.enable");
const consoleErrors = [];
listeners.push((msg) => {
  if (msg.method === "Runtime.exceptionThrown") consoleErrors.push(msg.params.exceptionDetails.text);
});

async function navigate(url) {
  const loaded = waitFor("Page.loadEventFired");
  await send("Page.navigate", { url });
  await loaded;
}
const focus = (idAttr) => evaluate(`document.getElementById(${JSON.stringify(idAttr)}).focus()`);
const value = (idAttr) => evaluate(`document.getElementById(${JSON.stringify(idAttr)}).value`);

const KEYS = { Backspace: 8, Tab: 9, Enter: 13, Delete: 46 };
async function press(key) {
  const isChar = key.length === 1;
  const code = isChar ? key.toUpperCase().charCodeAt(0) : KEYS[key];
  const base = { key, windowsVirtualKeyCode: code, nativeVirtualKeyCode: code };
  await send("Input.dispatchKeyEvent", { type: isChar ? "keyDown" : "rawKeyDown", ...base, ...(isChar ? { text: key, unmodifiedText: key } : {}) });
  await send("Input.dispatchKeyEvent", { type: "keyUp", ...base });
}
async function type(text) { for (const c of text) await press(c); }

async function submit() {
  const loaded = waitFor("Page.loadEventFired");
  // A real mouse click on the submit button.
  const box = await evaluate(`(() => { const r = document.getElementById("btnSubmit").getBoundingClientRect(); return { x: r.x + r.width / 2, y: r.y + r.height / 2 }; })()`);
  for (const t of ["mousePressed", "mouseReleased"])
    await send("Input.dispatchMouseEvent", { type: t, x: box.x, y: box.y, button: "left", clickCount: 1 });
  await loaded;
}
const text = (idAttr) => evaluate(`(document.getElementById(${JSON.stringify(idAttr)}) || {}).innerText || null`);

// ── 1. Page and script load ────────────────────────────────────────────
await navigate(baseUrl + "/Default.aspx");
check("script loaded (window.EndOfDayTime.init is a function)", await evaluate("typeof (window.EndOfDayTime || {}).init"), "function");
check("input rendered with data-eodt-input", await evaluate(`document.getElementById("eodtStart").getAttribute("data-eodt-input")`), "true");
check("input is type=text maxlength=5", await evaluate(`(() => { const e = document.getElementById("eodtStart"); return [e.type, e.maxLength, e.placeholder]; })()`), ["text", 5, "HH:mm"]);
check("default style applied (width 70px, centered)", await evaluate(`(() => { const s = getComputedStyle(document.getElementById("eodtStart")); return [s.width, s.textAlign]; })()`), ["70px", "center"]);

// ── 2. Typing with real key events ─────────────────────────────────────
await focus("eodtStart");
await type("1");
check("after '1' -> '1'", await value("eodtStart"), "1");
await type("2");
check("after '12' -> '12:' (colon inserted)", await value("eodtStart"), "12:");
await type("30");
check("after '1230' -> '12:30'", await value("eodtStart"), "12:30");
await type("9");
check("fifth digit ignored", await value("eodtStart"), "12:30");
await press("Backspace");
check("Backspace clears the field", await value("eodtStart"), "");
await type("ab:x-");
check("letters and symbols rejected", await value("eodtStart"), "");
await type("0a9b0c0");
check("mixed input keeps digits only -> '09:00'", await value("eodtStart"), "09:00");

// ── 3. Valid postback: 09:00 – 24:00 ───────────────────────────────────
await focus("eodtEnd");
await type("2400");
check("end field shows '24:00'", await value("eodtEnd"), "24:00");
await submit();
check("valid postback: result shown", (await text("lblResult") || "").replace(/\s+/g, " "), "Inizio: 09:00 – Fine: 24:00 Durata: 15h 00m");
check("valid postback: no error panel", await text("lblError"), null);
check("valid postback: start redisplayed", await value("eodtStart"), "09:00");
check("valid postback: end redisplayed", await value("eodtEnd"), "24:00");
check("script re-initialised after postback", await evaluate("typeof window.EndOfDayTime.init"), "function");

// ── 4. Midnight start: 00:00 – 08:30 ───────────────────────────────────
await focus("eodtStart");
await press("Backspace");
await type("0000");
await focus("eodtEnd");
await press("Backspace");
await type("0830");
await submit();
check("00:00 postback: result shown", (await text("lblResult") || "").replace(/\s+/g, " "), "Inizio: 00:00 – Fine: 08:30 Durata: 8h 30m");
check("00:00 postback: start redisplayed as 00:00", await value("eodtStart"), "00:00");

// ── 5. Invalid value typed with the keyboard: 25:00 ────────────────────
await focus("eodtEnd");
await press("Backspace");
await type("2500");
check("typed '2500' shows '25:00'", await value("eodtEnd"), "25:00");
await submit();
check("invalid postback: error shown", await text("lblError"), "Inserire orari validi nel formato HH:mm (00:00–24:00).");
check("invalid postback: no result panel", await text("lblResult"), null);
check("invalid postback: typed text kept for correction", await value("eodtEnd"), "25:00");
check("invalid postback: valid field kept", await value("eodtStart"), "00:00");

// ── 6. Correct it and resubmit ─────────────────────────────────────────
await focus("eodtEnd");
await press("Backspace");
await type("2359");
await submit();
check("corrected postback: result shown", (await text("lblResult") || "").replace(/\s+/g, " "), "Inizio: 00:00 – Fine: 23:59 Durata: 23h 59m");
check("corrected postback: error gone", await text("lblError"), null);

// ── 7. Incomplete value ────────────────────────────────────────────────
await focus("eodtEnd");
await press("Backspace");
await type("12");
await submit();
check("incomplete '12:' postback: error shown", await text("lblError"), "Inserire orari validi nel formato HH:mm (00:00–24:00).");
check("incomplete postback: typed text kept", await value("eodtEnd"), "12:");

// ── 8. Empty field ─────────────────────────────────────────────────────
await focus("eodtEnd");
await press("Backspace");
check("field emptied", await value("eodtEnd"), "");
await submit();
check("empty postback: error shown (sample requires both)", await text("lblError"), "Inserire orari validi nel formato HH:mm (00:00–24:00).");
check("empty postback: field still empty", await value("eodtEnd"), "");

// ── 9. End before start ────────────────────────────────────────────────
await focus("eodtStart");
await press("Backspace");
await type("1800");
await focus("eodtEnd");
await type("0900");
await submit();
check("end before start: business error shown", await text("lblError"), "L'orario di fine turno deve essere successivo all'orario di inizio.");

check("no JavaScript errors on the page", consoleErrors, []);

console.log(results.join("\n"));
console.log(`\n${results.length - failed} passed, ${failed} failed`);
ws.close();
process.exit(failed ? 1 : 0);
