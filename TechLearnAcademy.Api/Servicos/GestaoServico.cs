using Microsoft.Data.SqlClient;
using TechLearnAcademy.Api.Banco;
using TechLearnAcademy.Api.Modelos;

namespace TechLearnAcademy.Api.Servicos;

// Contas, chamados, certificados e relatórios usam o mesmo serviço de gestão.
public class GestaoServico(ConexaoBanco conexao, CursosServico cursos)
{
    public async Task<object> PainelAdmin()
    {
        using var banco = await conexao.Abrir();
        var totais = await ConexaoBanco.Consultar(banco, "SELECT COUNT(CASE WHEN Ativo=1 THEN 1 END) AS ativos,COUNT(CASE WHEN Ativo=0 THEN 1 END) AS inativos FROM Usuarios");
        var perfis = await ConexaoBanco.Consultar(banco, "SELECT Perfil AS perfil,COUNT(*) AS quantidade FROM Usuarios GROUP BY Perfil ORDER BY Perfil");
        return new { ativos = totais[0]["ativos"], inativos = totais[0]["inativos"], perfis };
    }

    public async Task<object> Usuarios(Usuario autor, string busca, int pagina)
    {
        Regras.Exigir(pagina >= 1 && pagina <= 1000000, "Informe uma página válida.");
        using var banco = await conexao.Abrir();
        var dados = new { busca = "%" + (busca ?? "").Trim() + "%", inicio = (pagina - 1) * 15, admin = autor.Perfil == "Administrador" };
        string filtro = " WHERE (@admin=1 OR Perfil IN('Aluno','Mentor')) AND (Nome LIKE @busca OR Email LIKE @busca)";
        int total = await ConexaoBanco.Numero(banco, "SELECT COUNT(*) FROM Usuarios" + filtro, dados);
        var itens = await ConexaoBanco.Consultar(banco, "SELECT Id AS id,Nome AS nome,Email AS email,EmailContato AS emailContato,Perfil AS perfil,Ativo AS ativo FROM Usuarios" + filtro + " ORDER BY Nome,Id OFFSET @inicio ROWS FETCH NEXT 15 ROWS ONLY", dados);
        return new { itens, pagina, totalPaginas = Math.Max(1, (total + 14) / 15), total };
    }

    public async Task<object> UsuarioDetalhes(Usuario autor, int id)
    {
        using var banco = await conexao.Abrir();
        var lista = await ConexaoBanco.Consultar(banco, "SELECT Id AS id,Nome AS nome,Email AS email,EmailContato AS emailContato,Perfil AS perfil,Ativo AS ativo FROM Usuarios WHERE Id=@id AND (@admin=1 OR Perfil IN('Aluno','Mentor'))", new { id, admin = autor.Perfil == "Administrador" });
        Regras.Exigir(lista.Count == 1, "Usuário não encontrado ou sem permissão.", 404);
        return lista[0];
    }

