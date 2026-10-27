using Microsoft.Data.SqlClient;
using TechLearnAcademy.Api.Banco;
using TechLearnAcademy.Api.Modelos;

namespace TechLearnAcademy.Api.Servicos;

public class CursosServico(ConexaoBanco conexao)
{
    public const string ProgressoSql = "CAST(CASE WHEN COUNT(*)=0 THEN 0 ELSE 100.0*SUM(CASE WHEN p.Id IS NOT NULL THEN 1 ELSE 0 END)/COUNT(*) END AS decimal(5,2))";

    public async Task<object> Listar(Usuario usuario, string busca, int pagina, bool todos)
    {
        Regras.Exigir(pagina >= 1 && pagina <= 1000000, "Informe uma página válida.");
        using var banco = await conexao.Abrir();
        var dados = new { busca = "%" + (busca ?? "").Trim() + "%", todos = todos && usuario.Perfil == "Gestor", inicio = (pagina - 1) * 12 };
        string filtro = " WHERE Excluido=0 AND (Ativo=1 OR @todos=1) AND (Nome LIKE @busca OR Descricao LIKE @busca)";
        int total = await ConexaoBanco.Numero(banco, "SELECT COUNT(*) FROM Cursos" + filtro, dados);
        var itens = await ConexaoBanco.Consultar(banco, "SELECT Id AS id,Nome AS nome,Descricao AS descricao,CargaHoraria AS cargaHoraria,Ativo AS ativo FROM Cursos" + filtro + " ORDER BY Nome,Id OFFSET @inicio ROWS FETCH NEXT 12 ROWS ONLY", dados);
        return new { itens, pagina, totalPaginas = Math.Max(1, (total + 11) / 12), total };
    }

    public async Task<object> Detalhes(int id, Usuario usuario)
    {
        using var banco = await conexao.Abrir();
        var lista = await ConexaoBanco.Consultar(banco, "SELECT Id AS id,Nome AS nome,Descricao AS descricao,CargaHoraria AS cargaHoraria,Ativo AS ativo FROM Cursos WHERE Id=@id AND Excluido=0 AND (Ativo=1 OR @gestor=1)", new { id, gestor = usuario.Perfil == "Gestor" });
        Regras.Exigir(lista.Count == 1, "Curso não encontrado.", 404);
        var curso = lista[0];
        curso["matriculado"] = await ConexaoBanco.Numero(banco, "SELECT COUNT(*) FROM Matriculas WHERE CursoId=@id AND AlunoId=@alunoId AND Ativo=1", new { id, alunoId = usuario.Id }) > 0;
        curso["reprovado"] = usuario.Perfil == "Aluno" && (bool)curso["matriculado"]! && await CursoReprovado(banco, usuario.Id, id);
        curso["modulos"] = await ConexaoBanco.Consultar(banco, "SELECT m.Id AS id,m.Titulo AS titulo,m.Ordem AS ordem,m.Ativo AS ativo,CAST(CASE WHEN p.Id IS NULL THEN 0 ELSE 1 END AS bit) AS concluido FROM Modulos m LEFT JOIN ProgressoModulos p ON p.ModuloId=m.Id AND p.AlunoId=@alunoId WHERE m.CursoId=@id AND m.Excluido=0 AND (m.Ativo=1 OR @gestor=1) ORDER BY m.Ordem,m.Id", new { id, alunoId = usuario.Id, gestor = usuario.Perfil == "Gestor" });
        return curso;
    }

    public async Task<int> CriarCurso(DadosCurso dados)
    {
        var curso = new Curso(dados.Nome, dados.Descricao, dados.CargaHoraria, dados.Ativo);
        using var banco = await conexao.Abrir();
        return await ConexaoBanco.Numero(banco, "INSERT INTO Cursos(Nome,Descricao,CargaHoraria) VALUES(@nome,@descricao,@carga); SELECT CAST(SCOPE_IDENTITY() AS int)", new { nome = curso.Nome, descricao = curso.Descricao, carga = curso.CargaHoraria });
    }

