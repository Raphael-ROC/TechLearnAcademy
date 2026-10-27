using TechLearnAcademy.Api.Servicos;
using TechLearnAcademy.Api.Modelos;
using TechLearnAcademy.Api.Banco;
using System.Text.Json;

// Estes testes não criam usuário e não consultam o SQL Server.
int total = 0;
string senha = "A1!" + Guid.NewGuid();

Regras.Senha(senha, senha);
Verificar(true, "Senha válida aceita");
Recusar(() => Regras.Senha("A1!curt", "A1!curt"), "Senha curta recusada");
Recusar(() => Regras.Senha(new string('a', 20) + "1!", new string('a', 20) + "1!"), "Sem maiúscula recusada");
Recusar(() => Regras.Senha(new string('A', 20) + "1", new string('A', 20) + "1"), "Sem símbolo recusada");
Recusar(() => Regras.Senha(new string('A', 20) + "!", new string('A', 20) + "!"), "Sem número recusada");
Recusar(() => Regras.Senha(senha, senha + " "), "Confirmação diferente recusada");

string hash = Regras.HashSenha(senha);
string outroHash = Regras.HashSenha(senha);
Verificar(hash != senha, "Senha não é armazenada em texto");
Verificar(hash != outroHash, "Sal aleatório muda o hash");
Verificar(Regras.ConferirSenha(senha, hash), "Hash reconhece a senha correta");
Recusar(() => Regras.Senha(senha + " ", senha + " "), "Senha com espaço recusada");
Regras.Senha("Abcde1!x", "Abcde1!x");
Verificar(true, "Senha de oito caracteres aceita");
Recusar(() => new Questao(1, "Pergunta", Enumerable.Range(0, 6).Select(i => ("Opção" + i, i == 0)).ToList()), "Mais de cinco alternativas recusado");
Verificar(UsuariosServico.EmailInstitucional("José Luís Ramos", 12, "Gestor") == "jose.luis.12@gestor.techlearn.com", "E-mail institucional usa os dois primeiros nomes e o ID");
Verificar(!Regras.ConferirSenha("incorreta", hash), "Hash recusa senha incorreta");
Verificar(!Regras.ConferirSenha(senha, "hash-inválido"), "Hash inválido não aceita login");

string token = Regras.NovoToken();
Verificar(token.Length == 64 && token != Regras.NovoToken(), "Tokens aleatórios têm 64 caracteres");
Verificar(Regras.HashToken(token) != token, "Banco recebe hash do token");
Verificar(Regras.HashToken(token) == Regras.HashToken(token), "Mesmo token confere com seu hash");
Verificar(Regras.Texto("  Raphael  ", 2, 100, "Nome") == "Raphael", "Nome remove só espaços das pontas");
Recusar(() => Regras.Texto("A", 2, 100, "Nome"), "Nome curto recusado");
Recusar(() => Regras.Texto(new string('A', 101), 2, 100, "Nome"), "Nome longo recusado");
Verificar(Regras.Email("  Pessoa@Example.test  ") == "pessoa@example.test", "E-mail normalizado");
Recusar(() => Regras.Email("email-sem-formato"), "E-mail inválido recusado");
Recusar(() => Regras.Url("javascript:alert(1)"), "Material não executa URL JavaScript");
Verificar(Regras.Url("https://example.test/material.pdf") == "https://example.test/material.pdf", "Material HTTPS aceito");