    public async Task AlterarUsuario(Usuario autor, int id, DadosUsuario dados)
    {
        using var banco = await conexao.Abrir();
        using var transacao = banco.BeginTransaction(System.Data.IsolationLevel.Serializable);
        var admins = await ConexaoBanco.Consultar(banco, "SELECT Id AS id FROM Usuarios WITH(UPDLOCK,HOLDLOCK) WHERE Perfil='Administrador' AND Ativo=1", null, transacao);
        var lista = await ConexaoBanco.Consultar(banco, "SELECT Nome AS nome,Email AS email,EmailContato AS emailContato,Perfil AS perfil,Ativo AS ativo FROM Usuarios WITH(UPDLOCK,HOLDLOCK) WHERE Id=@id", new { id }, transacao);
        Regras.Exigir(lista.Count == 1, "Usuário não encontrado.", 404);
        var atual = lista[0];
        string perfil = (string)atual["perfil"]!;
        Regras.Exigir(autor.Perfil == "Administrador" || (autor.Perfil == "Gestor" && (perfil == "Aluno" || perfil == "Mentor")), "O gestor pode editar somente alunos e mentores.", 403);
        bool ultimoAdmin = admins.Count == 1 && (int)admins[0]["id"]! == id;
        Regras.Exigir(!ultimoAdmin || dados.Ativo, "Mantenha ao menos um Administrador ativo.", 409);
        Regras.Exigir(autor.Perfil == "Administrador" || dados.Ativo == (bool)atual["ativo"]!, "Somente o administrador pode ativar ou desativar contas.", 403);
        string nome = Regras.Texto(string.IsNullOrWhiteSpace(dados.Nome) ? (string)atual["nome"]! : dados.Nome, 2, 100, "Nome");
        string email = perfil == "Aluno" && !string.IsNullOrWhiteSpace(dados.Email) ? Regras.Email(dados.Email) : (string)atual["email"]!;
        string contato = Regras.Email(string.IsNullOrWhiteSpace(dados.EmailContato) ? (string?)atual["emailContato"] ?? email : dados.EmailContato);
        await Regras.EmailDisponivel(banco, email, contato, id, transacao);
        // Perfil e e-mail institucional não mudam durante uma edição.
        await ConexaoBanco.Gravar(banco, "UPDATE Usuarios SET Nome=@nome,Email=@email,EmailContato=@contato,Ativo=@ativo WHERE Id=@id; UPDATE Sessoes SET Revogada=1 WHERE UsuarioId=@id", new { id, nome, email, contato, ativo = dados.Ativo }, transacao);
        if (!dados.Ativo)
            await ConexaoBanco.Gravar(banco, "UPDATE Chamados SET ResponsavelId=NULL,Status='Aberto' WHERE ResponsavelId=@id AND Status='Em atendimento'", new { id }, transacao);
        await transacao.CommitAsync();
    }

    // Chamados: aluno -> mentor/gestor; mentor -> gestor; gestor -> administrador.
    public async Task<int> AbrirChamado(Usuario usuario, DadosChamado dados)
    {
        Regras.Exigir(usuario.Perfil != "Administrador", "O administrador recebe os chamados dos gestores.", 403);
        string assunto = Regras.Texto(dados.Assunto, 3, 150, "Assunto");
        string texto = Regras.Texto(dados.Texto, 3, 4000, "Mensagem");
        string tipo = usuario.Perfil == "Gestor" ? "ContatoAdministracao" : "Ouvidoria";
        string destino = usuario.Perfil == "Gestor" ? "Administrador" : "Gestor";
        int? cursoId = null;
        using var banco = await conexao.Abrir();
        if (dados.CursoId > 0)
        {
            Regras.Aluno(usuario);
            await Regras.AcessoCurso(banco, dados.CursoId, usuario);
            tipo = "AjudaCurso";
            destino = "Mentor";
            cursoId = dados.CursoId;
        }
        return await ConexaoBanco.Numero(banco, "INSERT INTO Chamados(SolicitanteId,CursoId,Tipo,DestinatarioPerfil,Assunto,Descricao) VALUES(@autor,@cursoId,@tipo,@destino,@assunto,@texto); SELECT CAST(SCOPE_IDENTITY() AS int)", new { autor = usuario.Id, cursoId, tipo, destino, assunto, texto });
    }

    private const string ChamadoSql = "SELECT c.Id AS id,c.SolicitanteId AS solicitanteId,c.ResponsavelId AS responsavelId,c.CursoId AS cursoId,c.Tipo AS tipo,c.DestinatarioPerfil AS destinatarioPerfil,c.Assunto AS assunto,c.Descricao AS descricao,c.Status AS status,c.DataAbertura AS data,c.DataEncerramento AS dataEncerramento,u.Nome AS solicitante,r.Nome AS responsavel,cu.Nome AS curso FROM Chamados c JOIN Usuarios u ON u.Id=c.SolicitanteId LEFT JOIN Usuarios r ON r.Id=c.ResponsavelId LEFT JOIN Cursos cu ON cu.Id=c.CursoId";

