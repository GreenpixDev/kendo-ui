// Статический сервер для стенда marimekko-harness.html (корень - папка Solution1).
// Запуск: node tests/serve.js [порт]   ->   http://localhost:8765/tests/marimekko-harness.html
var http = require("http"), fs = require("fs"), path = require("path");
var root = path.resolve(__dirname, "..");
var port = Number(process.argv[2]) || 8765;
var types = { ".html": "text/html; charset=utf-8", ".js": "application/javascript; charset=utf-8", ".css": "text/css; charset=utf-8" };

http.createServer(function (req, res) {
    var file = path.join(root, decodeURIComponent(req.url.split("?")[0]));
    if (file.indexOf(root) !== 0) { res.writeHead(403); return res.end(); }
    fs.readFile(file, function (err, data) {
        if (err) { res.writeHead(404); return res.end("Not found"); }
        res.writeHead(200, { "Content-Type": types[path.extname(file)] || "application/octet-stream" });
        res.end(data);
    });
}).listen(port, function () { console.log("http://localhost:" + port + "/tests/marimekko-harness.html"); });