// Verificações da refatoração: perfis, sigilo e resultados continuam compatíveis.
var aluno = new Usuario();
aluno.AtualizarDados("  Pessoa Aluna  ", "  Pessoa@Example.test  ");
aluno.DefinirSenha(senha, senha);
Verificar(aluno.Nome == "Pessoa Aluna" && aluno.Email == "pessoa@example.test", "Cadastro normaliza nome e e-mail pela classe Usuario");
Verificar(aluno.ConferirSenha(senha) && !aluno.ConferirSenha("incorreta"), "Usuário confere a senha pelo hash");
string jsonUsuario = JsonSerializer.Serialize<Usuario>(aluno, new JsonSerializerOptions(JsonSerializerDefaults.Web));
Verificar(!jsonUsuario.Contains("senhaHash") && !jsonUsuario.Contains(aluno.SenhaHash), "Serialização de usuário não revela hash");
Recusar(() => aluno.AtualizarDados("Outro Nome", "invalido"), "Usuário rejeita e-mail inválido");
Verificar(aluno.Nome == "Pessoa Aluna", "Atualização inválida mantém os dados anteriores");
Usuario[] perfis = [
    new Usuario(perfil: "Aluno"), new Usuario(perfil: "Mentor"),
    new Usuario(perfil: "Gestor"), new Usuario(perfil: "Administrador")
];
Verificar(perfis.Select(p => p.Perfil).SequenceEqual(new[] { "Aluno", "Mentor", "Gestor", "Administrador" }), "Uma classe Usuario representa os quatro perfis");
Recusar(() => new Usuario(perfil: "Desconhecido"), "Perfil desconhecido recusado");
var usuarioLido = Usuario.DoBanco(new Dictionary<string, object?> {
    ["id"] = 7, ["nome"] = "Mentor Salvo", ["email"] = "mentor@example.test",
    ["perfil"] = "Mentor", ["senhaHash"] = hash, ["ativo"] = false
});
Verificar(usuarioLido.Perfil == "Mentor" && usuarioLido.Id == 7 && !usuarioLido.Ativo && usuarioLido.ConferirSenha(senha), "Leitura do banco preserva identidade, situação e hash");
var curso = new Curso("  Programação  ", "  Introdução  ", 20, false);
Verificar(curso.Nome == "Programação" && curso.Descricao == "Introdução" && !curso.Ativo, "Curso preserva normalização e situação");
Recusar(() => new Curso("Curso", "Descrição", 0), "Curso sem carga horária recusado");
Recusar(() => Modulo.ParaCadastro(0, "Módulo", 1), "Novo módulo exige curso");
Verificar(new Modulo(" Título ", 1, false).Titulo == "Título", "Alteração de módulo não exige novo curso");
Recusar(() => new Material("Material", "javascript:alert(1)", "Link"), "Classe Material recusa endereço inseguro");
Recusar(() => new Material("Material", "https://example.test", "Outro"), "Classe Material mantém tipos permitidos");
Verificar(!new Resultado(59, 100).Aprovado && new Resultado(60, 100).Aprovado, "Aprovação continua no limite de 60 por cento");
Verificar(new Resultado(1, 16).Nota == 0.63m, "Nota mantém arredondamento compatível com o SQL");
Verificar(new Resultado(1, 3).Percentual == 33.33m, "Percentual mantém duas casas decimais");
Recusar(() => new Resultado(0, 0), "Resultado sem questões recusado");
var questao = new Questao(1, " Pergunta ", [(" A ", false), (" B ", true)]);
Verificar(questao.Enunciado == "Pergunta" && questao.Alternativas[1].Texto == "B", "Questão normaliza enunciado e alternativas");
Recusar(() => new Questao(1, "Pergunta", [("A", true), ("B", true)]), "Questão recusa duas alternativas corretas");
Recusar(() => new Questao(1, "Pergunta", [("A", false), ("B", false)]), "Questão exige uma alternativa correta");
Recusar(() => new Questao(1, "Pergunta", null), "Questão exige alternativas");
var pedido = JsonSerializer.Deserialize<DadosQuestao>("""{"avaliacaoId":1,"enunciado":"Pergunta","alternativas":[{"texto":"A","correta":true},{"texto":"B","correta":false}]}""", new JsonSerializerOptions(JsonSerializerDefaults.Web));
Verificar(pedido.Ativo && pedido.Alternativas.Count == 2, "Formulário JSON mantém campos e valores padrão");
var matricula = new Matricula(7, 2, 50m);
Verificar(matricula.AlunoId == 7 && matricula.CursoId == 2 && matricula.Progresso == 50m, "Matrícula associa aluno, curso e progresso");
Verificar(new OrientacaoMentor(2, 7, " Orientação ").Texto == "Orientação", "Orientação mantém validação do texto");
// Regras dos chamados conferidas com a versão publicada, sem abrir o banco.
var permissoesChamado = typeof(GestaoServico).GetMethod("PermissoesChamado", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!;
void ConferirChamado(string nome, int usuarioId, string perfil, int solicitanteId, int? responsavelId, string status, bool podeAtender, bool podeResponder, bool podeEncerrar)
{
    var chamado = new Dictionary<string, object?>
    {
        ["solicitanteId"] = solicitanteId,
        ["responsavelId"] = responsavelId,
        ["status"] = status,
        ["destinatarioPerfil"] = "Mentor"
    };
    var usuarioChamado = new Usuario(usuarioId, "Pessoa de teste", "pessoa@example.test", perfil);
    permissoesChamado.Invoke(null, [chamado, usuarioChamado]);
    Verificar((bool)chamado["podeAtender"]! == podeAtender && (bool)chamado["podeResponder"]! == podeResponder && (bool)chamado["podeEncerrar"]! == podeEncerrar, nome);
}
ConferirChamado("Aluno aguarda responsável antes de responder", 1, "Aluno", 1, null, "Aberto", false, false, false);
ConferirChamado("Aluno responde depois do início do atendimento", 1, "Aluno", 1, 2, "Em atendimento", false, true, false);
ConferirChamado("Mentor não responde ao próprio chamado", 1, "Mentor", 1, 2, "Em atendimento", false, false, false);
ConferirChamado("Gestor não responde ao próprio chamado", 1, "Gestor", 1, 2, "Em atendimento", false, false, false);
ConferirChamado("Responsável responde e pode encerrar", 2, "Mentor", 1, 2, "Em atendimento", false, true, true);
ConferirChamado("Outro mentor não responde pelo responsável", 3, "Mentor", 1, 2, "Em atendimento", false, false, false);
ConferirChamado("Chamado encerrado não recebe resposta", 2, "Mentor", 1, 2, "Encerrado", false, false, false);
ConferirChamado("Mentor pode assumir chamado disponível", 2, "Mentor", 1, null, "Aberto", true, false, false);
Console.WriteLine($"Resultado: {total} testes passaram.");

void Verificar(bool condicao, string nome)
{
    if (!condicao) throw new Exception("FALHOU: " + nome);
    total++;
    Console.WriteLine("PASSOU: " + nome);
}

void Recusar(Action acao, string nome)
{
    bool recusou = false;
    try { acao(); }
    catch (ErroAplicacao) { recusou = true; }
    Verificar(recusou, nome);
}

