package br.com.techlearn.academy;

import android.os.Bundle;
import android.view.View;
import android.widget.*;
import android.text.InputType;
import org.json.JSONObject;

public class CursoFormActivity extends TelaBase {
    private int cursoId;
    private EditText nome, descricao, carga;
    private CheckBox ativo;

    @Override public void onCreate(Bundle estado) {
        super.onCreate(estado); if (isFinishing()) return;
        cursoId = getIntent().getIntExtra("id", 0);
        preparar(cursoId == 0 ? "Cadastrar curso" : "Editar curso");
        nome = campo(corpo, "Nome do curso", InputType.TYPE_CLASS_TEXT);
        descricao = campo(corpo, "Descrição", InputType.TYPE_CLASS_TEXT | InputType.TYPE_TEXT_FLAG_MULTI_LINE);
        carga = campo(corpo, "Carga horária em horas", InputType.TYPE_CLASS_NUMBER);
        ativo = marcar(corpo, "Curso ativo", true);
        ativo.setVisibility(cursoId > 0 ? View.VISIBLE : View.GONE);
        botao(corpo, "Salvar curso", "salvar");
        botao(corpo, "Cancelar", "voltar");
        if (cursoId > 0) pedir("GET", "api/cursos/" + cursoId, null, "curso");
    }

    @Override protected void receber(String operacao, String resposta) throws Exception {
        JSONObject dados = new JSONObject(resposta);
        if (operacao.equals("curso")) {
            nome.setText(dados.getString("nome"));
            descricao.setText(dados.getString("descricao"));
            carga.setText(String.valueOf(dados.getInt("cargaHoraria")));
            ativo.setChecked(dados.getBoolean("ativo"));
        } else {
            Toast.makeText(this, dados.getString("mensagem"), Toast.LENGTH_LONG).show();
            finish();
        }
    }

    @Override public void onClick(View tela) {
        if (ocupado) return;
        if (acao(tela).equals("voltar")) { finish(); return; }
        try {
            Validacao.exigir(!valor(nome).trim().isEmpty() && !valor(descricao).trim().isEmpty(), "Informe o nome e a descrição do curso.");
            Validacao.exigir(!valor(carga).isEmpty(), "Informe a carga horária.");
            JSONObject dados = new JSONObject();
            dados.put("nome", valor(nome).trim());
            dados.put("descricao", valor(descricao).trim());
            dados.put("cargaHoraria", Integer.parseInt(valor(carga)));
            dados.put("ativo", ativo.isChecked());
            pedir(cursoId == 0 ? "POST" : "PUT", "api/cursos" + (cursoId == 0 ? "" : "/" + cursoId), dados, "salvar");
        } catch (NumberFormatException falha) { erro(new Exception("Informe uma carga horária válida.")); }
        catch (Exception falha) { erro(falha); }
    }
}
