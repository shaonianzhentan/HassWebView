
(function(window) {
    'use strict';

    if (window.HassVideoPanel) {
        return; // Already loaded
    }

    const panelId = 'video-panel-container';
    const tabsId = 'video-panel-tabs';
    const contentId = 'video-panel-content';

    // Style constants
    const defaultTabStyle = {
        padding: '5px 10px',
        backgroundColor: '#333',
        color: '#fff',
        border: '1px solid #555',
        borderRadius: '3px',
        margin: '2px',
        cursor: 'pointer',
        opacity: '0.7'
    };

    const activeTabStyle = {
        backgroundColor: '#007bff',
        opacity: '1'
    };

    function getFileType(url) {
        if (typeof url !== 'string') return 'unknown';
        const urlWithoutQuery = url.split('?')[0];
        const parts = urlWithoutQuery.split('.');
        if (parts.length > 1) {
            const ext = parts.pop().toLowerCase();
            if (['m3u8', 'm3u'].includes(ext)) return 'm3u8';
            if (['flv', 'xs', 'acc'].includes(ext)) return 'flv';
            if (['mp4', 'mov', 'avi', 'wmv'].includes(ext)) return 'video';
            return ext;
        }
        return 'other';
    }

    function switchTab(fileType) {
        const container = document.getElementById(panelId);
        if (!container) return;

        container.querySelectorAll('.video-tab-button').forEach(btn => {
            Object.assign(btn.style, defaultTabStyle);
        });
        container.querySelectorAll('.video-tab-content').forEach(content => {
            content.style.display = 'none';
        });

        const tabButton = container.querySelector(`.video-tab-button[data-tab="${fileType}"]`);
        const tabContent = document.getElementById(`video-tab-content-${fileType}`);

        if (tabButton) {
            Object.assign(tabButton.style, defaultTabStyle, activeTabStyle);
        }
        if (tabContent) {
            tabContent.style.display = 'block';
        }
    }

    function createPanel() {
        let container = document.createElement('div');
        container.id = panelId;
        Object.assign(container.style, {
            position: 'fixed',
            left: '0',
            top: '0',
            height: '100%',
            width: '30%',
            minWidth: '250px',
            display: 'flex',
            flexDirection: 'column',
            backgroundColor: 'rgba(0,0,0,0.7)',
            borderRadius: '0 10px 10px 0',
            boxSizing: 'border-box',
            zIndex: '2147483647',
            overflow: 'hidden',
            pointerEvents: 'auto'
        });

        // 阻止所有触摸/鼠标/指针事件穿透到下层页面元素
        ['touchstart', 'touchmove', 'touchend', 'mousedown', 'mousemove', 'mouseup', 'click', 'pointerdown', 'pointermove', 'pointerup'].forEach(function(evt) {
            container.addEventListener(evt, function(e) {
                e.stopPropagation();
            }, true);
        });

        const tabsContainer = document.createElement('div');
        tabsContainer.id = tabsId;
        Object.assign(tabsContainer.style, {
            display: 'flex',
            flexWrap: 'wrap',
            padding: '5px',
            borderBottom: '1px solid #555'
        });

        const contentContainer = document.createElement('div');
        contentContainer.id = contentId;
        Object.assign(contentContainer.style, {
            overflowY: 'auto',
            flex: '1',
            padding: '10px',
            display: 'flex',
            flexDirection: 'column',
            gap: '8px'
        });

        container.appendChild(tabsContainer);
        container.appendChild(contentContainer);
        document.body.appendChild(container);

        return container;
    }

    function addVideoToPanel(videoUrl) {
        const fileType = getFileType(videoUrl);
        const safeId = 'video-panel-item-' + encodeURIComponent(videoUrl).replace(/[^a-zA-Z0-9_-]/g, '_');
        
        let container = document.getElementById(panelId) || createPanel();
        let tabsContainer = document.getElementById(tabsId);
        let contentContainer = document.getElementById(contentId);
        let tabButton = tabsContainer.querySelector(`.video-tab-button[data-tab="${fileType}"]`);
        let tabContent;

        if (!tabButton) {
            tabButton = document.createElement('button');
            tabButton.className = 'video-tab-button';
            tabButton.dataset.tab = fileType;
            tabButton.textContent = `.${fileType}`;
            Object.assign(tabButton.style, defaultTabStyle);
            
            tabButton.addEventListener('click', () => switchTab(fileType));
            tabsContainer.appendChild(tabButton);

            tabContent = document.createElement('div');
            tabContent.className = 'video-tab-content';
            tabContent.id = `video-tab-content-${fileType}`;
            tabContent.style.display = 'none';
            contentContainer.appendChild(tabContent);
            
            if (tabsContainer.children.length === 1) {
                switchTab(fileType);
            }
        } else {
            tabContent = document.getElementById(`video-tab-content-${fileType}`);
        }
        
        if (tabContent.querySelector(`#${safeId}`)) {
            return;
        }

        const item = document.createElement('div');
        item.id = safeId;
        Object.assign(item.style, {
            padding: '8px',
            backgroundColor: 'rgba(255, 255, 255, 0.1)',
            color: '#ffffff',
            border: '1px solid #555',
            borderRadius: '5px',
            display: 'flex',
            flexDirection: 'column',
            gap: '8px'
        });

        const urlText = document.createElement('div');
        urlText.textContent = videoUrl;
        Object.assign(urlText.style, {
            wordBreak: 'break-all',
            cursor: 'pointer'
        });
        urlText.addEventListener('click', () => {
            window.externalApp.externalBus(JSON.stringify({
                type: 'video/play',
                data: videoUrl,
                origin: top.location.origin
            }));
        });

        const externalPlayButton = document.createElement('button');
        externalPlayButton.textContent = '使用外部播放器';
        Object.assign(externalPlayButton.style, {
            padding: '4px 8px',
            backgroundColor: '#555',
            color: '#fff',
            border: '1px solid #777',
            borderRadius: '3px',
            cursor: 'pointer',
            alignSelf: 'flex-end'
        });
        externalPlayButton.addEventListener('click', (e) => {
            e.stopPropagation();
            window.externalApp.externalBus(JSON.stringify({
                type: 'play/video',
                data: videoUrl
            }));
        });

        item.appendChild(urlText);
        item.appendChild(externalPlayButton);
        
        tabContent.insertBefore(item, tabContent.firstChild);

        while (tabContent.children.length > 30) {
            tabContent.removeChild(tabContent.lastChild);
        }
    }

    function toggleVideoPanel() {
        const div = document.getElementById(panelId);
        if (div) {
            div.style.display = div.style.display === 'none' ? 'flex' : 'none';
        }
    }

    window.HassVideoPanel = {
        add: addVideoToPanel,
        toggle: toggleVideoPanel
    };

})(window);
