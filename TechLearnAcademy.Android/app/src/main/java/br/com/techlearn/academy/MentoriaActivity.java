package br.com.techlearn.academy;

import android.os.Bundle;
import android.view.View;
import android.widget.*;
import android.text.InputType;
import org.json.JSONObject;
import org.json.JSONArray;

public class MentoriaActivity extends TelaBase {
    private int alunoId;
    private EditText orientacao;

    @Override public void onCreate(Bundle estado) {
        super.onCreate(estado); if (isFinishing()) return;
        alunoId = getIntent().getIntExtra("id", 0);
        preparar(Sessao.perfil("Aluno") ? "Orientações dos mentores" : "Acompanhamento do aluno");
        carregar();
    }

    private void carregar() {
        String caminho = Sessao.perfil("Aluno") ? "api/aluno/orientacoes" : "api/mentor/alunos" + (alunoId == 0 ? "" : "/" + alunoId);
        pedir("GET", caminho, null, "dados");
    }

    @Override protected void receber(String operacao, String resposta) throws Exception {
        if (operacao.equals("orientar")) { Toast.makeText(this, new JSONObject(resposta).getString("mensagem"), Toast.LENGTH_LONG).show(); carregar(); return; }
        limparCorpo();
        if (Sessao.perfil("Aluno")) {
            JSONArray orientacoes = new JSONArray(resposta);
            listaVazia(orientacoes);
            for (int i = 0; i < orientacoes.length(); i++) {
                JSONObject item = orientacoes.getJSONObject(i);
                LinearLayout cartao = cartao(corpo, item.optString("mentor"));
                texto(cartao, item.getString("texto"), 17, false);
                texto(cartao, item.optString("data").replace("T", " "), 14, false);
            }
        } else if (alunoId == 0) {
            JSONArray alunos = new JSONArray(resposta);
            listaVazia(alunos);
            for (int i = 0; i < alunos.length(); i++) {
                JSONObject aluno = alunos.getJSONObject(i);
                LinearLayout cartao = cartao(corpo, aluno.getString("nome"));
                texto(cartao, aluno.getString("email"), 16, false);
                botao(cartao, "Acompanhar aluno", "aluno/" + aluno.getInt("id"));
            }
        } else {
            JSONObject aluno = new JSONObject(resposta);
            texto(corpo, aluno.getString("nome"), 23, true);
            texto(corpo, aluno.getString("email"), 16, false);
            texto(corpo, "Cursos acompanhados", 20, true);
            JSONArray cursos = aluno.getJSONArray("cursos");
            for (int i = 0; i < cursos.length(); i++) texto(corpo, cursos.getJSONObject(i).getString("nome") + " · " + cursos.getJSONObject(i).optDouble("progresso") + "%", 17, false);
            texto(corpo, "Avaliações", 20, true);
            JSONArray resultados = aluno.getJSONArray("resultados");
            for (int i = 0; i < resultados.length(); i++) {
                JSONObject item = resultados.getJSONObject(i);
                botao(corpo, item.getString("titulo") + " · Nota " + item.optDouble("nota"), "resultado/" + item.getInt("id"));
            }
            texto(corpo, "Orientações", 20, true);
            JSONArray orientacoes = aluno.getJSONArray("orientacoes");
            for (int i = 0; i < orientacoes.length(); i++) texto(cartao(corpo, ""), orientacoes.getJSONObject(i).getString("texto"), 16, false);
            orientacao = campo(corpo, "Nova orientação", InputType.TYPE_CLASS_TEXT | InputType.TYPE_TEXT_FLAG_MULTI_LINE);
            botao(corpo, "Registrar orientação", "orientar");
        }
        botao(corpo, "Atualizar", "atualizar");
        botao(corpo, "Voltar", "voltar");
    }

    @Override public void onClick(View tela) {
        if (ocupado) return;
        String acao = acao(tela);
        if (acao.equals("voltar")) { finish(); return; }
        if (acao.equals("atualizar")) { carregar(); return; }
        if (acao.startsWith("aluno/")) { abrir(MentoriaActivity.class, id(acao)); return; }
        if (acao.startsWith("resultado/")) { abrir(ResultadosActivity.class, id(acao)); return; }
        try {
            Validacao.exigir(!valor(orientacao).trim().isEmpty(), "Escreva a orientação.");
            JSONObject dados = new JSONObject(); dados.put("alunoId", alunoId); dados.put("texto", valor(orientacao).trim());
            pedir("POST", "api/mentor/orientacoes", dados, "orientar");
        } catch (Exception falha) { erro(falha); }
    }
}
