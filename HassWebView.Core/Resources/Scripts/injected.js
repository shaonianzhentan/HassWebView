(function(window) {
    'use strict';

    if (window.HassWebView) {
        return;
    }

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

    function insertText(content, append) {
        const el = document.activeElement;
        if (!el || (el.tagName !== 'HA-INPUT' && el.tagName !== 'INPUT' && el.tagName !== 'TEXTAREA')) {
             console.warn('[HassWebView] insertText: No active text input element found.');
             return;
        }

        el.value = append ? el.value + content : content;
        
        try {
            ['input', 'change', 'compositionstart', 'compositionend', 'blur', 'focus'].forEach(evt => {
                const event = new Event(evt, { bubbles: true, cancelable: true, view: window });
                el.dispatchEvent(event);
            });
        } catch (ex) {
            console.error('[HassWebView] Error dispatching events for insertText:', ex);
        }
        
        el.selectionStart = el.selectionEnd = el.value.length;
    }

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
            setTimeout(() => {
                if (toast.parentNode) {
                    toast.parentNode.removeChild(toast);
                }
            }, 500);
        }, duration);
    }

    /**
     * Simulates a key press event on the currently focused element, traversing Shadow DOM.
     * @param {string} key The key identifier to simulate (e.g., 'Tab', 'Enter').
     */
    function simulateKeyPress(key) {
        function getDeepActiveElement() {
            let activeEl = document.activeElement;
            while (activeEl && activeEl.shadowRoot && activeEl.shadowRoot.activeElement) {
                activeEl = activeEl.shadowRoot.activeElement;
            }
            return activeEl;
        }

        const target = getDeepActiveElement() || document.body;
        console.log(`[HassWebView] Simulating '${key}' press on`, target);

        const eventOptions = {
            key: key,
            bubbles: true,
            composed: true, // Allows event to cross Shadow DOM boundaries
            cancelable: true,
            view: window
        };

        target.dispatchEvent(new KeyboardEvent('keydown', eventOptions));
        target.dispatchEvent(new KeyboardEvent('keyup', eventOptions));
    }

    // --- Expose the Public API ---
    window.HassWebView = {
        injectCss: injectCss,
        insertText: insertText,
        toast: showToast,
        simulateKeyPress: simulateKeyPress
    };

    console.log('[HassWebView] Injected script and API are ready.');

    /**
     * 按键事件拦截与透传
     * 在 capture 阶段拦截导航按键（方向键/Enter/Escape），
     * 透传到原生 Bridge -> KeyService 处理，避免被网页元素消费。
     */
    var _hwv_keyMap = {
        'ArrowUp': 'DpadUp',
        'ArrowDown': 'DpadDown',
        'ArrowLeft': 'DpadLeft',
        'ArrowRight': 'DpadRight',
        'Enter': 'DpadCenter',
        'Escape': 'Escape'
    };

    function _hwv_forwardKeyEvent(eventType, jsKey) {
        var nativeKey = _hwv_keyMap[jsKey];
        if (!nativeKey) return false;

        try {
            // Android: 通过 AddJavascriptInterface 调用
            if (window.externalApp && window.externalApp.onKeyEvent) {
                window.externalApp.onKeyEvent(eventType, nativeKey);
                return true;
            }
        } catch (e) { /* ignore */ }

        try {
            // Windows: 通过 WebView2 WebMessage 调用
            if (window.chrome && window.chrome.webview) {
                window.chrome.webview.postMessage({
                    BridgeName: 'externalApp',
                    MethodName: 'onKeyEvent',
                    Arguments: [eventType, nativeKey]
                });
                return true;
            }
        } catch (e) { /* ignore */ }

        return false;
    }

    function _hwv_handleKeyEvent(e, eventType) {
        if (!_hwv_keyMap[e.key]) return; // 非导航按键，放行给网页正常处理

        if (_hwv_forwardKeyEvent(eventType, e.key)) {
            e.preventDefault();
            e.stopPropagation();
            e.stopImmediatePropagation();
        }
    }

    // 使用 capture 阶段拦截，确保在所有其他监听器之前执行
    document.addEventListener('keydown', function(e) {
        _hwv_handleKeyEvent(e, 'down');
    }, true);

    document.addEventListener('keyup', function(e) {
        _hwv_handleKeyEvent(e, 'up');
    }, true);

    console.log('[HassWebView] Key event forwarding initialized.');

})(window);