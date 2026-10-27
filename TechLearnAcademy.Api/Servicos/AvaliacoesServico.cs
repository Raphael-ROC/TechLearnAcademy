using Microsoft.Data.SqlClient;
using TechLearnAcademy.Api.Banco;
using TechLearnAcademy.Api.Modelos;

namespace TechLearnAcademy.Api.Servicos;

public class AvaliacoesServico(ConexaoBanco conexao)
{
    public async Task<object> Listar(int moduloId, Usuario usuario)
    {
        using var banco = await conexao.Abrir();
        int cursoId = await Regras.AcessoModulo(banco, moduloId, usuario);
        bool reprovado = usuario.Perfil == "Aluno" && await CursosServico.CursoReprovado(banco, usuario.Id, cursoId);
        var lista = await ConexaoBanco.Consultar(banco, "SELECT a.Id AS id,a.ModuloId AS moduloId,a.Titulo AS titulo,a.Ativo AS ativo,t.Id AS resultadoId,t.Nota AS nota,t.Aprovado AS aprovado FROM Avaliacoes a JOIN Modulos m ON m.Id=a.ModuloId LEFT JOIN Matriculas ma ON ma.CursoId=m.CursoId AND ma.AlunoId=@alunoId AND ma.Ativo=1 OUTER APPLY(SELECT TOP(1) h.Id,h.Nota,h.Aprovado FROM TentativasAvaliacao h WHERE h.AvaliacaoId=a.Id AND h.AlunoId=@alunoId AND h.TentativaCurso=ma.TentativaCurso ORDER BY h.Id DESC) t WHERE a.ModuloId=@moduloId AND a.Excluido=0 AND (a.Ativo=1 OR @gestor=1) ORDER BY a.Id", new { moduloId, alunoId = usuario.Id, gestor = usuario.Perfil == "Gestor" });
        foreach (var avaliacao in lista) avaliacao["podeResponder"] = usuario.Perfil == "Aluno" && !reprovado && avaliacao["resultadoId"] == null;
        return lista;
    }

    public async Task<int> Criar(DadosAvaliacao dados)
    {
        var avaliacao = new Modelos.Avaliacao(dados.ModuloId, dados.Titulo, dados.Ativo);
        using var banco = await conexao.Abrir();
        await Regras.AcessoModulo(banco, dados.ModuloId, new Usuario(perfil: "Gestor"));
        using var transacao = banco.BeginTransaction();
        int id = await ConexaoBanco.Numero(banco, "INSERT INTO Avaliacoes(ModuloId,Titulo) VALUES(@moduloId,@titulo); SELECT CAST(SCOPE_IDENTITY() AS int)", new { moduloId = avaliacao.ModuloId, titulo = avaliacao.Titulo }, transacao);
        await ConexaoBanco.Gravar(banco, "DELETE FROM ProgressoModulos WHERE ModuloId=@moduloId", new { moduloId = avaliacao.ModuloId }, transacao);
        await transacao.CommitAsync();
        return id;
    }

    public async Task Alterar(int id, DadosAvaliacao dados)
    {
        var avaliacao = new Modelos.Avaliacao(dados.ModuloId, dados.Titulo, dados.Ativo);
        using var banco = await conexao.Abrir();
        using var transacao = banco.BeginTransaction(System.Data.IsolationLevel.Serializable);
        int moduloId = await ConexaoBanco.Numero(banco, "SELECT ModuloId FROM Avaliacoes WITH(UPDLOCK,HOLDLOCK) WHERE Id=@id AND Excluido=0", new { id }, transacao);
        Regras.Exigir(moduloId > 0, "Avaliação não encontrada.", 404);
        await ConexaoBanco.Gravar(banco, "UPDATE Avaliacoes SET Titulo=@titulo,Ativo=@ativo WHERE Id=@id", new { id, titulo = avaliacao.Titulo, ativo = avaliacao.Ativo }, transacao);
        await CursosServico.RecalcularProgressoModulo(banco, moduloId, transacao);
        await transacao.CommitAsync();
    }

