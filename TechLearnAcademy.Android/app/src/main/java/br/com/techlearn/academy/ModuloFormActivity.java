package br.com.techlearn.academy;

import android.os.Bundle;
import android.view.View;
import android.widget.*;
import android.text.InputType;
import org.json.JSONObject;

public class ModuloFormActivity extends TelaBase {
    private int moduloId, cursoId;
    private EditText titulo, ordem;
    private CheckBox ativo;

    @Override public void onCreate(Bundle estado) {
        super.onCreate(estado); if (isFinishing()) return;
        moduloId = getIntent().getIntExtra("id", 0);
        cursoId = getIntent().getIntExtra("cursoId", 0);
        preparar(moduloId == 0 ? "Adicionar módulo" : "Editar módulo");
        titulo = campo(corpo, "Título do módulo", InputType.TYPE_CLASS_TEXT);
        ordem = campo(corpo, "Ordem", InputType.TYPE_CLASS_NUMBER);
        ativo = marcar(corpo, "Módulo ativo", true);
        ativo.setVisibility(moduloId > 0 ? View.VISIBLE : View.GONE);
        if (moduloId > 0) {
            try {
                JSONObject modulo = new JSONObject(getIntent().getStringExtra("dados"));
                titulo.setText(modulo.getString("titulo"));
                ordem.setText(String.valueOf(modulo.getInt("ordem")));
                ativo.setChecked(modulo.getBoolean("ativo"));
            } catch (Exception falha) { erro(falha); }
            botao(corpo, "Excluir módulo", "excluir");
        }
        botao(corpo, "Salvar módulo", "salvar");
        botao(corpo, "Cancelar", "voltar");
    }

    @Override public void onClick(View tela) {
        if (ocupado) return;
        if (acao(tela).equals("voltar")) { finish(); return; }
        if (acao(tela).equals("excluir")) { confirmar("Excluir este módulo e seu conteúdo das telas? O histórico será mantido.", "excluir"); return; }
        try {
            Validacao.exigir(!valor(titulo).trim().isEmpty(), "Informe o título do módulo.");
            Validacao.exigir(!valor(ordem).isEmpty(), "Informe a ordem do módulo.");
            JSONObject dados = new JSONObject();
            dados.put("cursoId", cursoId); dados.put("titulo", valor(titulo).trim());
            dados.put("ordem", Integer.parseInt(valor(ordem))); dados.put("ativo", ativo.isChecked());
            pedir(moduloId == 0 ? "POST" : "PUT", "api/modulos" + (moduloId == 0 ? "" : "/" + moduloId), dados, "salvar");
        } catch (NumberFormatException falha) { erro(new Exception("Informe uma ordem válida.")); }
        catch (Exception falha) { erro(falha); }
    }

    @Override protected void executarAcao(String acao) { pedir("DELETE", "api/modulos/" + moduloId, null, "excluir"); }
    @Override protected void receber(String operacao, String resposta) throws Exception {
        Toast.makeText(this, new JSONObject(resposta).getString("mensagem"), Toast.LENGTH_LONG).show();
        finish();
    }
}
