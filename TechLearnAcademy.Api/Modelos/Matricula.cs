using TechLearnAcademy.Api.Servicos;

namespace TechLearnAcademy.Api.Modelos;

public sealed class Matricula
{
    public int AlunoId { get; }
    public int CursoId { get; }
    public decimal Progresso { get; private set; }
    public int TentativaCurso { get; private set; }

    public Matricula(int alunoId, int cursoId, decimal progresso = 0, int tentativaCurso = 1)
    {
        Regras.Exigir(cursoId > 0, "Selecione um curso.");
        AlunoId = alunoId;
        CursoId = cursoId;
        Progresso = progresso;
        TentativaCurso = tentativaCurso;
    }

    public void Reiniciar(bool reprovado)
    {
        Regras.Exigir(reprovado, "O curso só pode ser reiniciado após uma reprovação.", 409);
        TentativaCurso++;
        Progresso = 0;
    }
}