    private static async Task<Dictionary<string, object?>> Avaliacao(SqlConnection banco, int id, Usuario usuario, SqlTransaction? transacao = null)
    {
        var lista = await ConexaoBanco.Consultar(banco, "SELECT a.Id AS id,a.ModuloId AS moduloId,m.CursoId AS cursoId,a.Titulo AS titulo,a.Ativo AS ativo FROM Avaliacoes a JOIN Modulos m ON m.Id=a.ModuloId WHERE a.Id=@id AND a.Excluido=0", new { id }, transacao);
        Regras.Exigir(lista.Count == 1, "Avaliação não encontrada.", 404);
        Regras.Exigir(usuario.Perfil == "Gestor" || (bool)lista[0]["ativo"]!, "Esta avaliação está inativa.", 403);
        await Regras.AcessoModulo(banco, (int)lista[0]["moduloId"]!, usuario, transacao);
        return lista[0];
    }

    public async Task<object> Detalhes(int id, Usuario usuario)
    {
        using var banco = await conexao.Abrir();
        var avaliacao = await Avaliacao(banco, id, usuario);
        if (usuario.Perfil == "Aluno")
        {
            int cursoId = (int)avaliacao["cursoId"]!;
            int tentativa = await CursosServico.TentativaAtual(banco, usuario.Id, cursoId);
            int resultadoId = await ConexaoBanco.Numero(banco, "SELECT TOP(1) Id FROM TentativasAvaliacao WHERE AlunoId=@alunoId AND AvaliacaoId=@id AND TentativaCurso=@tentativa ORDER BY Id DESC", new { alunoId = usuario.Id, id, tentativa });
            bool reprovado = await CursosServico.CursoReprovado(banco, usuario.Id, cursoId);
            avaliacao["tentativaCurso"] = tentativa;
            avaliacao["resultadoId"] = resultadoId;
            avaliacao["podeResponder"] = resultadoId == 0 && !reprovado;
            avaliacao["motivoBloqueio"] = reprovado ? "Você reprovou em uma avaliação. Reinicie o curso para fazer novas avaliações." : "Esta avaliação já foi respondida. Você pode consultar seu resultado.";
            if (!(bool)avaliacao["podeResponder"]!)
            {
                avaliacao["questoes"] = new List<object>();
                return avaliacao;
            }
        }
        var questoes = await ConexaoBanco.Consultar(banco, "SELECT Id AS id,Enunciado AS enunciado,Ativo AS ativo FROM Questoes WHERE AvaliacaoId=@id AND (Ativo=1 OR @gestor=1) ORDER BY Id", new { id, gestor = usuario.Perfil == "Gestor" });
        foreach (var questao in questoes)
        {
            string sql = usuario.Perfil == "Gestor"
                ? "SELECT Id AS id,Texto AS texto,Correta AS correta FROM Alternativas WHERE QuestaoId=@questaoId ORDER BY Id"
                : "SELECT Id AS id,Texto AS texto FROM Alternativas WHERE QuestaoId=@questaoId ORDER BY Id";
            questao["alternativas"] = await ConexaoBanco.Consultar(banco, sql, new { questaoId = questao["id"] });
        }
        // O gabarito nunca é enviado ao aluno antes da correção.
        avaliacao["questoes"] = questoes;
        return avaliacao;
    }

    private static Questao PrepararQuestao(DadosQuestao dados) =>
        new(dados.AvaliacaoId, dados.Enunciado,
            dados.Alternativas?.Select(a => (a.Texto, a.Correta)).ToList(), dados.Ativo);

    private static async Task Alternativas(SqlConnection banco, SqlTransaction transacao, int id, IReadOnlyList<(string Texto, bool Correta)> alternativas)
    {
        foreach (var alternativa in alternativas)
            await ConexaoBanco.Gravar(banco, "INSERT INTO Alternativas(QuestaoId,Texto,Correta) VALUES(@id,@texto,@correta)", new { id, texto = alternativa.Texto.Trim(), correta = alternativa.Correta }, transacao);
    }