    public async Task AlterarCurso(int id, DadosCurso dados)
    {
        var curso = new Curso(dados.Nome, dados.Descricao, dados.CargaHoraria, dados.Ativo);
        using var banco = await conexao.Abrir();
        int alterados = await ConexaoBanco.Gravar(banco, "UPDATE Cursos SET Nome=@nome,Descricao=@descricao,CargaHoraria=@carga,Ativo=@ativo WHERE Id=@id AND Excluido=0", new { id, nome = curso.Nome, descricao = curso.Descricao, carga = curso.CargaHoraria, ativo = curso.Ativo });
        Regras.Exigir(alterados == 1, "Curso não encontrado.", 404);
    }

    public async Task<int> CriarModulo(DadosModulo dados)
    {
        var modulo = Modulo.ParaCadastro(dados.CursoId, dados.Titulo, dados.Ordem);
        using var banco = await conexao.Abrir();
        using var transacao = banco.BeginTransaction(System.Data.IsolationLevel.Serializable);
        int existe = await ConexaoBanco.Numero(banco, "SELECT COUNT(*) FROM Cursos WITH(UPDLOCK,HOLDLOCK) WHERE Id=@id AND Excluido=0", new { id = dados.CursoId }, transacao);
        Regras.Exigir(existe == 1, "Curso não encontrado.", 404);
        int quantidade = await ConexaoBanco.Numero(banco, "SELECT COUNT(*) FROM Modulos WHERE CursoId=@id AND Excluido=0", new { id = dados.CursoId }, transacao);
        Regras.Exigir(quantidade < 4, "O curso pode ter no máximo quatro módulos. Exclua um módulo para adicionar outro.", 409);
        int id = await ConexaoBanco.Numero(banco, "INSERT INTO Modulos(CursoId,Titulo,Ordem) VALUES(@cursoId,@titulo,@ordem); SELECT CAST(SCOPE_IDENTITY() AS int)", new { cursoId = dados.CursoId, titulo = modulo.Titulo, ordem = modulo.Ordem }, transacao);
        await transacao.CommitAsync();
        return id;
    }

    public async Task AlterarModulo(int id, DadosModulo dados)
    {
        var modulo = new Modulo(dados.Titulo, dados.Ordem, dados.Ativo);
        using var banco = await conexao.Abrir();
        int alterados = await ConexaoBanco.Gravar(banco, "UPDATE Modulos SET Titulo=@titulo,Ordem=@ordem,Ativo=@ativo WHERE Id=@id AND Excluido=0", new { id, titulo = modulo.Titulo, ordem = modulo.Ordem, ativo = modulo.Ativo });
        Regras.Exigir(alterados == 1, "Módulo não encontrado.", 404);
    }

    public async Task<object> Materiais(int moduloId, Usuario usuario)
    {
        using var banco = await conexao.Abrir();
        await Regras.AcessoModulo(banco, moduloId, usuario);
        return await ConexaoBanco.Consultar(banco, "SELECT Id AS id,ModuloId AS moduloId,Titulo AS titulo,Tipo AS tipo,Url AS url,Ativo AS ativo,ArquivoNome AS arquivoNome,CAST(CASE WHEN ArquivoPdf IS NULL THEN 0 ELSE 1 END AS bit) AS arquivo FROM Materiais WHERE ModuloId=@moduloId AND Excluido=0 AND (Ativo=1 OR @gestor=1) ORDER BY Id", new { moduloId, gestor = usuario.Perfil == "Gestor" });
    }

    public async Task<int> CriarMaterial(DadosMaterial dados)
    {
        var material = new Material(dados.Titulo, dados.Url, dados.Tipo, dados.Ativo);
        using var banco = await conexao.Abrir();
        await Regras.AcessoModulo(banco, dados.ModuloId, new Usuario(perfil: "Gestor"));
        return await ConexaoBanco.Numero(banco, "INSERT INTO Materiais(ModuloId,Titulo,Tipo,Url) VALUES(@moduloId,@titulo,@tipo,@url); SELECT CAST(SCOPE_IDENTITY() AS int)", new { moduloId = dados.ModuloId, titulo = material.Titulo, tipo = material.Tipo, url = material.Url });
    }

