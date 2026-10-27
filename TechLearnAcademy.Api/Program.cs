using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.FileProviders;
using TechLearnAcademy.Api.Banco;
using TechLearnAcademy.Api.Modelos;
using TechLearnAcademy.Api.Servicos;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers().ConfigureApiBehaviorOptions(opcoes =>
{
    opcoes.InvalidModelStateResponseFactory = contexto => new BadRequestObjectResult(new { mensagem = "Confira os campos obrigatórios e o formato dos dados enviados." });
});
builder.Services.AddScoped<ConexaoBanco>();
builder.Services.AddScoped<UsuariosServico>();
builder.Services.AddScoped<CursosServico>();
builder.Services.AddScoped<AvaliacoesServico>();
builder.Services.AddScoped<GestaoServico>();
builder.Services.AddAuthentication("Sessao").AddScheme<AuthenticationSchemeOptions, SessaoHandler>("Sessao", null);
builder.Services.AddAuthorization();

string[] origens = builder.Configuration.GetSection("OrigensPermitidas").Get<string[]>() ?? [];
builder.Services.AddCors(opcoes => opcoes.AddDefaultPolicy(politica =>
{
    if (origens.Length > 0) politica.WithOrigins(origens).AllowAnyHeader().AllowAnyMethod().AllowCredentials();
}));

var app = builder.Build();

// Este comando é opcional e nunca cria um administrador automaticamente.
if (args.Contains("--criar-admin"))
{
    using var escopo = app.Services.CreateScope();
    var usuarios = escopo.ServiceProvider.GetRequiredService<UsuariosServico>();
    Console.Write("Nome do primeiro Administrador: ");
    string nome = Console.ReadLine() ?? "";
    Console.Write("E-mail: ");
    string email = Console.ReadLine() ?? "";
    Console.Write("Senha (não será exibida): ");
    string senha = LerSenha();
    Console.Write("Confirmar senha: ");
    string confirmacao = LerSenha();
    try
    {
        await usuarios.PrimeiroAdministrador(new DadosCadastro { Nome = nome, Email = email, Senha = senha, ConfirmarSenha = confirmacao });
        Console.WriteLine("Administrador criado com os dados informados.");
    }
    catch (ErroAplicacao erro) { Console.Error.WriteLine(erro.Message); Environment.ExitCode = 1; }
    catch (SqlException) { Console.Error.WriteLine("Não foi possível acessar o banco configurado. Verifique a conexão e o script da nova solução."); Environment.ExitCode = 1; }
    return;
}

app.Use(async (contexto, proximo) =>
{
    try
    {
        if (contexto.Request.Path.StartsWithSegments("/api"))
        {

            // O cookie só pode alterar dados a partir da mesma origem autorizada.
            string origem = contexto.Request.Headers.Origin.ToString();
            string propriaOrigem = contexto.Request.Scheme + "://" + contexto.Request.Host;
            bool gravacao = contexto.Request.Method != "GET" && contexto.Request.Method != "HEAD" && contexto.Request.Method != "OPTIONS";
            if (gravacao && origem != "" && origem != propriaOrigem && !origens.Contains(origem))
                throw new ErroAplicacao(403, "Esta origem não pode enviar dados para a API.");
        }
        contexto.Response.Headers.CacheControl = "no-store";
        contexto.Response.Headers.XContentTypeOptions = "nosniff";
        contexto.Response.Headers["Referrer-Policy"] = "no-referrer";
        await proximo();
    }
    catch (ErroAplicacao erro)
    {
        contexto.Response.StatusCode = erro.Status;
        await contexto.Response.WriteAsJsonAsync(new { mensagem = erro.Message });
    }
    catch (SqlException erro)
    {
        // O log identifica a falha sem mostrar consultas, senha ou dados pessoais.
        app.Logger.LogError("Falha SQL {Numero} na operação {Caminho}.", erro.Number, contexto.Request.Path);
        int status = 503;
        string mensagem = "Não foi possível acessar o SQL Server. Verifique a conexão e a criação do banco da nova solução.";
        if (erro.Number == 2601 || erro.Number == 2627) { status = 409; mensagem = "Já existe um registro com esses dados."; }
        if (erro.Number == 547 || erro.Number == 51001 || erro.Number == 51002) { status = 400; mensagem = "Verifique os dados e os registros relacionados. O registro pode estar inativo."; }
        if (erro.Number == 51003) { status = 409; mensagem = "Você já está matriculado neste curso."; }
        if (erro.Number == 51004) { status = 409; mensagem = "O curso pode ter no máximo quatro módulos."; }
        if (erro.Number == 207 || erro.Number == 208) { mensagem = "O banco precisa da atualização. Execute Banco/05-atualizar-funcionalidades.sql e Banco/07-corrigir-recuperacao-e-avaliacoes.sql no SQL Server e reinicie o site."; }
        contexto.Response.StatusCode = status;
        await contexto.Response.WriteAsJsonAsync(new { mensagem });
    }
    catch (Exception erro)
    {
        app.Logger.LogError("Falha {Tipo} na operação {Caminho}.", erro.GetType().Name, contexto.Request.Path);
        contexto.Response.StatusCode = 500;
        await contexto.Response.WriteAsJsonAsync(new { mensagem = "Não foi possível concluir a operação. Tente novamente." });
    }
});

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
    app.UseHttpsRedirection();
}

// A API entrega também o site, para evitar configuração de origens no piloto.
string pastaWeb = Path.Combine(app.Environment.ContentRootPath, "wwwroot");
string pastaFonte = Path.GetFullPath(Path.Combine(app.Environment.ContentRootPath, "..", "TechLearnAcademy.Web", "wwwroot"));
if ((app.Environment.IsDevelopment() && Directory.Exists(pastaFonte)) || !File.Exists(Path.Combine(pastaWeb, "index.html")))
    pastaWeb = pastaFonte;
if (Directory.Exists(pastaWeb))
{
    var arquivos = new PhysicalFileProvider(pastaWeb);
    app.UseDefaultFiles(new DefaultFilesOptions { FileProvider = arquivos });

    // A versão muda quando algum arquivo é salvo. HTML, JS e CSS chegam juntos.
    string VersaoSite() => Math.Max(File.GetLastWriteTimeUtc(typeof(Usuario).Assembly.Location).Ticks,
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
    app.UseStaticFiles(new StaticFileOptions { FileProvider = arquivos });
}

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapFallback("/api/{**caminho}", () => Results.NotFound(new { mensagem = "Rota da API não encontrada." }));
app.Run();

static string LerSenha()
{
    string senha = "";
    while (true)
    {
        var tecla = Console.ReadKey(true);
        if (tecla.Key == ConsoleKey.Enter) { Console.WriteLine(); return senha; }
        if (tecla.Key == ConsoleKey.Backspace && senha.Length > 0) senha = senha[..^1];
        else if (!char.IsControl(tecla.KeyChar)) senha += tecla.KeyChar;
    }
}