    public async Task<int> CriarQuestao(DadosQuestao dados)
    {
        var questao = PrepararQuestao(dados);
        using var banco = await conexao.Abrir();
        using var transacao = banco.BeginTransaction();
        int existe = await ConexaoBanco.Numero(banco, "SELECT COUNT(*) FROM Avaliacoes WITH(UPDLOCK,HOLDLOCK) WHERE Id=@id AND Excluido=0", new { id = dados.AvaliacaoId }, transacao);
        Regras.Exigir(existe == 1, "Avaliação não encontrada.", 404);
        int id = await ConexaoBanco.Numero(banco, "INSERT INTO Questoes(AvaliacaoId,Enunciado) VALUES(@avaliacaoId,@enunciado); SELECT CAST(SCOPE_IDENTITY() AS int)", new { avaliacaoId = dados.AvaliacaoId, enunciado = questao.Enunciado }, transacao);
        await Alternativas(banco, transacao, id, questao.Alternativas);
        await ConexaoBanco.Gravar(banco, "DELETE FROM ProgressoModulos WHERE ModuloId=(SELECT ModuloId FROM Avaliacoes WHERE Id=@avaliacaoId)", new { avaliacaoId = questao.AvaliacaoId }, transacao);
        await transacao.CommitAsync();
        return id;
    }

    public async Task AlterarQuestao(int id, DadosQuestao dados)
    {
        using var banco = await conexao.Abrir();
        int avaliacaoId = await ConexaoBanco.Numero(banco, "SELECT AvaliacaoId FROM Questoes WHERE Id=@id", new { id });
        Regras.Exigir(avaliacaoId > 0, "Questão não encontrada.", 404);
        using var transacao = banco.BeginTransaction();
        await ConexaoBanco.Numero(banco, "SELECT COUNT(*) FROM Avaliacoes WITH(UPDLOCK,HOLDLOCK) WHERE Id=@avaliacaoId", new { avaliacaoId }, transacao);
        int tentativas = await ConexaoBanco.Numero(banco, "SELECT COUNT(*) FROM TentativasAvaliacao WHERE AvaliacaoId=@avaliacaoId", new { avaliacaoId }, transacao);
        if (tentativas > 0)
        {
            // Depois de uma prova, o gabarito fica preservado para o histórico.
            Regras.Exigir(!dados.Ativo, "Esta avaliação já tem tentativas. A questão pode somente ser desativada; crie outra para mudar o conteúdo.", 409);
            await ConexaoBanco.Gravar(banco, "UPDATE Questoes SET Ativo=0 WHERE Id=@id", new { id }, transacao);
        }
        else
        {
            var questao = PrepararQuestao(dados);
            await ConexaoBanco.Gravar(banco, "UPDATE Questoes SET Enunciado=@enunciado,Ativo=@ativo WHERE Id=@id; DELETE FROM Alternativas WHERE QuestaoId=@id", new { id, enunciado = questao.Enunciado, ativo = dados.Ativo }, transacao);
            await Alternativas(banco, transacao, id, questao.Alternativas);
        }
        await ConexaoBanco.Gravar(banco, "DELETE FROM ProgressoModulos WHERE ModuloId=(SELECT ModuloId FROM Avaliacoes WHERE Id=@avaliacaoId)", new { avaliacaoId }, transacao);
        await transacao.CommitAsync();
    }