    public async Task AlterarMaterial(int id, DadosMaterial dados)
    {
        using var banco = await conexao.Abrir();
        bool manter = string.IsNullOrWhiteSpace(dados.Url) && await ConexaoBanco.Numero(banco, "SELECT COUNT(*) FROM Materiais WHERE Id=@id AND ArquivoPdf IS NOT NULL AND Excluido=0", new { id }) == 1;
        var material = new Material(dados.Titulo, dados.Url, dados.Tipo, dados.Ativo, manter);
        int alterados = await ConexaoBanco.Gravar(banco, "UPDATE Materiais SET Titulo=@titulo,Tipo=@tipo,Url=@url,Ativo=@ativo,ArquivoPdf=CASE WHEN @manter=1 THEN ArquivoPdf ELSE NULL END,ArquivoNome=CASE WHEN @manter=1 THEN ArquivoNome ELSE NULL END WHERE Id=@id AND Excluido=0", new { id, titulo = material.Titulo, tipo = material.Tipo, url = material.Url, ativo = material.Ativo, manter });
        Regras.Exigir(alterados == 1, "Material não encontrado.", 404);
    }

    // O documento fica no banco. O acesso ao módulo é conferido no download.
    public async Task<int> SalvarPdf(int id, int moduloId, string titulo, bool ativo, IFormFile arquivo)
    {
        Regras.Exigir(arquivo != null && arquivo.Length > 0 && arquivo.Length <= 10 * 1024 * 1024, "Selecione um PDF de até 10 MB.");
        string nome = Path.GetFileName(arquivo!.FileName);
        Regras.Exigir(nome.Length <= 255 && Path.GetExtension(nome).Equals(".pdf", StringComparison.OrdinalIgnoreCase), "Selecione um documento PDF.");
        using var memoria = new MemoryStream();
        await arquivo.CopyToAsync(memoria);
        byte[] bytes = memoria.ToArray();
        Regras.Exigir(bytes.Length >= 5 && System.Text.Encoding.ASCII.GetString(bytes, 0, 5) == "%PDF-", "O arquivo enviado não é um PDF válido.");
        var material = new Material(titulo, "", "PDF", ativo, true);
        using var banco = await conexao.Abrir();
        if (id == 0)
        {
            await Regras.AcessoModulo(banco, moduloId, new Usuario(perfil: "Gestor"));
            return await ConexaoBanco.Numero(banco, "INSERT INTO Materiais(ModuloId,Titulo,Tipo,Url,ArquivoNome,ArquivoPdf) VALUES(@moduloId,@titulo,'PDF','',@nome,@bytes); SELECT CAST(SCOPE_IDENTITY() AS int)", new { moduloId, titulo = material.Titulo, nome, bytes });
        }
        int alterados = await ConexaoBanco.Gravar(banco, "UPDATE Materiais SET Titulo=@titulo,Tipo='PDF',Url='',ArquivoNome=@nome,ArquivoPdf=@bytes,Ativo=@ativo WHERE Id=@id AND Excluido=0", new { id, titulo = material.Titulo, nome, bytes, ativo });
        Regras.Exigir(alterados == 1, "Material não encontrado.", 404);
        return id;
    }

    public async Task<(byte[] Bytes, string Nome)> Arquivo(int id, Usuario usuario)
    {
        using var banco = await conexao.Abrir();
        var lista = await ConexaoBanco.Consultar(banco, "SELECT ModuloId AS moduloId,ArquivoPdf AS bytes,ArquivoNome AS nome FROM Materiais WHERE Id=@id AND Excluido=0 AND ArquivoPdf IS NOT NULL AND (Ativo=1 OR @gestor=1)", new { id, gestor = usuario.Perfil == "Gestor" });
        Regras.Exigir(lista.Count == 1, "Documento não encontrado.", 404);
        await Regras.AcessoModulo(banco, (int)lista[0]["moduloId"]!, usuario);
        return ((byte[])lista[0]["bytes"]!, (string)lista[0]["nome"]!);
    }

