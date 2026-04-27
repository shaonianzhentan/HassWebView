(function(window) {
    'use strict';

    // Prevent re-injection if the object already exists
    if (window.HassWebView) {
        return;
    }

    /**
     * From CssInjector.js
     * Injects a CSS string into the document's head.
     * @param {string} css The CSS text to inject.
     * @param {string} [host] An optional identifier for the injection source (for logging).
     */
    function injectCss(css, host) {
        try {
            const style = document.createElement('style');
            style.type = 'text/css';
            style.innerHTML = css; 
            (document.head || document.documentElement).appendChild(style);
            console.log(`[HassWebView] Injected custom CSS for ${host || 'page'}.`);
            return true;
        } catch (e) {
            console.error(`[HassWebView] Failed to inject CSS for ${host || 'page'}:`, e);
            return false;
        }
    }

    /**
     * From TextInput.js
     * Inserts or replaces text in the currently active text input element.
     * @param {string} content The text content to insert.
     * @param {boolean} append If true, appends the content; otherwise, replaces the selection.
     */
    function insertText(content, append) {
        const el = document.activeElement;
        // The original check from TextInput.js
        if (!el || (el.tagName !== 'HA-INPUT' && el.tagName !== 'INPUT' && el.tagName !== 'TEXTAREA')) {
             console.warn('[HassWebView] insertText: No active text input element found.');
             return;
        }

        // The original implementation from TextInput.js
        el.value = append ? el.value + content : content;
        
        try {
            // The original events from TextInput.js
            ['input', 'change', 'compositionstart', 'compositionend', 'blur', 'focus'].forEach(evt => {
                const event = new Event(evt, { bubbles: true, cancelable: true, view: window });
                el.dispatchEvent(event);
            });
        } catch (ex) {
            console.error('[HassWebView] Error dispatching events for insertText:', ex);
        }
        
        // Move cursor to the end
        el.selectionStart = el.selectionEnd = el.value.length;
    }

    /**
     * Shows a toast message overlay on the screen.
     * @param {string} message The message to display.
     * @param {number} [duration=3000] The duration in milliseconds to show the toast.
     */
    function showToast(message, duration = 3000) {
        const styleId = 'hasswebview-toast-style';
        if (!document.getElementById(styleId)) {
            const style = document.createElement('style');
            style.id = styleId;
            style.innerHTML = `
                .hasswebview-toast-container {
                    position: fixed;
                    bottom: 25px;
                    left: 50%;
                    transform: translateX(-50%);
                    background-color: rgba(40, 40, 40, 0.9);
                    color: white;
                    padding: 12px 22px;
                    border-radius: 25px;
                    z-index: 10000;
                    font-family: -apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, Helvetica, Arial, sans-serif;
                    font-size: 14px;
                    font-weight: 500;
                    opacity: 0;
                    transition: opacity 0.4s ease, bottom 0.4s ease;
                    box-shadow: 0 4px 12px rgba(0,0,0,0.15);
                    pointer-events: none;
                }
                .hasswebview-toast-container.show {
                    opacity: 1;
                    bottom: 45px;
                }
            `;
            (document.head || document.documentElement).appendChild(style);
        }

        let toast = document.createElement('div');
        toast.className = 'hasswebview-toast-container';
        toast.textContent = message;
        document.body.appendChild(toast);

        setTimeout(() => {
            toast.classList.add('show');
        }, 10);

        setTimeout(() => {
            toast.classList.remove('show');
            toast.addEventListener('transitionend', () => {
                if (toast.parentNode) {
                    toast.parentNode.removeChild(toast);
                }
            }, { once: true });
            // Fallback for safety
            setTimeout(() => {
                if (toast.parentNode) {
                    toast.parentNode.removeChild(toast);
                }
            }, 500);
        }, duration);
    }

    // --- Expose the Public API ---
    window.HassWebView = {
        injectCss: injectCss,
        insertText: insertText,
        toast: showToast
    };

    console.log('[HassWebView] Injected script and API are ready.');

})(window);
