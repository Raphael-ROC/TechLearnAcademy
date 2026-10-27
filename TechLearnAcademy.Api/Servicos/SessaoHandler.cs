using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using TechLearnAcademy.Api.Banco;
using TechLearnAcademy.Api.Modelos;

namespace TechLearnAcademy.Api.Servicos;

// Cada solicitação consulta a sessão real e o perfil atual no banco.
public class SessaoHandler(IOptionsMonitor<AuthenticationSchemeOptions> opcoes, ILoggerFactory log, UrlEncoder encoder, ConexaoBanco conexao)
    : AuthenticationHandler<AuthenticationSchemeOptions>(opcoes, log, encoder)
{
    public static string Token(HttpRequest pedido)
    {
        string cabecalho = pedido.Headers.Authorization.ToString();
        if (cabecalho.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)) return cabecalho[7..];
        return pedido.Cookies["TechLearnSessao"] ?? "";
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        string token = Token(Request);
        if (token == "") return AuthenticateResult.NoResult();
        if (token.Length != 64 || token.Any(c => !Uri.IsHexDigit(c))) return AuthenticateResult.Fail("Sessão inválida.");
        using var banco = await conexao.Abrir();
        var usuarios = await ConexaoBanco.Consultar(banco, "SELECT u.Id AS id,u.Nome AS nome,u.Email AS email,u.Perfil AS perfil FROM Sessoes s JOIN Usuarios u ON u.Id=s.UsuarioId WHERE s.TokenHash=@hash AND s.Revogada=0 AND s.Validade>SYSUTCDATETIME() AND u.Ativo=1", new { hash = Regras.HashToken(token) });
        if (usuarios.Count != 1) return AuthenticateResult.Fail("Sessão vencida ou revogada.");
        var usuario = Usuario.DoBanco(usuarios[0]);
        var identidade = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new Claim(ClaimTypes.Name, usuario.Nome),
            new Claim(ClaimTypes.Email, usuario.Email),
            new Claim(ClaimTypes.Role, usuario.Perfil)
        }, Scheme.Name);
        return AuthenticateResult.Success(new AuthenticationTicket(new ClaimsPrincipal(identidade), Scheme.Name));
    }

    protected override Task HandleChallengeAsync(AuthenticationProperties propriedades)
    {
        Response.StatusCode = 401;
        return Response.WriteAsJsonAsync(new { mensagem = "Entre novamente para acessar esta função." });
    }

    protected override Task HandleForbiddenAsync(AuthenticationProperties propriedades)
    {
        Response.StatusCode = 403;
        return Response.WriteAsJsonAsync(new { mensagem = "Seu perfil não permite esta função." });
    }
}