    // Excluir esconde o conteúdo, preservando notas e certificados emitidos.
    public async Task Excluir(string tipo, int id)
    {
        using var banco = await conexao.Abrir();
        using var transacao = banco.BeginTransaction(System.Data.IsolationLevel.Serializable);
        string tabela = tipo switch { "curso" => "Cursos", "modulo" => "Modulos", "material" => "Materiais", _ => throw new ErroAplicacao(400, "Conteúdo inválido.") };
        int alterados = await ConexaoBanco.Gravar(banco, "UPDATE " + tabela + " SET Excluido=1,Ativo=0 WHERE Id=@id AND Excluido=0", new { id }, transacao);
        Regras.Exigir(alterados == 1, "Conteúdo não encontrado.", 404);
        if (tipo == "curso")
            await ConexaoBanco.Gravar(banco, "UPDATE Modulos SET Excluido=1,Ativo=0 WHERE CursoId=@id; UPDATE Materiais SET Excluido=1,Ativo=0 WHERE ModuloId IN(SELECT Id FROM Modulos WHERE CursoId=@id); UPDATE Avaliacoes SET Excluido=1,Ativo=0 WHERE ModuloId IN(SELECT Id FROM Modulos WHERE CursoId=@id)", new { id }, transacao);
        if (tipo == "modulo")
            await ConexaoBanco.Gravar(banco, "UPDATE Materiais SET Excluido=1,Ativo=0 WHERE ModuloId=@id; UPDATE Avaliacoes SET Excluido=1,Ativo=0 WHERE ModuloId=@id", new { id }, transacao);
        await transacao.CommitAsync();
    }

    // A matrícula guarda em qual tentativa do curso o aluno está.
    public static async Task<int> TentativaAtual(SqlConnection banco, int alunoId, int cursoId, SqlTransaction? transacao = null)
    {
        string bloqueio = transacao == null ? "" : " WITH(UPDLOCK,HOLDLOCK)";
        int tentativa = await ConexaoBanco.Numero(banco, "SELECT TentativaCurso FROM Matriculas" + bloqueio + " WHERE AlunoId=@alunoId AND CursoId=@cursoId AND Ativo=1", new { alunoId, cursoId }, transacao);
        Regras.Exigir(tentativa > 0, "Matrícula não encontrada.", 403);
        return tentativa;
    }

    public static async Task<bool> CursoReprovado(SqlConnection banco, int alunoId, int cursoId, SqlTransaction? transacao = null)
    {
        int reprovacoes = await ConexaoBanco.Numero(banco,
            "SELECT COUNT(*) FROM Avaliacoes a JOIN Modulos m ON m.Id=a.ModuloId JOIN Matriculas ma ON ma.CursoId=m.CursoId AND ma.AlunoId=@alunoId AND ma.Ativo=1 CROSS APPLY(SELECT TOP(1) t.Aprovado FROM TentativasAvaliacao t WHERE t.AlunoId=@alunoId AND t.AvaliacaoId=a.Id AND t.TentativaCurso=ma.TentativaCurso ORDER BY t.Id DESC) ultima WHERE m.CursoId=@cursoId AND m.Ativo=1 AND m.Excluido=0 AND a.Ativo=1 AND a.Excluido=0 AND ultima.Aprovado=0",
            new { alunoId, cursoId }, transacao);
        return reprovacoes > 0;
    }

    public async Task Reiniciar(Usuario usuario, int cursoId)
    {
        Regras.Aluno(usuario);
        using var banco = await conexao.Abrir();
        using var transacao = banco.BeginTransaction(System.Data.IsolationLevel.Serializable);
        int tentativaAtual = await TentativaAtual(banco, usuario.Id, cursoId, transacao);
        await Regras.AcessoCurso(banco, cursoId, usuario, transacao);
        var matricula = new Matricula(usuario.Id, cursoId, tentativaCurso: tentativaAtual);
        matricula.Reiniciar(await CursoReprovado(banco, usuario.Id, cursoId, transacao));
        await ConexaoBanco.Gravar(banco, "UPDATE Matriculas SET TentativaCurso=@tentativa WHERE AlunoId=@alunoId AND CursoId=@cursoId AND Ativo=1; DELETE p FROM ProgressoModulos p JOIN Modulos m ON m.Id=p.ModuloId WHERE p.AlunoId=@alunoId AND m.CursoId=@cursoId; UPDATE SolicitacoesCertificado SET Status='Recusado',Motivo=N'Curso reiniciado após reprovação. Conclua novamente os módulos antes de solicitar o certificado.' WHERE AlunoId=@alunoId AND CursoId=@cursoId AND Status='Pendente'", new { alunoId = usuario.Id, cursoId, tentativa = matricula.TentativaCurso }, transacao);
        await transacao.CommitAsync();
    }

