package br.com.techlearn.academy;

import android.os.Bundle;
import android.view.View;
import android.widget.LinearLayout;
import org.json.JSONObject;
import org.json.JSONArray;

public class ResultadosActivity extends TelaBase {
    private int resultadoId;

    @Override public void onCreate(Bundle estado) {
        super.onCreate(estado); if (isFinishing()) return;
        resultadoId = getIntent().getIntExtra("id", 0);
        preparar(resultadoId == 0 ? "Resultados" : "Resultado da avaliação");
        pedir("GET", "api/resultados" + (resultadoId == 0 ? "" : "/" + resultadoId), null, "resultados");
    }

    @Override protected void receber(String operacao, String resposta) throws Exception {
        limparCorpo();
        if (resultadoId == 0) {
            JSONArray resultados = new JSONArray(resposta);
            listaVazia(resultados);
            for (int i = 0; i < resultados.length(); i++) {
                JSONObject resultado = resultados.getJSONObject(i);
                LinearLayout item = cartao(corpo, resultado.getString("titulo"));
                resumo(item, resultado);
                texto(item, "Tentativa do curso: " + resultado.optInt("tentativaCurso"), 15, false);
                botao(item, "Ver respostas", "resultado/" + resultado.getInt("id"));
            }
        } else {
            JSONObject resultado = new JSONObject(resposta);
            texto(corpo, resultado.getString("titulo"), 23, true);
            resumo(corpo, resultado);
            texto(corpo, "Tentativa do curso: " + resultado.optInt("tentativaCurso"), 16, false);
            JSONArray respostas = resultado.getJSONArray("respostas");
            for (int i = 0; i < respostas.length(); i++) {
                JSONObject item = respostas.getJSONObject(i);
                LinearLayout cartao = cartao(corpo, (i + 1) + ". " + item.getString("enunciado"));
                texto(cartao, "Sua resposta: " + item.getString("texto"), 16, false);
                texto(cartao, item.optBoolean("correta") ? "Correta" : "Incorreta. Resposta correta: " + item.getString("respostaCorreta"), 16, true);
            }
        }
        botao(corpo, "Voltar", "voltar");
    }

    private void resumo(LinearLayout pai, JSONObject resultado) {
        texto(pai, "Nota: " + resultado.optDouble("nota") + " · " + (resultado.optBoolean("aprovado") ? "Aprovado" : "Reprovado") +
            "\nAcertos: " + resultado.optInt("acertos") + " de " + resultado.optInt("total"), 17, true);
    }

    @Override public void onClick(View tela) {
        if (ocupado) return;
        if (acao(tela).equals("voltar")) finish();
        else abrir(ResultadosActivity.class, id(acao(tela)));
    }
}
