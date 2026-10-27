using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechLearnAcademy.Api.Modelos;
using TechLearnAcademy.Api.Servicos;

namespace TechLearnAcademy.Api.Controllers;

[Authorize, ApiController, Route("api")]
public class CursosController(CursosServico servico) : BaseController
{
    [Authorize(Roles = "Aluno,Gestor"), HttpGet("cursos")]
    public async Task<IActionResult> Listar(string busca = "", int pagina = 1, bool todos = false) => Ok(await servico.Listar(Usuario, busca, pagina, todos));

    [Authorize(Roles = "Aluno,Gestor"), HttpGet("cursos/{id:int}")]
    public async Task<IActionResult> Detalhes(int id) => Ok(await servico.Detalhes(id, Usuario));

    [Authorize(Roles = "Gestor"), HttpPost("cursos")]
    public async Task<IActionResult> CriarCurso(DadosCurso dados) => StatusCode(201, new { id = await servico.CriarCurso(dados), mensagem = "Curso cadastrado." });

    [Authorize(Roles = "Gestor"), HttpPut("cursos/{id:int}")]
    public async Task<IActionResult> AlterarCurso(int id, DadosCurso dados) { await servico.AlterarCurso(id, dados); return Mensagem("Curso atualizado."); }

    [Authorize(Roles = "Gestor"), HttpPost("modulos")]
    public async Task<IActionResult> CriarModulo(DadosModulo dados) => StatusCode(201, new { id = await servico.CriarModulo(dados), mensagem = "Módulo cadastrado." });

    [Authorize(Roles = "Gestor"), HttpPut("modulos/{id:int}")]
    public async Task<IActionResult> AlterarModulo(int id, DadosModulo dados) { await servico.AlterarModulo(id, dados); return Mensagem("Módulo atualizado."); }

    [Authorize(Roles = "Aluno,Gestor"), HttpGet("modulos/{id:int}/materiais")]
    public async Task<IActionResult> Materiais(int id) => Ok(await servico.Materiais(id, Usuario));

    [Authorize(Roles = "Gestor"), HttpPost("materiais")]
    public async Task<IActionResult> CriarMaterial(DadosMaterial dados) => StatusCode(201, new { id = await servico.CriarMaterial(dados), mensagem = "Material cadastrado." });

    [Authorize(Roles = "Gestor"), HttpPut("materiais/{id:int}")]
    public async Task<IActionResult> AlterarMaterial(int id, DadosMaterial dados) { await servico.AlterarMaterial(id, dados); return Mensagem("Material atualizado."); }

    [Authorize(Roles = "Aluno"), HttpPost("matriculas")]
    public async Task<IActionResult> Matricular(DadosMatricula dados) { await servico.Matricular(Usuario.Id, dados.CursoId); return StatusCode(201, new { mensagem = "Matrícula realizada." }); }

    [Authorize(Roles = "Aluno"), HttpGet("matriculas/minhas")]
    public async Task<IActionResult> Minhas() => Ok(await servico.Minhas(Usuario.Id));

    [Authorize(Roles = "Aluno"), HttpPost("cursos/{id:int}/reiniciar")]
    public async Task<IActionResult> Reiniciar(int id) { await servico.Reiniciar(Usuario, id); return Mensagem("Curso reiniciado. O progresso foi zerado e as notas anteriores continuam no histórico."); }

    [Authorize(Roles = "Aluno"), HttpPost("progresso")]
    public async Task<IActionResult> Concluir(DadosProgresso dados) { await servico.Concluir(Usuario, dados.ModuloId); return Mensagem("Módulo concluído."); }

    [Authorize(Roles = "Aluno"), HttpGet("aluno/painel")]
    public async Task<IActionResult> Painel() => Ok(await servico.Painel(Usuario));

    [Authorize(Roles = "Gestor"), HttpDelete("cursos/{id:int}")]
    public async Task<IActionResult> ExcluirCurso(int id) { await servico.Excluir("curso", id); return Mensagem("Curso excluído."); }
    [Authorize(Roles = "Gestor"), HttpDelete("modulos/{id:int}")]
    public async Task<IActionResult> ExcluirModulo(int id) { await servico.Excluir("modulo", id); return Mensagem("Módulo excluído."); }
    [Authorize(Roles = "Gestor"), HttpDelete("materiais/{id:int}")]
    public async Task<IActionResult> ExcluirMaterial(int id) { await servico.Excluir("material", id); return Mensagem("Material excluído."); }

    [Authorize(Roles = "Gestor"), HttpPost("materiais/pdf"), RequestSizeLimit(11 * 1024 * 1024)]
    public async Task<IActionResult> CriarPdf([FromForm] int moduloId, [FromForm] string titulo, IFormFile arquivo)
        => StatusCode(201, new { id = await servico.SalvarPdf(0, moduloId, titulo, true, arquivo), mensagem = "PDF cadastrado." });
    [Authorize(Roles = "Gestor"), HttpPut("materiais/{id:int}/pdf"), RequestSizeLimit(11 * 1024 * 1024)]
    public async Task<IActionResult> AlterarPdf(int id, [FromForm] string titulo, [FromForm] bool ativo, IFormFile arquivo)
        { await servico.SalvarPdf(id, 0, titulo, ativo, arquivo); return Mensagem("PDF atualizado."); }
    [Authorize(Roles = "Aluno,Gestor"), HttpGet("materiais/{id:int}/arquivo")]
    public async Task<IActionResult> Arquivo(int id) { var pdf = await servico.Arquivo(id, Usuario); return File(pdf.Bytes, "application/pdf", pdf.Nome); }
}
