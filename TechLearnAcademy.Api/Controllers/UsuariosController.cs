using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechLearnAcademy.Api.Modelos;
using TechLearnAcademy.Api.Servicos;

namespace TechLearnAcademy.Api.Controllers;

[ApiController, Route("api/usuarios")]
public class UsuariosController(UsuariosServico servico) : BaseController
{
    [HttpPost("cadastro")]
    public async Task<IActionResult> Cadastro(DadosCadastro dados)
    {
        // O cadastro público sempre cria um Aluno.
        await servico.Cadastro(dados);
        return StatusCode(201, new { mensagem = "Usuário cadastrado com sucesso." });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(DadosLogin dados) => Ok(await servico.Login(dados, Response, Request.IsHttps));

    [HttpPost("recuperar-senha")]
    public async Task<IActionResult> Recuperar(DadosEmail dados)
    {
        await servico.Recuperar(dados);
        return Mensagem("Se o e-mail estiver cadastrado, você receberá as instruções de recuperação.");
    }

    [HttpPost("redefinir-senha")]
    public async Task<IActionResult> Redefinir(DadosSenha dados)
    {
        await servico.Redefinir(dados);
        Response.Cookies.Delete("TechLearnSessao");
        return Mensagem("Senha alterada. Entre com a nova senha.");
    }

    [Authorize, HttpGet("perfil")]
    public async Task<IActionResult> Perfil() => Ok(await servico.Perfil(Usuario.Id));

    [Authorize, HttpPut("perfil")]
    public async Task<IActionResult> Alterar(DadosPerfil dados)
    {
        await servico.AlterarPerfil(Usuario.Id, dados);
        return Mensagem("Dados atualizados.");
    }

    [Authorize(Roles = "Aluno,Mentor,Gestor"), HttpPost("trocar-senha")]
    public async Task<IActionResult> Trocar(DadosSenha dados)
    {
        await servico.TrocarSenha(Usuario.Id, dados);
        Response.Cookies.Delete("TechLearnSessao");
        return Mensagem("Senha alterada. Entre novamente.");
    }

    [Authorize, HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await servico.Logout(SessaoHandler.Token(Request));
        Response.Cookies.Delete("TechLearnSessao");
        return Mensagem("Você saiu da plataforma.");
    }
}
