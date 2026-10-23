// Guilloche: the fine looping line patterns of banknotes, drawn on <canvas data-guilloche="...">.
// Kinds: "rosette" (logo), "seal" (customer seal), "band" (trim under the command bar),
// "note" (banknote background). Colour comes from the canvas's CSS `color`.
(function () {
    var TAU = Math.PI * 2;

    function setup(canvas) {
        var dpr = Math.min(window.devicePixelRatio || 1, 2);
        var w = canvas.clientWidth, h = canvas.clientHeight;
        if (!w || !h) return null;
        canvas.width = Math.round(w * dpr);
        canvas.height = Math.round(h * dpr);
        var ctx = canvas.getContext("2d");
        ctx.setTransform(dpr, 0, 0, dpr, 0, 0);
        ctx.clearRect(0, 0, w, h);
        ctx.strokeStyle = getComputedStyle(canvas).color;
        ctx.lineJoin = "round";
        return { ctx: ctx, w: w, h: h };
    }

    // Epitrochoid ring: a circle rolling around a circle, with a pen offset.
    function ring(ctx, cx, cy, R, r, d, scale, steps, phase) {
        ctx.beginPath();
        for (var i = 0; i <= steps; i++) {
            var t = (i / steps) * TAU * r / gcd(R, r) + (phase || 0);
            var x = (R + r) * Math.cos(t) - d * Math.cos(((R + r) / r) * t);
            var y = (R + r) * Math.sin(t) - d * Math.sin(((R + r) / r) * t);
            if (i === 0) ctx.moveTo(cx + x * scale, cy + y * scale);
            else ctx.lineTo(cx + x * scale, cy + y * scale);
        }
        ctx.stroke();
    }

    function gcd(a, b) { return b ? gcd(b, a % b) : a; }

    function rosette(ctx, cx, cy, size, layers, alpha, width) {
        ctx.lineWidth = width;
        for (var k = 0; k < layers; k++) {
            ctx.globalAlpha = alpha * (1 - k / (layers + 2));
            var R = 60 + k * 7, r = 11 + (k % 3) * 2, d = 22 + k * 3;
            var extent = R + r + d;
            ring(ctx, cx, cy, R, r, d, size / extent, 1400, k * 0.09);
        }
        ctx.globalAlpha = 1;
    }

    // Interleaved sine waves, like the security bands along a banknote's edge.
    function waves(ctx, x0, y0, w, h, lines, cycles, alpha, width) {
        ctx.lineWidth = width;
        for (var k = 0; k < lines; k++) {
            ctx.globalAlpha = alpha;
            ctx.beginPath();
            var phase = (k / lines) * TAU;
            for (var x = 0; x <= w; x += 2) {
                var t = (x / w) * TAU * cycles;
                var y = y0 + h / 2 + Math.sin(t + phase) * (h / 2) * (0.55 + 0.45 * Math.cos(t / 3 + phase));
                if (x === 0) ctx.moveTo(x0 + x, y); else ctx.lineTo(x0 + x, y);
            }
            ctx.stroke();
        }
        ctx.globalAlpha = 1;
    }

    var painters = {
        rosette: function (s) { rosette(s.ctx, s.w / 2, s.h / 2, Math.min(s.w, s.h) / 2 - 1, 2, 0.75, 0.35); },
        seal: function (s) { rosette(s.ctx, s.w / 2, s.h / 2, Math.min(s.w, s.h) / 2 - 1, 6, 0.55, 0.5); },
        band: function (s) { waves(s.ctx, 0, 1, s.w, s.h - 2, 6, Math.max(8, s.w / 60), 0.55, 0.6); },
        // Banner: one oversized rosette cropped by the right edge, over faint waves.
        hero: function (s) {
            var ctx = s.ctx, w = s.w, h = s.h;
            waves(ctx, 0, h * 0.1, w, h * 0.8, 14, Math.max(2, w / 220), 0.08, 0.6);
            rosette(ctx, w * 0.78, h * 0.5, Math.max(h, w * 0.6) * 0.62, 9, 0.32, 0.6);
            rosette(ctx, w * 0.78, h * 0.5, Math.max(h, w * 0.6) * 0.3, 5, 0.25, 0.5);
        },
        note: function (s) {
            var ctx = s.ctx, w = s.w, h = s.h;
            waves(ctx, 0, h * 0.08, w, h * 0.84, 18, Math.max(3, w / 140), 0.12, 0.6);
            rosette(ctx, w * 0.77, h * 0.56, h * 0.36, 7, 0.45, 0.5);
            waves(ctx, 0, h - 16, w, 10, 5, w / 28, 0.45, 0.5);
            waves(ctx, 0, 6, w, 10, 5, w / 28, 0.45, 0.5);
        }
    };

    function paintAll() {
        document.querySelectorAll("canvas[data-guilloche]").forEach(function (c) {
            var s = setup(c);
            var paint = painters[c.getAttribute("data-guilloche")];
            if (s && paint) paint(s);
        });
    }

    var pending;
    function schedule() {
        cancelAnimationFrame(pending);
        pending = requestAnimationFrame(paintAll);
    }

    if (document.readyState === "loading") document.addEventListener("DOMContentLoaded", paintAll);
    else paintAll();
    window.addEventListener("resize", schedule);
    // Repaint when the light/dark theme changes, since colours come from CSS.
    new MutationObserver(schedule).observe(document.documentElement, { attributes: true, attributeFilter: ["data-theme"] });
    if (window.matchMedia) {
        var mq = window.matchMedia("(prefers-color-scheme: dark)");
        if (mq.addEventListener) mq.addEventListener("change", schedule);
    }
    if (document.fonts && document.fonts.ready) document.fonts.ready.then(schedule);
})();
