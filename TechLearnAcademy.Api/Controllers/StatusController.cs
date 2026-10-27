using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechLearnAcademy.Api.Banco;

namespace TechLearnAcademy.Api.Controllers;

[ApiController, Route("api/status")]
public class StatusController(ConexaoBanco conexao) : BaseController
{
    [HttpGet]
    public IActionResult Status() => Mensagem("API TechLearn Academy disponível.");

    [Authorize(Roles = "Administrador"), HttpGet("banco")]
    public async Task<IActionResult> Banco()
    {
        using var banco = await conexao.Abrir();
        await ConexaoBanco.Numero(banco, "SELECT 1");
        return Mensagem("Conexão com o SQL Server disponível.");
    }
}
