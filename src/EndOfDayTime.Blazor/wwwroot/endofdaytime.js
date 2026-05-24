export function initEndOfDayTimeInput(element, dotNetRef) {
  let digits = "";

  function render() {
    const rendered =
      digits.length >= 2
        ? digits.substring(0, 2) + ":" + digits.substring(2)
        : digits;
    element.value = rendered;
    element.setSelectionRange(rendered.length, rendered.length);
    dotNetRef.invokeMethodAsync("OnDigitsChanged", rendered);
  }

  element.addEventListener("keydown", function (e) {
    // Allow Ctrl+V, Ctrl+C, Ctrl+X, Ctrl+A etc.
    if (e.ctrlKey || e.metaKey) return;

    if (e.key === "Backspace" || e.key === "Delete") {
      e.preventDefault();
      digits = "";
      render();
      return;
    }

    if (e.key === "Enter") {
      e.preventDefault();
      dotNetRef.invokeMethodAsync("OnCommit");
      return;
    }

    if (e.key.length === 1 && !/\d/.test(e.key)) {
      e.preventDefault();
      return;
    }

    if (e.key.length === 1 && /\d/.test(e.key)) {
      e.preventDefault();
      if (digits.length < 4) {
        digits += e.key;
        render();
      }
    }
  });

  element.addEventListener("paste", function (e) {
    e.preventDefault();
    const pasted = (e.clipboardData || window.clipboardData).getData("text");
    digits = "";
    for (const c of pasted) {
      if (/\d/.test(c) && digits.length < 4) digits += c;
    }
    render();
  });

  element.addEventListener("blur", function () {
    dotNetRef.invokeMethodAsync("OnBlur");
  });

  element.addEventListener("input", function (e) {
    e.preventDefault();
  });

  return {
    setValue: function (value) {
      digits = "";
      if (value) {
        for (const c of value.replace(":", "")) {
          if (/\d/.test(c) && digits.length < 4) digits += c;
        }
      }
      render();
    },
  };
}
