import br.com.techlearn.academy.*;
import org.json.*;
import java.util.Arrays;

public class VerificarApiJava {
    private static int total;
    private static final String senha = System.getenv("SENHA_TESTE");
    private static JSONObject json(Object... campos) throws Exception {
        JSONObject dados = new JSONObject();
        for (int i = 0; i < campos.length; i += 2) dados.put((String)campos[i], campos[i+1]);
        return dados;
    }
    private static JSONObject obj(String metodo, String caminho, JSONObject dados) throws Exception { return new JSONObject(Api.enviar(metodo, caminho, dados)); }
    private static JSONArray lista(String caminho) throws Exception { return new JSONArray(Api.enviar("GET", caminho, null)); }
    private static void ok(String nome, boolean condicao) {
        if (!condicao) throw new IllegalStateException("Falhou: " + nome);
        total++; System.out.println("OK: " + nome);
    }
    private static void login(String perfil) throws Exception {
        Sessao.limpar();
        JSONObject usuario = obj("POST","api/usuarios/login",json("email",perfil.toLowerCase()+"@mobile.test","senha",senha));
        Sessao.token = usuario.getString("token");
        Sessao.usuario = obj("GET","api/usuarios/perfil",null);
        ok("Login Java real: " + perfil, Sessao.perfil(perfil) && Sessao.token.length()==64);
    }
    private static void recusar(String metodo,String caminho,JSONObject dados,int status) throws Exception {
        try { Api.enviar(metodo,caminho,dados); throw new IllegalStateException("Deveria recusar " + caminho); }
        catch(Api.Erro erro) { ok("Recusa HTTP " + status + ": " + caminho, erro.status==status); }
    }
    public static void main(String[] args) throws Exception {
        obj("GET","api/status",null);
        login("Administrador");
        JSONObject painel = obj("GET","api/administracao/painel",null);
        ok("Resumo real dos quatro perfis",painel.getInt("ativos")==4 && painel.getJSONArray("perfis").length()==4);
        JSONObject novo = obj("POST","api/administracao/usuarios",json("nome","Mentor Novo","emailContato","novo@mobile.test","perfil","Mentor","senha",senha,"confirmarSenha",senha));
        ok("Cadastro administrativo e e-mail institucional",novo.getString("email").endsWith("@mentor.techlearn.com"));
        recusar("POST","api/administracao/usuarios",json("nome","Outro Mentor","emailContato","novo@mobile.test","perfil","Mentor","senha",senha,"confirmarSenha",senha),409);
        login("Gestor");
        int curso = obj("POST","api/cursos",json("nome","Curso de teste mobile","descricao","Curso descartável para integração Java.","cargaHoraria",8)).getInt("id");
        int modulo = obj("POST","api/modulos",json("cursoId",curso,"titulo","Módulo de teste","ordem",1)).getInt("id");
        int material = obj("POST","api/materiais",json("moduloId",modulo,"titulo","Material de teste","tipo","Link","url","https://example.com/material")).getInt("id");
        ok("Curso, módulo e material gravados pela API",curso>0 && modulo>0 && material>0);
        byte[] pdf = "%PDF-1.4\n% Documento exclusivo do teste de transporte HTTP.\n%%EOF".getBytes("US-ASCII");
        JSONObject enviado = new JSONObject(Api.enviarPdf("POST","api/materiais/pdf",json("moduloId",modulo,"titulo","PDF de teste"),pdf));
        int pdfId = enviado.getInt("id");
        ok("Upload multipart pelo cliente Java",pdfId>0);
        ok("Download protegido preserva os bytes",Arrays.equals(Api.baixar("api/materiais/"+pdfId+"/arquivo"),pdf));
        int avaliacao = obj("POST","api/avaliacoes",json("moduloId",modulo,"titulo","Avaliação de teste")).getInt("id");
        JSONArray alternativas = new JSONArray();
        alternativas.put(json("texto","Resposta correta","correta",true));
        alternativas.put(json("texto","Resposta incorreta","correta",false));
        int questao = obj("POST","api/questoes",json("avaliacaoId",avaliacao,"enunciado","Selecione a resposta correta.","alternativas",alternativas)).getInt("id");
        ok("Avaliação e questão cadastradas",avaliacao>0 && questao>0);
        login("Aluno");
        recusar("GET","api/administracao/painel",null,403);
        obj("POST","api/matriculas",json("cursoId",curso));
        JSONObject detalhesCurso = obj("GET","api/cursos/"+curso,null);
        ok("Matrícula consultada pelo Java",detalhesCurso.getBoolean("matriculado"));
        ok("Materiais do aluno",lista("api/modulos/"+modulo+"/materiais").length()==2);
        JSONObject prova = obj("GET","api/avaliacoes/"+avaliacao,null);
        JSONObject primeira = prova.getJSONArray("questoes").getJSONObject(0);
        ok("Gabarito não é enviado ao aluno",!primeira.getJSONArray("alternativas").getJSONObject(0).has("correta"));
        JSONArray respostas = new JSONArray().put(json("questaoId",questao,"alternativaId",primeira.getJSONArray("alternativas").getJSONObject(0).getInt("id")));
        JSONObject resultado = obj("POST","api/avaliacoes/"+avaliacao+"/respostas",json("tentativaCurso",prova.getInt("tentativaCurso"),"respostas",respostas));
        ok("Correção verdadeira no servidor",resultado.getBoolean("aprovado") && resultado.getDouble("nota")==10);
        recusar("POST","api/avaliacoes/"+avaliacao+"/respostas",json("tentativaCurso",1,"respostas",respostas),409);
        obj("POST","api/certificados",json("cursoId",curso));
        int ajuda = obj("POST","api/chamados",json("cursoId",curso,"assunto","Ajuda de teste","texto","Preciso de uma orientação no curso de teste.")).getInt("id");
        login("Mentor");
        JSONObject conversa = obj("GET","api/chamados/"+ajuda,null);
        ok("Mentor recebe o chamado do aluno",conversa.getBoolean("podeAtender"));
        obj("POST","api/chamados/"+ajuda+"/assumir",null);
        obj("POST","api/chamados/"+ajuda+"/mensagens",json("texto","Resposta de teste do Mentor."));
        ok("Mentor passa a acompanhar o aluno",lista("api/mentor/alunos").length()==1);
        int alunoId = lista("api/mentor/alunos").getJSONObject(0).getInt("id");
        obj("POST","api/mentor/orientacoes",json("alunoId",alunoId,"texto","Orientação registrada pelo Java."));
        ok("Acompanhamento detalhado",obj("GET","api/mentor/alunos/"+alunoId,null).getJSONArray("resultados").length()==1);
        obj("POST","api/chamados/"+ajuda+"/encerrar",null);
        int ouvidoria = obj("POST","api/chamados",json("assunto","Ouvidoria teste","texto","Contato de teste com o Gestor.")).getInt("id");
        login("Gestor");
        int certificado = lista("api/certificados").getJSONObject(0).getInt("id");
        obj("POST","api/certificados/"+certificado+"/decisao",json("aprovar",true));
        ok("Certificado aprovado com código real",obj("GET","api/certificados/"+certificado,null).getString("codigo").startsWith("TL-"));
        obj("POST","api/chamados/"+ouvidoria+"/assumir",null);
        obj("POST","api/chamados/"+ouvidoria+"/mensagens",json("texto","Resposta de teste do Gestor."));
        obj("POST","api/chamados/"+ouvidoria+"/encerrar",null);
        int contatoAdmin = obj("POST","api/chamados",json("assunto","Contato de teste","texto","Contato de teste com o Administrador.")).getInt("id");
        ok("Relatórios e desempenho do aluno",obj("GET","api/relatorios/cursos/"+curso+"/alunos/"+alunoId,null).getBoolean("aprovadoCurso"));
        ok("Relatório geral contém atendimentos",obj("GET","api/relatorios",null).getJSONArray("atendimentos").length()>=2);
        login("Administrador");
        obj("POST","api/chamados/"+contatoAdmin+"/assumir",null);
        obj("POST","api/chamados/"+contatoAdmin+"/mensagens",json("texto","Resposta de teste do Administrador."));
        obj("POST","api/chamados/"+contatoAdmin+"/encerrar",null);
        ok("Administrador responde e encerra",obj("GET","api/chamados/"+contatoAdmin,null).getString("status").equals("Encerrado"));
        login("Aluno");
        ok("Aluno recebe certificado aprovado",obj("GET","api/certificados/"+certificado,null).getString("aluno").equals("Aluno Teste"));
        ok("Orientações do mentor disponíveis",lista("api/aluno/orientacoes").length()>=2);
        ok("Resultado detalhado real",obj("GET","api/resultados/"+resultado.getInt("id"),null).getJSONArray("respostas").getJSONObject(0).getBoolean("correta"));
        obj("PUT","api/usuarios/perfil",json("nome","Aluno Atualizado","email","aluno@mobile.test","emailContato","aluno@mobile.test"));
        ok("Perfil atualizado sem armazenamento local",obj("GET","api/usuarios/perfil",null).getString("nome").equals("Aluno Atualizado"));
        obj("POST","api/usuarios/logout",null);
        recusar("GET","api/usuarios/perfil",null,401);
        Sessao.limpar();
        System.out.println("APROVADOS: "+total);
    }
}
