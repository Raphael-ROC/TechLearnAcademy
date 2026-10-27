var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();
string pastaWeb = Path.Combine(app.Environment.ContentRootPath, "wwwroot");
app.Use(async (contexto, proximo) =>
{
    contexto.Response.Headers.CacheControl = "no-store";
    await proximo();
});
app.UseDefaultFiles();

// A versão muda quando algum arquivo é salvo. HTML, JS e CSS chegam juntos.
string VersaoSite() => Math.Max(File.GetLastWriteTimeUtc(typeof(Program).Assembly.Location).Ticks,
    Directory.EnumerateFiles(pastaWeb, "*", SearchOption.AllDirectories).Select(f => File.GetLastWriteTimeUtc(f).Ticks).DefaultIfEmpty(0).Max()).ToString();
app.MapGet("/versao-site", () => new { versao = VersaoSite() });
app.Use(async (contexto, proximo) =>
{
    string caminho = contexto.Request.Path.Value ?? "";
    if ((contexto.Request.Method == "GET" || contexto.Request.Method == "HEAD") && caminho.EndsWith(".html", StringComparison.OrdinalIgnoreCase))
    {
        string arquivo = Path.GetFullPath(Path.Combine(pastaWeb, caminho.TrimStart('/')));
        if (arquivo.StartsWith(pastaWeb + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase) && File.Exists(arquivo))
        {
            string versao = VersaoSite();
            string html = await File.ReadAllTextAsync(arquivo);
            html = System.Text.RegularExpressions.Regex.Replace(html, "(src|href)=\"((js|css)/[^\"?]+)\"",
                m => m.Groups[1].Value + "=\"" + m.Groups[2].Value + "?v=" + versao + "\"");
            html = html.Replace("</head>", "<meta name=\"versao-site\" content=\"" + versao + "\"></head>");
            contexto.Response.ContentType = "text/html; charset=utf-8";
            if (contexto.Request.Method != "HEAD") await contexto.Response.WriteAsync(html);
            return;
        }
    }
    await proximo();
});

app.UseStaticFiles();
app.Run();


