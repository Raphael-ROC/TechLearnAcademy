using System.Net;
using System.Text;
using System.Text.Json;

namespace TechLearnAcademy.Desktop;

public class ErroApi(int status, string mensagem) : Exception(mensagem)
{
    public int Status { get; } = status;
}

public sealed class ApiCliente : IDisposable
{
    private readonly HttpClient cliente;
    private readonly CookieContainer cookies = new();
    private readonly JsonSerializerOptions json = new() { PropertyNameCaseInsensitive = true };
    public Uri Endereco { get; }

    public ApiCliente(Uri endereco)
    {
        Endereco = endereco;
        // A sessão real fica somente na memória deste programa.
        var handler = new HttpClientHandler { CookieContainer = cookies, AllowAutoRedirect = false };
        // A primeira conexão pode aguardar a inicialização do banco na Azure.
        cliente = new HttpClient(handler) { BaseAddress = endereco, Timeout = TimeSpan.FromSeconds(90) };
    }

    public async Task<Usuario> EntrarAdministrador(string email, string senha)
    {
        var usuario = await Enviar<Usuario>("api/usuarios/login", HttpMethod.Post, new { email, senha });
        if (usuario.Perfil == "Administrador")
        {
            usuario = await Enviar<Usuario>("api/usuarios/perfil", HttpMethod.Get);
            if (usuario.Perfil == "Administrador") return usuario;
        }
        try { await Sair(); }
        catch (ErroApi) { LimparSessao(); }
        throw new ErroApi(403, "Este aplicativo é exclusivo para Administradores. Use o site com o seu perfil.");
    }

    // T indica a classe que receberá a resposta, como Usuario ou ResumoAdmin.
    public async Task<T> Enviar<T>(string caminho, HttpMethod metodo, object? dados = null)
    {
        using var pedido = new HttpRequestMessage(metodo, caminho);
        if (dados != null)
            pedido.Content = new StringContent(JsonSerializer.Serialize(dados), Encoding.UTF8, "application/json");
        try
        {
            using var resposta = await cliente.SendAsync(pedido);
            string texto = await resposta.Content.ReadAsStringAsync();
            if (!resposta.IsSuccessStatusCode)
            {
                string mensagem = "O servidor não concluiu a operação (HTTP " + (int)resposta.StatusCode + ").";
                try { mensagem = JsonSerializer.Deserialize<RespostaApi>(texto, json)?.Mensagem ?? mensagem; }
                catch (JsonException) { /* Uma falha de hospedagem pode devolver HTML. */ }
                if (string.IsNullOrWhiteSpace(mensagem)) mensagem = "O servidor não concluiu a operação.";
                throw new ErroApi((int)resposta.StatusCode, mensagem);
            }
            return JsonSerializer.Deserialize<T>(texto, json)
                ?? throw new ErroApi(0, "O servidor devolveu uma resposta vazia.");
        }
        catch (HttpRequestException)
        { throw new ErroApi(0, "Não foi possível conectar ao servidor. Verifique sua internet e a disponibilidade da API."); }
        catch (TaskCanceledException)
        { throw new ErroApi(408, "O servidor demorou para responder. Tente novamente."); }
        catch (JsonException)
        { throw new ErroApi(0, "O servidor devolveu uma resposta inesperada."); }
    }

    public async Task Sair()
    {
        try { await Enviar<RespostaApi>("api/usuarios/logout", HttpMethod.Post); }
        finally { LimparSessao(); }
    }

    public void LimparSessao()
    {
        foreach (Cookie cookie in cookies.GetAllCookies()) cookie.Expired = true;
    }

    public void Dispose()
    {
        LimparSessao();
        cliente.Dispose();
    }
}
