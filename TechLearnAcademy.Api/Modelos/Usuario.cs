using System.Text.Json.Serialization;
using TechLearnAcademy.Api.Servicos;

namespace TechLearnAcademy.Api.Modelos;

public sealed class Usuario
{
    public int Id { get; }
    public string Nome { get; private set; }
    public string Email { get; private set; }
    public string Perfil { get; }
    public bool Ativo { get; }

    [JsonIgnore]
    public string SenhaHash { get; private set; }

    public Usuario(int id = 0, string nome = "", string email = "", string perfil = "Aluno",
        string senhaHash = "", bool ativo = true)
    {
        Regras.Exigir(new[] { "Aluno", "Mentor", "Gestor", "Administrador" }.Contains(perfil), "Informe um perfil válido.");
        Id = id;
        Nome = nome;
        Email = email;
        Perfil = perfil;
        SenhaHash = senhaHash;
        Ativo = ativo;
    }

    public void AtualizarDados(string nome, string email)
    {
        string nomeValidado = Regras.Texto(nome, 2, 100, "Nome");
        string emailValidado = Regras.Email(email);
        Nome = nomeValidado;
        Email = emailValidado;
    }

    public void DefinirSenha(string senha, string confirmacao)
    {
        Regras.Senha(senha, confirmacao);
        SenhaHash = Regras.HashSenha(senha);
    }

    public bool ConferirSenha(string senha) => Regras.ConferirSenha(senha, SenhaHash);

    // Carrega os dados retornados pelas consultas existentes, sem alterá-los.
    public static Usuario DoBanco(IReadOnlyDictionary<string, object?> linha) => new(
        (int)linha["id"]!, (string)linha["nome"]!, (string)linha["email"]!,
        (string)linha["perfil"]!,
        linha.TryGetValue("senhaHash", out var hash) ? (string)hash! : "",
        !linha.TryGetValue("ativo", out var ativo) || (bool)ativo!);
}
