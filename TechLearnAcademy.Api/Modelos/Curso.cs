using TechLearnAcademy.Api.Servicos;

namespace TechLearnAcademy.Api.Modelos;

public sealed class Curso
{
    public string Nome { get; }
    public string Descricao { get; }
    public int CargaHoraria { get; }
    public bool Ativo { get; }

    public Curso(string nome, string descricao, int cargaHoraria, bool ativo = true)
    {
        Nome = Regras.Texto(nome, 2, 150, "Nome do curso");
        Descricao = Regras.Texto(descricao, 1, 2000, "Descrição");
        Regras.Exigir(cargaHoraria > 0, "A carga horária deve ser positiva.");
        CargaHoraria = cargaHoraria;
        Ativo = ativo;
    }
}