    public async Task<object> Chamados(Usuario usuario)
    {
        using var banco = await conexao.Abrir();
        string filtro = " WHERE c.SolicitanteId=@id OR (c.SolicitanteId<>@id AND c.DestinatarioPerfil=@perfil AND (c.ResponsavelId IS NULL OR c.ResponsavelId=@id)) OR (@perfil='Gestor' AND c.SolicitanteId<>@id AND c.DestinatarioPerfil IN('Mentor','Gestor'))";
        var lista = await ConexaoBanco.Consultar(banco, ChamadoSql + filtro + " ORDER BY CASE WHEN c.Status='Encerrado' THEN 1 ELSE 0 END,c.DataAbertura DESC,c.Id DESC", new { id = usuario.Id, perfil = usuario.Perfil });
        foreach (var chamado in lista) PermissoesChamado(chamado, usuario);
        return lista;
    }

    private static void PermissoesChamado(Dictionary<string, object?> chamado, Usuario usuario)
    {
        bool proprio = (int)chamado["solicitanteId"]! == usuario.Id;
        bool responsavel = !proprio && chamado["responsavelId"] != null && (int)chamado["responsavelId"]! == usuario.Id;
        bool aberto = (string)chamado["status"]! != "Encerrado";
        chamado["podeAtender"] = !proprio && (string)chamado["status"]! == "Aberto" && chamado["responsavelId"] == null && (string)chamado["destinatarioPerfil"]! == usuario.Perfil;
        chamado["podeResponder"] = aberto && (responsavel || (proprio && usuario.Perfil == "Aluno" && chamado["responsavelId"] != null));
        chamado["podeEncerrar"] = aberto && responsavel;
    }

    private static async Task<Dictionary<string, object?>> AcessoChamado(SqlConnection banco, int id, Usuario usuario, SqlTransaction? transacao = null)
    {
        var lista = await ConexaoBanco.Consultar(banco, ChamadoSql + " WHERE c.Id=@id", new { id }, transacao);
        Regras.Exigir(lista.Count == 1, "Chamado não encontrado.", 404);
        var chamado = lista[0];
        bool proprio = (int)chamado["solicitanteId"]! == usuario.Id;
        bool destino = (string)chamado["destinatarioPerfil"]! == usuario.Perfil;
        bool moderacao = usuario.Perfil == "Gestor" && (string)chamado["tipo"]! != "ContatoAdministracao";
        bool responsavel = chamado["responsavelId"] != null && (int)chamado["responsavelId"]! == usuario.Id;
        Regras.Exigir(proprio || moderacao || (destino && (responsavel || chamado["responsavelId"] == null)), "Você não tem acesso a este chamado.", 403);
        return chamado;
    }

    public async Task<object> DetalhesChamado(int id, Usuario usuario)
    {
        using var banco = await conexao.Abrir();
        var chamado = await AcessoChamado(banco, id, usuario);
        PermissoesChamado(chamado, usuario);
        chamado["mensagens"] = await ConexaoBanco.Consultar(banco, "SELECT m.Id AS id,m.Texto AS texto,m.DataEnvio AS data,u.Nome AS nome,u.Perfil AS perfil FROM MensagensChamado m JOIN Usuarios u ON u.Id=m.UsuarioId WHERE m.ChamadoId=@id ORDER BY m.Id", new { id });
        return chamado;
    }

    public async Task AssumirChamado(int id, Usuario usuario)
    {
        using var banco = await conexao.Abrir();
        int alterados = await ConexaoBanco.Gravar(banco, "UPDATE Chamados SET ResponsavelId=@usuarioId,Status='Em atendimento' WHERE Id=@id AND DestinatarioPerfil=@perfil AND ResponsavelId IS NULL AND Status='Aberto' AND SolicitanteId<>@usuarioId", new { id, usuarioId = usuario.Id, perfil = usuario.Perfil });
        Regras.Exigir(alterados == 1, "Este chamado já foi assumido ou não pertence à sua fila. Atualize a lista.", 409);
    }