    public async Task<object> Corrigir(int id, DadosProva dados, Usuario usuario)
    {
        Regras.Exigir(dados.Respostas != null && dados.Respostas.Count > 0, "Responda todas as questões.");
        using var banco = await conexao.Abrir();
        using var transacao = banco.BeginTransaction(System.Data.IsolationLevel.Serializable);
        int cursoId = await ConexaoBanco.Numero(banco, "SELECT m.CursoId FROM Avaliacoes a JOIN Modulos m ON m.Id=a.ModuloId WHERE a.Id=@id AND a.Excluido=0", new { id }, transacao);
        Regras.Exigir(cursoId > 0, "Avaliação não encontrada.", 404);
        // A mesma matrícula fica bloqueada até o fim: duas abas não gravam duas provas.
        int tentativaCurso = await CursosServico.TentativaAtual(banco, usuario.Id, cursoId, transacao);
        // O bloqueio evita mudar a avaliação durante a correção.
        await ConexaoBanco.Numero(banco, "SELECT COUNT(*) FROM Avaliacoes WITH(UPDLOCK,HOLDLOCK) WHERE Id=@id", new { id }, transacao);
        var avaliacao = await Avaliacao(banco, id, usuario, transacao);
        Regras.Exigir(dados.TentativaCurso == tentativaCurso, "O curso foi reiniciado após a abertura desta prova. Volte ao curso e abra a avaliação novamente.", 409);
        int respondidas = await ConexaoBanco.Numero(banco, "SELECT COUNT(*) FROM TentativasAvaliacao WHERE AlunoId=@alunoId AND AvaliacaoId=@id AND TentativaCurso=@tentativaCurso", new { alunoId = usuario.Id, id, tentativaCurso }, transacao);
        Regras.Exigir(respondidas == 0, "Esta avaliação já foi respondida nesta tentativa do curso. Consulte seu resultado.", 409);
        Regras.Exigir(!await CursosServico.CursoReprovado(banco, usuario.Id, cursoId, transacao), "Você reprovou em uma avaliação. Reinicie o curso antes de fazer novas avaliações.", 409);
        var questoes = await ConexaoBanco.Consultar(banco, "SELECT Id AS id FROM Questoes WHERE AvaliacaoId=@id AND Ativo=1 ORDER BY Id", new { id }, transacao);
        Regras.Exigir(questoes.Count > 0, "Esta avaliação ainda não possui questões.", 409);
        Regras.Exigir(dados.Respostas!.Count == questoes.Count && dados.Respostas.Select(r => r.QuestaoId).Distinct().Count() == questoes.Count, "Envie uma resposta para cada questão, sem repetir questões.");
        int acertos = 0;
        var correcoes = new List<(DadosResposta resposta, bool correta)>();
        foreach (var questao in questoes)
        {
            int questaoId = (int)questao["id"]!;
            var resposta = dados.Respostas.Where(r => r.QuestaoId == questaoId).Select(r => (DadosResposta?)r).SingleOrDefault();
            Regras.Exigir(resposta != null, "Responda todas as questões desta avaliação.");
            var alternativas = await ConexaoBanco.Consultar(banco, "SELECT Id AS id,Correta AS correta FROM Alternativas WHERE QuestaoId=@questaoId", new { questaoId }, transacao);
            Regras.Exigir(alternativas.Count >= 2 && alternativas.Count(a => (bool)a["correta"]!) == 1, "Há uma questão incompleta. Avise o Gestor.", 409);
            var marcada = alternativas.SingleOrDefault(a => (int)a["id"]! == resposta!.Value.AlternativaId);
            Regras.Exigir(marcada != null, "Uma alternativa não pertence à questão informada.");
            bool correta = (bool)marcada!["correta"]!;
            if (correta) acertos++;
            correcoes.Add((resposta!.Value, correta));
        }
        var resultado = new Modelos.Resultado(acertos, questoes.Count);
        int total = resultado.Total;
        decimal percentual = resultado.Percentual;
        decimal nota = resultado.Nota;
        bool aprovado = resultado.Aprovado;
        int tentativaId = await ConexaoBanco.Numero(banco, "INSERT INTO TentativasAvaliacao(AlunoId,AvaliacaoId,TentativaCurso,Acertos,TotalQuestoes,Percentual,Nota,Aprovado) VALUES(@alunoId,@id,@tentativaCurso,@acertos,@total,@percentual,@nota,@aprovado); SELECT CAST(SCOPE_IDENTITY() AS int)", new { alunoId = usuario.Id, id, tentativaCurso, acertos, total, percentual, nota, aprovado }, transacao);
        foreach (var correcao in correcoes)
            await ConexaoBanco.Gravar(banco, "INSERT INTO RespostasAluno(TentativaId,AvaliacaoId,QuestaoId,AlternativaId,Correta) VALUES(@tentativaId,@id,@questaoId,@alternativaId,@correta)", new { tentativaId, id, questaoId = correcao.resposta.QuestaoId, alternativaId = correcao.resposta.AlternativaId, correta = correcao.correta }, transacao);
        if (await CursosServico.ModuloAprovado(banco, usuario.Id, (int)avaliacao["moduloId"]!, transacao))
            await ConexaoBanco.Gravar(banco, "IF NOT EXISTS(SELECT 1 FROM ProgressoModulos WITH(UPDLOCK,HOLDLOCK) WHERE AlunoId=@alunoId AND ModuloId=@moduloId) INSERT INTO ProgressoModulos(AlunoId,ModuloId) VALUES(@alunoId,@moduloId)", new { alunoId = usuario.Id, moduloId = avaliacao["moduloId"] }, transacao);
        else
            await ConexaoBanco.Gravar(banco, "DELETE FROM ProgressoModulos WHERE AlunoId=@alunoId AND ModuloId=@moduloId", new { alunoId = usuario.Id, moduloId = avaliacao["moduloId"] }, transacao);
        await transacao.CommitAsync();
        return new { id = tentativaId, acertos, total, percentual, nota, aprovado };
    }

