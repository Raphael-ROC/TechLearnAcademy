package br.com.techlearn.academy;

import android.os.Bundle;
import android.view.View;
import android.widget.*;
import android.text.InputType;
import org.json.JSONObject;

public class AvaliacaoFormActivity extends TelaBase {
    private int avaliacaoId, moduloId;
    private EditText titulo;
    private CheckBox ativo;

    @Override public void onCreate(Bundle estado) {
        super.onCreate(estado); if (isFinishing()) return;
        avaliacaoId = getIntent().getIntExtra("id", 0);
        moduloId = getIntent().getIntExtra("moduloId", 0);
        preparar(avaliacaoId == 0 ? "Adicionar avaliação" : "Editar avaliação");
        titulo = campo(corpo, "Título da avaliação", InputType.TYPE_CLASS_TEXT);
        ativo = marcar(corpo, "Avaliação ativa", true);
        ativo.setVisibility(avaliacaoId > 0 ? View.VISIBLE : View.GONE);
        botao(corpo, "Salvar avaliação", "salvar");
        botao(corpo, "Cancelar", "voltar");
        if (avaliacaoId > 0) pedir("GET", "api/avaliacoes/" + avaliacaoId, null, "avaliacao");
    }

    @Override public void onClick(View tela) {
        if (ocupado) return;
        if (acao(tela).equals("voltar")) { finish(); return; }
        try {
            Validacao.exigir(!valor(titulo).trim().isEmpty(), "Informe o título da avaliação.");
            JSONObject dados = new JSONObject();
            dados.put("titulo", valor(titulo).trim()); dados.put("moduloId", moduloId); dados.put("ativo", ativo.isChecked());
            pedir(avaliacaoId == 0 ? "POST" : "PUT", "api/avaliacoes" + (avaliacaoId == 0 ? "" : "/" + avaliacaoId), dados, "salvar");
        } catch (Exception falha) { erro(falha); }
    }

    @Override protected void receber(String operacao, String resposta) throws Exception {
        JSONObject dados = new JSONObject(resposta);
        if (operacao.equals("avaliacao")) { titulo.setText(dados.getString("titulo")); ativo.setChecked(dados.getBoolean("ativo")); moduloId = dados.getInt("moduloId"); }
        else { Toast.makeText(this, dados.getString("mensagem"), Toast.LENGTH_LONG).show(); finish(); }
    }
}