    // Apenas os resultados da tentativa atual contam para concluir o módulo.
    public static async Task<bool> ModuloAprovado(SqlConnection banco, int alunoId, int moduloId, SqlTransaction? transacao = null)
    {
        var dados = new { alunoId, moduloId };
        int total = await ConexaoBanco.Numero(banco, "SELECT COUNT(*) FROM Avaliacoes WHERE ModuloId=@moduloId AND Ativo=1 AND Excluido=0", dados, transacao);
        int pendentes = await ConexaoBanco.Numero(banco, "SELECT COUNT(*) FROM Avaliacoes a OUTER APPLY(SELECT TOP(1) t.Id,t.Aprovado,t.TotalQuestoes FROM TentativasAvaliacao t JOIN Modulos m ON m.Id=a.ModuloId JOIN Matriculas ma ON ma.CursoId=m.CursoId AND ma.AlunoId=t.AlunoId AND ma.Ativo=1 WHERE t.AlunoId=@alunoId AND t.AvaliacaoId=a.Id AND t.TentativaCurso=ma.TentativaCurso ORDER BY t.Id DESC) ultima WHERE a.ModuloId=@moduloId AND a.Ativo=1 AND a.Excluido=0 AND (COALESCE(ultima.Aprovado,0)=0 OR COALESCE(ultima.TotalQuestoes,0)<>(SELECT COUNT(*) FROM Questoes q WHERE q.AvaliacaoId=a.Id AND q.Ativo=1) OR EXISTS(SELECT 1 FROM Questoes q WHERE q.AvaliacaoId=a.Id AND q.Ativo=1 AND NOT EXISTS(SELECT 1 FROM RespostasAluno r WHERE r.TentativaId=ultima.Id AND r.QuestaoId=q.Id)))", dados, transacao);
        return total > 0 && pendentes == 0;
    }

    // Quando o gestor altera uma avaliação, a conclusão acompanha as provas atuais.
    public static async Task RecalcularProgressoModulo(SqlConnection banco, int moduloId, SqlTransaction transacao)
    {
        var alunos = await ConexaoBanco.Consultar(banco, "SELECT ma.AlunoId AS id FROM Matriculas ma JOIN Modulos m ON m.CursoId=ma.CursoId WHERE m.Id=@moduloId AND ma.Ativo=1", new { moduloId }, transacao);
        foreach (var aluno in alunos)
        {
            int alunoId = (int)aluno["id"]!;
            if (await ModuloAprovado(banco, alunoId, moduloId, transacao))
                await ConexaoBanco.Gravar(banco, "IF NOT EXISTS(SELECT 1 FROM ProgressoModulos WHERE AlunoId=@alunoId AND ModuloId=@moduloId) INSERT INTO ProgressoModulos(AlunoId,ModuloId) VALUES(@alunoId,@moduloId)", new { alunoId, moduloId }, transacao);
            else
                await ConexaoBanco.Gravar(banco, "DELETE FROM ProgressoModulos WHERE AlunoId=@alunoId AND ModuloId=@moduloId", new { alunoId, moduloId }, transacao);
        }
    }

    public static async Task<bool> CursoAprovado(SqlConnection banco, int alunoId, int cursoId, SqlTransaction? transacao = null)
    {
        int matriculado = await ConexaoBanco.Numero(banco, "SELECT COUNT(*) FROM Matriculas ma JOIN Cursos c ON c.Id=ma.CursoId WHERE ma.AlunoId=@alunoId AND ma.CursoId=@cursoId AND ma.Ativo=1 AND c.Ativo=1 AND c.Excluido=0", new { alunoId, cursoId }, transacao);
        var modulos = await ConexaoBanco.Consultar(banco, "SELECT Id AS id FROM Modulos WHERE CursoId=@cursoId AND Ativo=1 AND Excluido=0", new { cursoId }, transacao);
        if (matriculado == 0 || modulos.Count == 0 || modulos.Count > 4) return false;
        foreach (var modulo in modulos)
            if (!await ModuloAprovado(banco, alunoId, (int)modulo["id"]!, transacao)) return false;
        return true;
    }

