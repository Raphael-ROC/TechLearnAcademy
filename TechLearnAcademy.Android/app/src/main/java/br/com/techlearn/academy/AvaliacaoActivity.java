package br.com.techlearn.academy;

import android.os.Bundle;
import android.view.View;
import android.widget.*;
import android.content.Intent;
import org.json.JSONObject;
import org.json.JSONArray;
import java.util.ArrayList;

public class AvaliacaoActivity extends TelaBase {
    private int avaliacaoId;
    private JSONObject avaliacao;
    private JSONArray questoes;
    private final ArrayList<RadioGroup> respostas = new ArrayList<>();

    @Override public void onCreate(Bundle estado) { super.onCreate(estado); if (isFinishing()) return; avaliacaoId = getIntent().getIntExtra("id", 0); preparar("Avaliação"); }
    @Override public void onResume() { super.onResume(); if (avaliacao == null || Sessao.perfil("Gestor")) pedir("GET", "api/avaliacoes/" + avaliacaoId, null, "avaliacao"); }

    @Override protected void receber(String operacao, String resposta) throws Exception {
        if (operacao.equals("excluir")) { Toast.makeText(this, new JSONObject(resposta).getString("mensagem"), Toast.LENGTH_LONG).show(); finish(); return; }
        if (operacao.equals("responder")) { abrir(ResultadosActivity.class, new JSONObject(resposta).getInt("id")); finish(); return; }
        avaliacao = new JSONObject(resposta);
        questoes = avaliacao.getJSONArray("questoes");
        limparCorpo(); respostas.clear();
        texto(corpo, avaliacao.getString("titulo"), 23, true);
        if (Sessao.perfil("Gestor")) {
            botao(corpo, "Editar título e situação", "editar");
            botao(corpo, "Adicionar questão", "nova");
            botao(corpo, "Excluir avaliação", "excluir");
        } else if (!avaliacao.optBoolean("podeResponder")) {
            texto(corpo, avaliacao.optString("motivoBloqueio"), 17, true);
            if (avaliacao.optInt("resultadoId") > 0) botao(corpo, "Ver resultado", "resultado");
        } else texto(corpo, "Responda todas as questões. A avaliação pode ser enviada uma vez nesta tentativa do curso. A aprovação exige nota 6.", 16, false);
        for (int i = 0; i < questoes.length(); i++) {
            JSONObject questao = questoes.getJSONObject(i);
            LinearLayout item = cartao(corpo, (i + 1) + ". " + questao.getString("enunciado"));
            JSONArray alternativas = questao.getJSONArray("alternativas");
            RadioGroup grupo = new RadioGroup(this);
            item.addView(grupo);
            for (int a = 0; a < alternativas.length(); a++) {
                JSONObject alternativa = alternativas.getJSONObject(a);
                if (Sessao.perfil("Gestor"))
                    texto(item, (char)('A' + a) + ") " + alternativa.getString("texto") + (alternativa.optBoolean("correta") ? " · Correta" : ""), 16, false);
                else {
                    RadioButton opcao = new RadioButton(this);
                    opcao.setId(View.generateViewId());
                    opcao.setTag(alternativa.getInt("id"));
                    opcao.setText((char)('A' + a) + ") " + alternativa.getString("texto"));
                    opcao.setMinHeight(dp(48));
                    grupo.addView(opcao);
                }
            }
            respostas.add(grupo);
            if (Sessao.perfil("Gestor")) {
                texto(item, questao.optBoolean("ativo") ? "Ativa" : "Inativa", 15, false);
                botao(item, "Editar questão", "questao/" + i);
            }
        }
        if (Sessao.perfil("Aluno") && avaliacao.optBoolean("podeResponder") && questoes.length() > 0) botao(corpo, "Enviar respostas", "enviar");
        botao(corpo, "Voltar", "voltar");
    }

    @Override public void onClick(View tela) {
        if (ocupado) return;
        String acao = acao(tela);
        try {
            if (acao.equals("voltar")) { finish(); return; }
            if (acao.equals("resultado")) { abrir(ResultadosActivity.class, avaliacao.getInt("resultadoId")); return; }
            if (acao.equals("editar")) { abrir(AvaliacaoFormActivity.class, avaliacaoId); return; }
            if (acao.equals("excluir")) { confirmar("Excluir esta avaliação das telas? As notas anteriores serão mantidas.", "excluir"); return; }
            if (acao.equals("enviar")) { confirmar("Enviar as respostas para correção? Elas não poderão ser alteradas depois.", "enviar"); return; }
            Intent pedido = new Intent(this, QuestaoActivity.class);
            pedido.putExtra("avaliacaoId", avaliacaoId);
            if (acao.startsWith("questao/")) pedido.putExtra("dados", questoes.getJSONObject(id(acao)).toString());
            startActivity(pedido);
        } catch (Exception falha) { erro(falha); }
    }

    @Override protected void executarAcao(String acao) {
        if (acao.equals("excluir")) { pedir("DELETE", "api/avaliacoes/" + avaliacaoId, null, "excluir"); return; }
        try {
            JSONArray marcadas = new JSONArray();
            for (int i = 0; i < questoes.length(); i++) {
                RadioGroup grupo = respostas.get(i);
                int selecionada = grupo.getCheckedRadioButtonId();
                Validacao.exigir(selecionada != -1, "Responda a questão " + (i + 1) + ".");
                JSONObject resposta = new JSONObject();
                resposta.put("questaoId", questoes.getJSONObject(i).getInt("id"));
                resposta.put("alternativaId", grupo.findViewById(selecionada).getTag());
                marcadas.put(resposta);
            }
            JSONObject dados = new JSONObject();
            dados.put("tentativaCurso", avaliacao.getInt("tentativaCurso"));
            dados.put("respostas", marcadas);
            pedir("POST", "api/avaliacoes/" + avaliacaoId + "/respostas", dados, "responder");
        } catch (Exception falha) { erro(falha); }
    }
}
