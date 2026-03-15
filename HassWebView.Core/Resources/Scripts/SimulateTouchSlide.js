(()=>{
    var vw = __VW__; var vh = __VH__;
    var cw = document.documentElement.clientWidth; var ch = document.documentElement.clientHeight;
    var x1 = __X1__ * (cw/vw); var y1 = __Y1__ * (ch/vh);
    var x2 = __X2__ * (cw/vw); var y2 = __Y2__ * (ch/vh);
    var duration = __DURATION__;
    var el = document.elementFromPoint(x1, y1);
    if (!el) return;
    const createTouch = (x, y) => new Touch({
        identifier: Date.now(), target: el, clientX: x, clientY: y, pageX: x, pageY: y
    });
    const dispatchTouchEvent = (type, touches) => el.dispatchEvent(new TouchEvent(type, {
        bubbles: true, cancelable: true, view: window, touches: touches, targetTouches: touches, changedTouches: touches
    }));
    dispatchTouchEvent('touchstart', [createTouch(x1, y1)]);
    let startTime = performance.now();
    function animate(currentTime) {
        let elapsedTime = currentTime - startTime;
        if (elapsedTime >= duration) {
            dispatchTouchEvent('touchmove', [createTouch(x2, y2)]);
            dispatchTouchEvent('touchend', [createTouch(x2, y2)]);
            return;
        }
        let progress = elapsedTime / duration;
        let currentX = x1 + (x2 - x1) * progress;
        let currentY = y1 + (y2 - y1) * progress;
        dispatchTouchEvent('touchmove', [createTouch(currentX, currentY)]);
        requestAnimationFrame(animate);
    }
    requestAnimationFrame(animate);
})();