    public async Task Matricular(int alunoId, int cursoId)
    {
        var matricula = new Matricula(alunoId, cursoId);
        using var banco = await conexao.Abrir();
        await ConexaoBanco.Numero(banco, "EXEC dbo.sp_RealizarMatricula @AlunoId=@alunoId,@CursoId=@cursoId", new { alunoId = matricula.AlunoId, cursoId = matricula.CursoId });
    }

    public async Task<List<Dictionary<string, object?>>> Minhas(int alunoId)
    {
        using var banco = await conexao.Abrir();
        var lista = await ConexaoBanco.Consultar(banco, "SELECT ma.Id AS id,c.Id AS cursoId,c.Nome AS nome,c.Descricao AS descricao,c.CargaHoraria AS cargaHoraria,ma.TentativaCurso AS tentativaCurso,v.valor AS progresso FROM Matriculas ma JOIN Cursos c ON c.Id=ma.CursoId CROSS APPLY(SELECT " + ProgressoSql + " AS valor FROM Modulos m LEFT JOIN ProgressoModulos p ON p.ModuloId=m.Id AND p.AlunoId=@alunoId WHERE m.CursoId=c.Id AND m.Ativo=1 AND m.Excluido=0) v WHERE ma.AlunoId=@alunoId AND ma.Ativo=1 AND c.Excluido=0 AND c.Ativo=1 ORDER BY c.Nome", new { alunoId });
        foreach (var curso in lista) curso["reprovado"] = await CursoReprovado(banco, alunoId, (int)curso["cursoId"]!);
        return lista;
    }

    public async Task Concluir(Usuario usuario, int moduloId)
    {
        using var banco = await conexao.Abrir();
        using var transacao = banco.BeginTransaction(System.Data.IsolationLevel.Serializable);
        int cursoId = await ConexaoBanco.Numero(banco, "SELECT CursoId FROM Modulos WHERE Id=@moduloId AND Excluido=0", new { moduloId }, transacao);
        await TentativaAtual(banco, usuario.Id, cursoId, transacao);
        await Regras.AcessoModulo(banco, moduloId, usuario, transacao);
        Regras.Exigir(!await CursoReprovado(banco, usuario.Id, cursoId, transacao), "Você reprovou em uma avaliação. Reinicie o curso para estudar novamente e fazer novas avaliações.", 409);
        Regras.Exigir(await ModuloAprovado(banco, usuario.Id, moduloId, transacao), "Alcance nota 6 em todas as avaliações deste módulo.", 409);
        await ConexaoBanco.Gravar(banco, "IF NOT EXISTS(SELECT 1 FROM ProgressoModulos WITH(UPDLOCK,HOLDLOCK) WHERE AlunoId=@alunoId AND ModuloId=@moduloId) INSERT INTO ProgressoModulos(AlunoId,ModuloId) VALUES(@alunoId,@moduloId)", new { alunoId = usuario.Id, moduloId }, transacao);
        await transacao.CommitAsync();
    }

    public async Task<object> Painel(Usuario usuario)
    {
        var lista = await Minhas(usuario.Id);
        var matriculas = lista.Select(c => new Matricula(usuario.Id, (int)c["cursoId"]!, Convert.ToDecimal(c["progresso"]))).ToList();
        using var banco = await conexao.Abrir();
        var avaliacoesRecentes = await ConexaoBanco.Consultar(banco, "SELECT TOP(5) t.Id AS id,a.Titulo AS titulo,t.Nota AS nota,t.Aprovado AS aprovado FROM TentativasAvaliacao t JOIN Avaliacoes a ON a.Id=t.AvaliacaoId WHERE t.AlunoId=@alunoId ORDER BY t.DataTentativa DESC,t.Id DESC", new { alunoId = usuario.Id });
        return new { nome = usuario.Nome, cursosMatriculados = lista.Count, progressoGeral = matriculas.Count == 0 ? 0 : decimal.Round(matriculas.Average(m => m.Progresso), 2), avaliacoesRecentes };
    }
}