    public async Task ResponderChamado(int id, Usuario usuario, DadosMensagem dados)
    {
        string texto = Regras.Texto(dados.Texto, 1, 4000, "Mensagem");
        using var banco = await conexao.Abrir();
        using var transacao = banco.BeginTransaction(System.Data.IsolationLevel.Serializable);
        var chamado = await AcessoChamado(banco, id, usuario, transacao);
        Regras.Exigir((string)chamado["status"]! != "Encerrado", "Este chamado já foi encerrado.", 409);
        PermissoesChamado(chamado, usuario);
        bool autor = (int)chamado["solicitanteId"]! == usuario.Id;
        bool responsavel = !autor && chamado["responsavelId"] != null && (int)chamado["responsavelId"]! == usuario.Id;
        Regras.Exigir(!autor || usuario.Perfil == "Aluno", "Você pode acompanhar o status do seu chamado, mas não responder a ele.", 403);
        Regras.Exigir((bool)chamado["podeResponder"]!, autor ? "Aguarde um responsável assumir seu chamado para continuar a conversa." : "Assuma o chamado antes de responder.", 403);
        await ConexaoBanco.Gravar(banco, "INSERT INTO MensagensChamado(ChamadoId,UsuarioId,Texto) VALUES(@id,@usuarioId,@texto)", new { id, usuarioId = usuario.Id, texto }, transacao);
        if (responsavel && usuario.Perfil == "Mentor" && (string)chamado["tipo"]! == "AjudaCurso")
        {
            var orientacao = new OrientacaoMentor(usuario.Id, (int)chamado["solicitanteId"]!, texto);
            await ConexaoBanco.Gravar(banco, "INSERT INTO OrientacoesMentor(MentorId,AlunoId,Texto) VALUES(@mentorId,@alunoId,@texto)", new { mentorId = orientacao.MentorId, alunoId = orientacao.AlunoId, texto = orientacao.Texto }, transacao);
        }
        await transacao.CommitAsync();
    }

    public async Task EncerrarChamado(int id, Usuario usuario)
    {
        using var banco = await conexao.Abrir();
        using var transacao = banco.BeginTransaction(System.Data.IsolationLevel.Serializable);
        var chamado = await AcessoChamado(banco, id, usuario, transacao);
        PermissoesChamado(chamado, usuario);
        Regras.Exigir((bool)chamado["podeEncerrar"]!, "Somente o responsável pelo atendimento pode encerrar o chamado.", 403);
        Regras.Exigir((string)chamado["status"]! != "Encerrado", "Este chamado já foi encerrado.", 409);
        await ConexaoBanco.Gravar(banco, "UPDATE Chamados SET Status='Encerrado',DataEncerramento=SYSUTCDATETIME() WHERE Id=@id", new { id }, transacao);
        await transacao.CommitAsync();
    }

    public async Task<object> AlunosMentor(int mentorId)
    {
        using var banco = await conexao.Abrir();
        return await ConexaoBanco.Consultar(banco, "SELECT DISTINCT u.Id AS id,u.Nome AS nome,u.Email AS email,CAST(0 AS decimal(5,2)) AS progresso,CAST(0 AS decimal(5,2)) AS media FROM Chamados c JOIN Usuarios u ON u.Id=c.SolicitanteId WHERE c.ResponsavelId=@mentorId AND c.Tipo='AjudaCurso' ORDER BY u.Nome", new { mentorId });
    }

