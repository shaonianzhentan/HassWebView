
(function(window) {
    'use strict';

    if (window.HassTextInput) {
        return; // Already loaded
    }

    function inputText(content, append) {
        const el = document.activeElement;
        if (!el || (el.tagName !== 'INPUT' && el.tagName !== 'TEXTAREA')) return;

        // In JavaScript, backslashes and single quotes need to be escaped within a string literal.
        // The original C# code was: text.Replace("\\", "\\\\").Replace("'", "\'");
        // This is correctly handled by passing the string to the JS function.
        // No extra escaping is needed here as the `content` is already the text to be inserted.

        el.value = append ? el.value + content : content;

        // Dispatch events to ensure UI frameworks like React/Vue detect the change
        ['input', 'change', 'compositionstart', 'compositionend', 'blur', 'focus'].forEach(evt => {
            const event = new Event(evt, { bubbles: true, cancelable: true, view: window });
            el.dispatchEvent(event);
        });

        // Move cursor to the end
        el.selectionStart = el.selectionEnd = el.value.length;
    }

    window.HassTextInput = {
        insert: inputText
    };

})(window);
