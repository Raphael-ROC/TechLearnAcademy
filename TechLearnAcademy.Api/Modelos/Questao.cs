using TechLearnAcademy.Api.Servicos;

namespace TechLearnAcademy.Api.Modelos;

public sealed class Questao
{
    public int AvaliacaoId { get; }
    public string Enunciado { get; }
    public bool Ativo { get; }
    // As alternativas são dados da própria questão.
    public IReadOnlyList<(string Texto, bool Correta)> Alternativas { get; }

    public Questao(int avaliacaoId, string enunciado,
        List<(string Texto, bool Correta)>? alternativas, bool ativo = true)
    {
        Enunciado = Regras.Texto(enunciado, 1, 2000, "Enunciado");
        Regras.Exigir(alternativas != null && alternativas.Count >= 2 && alternativas.Count <= 5, "Informe de duas a cinco alternativas (A a E).");
        Regras.Exigir(alternativas!.Count(a => a.Correta) == 1, "Marque exatamente uma alternativa correta.");
        Alternativas = alternativas!.Select(a =>
            (Regras.Texto(a.Texto, 1, 1000, "Alternativa"), a.Correta)).ToList().AsReadOnly();
        AvaliacaoId = avaliacaoId;
        Ativo = ativo;
    }
}