    public async Task<object> AlunoMentor(int mentorId, int alunoId)
    {
        using var banco = await conexao.Abrir();
        await Regras.AcessoAlunoMentor(banco, mentorId, alunoId);
        var aluno = (await ConexaoBanco.Consultar(banco, "SELECT Id AS id,Nome AS nome,Email AS email FROM Usuarios WHERE Id=@alunoId", new { alunoId }))[0];
        var atendidos = await ConexaoBanco.Consultar(banco, "SELECT DISTINCT CursoId AS id FROM Chamados WHERE ResponsavelId=@mentorId AND SolicitanteId=@alunoId AND Tipo='AjudaCurso'", new { mentorId, alunoId });
        var ids = atendidos.Select(c => (int)c["id"]!).ToList();
        aluno["cursos"] = (await cursos.Minhas(alunoId)).Where(c => ids.Contains((int)c["cursoId"]!)).ToList();
        aluno["resultados"] = await ConexaoBanco.Consultar(banco, "SELECT t.Id AS id,a.Titulo AS titulo,t.Nota AS nota,t.Aprovado AS aprovado FROM TentativasAvaliacao t JOIN Avaliacoes a ON a.Id=t.AvaliacaoId JOIN Modulos m ON m.Id=a.ModuloId WHERE t.AlunoId=@alunoId AND EXISTS(SELECT 1 FROM Chamados c WHERE c.ResponsavelId=@mentorId AND c.SolicitanteId=@alunoId AND c.CursoId=m.CursoId AND c.Tipo='AjudaCurso') ORDER BY t.Id DESC", new { alunoId, mentorId });
        aluno["orientacoes"] = await ConexaoBanco.Consultar(banco, "SELECT Id AS id,Texto AS texto,DataOrientacao AS data FROM OrientacoesMentor WHERE MentorId=@mentorId AND AlunoId=@alunoId ORDER BY Id DESC", new { mentorId, alunoId });
        return aluno;
    }

    public async Task Orientar(int mentorId, DadosOrientacao dados)
    {
        var orientacao = new OrientacaoMentor(mentorId, dados.AlunoId, dados.Texto);
        using var banco = await conexao.Abrir();
        await Regras.AcessoAlunoMentor(banco, mentorId, dados.AlunoId);
        await ConexaoBanco.Gravar(banco, "INSERT INTO OrientacoesMentor(MentorId,AlunoId,Texto) VALUES(@mentorId,@alunoId,@texto)", new { mentorId, alunoId = orientacao.AlunoId, texto = orientacao.Texto });
    }

    public async Task<object> OrientacoesAluno(int alunoId)
    {
        using var banco = await conexao.Abrir();
        return await ConexaoBanco.Consultar(banco, "SELECT o.Id AS id,o.Texto AS texto,o.DataOrientacao AS data,u.Nome AS mentor FROM OrientacoesMentor o JOIN Usuarios u ON u.Id=o.MentorId WHERE o.AlunoId=@alunoId ORDER BY o.Id DESC", new { alunoId });
    }

    public async Task<object> Certificados(Usuario usuario)
    {
        using var banco = await conexao.Abrir();
        if (usuario.Perfil == "Gestor")
            return await ConexaoBanco.Consultar(banco, "SELECT s.Id AS id,s.CursoId AS cursoId,s.Status AS status,s.Motivo AS motivo,s.Codigo AS codigo,s.DataSolicitacao AS data,u.Nome AS aluno,c.Nome AS curso,g.Nome AS gestor FROM SolicitacoesCertificado s JOIN Usuarios u ON u.Id=s.AlunoId JOIN Cursos c ON c.Id=s.CursoId LEFT JOIN Usuarios g ON g.Id=s.GestorId ORDER BY CASE WHEN s.Status='Pendente' THEN 0 ELSE 1 END,s.Id DESC");
        var lista = await ConexaoBanco.Consultar(banco, "SELECT c.Id AS cursoId,c.Nome AS curso,s.Id AS id,s.Status AS status,s.Motivo AS motivo,s.Codigo AS codigo,s.DataAprovacao AS data FROM Cursos c LEFT JOIN SolicitacoesCertificado s ON s.CursoId=c.Id AND s.AlunoId=@id WHERE EXISTS(SELECT 1 FROM Matriculas ma WHERE ma.CursoId=c.Id AND ma.AlunoId=@id AND ma.Ativo=1) OR s.Status='Aprovado' ORDER BY c.Nome", new { id = usuario.Id });
        foreach (var item in lista)
            item["podeSolicitar"] = await CursosServico.CursoAprovado(banco, usuario.Id, (int)item["cursoId"]!);
        return lista;
    }

