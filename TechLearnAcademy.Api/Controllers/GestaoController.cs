using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechLearnAcademy.Api.Modelos;
using TechLearnAcademy.Api.Servicos;

namespace TechLearnAcademy.Api.Controllers;

[Authorize, ApiController, Route("api")]
public class GestaoController(GestaoServico servico, UsuariosServico usuarios) : BaseController
{
    [Authorize(Roles = "Mentor"), HttpGet("mentor/alunos")]
    public async Task<IActionResult> Alunos() => Ok(await servico.AlunosMentor(Usuario.Id));
    [Authorize(Roles = "Mentor"), HttpGet("mentor/alunos/{id:int}")]
    public async Task<IActionResult> Aluno(int id) => Ok(await servico.AlunoMentor(Usuario.Id, id));
    [Authorize(Roles = "Mentor"), HttpPost("mentor/orientacoes")]
    public async Task<IActionResult> Orientar(DadosOrientacao dados) { await servico.Orientar(Usuario.Id, dados); return Mensagem("Orientação registrada."); }
    [Authorize(Roles = "Aluno"), HttpGet("aluno/orientacoes")]
    public async Task<IActionResult> Orientacoes() => Ok(await servico.OrientacoesAluno(Usuario.Id));

    [Authorize(Roles = "Gestor"), HttpGet("relatorios")]
    public async Task<IActionResult> Relatorios() => Ok(await servico.Relatorios());
    [Authorize(Roles = "Gestor"), HttpGet("relatorios/cursos/{id:int}/alunos")]
    public async Task<IActionResult> AlunosCurso(int id) => Ok(await servico.AlunosCurso(id));
    [Authorize(Roles = "Gestor"), HttpGet("relatorios/cursos/{cursoId:int}/alunos/{alunoId:int}")]
    public async Task<IActionResult> Desempenho(int cursoId, int alunoId) => Ok(await servico.Desempenho(cursoId, alunoId));

    [Authorize(Roles = "Administrador"), HttpGet("administracao/painel")]
    public async Task<IActionResult> Painel() => Ok(await servico.PainelAdmin());
    [Authorize(Roles = "Administrador,Gestor"), HttpGet("administracao/usuarios")]
    public async Task<IActionResult> Usuarios(string busca = "", int pagina = 1) => Ok(await servico.Usuarios(Usuario, busca, pagina));
    [Authorize(Roles = "Administrador,Gestor"), HttpGet("administracao/usuarios/{id:int}")]
    public async Task<IActionResult> UsuarioDetalhes(int id) => Ok(await servico.UsuarioDetalhes(Usuario, id));
    [Authorize(Roles = "Administrador,Gestor"), HttpPost("administracao/usuarios")]
    public async Task<IActionResult> CriarConta(DadosConta dados) => StatusCode(201, await usuarios.CriarConta(Usuario, dados));
    [Authorize(Roles = "Administrador,Gestor"), HttpPut("administracao/usuarios/{id:int}")]
    public async Task<IActionResult> AlterarUsuario(int id, DadosUsuario dados) { await servico.AlterarUsuario(Usuario, id, dados); return Mensagem("Dados atualizados. O perfil permanece o mesmo."); }

    [HttpGet("chamados")]
    public async Task<IActionResult> Chamados() => Ok(await servico.Chamados(Usuario));
    [Authorize(Roles = "Aluno,Mentor,Gestor"), HttpPost("chamados")]
    public async Task<IActionResult> AbrirChamado(DadosChamado dados) => StatusCode(201, new { id = await servico.AbrirChamado(Usuario, dados), mensagem = "Chamado enviado." });
    [HttpGet("chamados/{id:int}")]
    public async Task<IActionResult> Chamado(int id) => Ok(await servico.DetalhesChamado(id, Usuario));
    [Authorize(Roles = "Mentor,Gestor,Administrador"), HttpPost("chamados/{id:int}/assumir")]
    public async Task<IActionResult> Assumir(int id) { await servico.AssumirChamado(id, Usuario); return Mensagem("Você assumiu este chamado."); }
    [HttpPost("chamados/{id:int}/mensagens")]
    public async Task<IActionResult> Responder(int id, DadosMensagem dados) { await servico.ResponderChamado(id, Usuario, dados); return Mensagem("Mensagem enviada."); }
    [HttpPost("chamados/{id:int}/encerrar")]
    public async Task<IActionResult> Encerrar(int id) { await servico.EncerrarChamado(id, Usuario); return Mensagem("Chamado encerrado."); }

    [Authorize(Roles = "Aluno,Gestor"), HttpGet("certificados")]
    public async Task<IActionResult> Certificados() => Ok(await servico.Certificados(Usuario));
    [Authorize(Roles = "Aluno"), HttpPost("certificados")]
    public async Task<IActionResult> Solicitar(DadosCertificado dados) { await servico.SolicitarCertificado(Usuario, dados.CursoId); return Mensagem("Solicitação enviada ao gestor."); }
    [Authorize(Roles = "Gestor"), HttpPost("certificados/{id:int}/decisao")]
    public async Task<IActionResult> Decidir(int id, DadosCertificado dados) { await servico.DecidirCertificado(id, Usuario, dados); return Mensagem(dados.Aprovar ? "Certificado aprovado e gerado." : "Solicitação recusada."); }
    [Authorize(Roles = "Aluno,Gestor"), HttpGet("certificados/{id:int}")]
    public async Task<IActionResult> Certificado(int id) => Ok(await servico.CertificadoEmitido(id, Usuario));
}
