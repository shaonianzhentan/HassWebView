
(function(window) {
    'use strict';

    if (window.HassVideoPanel) {
        return; // Already loaded
    }

    function addVideoToPanel(videoUrl) {
        const safeId = 'video-panel-item-' + encodeURIComponent(videoUrl).replace(/[^a-zA-Z0-9_-]/g, '_');
        let container = document.getElementById('video-panel-container');

        if (!container) {
            container = document.createElement('div');
            container.id = 'video-panel-container';
            Object.assign(container.style, {
                position: 'fixed',
                left: '0',
                top: '0',
                height: '100%',
                width: '30%',
                minWidth: '200px',
                display: 'flex',
                flexDirection: 'column',
                gap: '8px',
                padding: '10px',
                backgroundColor: 'rgba(0,0,0,0.6)',
                borderRadius: '0 10px 10px 0',
                boxSizing: 'border-box',
                zIndex: '2147483647',
                overflowY: 'auto'
            });
            document.body.appendChild(container);
        }

        let item = document.getElementById(safeId);
        if (!item) {
            item = document.createElement('div');
            item.id = safeId;
            item.textContent = videoUrl;
            Object.assign(item.style, {
                padding: '8px',
                backgroundColor: 'rgba(255, 255, 255, 0.1)',
                color: '#ffffff',
                border: '1px solid #555',
                borderRadius: '5px',
                wordBreak: 'break-all',
                cursor: 'pointer'
            });
            item.addEventListener('click', () => {
                window.externalApp.externalBus(JSON.stringify({
                    type: 'video/play',
                    data: videoUrl,
                    origin: top.location.origin
                }));
            });
            container.insertBefore(item, container.firstChild);
        }

        while (container.children.length > 6) {
            container.removeChild(container.lastChild);
        }
    }

    function toggleVideoPanel() {
        const div = document.getElementById('video-panel-container');
        if (div) {
            div.style.display = div.style.display === 'none' ? 'flex' : 'none';
        }
    }

    window.HassVideoPanel = {
        add: addVideoToPanel,
        toggle: toggleVideoPanel
    };

})(window);