    public async Task SolicitarCertificado(Usuario usuario, int cursoId)
    {
        using var banco = await conexao.Abrir();
        using var transacao = banco.BeginTransaction(System.Data.IsolationLevel.Serializable);
        Regras.Exigir(await CursosServico.CursoAprovado(banco, usuario.Id, cursoId, transacao), "Conclua e seja aprovado em todos os módulos do curso antes de solicitar o certificado.", 409);
        var atual = await ConexaoBanco.Consultar(banco, "SELECT Status AS status FROM SolicitacoesCertificado WITH(UPDLOCK,HOLDLOCK) WHERE AlunoId=@alunoId AND CursoId=@cursoId", new { alunoId = usuario.Id, cursoId }, transacao);
        Regras.Exigir(atual.Count == 0 || (string)atual[0]["status"]! == "Recusado", "Você já solicitou este certificado.", 409);
        await ConexaoBanco.Gravar(banco, "IF EXISTS(SELECT 1 FROM SolicitacoesCertificado WHERE AlunoId=@alunoId AND CursoId=@cursoId) UPDATE SolicitacoesCertificado SET Status='Pendente',Motivo=NULL,GestorId=NULL,DataSolicitacao=SYSUTCDATETIME() WHERE AlunoId=@alunoId AND CursoId=@cursoId ELSE INSERT INTO SolicitacoesCertificado(AlunoId,CursoId) VALUES(@alunoId,@cursoId)", new { alunoId = usuario.Id, cursoId }, transacao);
        await transacao.CommitAsync();
    }

    public async Task DecidirCertificado(int id, Usuario gestor, DadosCertificado dados)
    {
        string motivo = dados.Aprovar ? "" : Regras.Texto(dados.Motivo, 3, 1000, "Motivo da recusa");
        using var banco = await conexao.Abrir();
        using var transacao = banco.BeginTransaction(System.Data.IsolationLevel.Serializable);
        var lista = await ConexaoBanco.Consultar(banco, "SELECT s.AlunoId AS alunoId,s.CursoId AS cursoId,u.Nome AS aluno,c.Nome AS curso,c.CargaHoraria AS carga FROM SolicitacoesCertificado s WITH(UPDLOCK,HOLDLOCK) JOIN Usuarios u ON u.Id=s.AlunoId JOIN Cursos c ON c.Id=s.CursoId WHERE s.Id=@id AND s.Status='Pendente'", new { id }, transacao);
        Regras.Exigir(lista.Count == 1, "Solicitação não encontrada ou já analisada.", 409);
        var item = lista[0];
        if (dados.Aprovar)
            Regras.Exigir(await CursosServico.CursoAprovado(banco, (int)item["alunoId"]!, (int)item["cursoId"]!, transacao), "O aluno ainda não atende aos requisitos do curso.", 409);
        string? codigo = dados.Aprovar ? "TL-" + id + "-" + Guid.NewGuid().ToString("N")[..12].ToUpperInvariant() : null;
        await ConexaoBanco.Gravar(banco, "UPDATE SolicitacoesCertificado SET Status=@status,GestorId=@gestorId,Motivo=@motivo,Codigo=@codigo,NomeAluno=@aluno,NomeCurso=@curso,CargaHoraria=@carga,DataAprovacao=CASE WHEN @aprovar=1 THEN SYSUTCDATETIME() ELSE NULL END WHERE Id=@id", new { id, gestorId = gestor.Id, status = dados.Aprovar ? "Aprovado" : "Recusado", motivo, codigo, aluno = item["aluno"], curso = item["curso"], carga = item["carga"], aprovar = dados.Aprovar }, transacao);
        await transacao.CommitAsync();
    }

