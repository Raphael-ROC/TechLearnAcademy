package br.com.techlearn.academy;

import android.os.Bundle;
import android.view.View;
import android.widget.LinearLayout;
import android.content.Intent;
import org.json.JSONObject;
import org.json.JSONArray;

public class RelatoriosActivity extends TelaBase {
    private int cursoId, alunoId;

    @Override public void onCreate(Bundle estado) {
        super.onCreate(estado); if (isFinishing()) return;
        cursoId = getIntent().getIntExtra("cursoId", 0); alunoId = getIntent().getIntExtra("alunoId", 0);
        preparar("Relatórios");
        carregar();
    }

    private void carregar() {
        String caminho = "api/relatorios";
        if (cursoId > 0) caminho += "/cursos/" + cursoId + "/alunos" + (alunoId > 0 ? "/" + alunoId : "");
        pedir("GET", caminho, null, "relatorio");
    }

    @Override protected void receber(String operacao, String resposta) throws Exception {
        JSONObject relatorio = new JSONObject(resposta);
        limparCorpo();
        if (cursoId == 0) {
            texto(corpo, "Visão geral", 23, true);
            texto(corpo, "Cursos: " + relatorio.optInt("quantidadeCursos") + "\nMatrículas: " + relatorio.optInt("totalMatriculas") +
                "\nMédia geral: " + relatorio.optDouble("mediaGeral") + "\nAprovação: " + relatorio.optDouble("aprovacao") + "%", 18, false);
            JSONArray cursos = relatorio.getJSONArray("cursos");
            for (int i = 0; i < cursos.length(); i++) {
                JSONObject curso = cursos.getJSONObject(i);
                LinearLayout item = cartao(corpo, curso.getString("nome"));
                texto(item, "Matrículas: " + curso.optInt("matriculas") + "\nMédia: " + curso.optDouble("media") + "\nAprovação: " + curso.optDouble("aprovacao") + "%", 16, false);
                botao(item, "Desempenho dos alunos", "curso/" + curso.getInt("cursoId"));
            }
            texto(corpo, "Atendimentos", 21, true);
            JSONArray atendimentos = relatorio.getJSONArray("atendimentos");
            for (int i = 0; i < atendimentos.length(); i++) {
                JSONObject item = atendimentos.getJSONObject(i);
                texto(cartao(corpo, item.getString("nome")), item.getString("perfil") + "\nAssumidos: " + item.optInt("assumidos") + " · Encerrados: " + item.optInt("encerrados"), 16, false);
            }
            texto(corpo, "Conversas dos mentores", 21, true);
            JSONArray chamados = relatorio.getJSONArray("chamadosMentores");
            for (int i = 0; i < chamados.length(); i++) {
                JSONObject item = chamados.getJSONObject(i);
                botao(corpo, "#" + item.getInt("id") + " · " + item.optString("assunto") + " · " + item.optString("responsavel"), "chamado/" + item.getInt("id"));
            }
        } else if (alunoId == 0) {
            texto(corpo, relatorio.getString("nome"), 23, true);
            JSONArray alunos = relatorio.getJSONArray("alunos");
            listaVazia(alunos);
            for (int i = 0; i < alunos.length(); i++) {
                JSONObject aluno = alunos.getJSONObject(i);
                LinearLayout item = cartao(corpo, aluno.getString("nome"));
                texto(item, aluno.getString("email") + "\nProgresso: " + aluno.optDouble("progresso") + "%", 16, false);
                botao(item, "Ver desempenho", "aluno/" + aluno.getInt("id"));
            }
        } else {
            texto(corpo, relatorio.getString("nome"), 23, true);
            texto(corpo, relatorio.getString("curso") + "\nTentativa atual: " + relatorio.optInt("tentativaCurso") + "\n" +
                (relatorio.optBoolean("aprovadoCurso") ? "Curso aprovado" : "Curso ainda não aprovado"), 18, false);
            texto(corpo, "Módulos", 21, true);
            JSONArray modulos = relatorio.getJSONArray("modulos");
            for (int i = 0; i < modulos.length(); i++) {
                JSONObject modulo = modulos.getJSONObject(i);
                texto(corpo, modulo.getString("titulo") + " · " + (modulo.optBoolean("aprovado") ? "Aprovado" : "Pendente"), 17, false);
            }
            texto(corpo, "Avaliações atuais", 21, true);
            JSONArray avaliacoes = relatorio.getJSONArray("avaliacoes");
            for (int i = 0; i < avaliacoes.length(); i++) {
                JSONObject item = avaliacoes.getJSONObject(i);
                LinearLayout cartao = cartao(corpo, item.getString("titulo"));
                texto(cartao, item.isNull("nota") ? "Ainda não respondida" : "Nota: " + item.optDouble("nota"), 16, false);
                if (item.optInt("resultadoId") > 0) botao(cartao, "Ver respostas", "resultado/" + item.getInt("resultadoId"));
            }
            texto(corpo, "Histórico de tentativas", 21, true);
            JSONArray historico = relatorio.getJSONArray("historico");
            for (int i = 0; i < historico.length(); i++) {
                JSONObject item = historico.getJSONObject(i);
                botao(corpo, item.getString("titulo") + " · Tentativa " + item.optInt("tentativaCurso") + " · Nota " + item.optDouble("nota"), "resultado/" + item.getInt("id"));
            }
        }
        botao(corpo, "Atualizar", "atualizar");
        botao(corpo, "Voltar", "voltar");
    }

    @Override public void onClick(View tela) {
        if (ocupado) return;
        String acao = acao(tela);
        if (acao.equals("voltar")) { finish(); return; }
        if (acao.equals("atualizar")) { carregar(); return; }
        if (acao.startsWith("chamado/")) { abrir(ConversaActivity.class, id(acao)); return; }
        if (acao.startsWith("resultado/")) { abrir(ResultadosActivity.class, id(acao)); return; }
        Intent pedido = new Intent(this, RelatoriosActivity.class);
        pedido.putExtra("cursoId", acao.startsWith("curso/") ? id(acao) : cursoId);
        if (acao.startsWith("aluno/")) pedido.putExtra("alunoId", id(acao));
        startActivity(pedido);
    }
}
