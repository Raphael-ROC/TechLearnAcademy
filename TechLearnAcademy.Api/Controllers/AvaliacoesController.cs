using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechLearnAcademy.Api.Modelos;
using TechLearnAcademy.Api.Servicos;

namespace TechLearnAcademy.Api.Controllers;

[Authorize, ApiController, Route("api")]
public class AvaliacoesController(AvaliacoesServico servico) : BaseController
{
    [Authorize(Roles = "Aluno,Gestor"), HttpGet("modulos/{id:int}/avaliacoes")]
    public async Task<IActionResult> Listar(int id) => Ok(await servico.Listar(id, Usuario));

    [Authorize(Roles = "Aluno,Gestor"), HttpGet("avaliacoes/{id:int}")]
    public async Task<IActionResult> Detalhes(int id) => Ok(await servico.Detalhes(id, Usuario));

    [Authorize(Roles = "Gestor"), HttpPost("avaliacoes")]
    public async Task<IActionResult> Criar(DadosAvaliacao dados) => StatusCode(201, new { id = await servico.Criar(dados), mensagem = "Avaliação cadastrada." });

    [Authorize(Roles = "Gestor"), HttpPut("avaliacoes/{id:int}")]
    public async Task<IActionResult> Alterar(int id, DadosAvaliacao dados) { await servico.Alterar(id, dados); return Mensagem("Avaliação atualizada."); }

    [Authorize(Roles = "Gestor"), HttpPost("questoes")]
    public async Task<IActionResult> CriarQuestao(DadosQuestao dados) => StatusCode(201, new { id = await servico.CriarQuestao(dados), mensagem = "Questão cadastrada." });

    [Authorize(Roles = "Gestor"), HttpPut("questoes/{id:int}")]
    public async Task<IActionResult> AlterarQuestao(int id, DadosQuestao dados) { await servico.AlterarQuestao(id, dados); return Mensagem("Questão atualizada."); }

    [Authorize(Roles = "Aluno"), HttpPost("avaliacoes/{id:int}/respostas")]
    public async Task<IActionResult> Corrigir(int id, DadosProva dados) => Ok(await servico.Corrigir(id, dados, Usuario));

    [Authorize(Roles = "Aluno"), HttpGet("resultados")]
    public async Task<IActionResult> Resultados() => Ok(await servico.Resultados(Usuario.Id));

    [Authorize(Roles = "Aluno,Mentor,Gestor"), HttpGet("resultados/{id:int}")]
    public async Task<IActionResult> Resultado(int id) => Ok(await servico.Resultado(id, Usuario));

    [Authorize(Roles = "Gestor"), HttpDelete("avaliacoes/{id:int}")]
    public async Task<IActionResult> Excluir(int id) { await servico.Excluir(id); return Mensagem("Avaliação excluída."); }
}