    // A página usa estes dados aprovados para imprimir ou salvar o certificado em PDF.
    public async Task<object> CertificadoEmitido(int id, Usuario usuario)
    {
        using var banco = await conexao.Abrir();
        var lista = await ConexaoBanco.Consultar(banco, "SELECT s.NomeAluno AS aluno,s.NomeCurso AS curso,s.CargaHoraria AS carga,s.Codigo AS codigo,s.DataAprovacao AS data,u.Nome AS gestor FROM SolicitacoesCertificado s JOIN Usuarios u ON u.Id=s.GestorId WHERE s.Id=@id AND s.Status='Aprovado' AND (s.AlunoId=@usuarioId OR @gestor=1)", new { id, usuarioId = usuario.Id, gestor = usuario.Perfil == "Gestor" });
        Regras.Exigir(lista.Count == 1, "Certificado não disponível.", 404);
        return lista[0];
    }

    public async Task<object> Relatorios()
    {
        using var banco = await conexao.Abrir();
        var lista = await ConexaoBanco.Consultar(banco, "SELECT c.Id AS cursoId,c.Nome AS nome,(SELECT COUNT(*) FROM Matriculas ma WHERE ma.CursoId=c.Id AND ma.Ativo=1) AS matriculas,COALESCE(v.media,0) AS media,COALESCE(v.aprovacao,0) AS aprovacao FROM Cursos c OUTER APPLY(SELECT AVG(t.Nota) AS media,AVG(CAST(t.Aprovado AS decimal(5,2)))*100 AS aprovacao FROM TentativasAvaliacao t JOIN Avaliacoes a ON a.Id=t.AvaliacaoId JOIN Modulos m ON m.Id=a.ModuloId WHERE m.CursoId=c.Id) v WHERE c.Excluido=0 ORDER BY c.Nome");
        var totais = await ConexaoBanco.Consultar(banco, "SELECT (SELECT COUNT(*) FROM Cursos WHERE Excluido=0) AS cursos,(SELECT COUNT(*) FROM Matriculas WHERE Ativo=1) AS matriculas,COALESCE(AVG(Nota),0) AS media,COALESCE(AVG(CAST(Aprovado AS decimal(5,2)))*100,0) AS aprovacao FROM TentativasAvaliacao");
        var atendimento = await ConexaoBanco.Consultar(banco, "SELECT u.Id AS id,u.Nome AS nome,u.Perfil AS perfil,COUNT(c.Id) AS assumidos,COUNT(CASE WHEN c.Status='Encerrado' THEN 1 END) AS encerrados FROM Usuarios u LEFT JOIN Chamados c ON c.ResponsavelId=u.Id OR (c.DestinatarioPerfil=u.Perfil AND EXISTS(SELECT 1 FROM MensagensChamado mc WHERE mc.ChamadoId=c.Id AND mc.UsuarioId=u.Id)) WHERE u.Perfil IN('Mentor','Gestor') GROUP BY u.Id,u.Nome,u.Perfil ORDER BY u.Perfil,u.Nome");
        // As mensagens preservam quem atendeu, mesmo se a conta for desativada.
        string historicoMentores = ChamadoSql.Replace("LEFT JOIN Usuarios r ON r.Id=c.ResponsavelId",
            "JOIN Usuarios r ON r.Perfil='Mentor' AND (r.Id=c.ResponsavelId OR EXISTS(SELECT 1 FROM MensagensChamado mc WHERE mc.ChamadoId=c.Id AND mc.UsuarioId=r.Id))");
        var mentores = await ConexaoBanco.Consultar(banco, historicoMentores + " WHERE c.Tipo='AjudaCurso' ORDER BY c.Id DESC");
        return new { quantidadeCursos = totais[0]["cursos"], totalMatriculas = totais[0]["matriculas"], mediaGeral = totais[0]["media"], aprovacao = totais[0]["aprovacao"], cursos = lista, atendimentos = atendimento, chamadosMentores = mentores };
    }

