using TechLearnAcademy.Api.Servicos;

namespace TechLearnAcademy.Api.Modelos;

public sealed class Material
{
    public string Titulo { get; }
    public string Url { get; }
    public string Tipo { get; }
    public bool Ativo { get; }

    public Material(string titulo, string url, string tipo, bool ativo = true, bool arquivoPdf = false)
    {
        Titulo = Regras.Texto(titulo, 1, 150, "Título");
        Url = arquivoPdf ? "" : Regras.Url(url);
        Regras.Exigir(!arquivoPdf || tipo == "PDF", "O documento deve ser um PDF.");
        Regras.Exigir(tipo == "PDF" || tipo == "Link", "O tipo deve ser PDF ou Link.");
        Tipo = tipo;
        Ativo = ativo;
    }
}
