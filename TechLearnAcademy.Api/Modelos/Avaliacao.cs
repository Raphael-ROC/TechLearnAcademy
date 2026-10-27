using TechLearnAcademy.Api.Servicos;

namespace TechLearnAcademy.Api.Modelos;

public sealed class Avaliacao
{
    public int ModuloId { get; }
    public string Titulo { get; }
    public bool Ativo { get; }

    public Avaliacao(int moduloId, string titulo, bool ativo = true)
    {
        Titulo = Regras.Texto(titulo, 1, 150, "Título");
        ModuloId = moduloId;
        Ativo = ativo;
    }
}
