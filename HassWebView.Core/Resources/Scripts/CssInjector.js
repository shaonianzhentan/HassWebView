
(function(window) {
    'use strict';

    if (window.HassCssInjector) {
        return; // Already loaded
    }

    function inject(css, host) {
        try {
            const style = document.createElement('style');
            style.type = 'text/css';
            // No need to escape backticks, as we are not using template literals here.
            style.innerHTML = css; 
            document.head.appendChild(style);
            console.log(`[HassWebView] Injected custom CSS for ${host}.`);
            return true;
        } catch (e) {
            console.error(`[HassWebView] Failed to inject CSS for ${host}:`, e);
            return false;
        }
    }

    window.HassCssInjector = {
        inject: inject
    };

})(window);
