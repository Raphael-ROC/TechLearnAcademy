using TechLearnAcademy.Api.Servicos;

namespace TechLearnAcademy.Api.Modelos;

public sealed class OrientacaoMentor
{
    public int MentorId { get; }
    public int AlunoId { get; }
    public string Texto { get; }

    public OrientacaoMentor(int mentorId, int alunoId, string texto)
    {
        Texto = Regras.Texto(texto, 1, 4000, "Orientação");
        MentorId = mentorId;
        AlunoId = alunoId;
    }
}
