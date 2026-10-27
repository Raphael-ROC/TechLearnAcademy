package br.com.techlearn.academy;

import android.os.Bundle;
import android.view.View;
import android.widget.*;
import android.text.InputType;
import org.json.JSONObject;
import org.json.JSONArray;

public class QuestaoActivity extends TelaBase {
    private int questaoId, avaliacaoId;
    private EditText enunciado;
    private final EditText[] alternativas = new EditText[5];
    private Spinner correta;
    private CheckBox ativo;

    @Override public void onCreate(Bundle estado) {
        super.onCreate(estado); if (isFinishing()) return;
        avaliacaoId = getIntent().getIntExtra("avaliacaoId", 0);
        preparar("Questão da avaliação");
        enunciado = campo(corpo, "Enunciado", InputType.TYPE_CLASS_TEXT | InputType.TYPE_TEXT_FLAG_MULTI_LINE);
        texto(corpo, "Preencha de duas a cinco alternativas, na ordem. As letras são identificadas automaticamente.", 16, false);
        for (int i = 0; i < alternativas.length; i++)
            alternativas[i] = campo(corpo, "Alternativa " + (char)('A' + i), InputType.TYPE_CLASS_TEXT | InputType.TYPE_TEXT_FLAG_MULTI_LINE);
        correta = opcoes(corpo, "Alternativa correta", new String[]{"A", "B", "C", "D", "E"});
        ativo = marcar(corpo, "Questão ativa", true);
        try {
            String recebido = getIntent().getStringExtra("dados");
            if (recebido != null) {
                JSONObject questao = new JSONObject(recebido);
                questaoId = questao.getInt("id"); enunciado.setText(questao.getString("enunciado"));
                ativo.setChecked(questao.getBoolean("ativo"));
                JSONArray itens = questao.getJSONArray("alternativas");
                for (int i = 0; i < itens.length(); i++) {
                    alternativas[i].setText(itens.getJSONObject(i).getString("texto"));
                    if (itens.getJSONObject(i).optBoolean("correta")) correta.setSelection(i);
                }
            } else ativo.setVisibility(View.GONE);
        } catch (Exception falha) { erro(falha); }
        texto(corpo, "A API protege o histórico de avaliações já respondidas. Para retirar uma questão, desmarque Questão ativa.", 15, false);
        botao(corpo, "Salvar questão", "salvar");
        botao(corpo, "Cancelar", "voltar");
    }

    @Override public void onClick(View tela) {
        if (ocupado) return;
        if (acao(tela).equals("voltar")) { finish(); return; }
        try {
            Validacao.exigir(!valor(enunciado).trim().isEmpty(), "Informe o enunciado.");
            JSONArray itens = new JSONArray();
            boolean encontrouVazia = false;
            for (int i = 0; i < alternativas.length; i++) {
                String texto = valor(alternativas[i]).trim();
                if (texto.isEmpty()) { encontrouVazia = true; continue; }
                Validacao.exigir(!encontrouVazia, "Preencha as alternativas em sequência, sem pular letras.");
                JSONObject alternativa = new JSONObject();
                alternativa.put("texto", texto); alternativa.put("correta", correta.getSelectedItemPosition() == i);
                itens.put(alternativa);
            }
            Validacao.exigir(itens.length() >= 2, "Preencha pelo menos duas alternativas.");
            Validacao.exigir(correta.getSelectedItemPosition() < itens.length(), "Selecione uma alternativa correta que esteja preenchida.");
            JSONObject dados = new JSONObject();
            dados.put("avaliacaoId", avaliacaoId); dados.put("enunciado", valor(enunciado).trim());
            dados.put("ativo", ativo.isChecked()); dados.put("alternativas", itens);
            pedir(questaoId == 0 ? "POST" : "PUT", "api/questoes" + (questaoId == 0 ? "" : "/" + questaoId), dados, "salvar");
        } catch (Exception falha) { erro(falha); }
    }

    @Override protected void receber(String operacao, String resposta) throws Exception {
        Toast.makeText(this, new JSONObject(resposta).getString("mensagem"), Toast.LENGTH_LONG).show();
        finish();
    }
}
