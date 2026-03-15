(()=>{
    var vw = __VW__; var vh = __VH__;
    var cw = document.documentElement.clientWidth; var ch = document.documentElement.clientHeight;
    var x = __X__ * (cw/vw); var y = __Y__ * (ch/vh);
    var el = document.elementFromPoint(x, y);
    if (el) {
        el.scrollIntoView({block:'center',inline:'center'});
        if (el.tagName === 'INPUT' || el.tagName === 'TEXTAREA') el.focus();
        else {
            el.dispatchEvent(new Event('touchstart',{bubbles:true,cancelable:true}));
            el.dispatchEvent(new Event('touchend',{bubbles:true,cancelable:true}));
            el.dispatchEvent(new MouseEvent('click',{bubbles:true,cancelable:true,view:window}));
        }
    }
})();