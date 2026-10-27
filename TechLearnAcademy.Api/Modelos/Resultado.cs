using TechLearnAcademy.Api.Servicos;

namespace TechLearnAcademy.Api.Modelos;

// Encapsula o cálculo utilizado na correção e na gravação da tentativa.
public sealed class Resultado
{
    public int Acertos { get; }
    public int Total { get; }
    public decimal Percentual { get; }
    public decimal Nota { get; }
    public bool Aprovado => Percentual >= 60;

    public Resultado(int acertos, int total)
    {
        Regras.Exigir(total > 0 && acertos >= 0 && acertos <= total, "Informe uma quantidade válida de acertos e questões.");
        Acertos = acertos;
        Total = total;
        Percentual = decimal.Round(100m * acertos / total, 2, MidpointRounding.AwayFromZero);
        Nota = decimal.Round(10m * acertos / total, 2, MidpointRounding.AwayFromZero);
    }
}
