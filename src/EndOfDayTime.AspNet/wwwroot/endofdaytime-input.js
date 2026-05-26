(function () {
  function initEndOfDayTimeInput(element) {
    var digits = "";

    function render() {
      var rendered =
        digits.length >= 2
          ? digits.substring(0, 2) + ":" + digits.substring(2)
          : digits;
      element.value = rendered;
      element.setSelectionRange(rendered.length, rendered.length);
    }

    element.addEventListener("keydown", function (e) {
      if (e.ctrlKey || e.metaKey) return;

      if (e.key === "Backspace" || e.key === "Delete") {
        e.preventDefault();
        digits = "";
        render();
        return;
      }

      if (e.key === "Enter") {
        e.preventDefault();
        element.blur();
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
      var pasted = (e.clipboardData || window.clipboardData).getData("text");
      digits = "";
      for (var i = 0; i < pasted.length; i++) {
        if (/\d/.test(pasted[i]) && digits.length < 4) digits += pasted[i];
      }
      render();
    });

    // Initialize from existing value
    if (element.value) {
      var existing = element.value.replace(":", "");
      for (var i = 0; i < existing.length && digits.length < 4; i++) {
        if (/\d/.test(existing[i])) digits += existing[i];
      }
      render();
    }
  }

  function initAll() {
    var elements = document.querySelectorAll("[data-eodt-input]");
    for (var i = 0; i < elements.length; i++) {
      initEndOfDayTimeInput(elements[i]);
    }
  }

  if (document.readyState === "loading") {
    document.addEventListener("DOMContentLoaded", initAll);
  } else {
    initAll();
  }

  window.EndOfDayTime = window.EndOfDayTime || {};
  window.EndOfDayTime.init = initEndOfDayTimeInput;
  window.EndOfDayTime.initAll = initAll;
})();
