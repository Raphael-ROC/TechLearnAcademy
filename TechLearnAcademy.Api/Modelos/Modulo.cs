using TechLearnAcademy.Api.Servicos;

namespace TechLearnAcademy.Api.Modelos;

public sealed class Modulo
{
    public string Titulo { get; }
    public int Ordem { get; }
    public bool Ativo { get; }

    // A alteração de um módulo mantém o curso original no banco.
    public Modulo(string titulo, int ordem, bool ativo = true)
    {
        Titulo = Regras.Texto(titulo, 1, 150, "Título");
        Regras.Exigir(ordem > 0, "A ordem deve ser positiva.");
        Ordem = ordem;
        Ativo = ativo;
    }

    public static Modulo ParaCadastro(int cursoId, string titulo, int ordem)
    {
        string tituloValidado = Regras.Texto(titulo, 1, 150, "Título");
        Regras.Exigir(cursoId > 0 && ordem > 0, "Informe o curso e uma ordem positiva.");
        return new Modulo(tituloValidado, ordem);
    }
}