    public async Task<object> AlunosCurso(int cursoId)
    {
        using var banco = await conexao.Abrir();
        var lista = await ConexaoBanco.Consultar(banco, "SELECT Id AS id,Nome AS nome FROM Cursos WHERE Id=@cursoId AND Excluido=0", new { cursoId });
        Regras.Exigir(lista.Count == 1, "Curso não encontrado.", 404);
        lista[0]["alunos"] = await ConexaoBanco.Consultar(banco, "SELECT u.Id AS id,u.Nome AS nome,u.Email AS email,v.valor AS progresso FROM Matriculas ma JOIN Usuarios u ON u.Id=ma.AlunoId CROSS APPLY(SELECT " + CursosServico.ProgressoSql + " AS valor FROM Modulos m LEFT JOIN ProgressoModulos p ON p.ModuloId=m.Id AND p.AlunoId=u.Id WHERE m.CursoId=@cursoId AND m.Ativo=1 AND m.Excluido=0) v WHERE ma.CursoId=@cursoId AND ma.Ativo=1 ORDER BY u.Nome", new { cursoId });
        return lista[0];
    }

    public async Task<object> Desempenho(int cursoId, int alunoId)
    {
        using var banco = await conexao.Abrir();
        var lista = await ConexaoBanco.Consultar(banco, "SELECT u.Id AS id,u.Nome AS nome,c.Nome AS curso,ma.TentativaCurso AS tentativaCurso FROM Usuarios u JOIN Matriculas ma ON ma.AlunoId=u.Id JOIN Cursos c ON c.Id=ma.CursoId WHERE u.Id=@alunoId AND ma.CursoId=@cursoId AND ma.Ativo=1 AND c.Excluido=0", new { cursoId, alunoId });
        Regras.Exigir(lista.Count == 1, "Aluno não matriculado neste curso.", 404);
        var aluno = lista[0];
        aluno["modulos"] = await ConexaoBanco.Consultar(banco, "SELECT m.Id AS id,m.Titulo AS titulo,m.Ordem AS ordem,m.Ativo AS ativo FROM Modulos m WHERE m.CursoId=@cursoId AND m.Excluido=0 ORDER BY m.Ordem", new { cursoId });
        foreach (var modulo in (List<Dictionary<string, object?>>)aluno["modulos"]!)
            modulo["aprovado"] = await CursosServico.ModuloAprovado(banco, alunoId, (int)modulo["id"]!);
        aluno["avaliacoes"] = await ConexaoBanco.Consultar(banco, "SELECT a.Id AS id,a.Titulo AS titulo,m.Titulo AS modulo,a.Ativo AS ativo,t.Id AS resultadoId,t.Nota AS nota,t.Percentual AS percentual,t.Aprovado AS aprovado,t.Acertos AS acertos,t.TotalQuestoes AS total,t.DataTentativa AS data,(SELECT COUNT(*) FROM TentativasAvaliacao h WHERE h.AlunoId=@alunoId AND h.AvaliacaoId=a.Id) AS tentativas FROM Avaliacoes a JOIN Modulos m ON m.Id=a.ModuloId OUTER APPLY(SELECT TOP(1) h.* FROM TentativasAvaliacao h WHERE h.AlunoId=@alunoId AND h.AvaliacaoId=a.Id AND h.TentativaCurso=@tentativaCurso ORDER BY h.Id DESC) t WHERE m.CursoId=@cursoId AND m.Excluido=0 AND a.Excluido=0 ORDER BY m.Ordem,a.Id", new { cursoId, alunoId, tentativaCurso = aluno["tentativaCurso"] });
        aluno["historico"] = await ConexaoBanco.Consultar(banco, "SELECT t.Id AS id,t.TentativaCurso AS tentativaCurso,a.Titulo AS titulo,m.Titulo AS modulo,t.Nota AS nota,t.Percentual AS percentual,t.Aprovado AS aprovado,t.Acertos AS acertos,t.TotalQuestoes AS total,t.DataTentativa AS data FROM TentativasAvaliacao t JOIN Avaliacoes a ON a.Id=t.AvaliacaoId JOIN Modulos m ON m.Id=a.ModuloId WHERE m.CursoId=@cursoId AND t.AlunoId=@alunoId ORDER BY t.Id DESC", new { cursoId, alunoId });
        aluno["aprovadoCurso"] = await CursosServico.CursoAprovado(banco, alunoId, cursoId);
        return aluno;
    }
}