    public async Task Excluir(int id)
    {
        using var banco = await conexao.Abrir();
        using var transacao = banco.BeginTransaction(System.Data.IsolationLevel.Serializable);
        int moduloId = await ConexaoBanco.Numero(banco, "SELECT ModuloId FROM Avaliacoes WITH(UPDLOCK,HOLDLOCK) WHERE Id=@id AND Excluido=0", new { id }, transacao);
        Regras.Exigir(moduloId > 0, "Avaliação não encontrada.", 404);
        await ConexaoBanco.Gravar(banco, "UPDATE Avaliacoes SET Excluido=1,Ativo=0 WHERE Id=@id", new { id }, transacao);
        await CursosServico.RecalcularProgressoModulo(banco, moduloId, transacao);
        await transacao.CommitAsync();
    }

    public async Task<object> Resultados(int alunoId)
    {
        using var banco = await conexao.Abrir();
        return await ConexaoBanco.Consultar(banco, "SELECT t.Id AS id,t.AvaliacaoId AS avaliacaoId,t.TentativaCurso AS tentativaCurso,a.Titulo AS titulo,t.Acertos AS acertos,t.TotalQuestoes AS total,t.Percentual AS percentual,t.Nota AS nota,t.Aprovado AS aprovado,t.DataTentativa AS data FROM TentativasAvaliacao t JOIN Avaliacoes a ON a.Id=t.AvaliacaoId WHERE t.AlunoId=@alunoId ORDER BY t.DataTentativa DESC,t.Id DESC", new { alunoId });
    }

    public async Task<object> Resultado(int id, Usuario usuario)
    {
        using var banco = await conexao.Abrir();
        var lista = await ConexaoBanco.Consultar(banco, "SELECT t.Id AS id,t.AlunoId AS alunoId,t.TentativaCurso AS tentativaCurso,a.Titulo AS titulo,t.Acertos AS acertos,t.TotalQuestoes AS total,t.Percentual AS percentual,t.Nota AS nota,t.Aprovado AS aprovado,m.CursoId AS cursoId,m.Id AS moduloId FROM TentativasAvaliacao t JOIN Avaliacoes a ON a.Id=t.AvaliacaoId JOIN Modulos m ON m.Id=a.ModuloId WHERE t.Id=@id", new { id });
        Regras.Exigir(lista.Count == 1, "Resultado não encontrado.", 404);
        var resultado = lista[0];
        int alunoId = (int)resultado["alunoId"]!;
        if (usuario.Perfil == "Mentor") await Regras.AcessoAlunoMentor(banco, usuario.Id, alunoId, (int)resultado["cursoId"]!);
        else Regras.Exigir(usuario.Perfil == "Gestor" || usuario.Id == alunoId, "Você pode consultar apenas seus resultados.", 403);
        var progresso = await ConexaoBanco.Consultar(banco, "SELECT " + CursosServico.ProgressoSql + " AS valor FROM Modulos m LEFT JOIN ProgressoModulos p ON p.ModuloId=m.Id AND p.AlunoId=@alunoId WHERE m.CursoId=@cursoId AND m.Ativo=1 AND m.Excluido=0", new { alunoId, cursoId = resultado["cursoId"] });
        resultado["progresso"] = progresso[0]["valor"];
        resultado["podeReiniciar"] = usuario.Perfil == "Aluno" && await CursosServico.CursoReprovado(banco, alunoId, (int)resultado["cursoId"]!);
        resultado["respostas"] = await ConexaoBanco.Consultar(banco, "SELECT q.Enunciado AS enunciado,a.Texto AS texto,r.Correta AS correta,g.Texto AS respostaCorreta FROM RespostasAluno r JOIN Questoes q ON q.Id=r.QuestaoId JOIN Alternativas a ON a.Id=r.AlternativaId JOIN Alternativas g ON g.QuestaoId=q.Id AND g.Correta=1 WHERE r.TentativaId=@id ORDER BY q.Id", new { id });
        resultado.Remove("alunoId");
        return resultado;
    }
}
