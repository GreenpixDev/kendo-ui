/*
 * Диаграмма Маримекко на Kendo Drawing API (приложение А к брифу ТЗ).
 *
 * Ширина столбца пропорциональна его итогу, высота сегмента - доле значения внутри столбца.
 * Нужны: jQuery, kendo.core, kendo.drawing (входят в kendo.all / kendo.dataviz).
 * Формат чисел и процентов берётся из текущей культуры Kendo: до первого рендера вызвать kendo.culture("ru-RU").
 *
 * var chart = new MarimekkoChart("#mm", { format: function (v) { return kendo.toString(v, "n1"); } });
 * chart.setData([{ column: "ГП 11", segment: "ГРБС 001", value: 1250.5 }, ...]);
 *
 * Стиль ES5: работает во всех браузерах, поддерживаемых Kendo для ASP.NET MVC 5.
 */
(function (global, $, kendo) {
    "use strict";

    var geo = kendo.geometry, draw = kendo.drawing;
    var measureText = (draw.util && draw.util.measureText) || kendo.util.measureText;

    var PALETTE = ["#2e75b6", "#70ad47", "#ffc000", "#a5a5a5",
                   "#c55a11", "#7030a0", "#00b0f0", "#843c0c"];
    var OTHER = "Прочие";

    // Группировка плоских строк {column, segment, value} в столбцы с итогами.
    function buildModel(rows, maxColumns) {
        var cols = {}, order = [], segs = [], seen = {}, grand = 0;
        (rows || []).forEach(function (r) {
            var value = Number(r.value);
            if (!(value > 0)) { return; }               // нули, отрицательные и NaN пропускаются
            var column = String(r.column), segment = String(r.segment);
            var c = cols[column];
            if (!c) { c = cols[column] = { name: column, total: 0, cells: {} }; order.push(c); }
            c.total += value;
            c.cells[segment] = (c.cells[segment] || 0) + value;
            grand += value;
            if (!seen[segment]) { seen[segment] = true; segs.push(segment); }
        });
        order.sort(function (a, b) { return b.total - a.total; });
        if (maxColumns && order.length > maxColumns) {
            var rest = order.splice(maxColumns - 1), other = { name: OTHER, total: 0, cells: {} };
            rest.forEach(function (c) {
                other.total += c.total;
                Object.keys(c.cells).forEach(function (k) { other.cells[k] = (other.cells[k] || 0) + c.cells[k]; });
            });
            order.push(other);
        }
        return { columns: order, segments: segs, grand: grand };
    }

    // тёмный текст на светлом фоне и наоборот
    function textColor(hex) {
        var n = parseInt(hex.slice(1), 16);
        var lum = 0.299 * (n >> 16) + 0.587 * ((n >> 8) & 255) + 0.114 * (n & 255);
        return lum > 160 ? "#222" : "#fff";
    }

    function MarimekkoChart(element, options) {
        this.element = $(element).css("position", "relative");
        this.options = $.extend(true, {
            type: "canvas",                              // "canvas" | "svg"
            margin: { top: 12, right: 16, bottom: 36, left: 48 },
            gap: 2,
            font: "12px Roboto, Arial, sans-serif",
            minLabelWidth: 44,
            minLabelHeight: 16,
            maxColumns: 20,
            emptyText: "Нет данных для отображения",
            columnShareText: "Доля в столбце",
            totalShareText: "Доля от общего",
            colors: PALETTE,
            format: function (v) { return kendo.toString(v, "n1"); },
            click: null
        }, options);

        this.surface = draw.Surface.create(this.element, { type: this.options.type });
        this.tip = $('<div class="mm-tooltip" style="display:none;position:absolute;z-index:10;' +
                     'pointer-events:none;background:#fff;border:1px solid #999;' +
                     'padding:4px 8px;font-size:12px;white-space:nowrap;"></div>').appendTo(this.element);
        this.uid = ++MarimekkoChart.counter;
        this._ns = ".marimekko" + this.uid;
        this._bind();
    }
    MarimekkoChart.counter = 0;
    MarimekkoChart.buildModel = buildModel;

    MarimekkoChart.prototype = {
        setData: function (rows) {
            this.model = buildModel(rows, this.options.maxColumns);
            this.render();
        },

        // Рассчитанная геометрия последней отрисовки (для проверок и тестов).
        getLayout: function () { return this.layout; },

        render: function () {
            var o = this.options, m = this.model, self = this;
            var W = this.element.width(), H = this.element.height();
            this.tip.hide();
            this.layout = null;
            if (!W || !H) { return; }
            this.surface.resize();
            this.surface.clear();

            if (!m || !m.grand) {
                var empty = new draw.Group();
                empty.append(this._text(o.emptyText, W / 2, H / 2, "#666", "center"));
                this.surface.draw(empty);
                return;
            }

            var legend = this._legendLayout(m.segments, W);
            var plot = {
                x: o.margin.left,
                y: Math.max(o.margin.top, legend.height + 14),
                w: Math.max(W - o.margin.left - o.margin.right, 1)
            };
            plot.h = Math.max(H - plot.y - o.margin.bottom, 1);

            var gap = m.columns.length > 1 ? Math.min(o.gap, plot.w / (m.columns.length * 4)) : 0;
            var usable = plot.w - gap * (m.columns.length - 1);
            var root = new draw.Group();
            var layout = { plot: plot, columns: [] };
            var x = plot.x;

            m.columns.forEach(function (col) {
                var cw = usable * col.total / m.grand;
                var y = plot.y + plot.h;
                var colLayout = { name: col.name, total: col.total, x: x, w: cw, segments: [] };

                m.segments.forEach(function (seg, i) {
                    var v = col.cells[seg];
                    if (!v) { return; }
                    var ch = plot.h * v / col.total;
                    var fillColor = o.colors[i % o.colors.length];
                    var info = {
                        column: col.name, segment: seg, value: v,
                        shareInColumn: v / col.total, shareOfTotal: v / m.grand
                    };
                    var rect = new draw.Rect(new geo.Rect([x, y - ch], [cw, ch]), {
                        fill: { color: fillColor },
                        stroke: { color: "#fff", width: 1 },
                        cursor: o.click ? "pointer" : "default"
                    });
                    rect.info = info;
                    root.append(rect);
                    colLayout.segments.push({ segment: seg, value: v, x: x, y: y - ch, w: cw, h: ch, color: fillColor });

                    if (cw >= o.minLabelWidth && ch >= o.minLabelHeight) {
                        // подпись несёт тот же info, иначе наведение на неё снимает подсказку с прямоугольника
                        root.append(self._text(kendo.toString(info.shareInColumn, "p0"),
                                               x + cw / 2, y - ch / 2, textColor(fillColor), "center", info));
                    }
                    y -= ch;
                });

                var title = col.name + " (" + kendo.toString(col.total / m.grand, "p0") + ")";
                title = self._fit(title, cw);
                if (title) {
                    root.append(self._text(title, x + cw / 2, plot.y + plot.h + 14, "#333", "center"));
                }
                layout.columns.push(colLayout);
                x += cw + gap;
            });

            [0, 0.25, 0.5, 0.75, 1].forEach(function (t) {
                root.append(self._text(kendo.toString(t, "p0"),
                    o.margin.left - 6, plot.y + plot.h * (1 - t), "#666", "right"));
            });

            legend.items.forEach(function (it) {
                root.append(new draw.Rect(new geo.Rect([it.x, it.y + 2], [12, 12]),
                    { fill: { color: o.colors[it.index % o.colors.length] }, stroke: null }));
                root.append(new draw.Text(it.text, [it.x + 16, it.y], { font: o.font, fill: { color: "#333" } }));
            });

            this.layout = layout;
            this.surface.draw(root);
        },

        // Текст целиком, если помещается; иначе с многоточием; иначе пустая строка (значение остаётся в подсказке).
        _fit: function (text, width) {
            var font = this.options.font;
            if (measureText(text, { font: font }).width <= width) { return text; }
            if (width < 64) { return ""; }                // «ГП 4 (7…» читать нечего
            var lo = 0, hi = text.length;
            while (lo < hi) {
                var mid = Math.ceil((lo + hi) / 2);
                if (measureText(text.slice(0, mid) + "…", { font: font }).width <= width) { lo = mid; } else { hi = mid - 1; }
            }
            return lo > 0 ? text.slice(0, lo) + "…" : "";
        },

        // x: для align "left" - левый край, "center" - середина, "right" - правый край; y - вертикальный центр.
        _text: function (content, x, y, color, align, info) {
            var size = measureText(content, { font: this.options.font });
            var left = align === "center" ? x - size.width / 2 : align === "right" ? x - size.width : x;
            var t = new draw.Text(content, [left, y - size.height / 2],
                { font: this.options.font, fill: { color: color } });
            if (info) { t.info = info; }
            return t;
        },

        // Легенда переносится на новые строки; её высота сдвигает область диаграммы вниз.
        _legendLayout: function (segments, width) {
            var o = this.options, items = [], lx = o.margin.left, ly = 8, rowH = 20;
            var right = width - o.margin.right;
            segments.forEach(function (seg, i) {
                var w = measureText(seg, { font: o.font }).width;
                if (lx > o.margin.left && lx + 16 + w > right) { lx = o.margin.left; ly += rowH; }
                items.push({ x: lx, y: ly, text: seg, index: i });
                lx += 16 + w + 16;
            });
            return { items: items, height: ly + rowH - 8 };
        },

        _bind: function () {
            var self = this, last = { x: 0, y: 0 };
            function place() {
                var left = last.x + 12, top = last.y + 12;
                var maxLeft = self.element.width() - self.tip.outerWidth() - 4;
                if (left > maxLeft) { left = Math.max(last.x - self.tip.outerWidth() - 12, 0); }
                self.tip.css({ left: left, top: top });
            }
            this.element.on("mousemove" + this._ns, function (e) {
                var off = self.element.offset();
                last = { x: e.pageX - off.left, y: e.pageY - off.top };
                if (self.tip.is(":visible")) { place(); }
            });
            this.surface.bind("mouseenter", function (e) {
                var i = e.element.info;
                if (!i) { return; }
                self.tip.html("<b>" + kendo.htmlEncode(i.column) + "</b><br>" +
                    kendo.htmlEncode(i.segment) + ": " + kendo.htmlEncode(self.options.format(i.value)) +
                    "<br>" + self.options.columnShareText + ": " + kendo.toString(i.shareInColumn, "p1") +
                    "<br>" + self.options.totalShareText + ": " + kendo.toString(i.shareOfTotal, "p1")).show();
                place();
            });
            this.surface.bind("mouseleave", function () { self.tip.hide(); });
            this.surface.bind("click", function (e) {
                if (e.element.info && self.options.click) { self.options.click(e.element.info); }
            });
            $(global).on("resize" + this._ns, function () {
                clearTimeout(self._resizeTimer);
                self._resizeTimer = setTimeout(function () { self.render(); }, 50);
            });
        },

        destroy: function () {
            clearTimeout(this._resizeTimer);
            $(global).off("resize" + this._ns);
            this.element.off("mousemove" + this._ns);
            this.surface.destroy();
            this.tip.remove();
        }
    };

    global.MarimekkoChart = MarimekkoChart;
})(window, jQuery, kendo);
