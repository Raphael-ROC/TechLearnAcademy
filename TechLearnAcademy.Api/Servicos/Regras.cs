using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Data.SqlClient;
using TechLearnAcademy.Api.Banco;
using TechLearnAcademy.Api.Modelos;

namespace TechLearnAcademy.Api.Servicos;

public class ErroAplicacao(int status, string mensagem) : Exception(mensagem)
{
    public int Status { get; } = status;
}

public static class Regras
{
    public static void Exigir(bool condicao, string mensagem, int status = 400)
    {
        if (!condicao) throw new ErroAplicacao(status, mensagem);
    }

    public static string Texto(string? texto, int minimo, int maximo, string nome)
    {
        string valor = (texto ?? "").Trim();
        int tamanho = valor.Length;
        Exigir(tamanho >= minimo && tamanho <= maximo, $"{nome} deve ter de {minimo} a {maximo} caracteres.");
        return valor;
    }

    public static string Email(string? email)
    {
        string valor = (email ?? "").Trim();
        Exigir(valor.Length <= 254 && MailAddress.TryCreate(valor, out var endereco)
            && endereco.Address == valor && Regex.IsMatch(valor, @"^[^\s@]+@[^\s@]+\.[^\s@]+$"), "Digite um e-mail válido com até 254 caracteres.");
        return valor.ToLowerInvariant();
    }

    // Cada e-mail identifica somente uma pessoa, seja de acesso ou de recuperação.
    public static async Task EmailDisponivel(SqlConnection banco, string email, string contato, int usuarioId, SqlTransaction transacao)
    {
        int repetidos = await ConexaoBanco.Numero(banco,
            "SELECT COUNT(*) FROM Usuarios WITH(UPDLOCK,HOLDLOCK) WHERE Id<>@usuarioId AND (Email=@email OR EmailContato=@email OR Email=@contato OR EmailContato=@contato)",
            new { email, contato, usuarioId }, transacao);
        Exigir(repetidos == 0, "Este e-mail de acesso ou de recuperação já pertence a outro usuário. Informe outro e-mail pessoal.", 409);
    }

    public static void Senha(string? senha, string? confirmacao)
    {
        Exigir(senha != null && senha.EnumerateRunes().Count() >= 8,
            "A senha deve ter pelo menos 8 caracteres.");
        Exigir(!senha!.EnumerateRunes().Any(Rune.IsWhiteSpace), "A senha não pode conter espaços.");
        Exigir(senha!.EnumerateRunes().Any(Rune.IsUpper), "A senha deve ter uma letra maiúscula.");
        Exigir(senha.EnumerateRunes().Any(Rune.IsNumber), "A senha deve ter um número.");
        Exigir(senha.EnumerateRunes().Any(c => !Rune.IsLetter(c) && !Rune.IsNumber(c) && !Rune.IsWhiteSpace(c)), "A senha deve ter um caractere especial.");
        Exigir(senha == confirmacao, "As senhas devem ser exatamente iguais.");
    }

    public static string HashSenha(string senha)
    {
        byte[] sal = RandomNumberGenerator.GetBytes(16);
        byte[] hash = Rfc2898DeriveBytes.Pbkdf2(senha, sal, 600000, HashAlgorithmName.SHA256, 32);
        return "600000." + Convert.ToBase64String(sal) + "." + Convert.ToBase64String(hash);
    }

    public static bool ConferirSenha(string senha, string hashSalvo)
    {
        try
        {
            string[] partes = hashSalvo.Split('.');
            if (partes.Length != 3) return false;
            int repeticoes = int.Parse(partes[0]);
            if (repeticoes < 100000 || repeticoes > 1000000) return false;
            byte[] hash = Rfc2898DeriveBytes.Pbkdf2(senha, Convert.FromBase64String(partes[1]), repeticoes, HashAlgorithmName.SHA256, 32);
            return CryptographicOperations.FixedTimeEquals(hash, Convert.FromBase64String(partes[2]));
        }
        catch (FormatException) { return false; }
    }

    public static string NovoToken() => Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
    public static string HashToken(string token) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));

    public static void Gestor(Usuario usuario) => Exigir(usuario.Perfil == "Gestor", "Esta função é exclusiva do Gestor.", 403);
    public static void Aluno(Usuario usuario) => Exigir(usuario.Perfil == "Aluno", "Esta função é exclusiva do Aluno.", 403);

    public static async Task AcessoCurso(SqlConnection banco, int cursoId, Usuario usuario, SqlTransaction? transacao = null)
    {
        int existe = await ConexaoBanco.Numero(banco, "SELECT COUNT(*) FROM Cursos WHERE Id=@cursoId AND Excluido=0", new { cursoId }, transacao);
        Exigir(existe == 1, "Curso não encontrado.", 404);
        if (usuario.Perfil == "Gestor") return;
        Aluno(usuario);
        int possui = await ConexaoBanco.Numero(banco, "SELECT COUNT(*) FROM Matriculas ma JOIN Cursos c ON c.Id=ma.CursoId WHERE ma.AlunoId=@alunoId AND ma.CursoId=@cursoId AND ma.Ativo=1 AND c.Ativo=1", new { alunoId = usuario.Id, cursoId }, transacao);
        Exigir(possui > 0, "Matricule-se em um curso ativo para acessar este conteúdo.", 403);
    }

    public static async Task<int> AcessoModulo(SqlConnection banco, int moduloId, Usuario usuario, SqlTransaction? transacao = null)
    {
        var modulos = await ConexaoBanco.Consultar(banco, "SELECT CursoId AS cursoId, Ativo AS ativo FROM Modulos WHERE Id=@moduloId AND Excluido=0", new { moduloId }, transacao);
        Exigir(modulos.Count == 1, "Módulo não encontrado.", 404);
        Exigir(usuario.Perfil == "Gestor" || (bool)modulos[0]["ativo"]!, "Este módulo está inativo.", 403);
        int cursoId = (int)modulos[0]["cursoId"]!;
        await AcessoCurso(banco, cursoId, usuario, transacao);
        return cursoId;
    }

    public static async Task AcessoAlunoMentor(SqlConnection banco, int mentorId, int alunoId, int? cursoId = null)
    {
        int possui = await ConexaoBanco.Numero(banco, "SELECT COUNT(*) FROM Chamados c WHERE c.ResponsavelId=@mentorId AND c.SolicitanteId=@alunoId AND c.Tipo='AjudaCurso' AND (@cursoId IS NULL OR c.CursoId=@cursoId)", new { mentorId, alunoId, cursoId });
        Exigir(possui > 0, "Você só pode consultar alunos dos chamados que assumiu.", 403);
    }

    public static string Url(string? url)
    {
        string valor = Texto(url, 1, 2000, "Link");
        Exigir(Uri.TryCreate(valor, UriKind.Absolute, out var endereco) && (endereco.Scheme == "https" || endereco.Scheme == "http"), "Informe um link válido começando com https:// ou http://.");
        return valor;
    }
}
