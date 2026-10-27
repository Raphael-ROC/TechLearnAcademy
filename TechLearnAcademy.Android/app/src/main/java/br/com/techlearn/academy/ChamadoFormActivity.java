package br.com.techlearn.academy;

import android.os.Bundle;
import android.view.View;
import android.widget.*;
import android.text.InputType;
import org.json.JSONObject;

public class ChamadoFormActivity extends TelaBase {
    private int cursoId;
    private EditText assunto, mensagemChamado;

    @Override public void onCreate(Bundle estado) {
        super.onCreate(estado); if (isFinishing()) return;
        cursoId = getIntent().getIntExtra("cursoId", 0);
        preparar(cursoId > 0 ? "Ajuda com Mentor" : Sessao.perfil("Gestor") ? "Contato com Administrador" : "Ouvidoria com Gestor");
        texto(corpo, "A solicitação será encaminhada à fila do responsável. A conversa será liberada quando alguém assumir o chamado.", 16, false);
        assunto = campo(corpo, "Assunto", InputType.TYPE_CLASS_TEXT);
        mensagemChamado = campo(corpo, "Mensagem", InputType.TYPE_CLASS_TEXT | InputType.TYPE_TEXT_FLAG_MULTI_LINE);
        mensagemChamado.setMinLines(4);
        botao(corpo, "Enviar chamado", "enviar");
        botao(corpo, "Cancelar", "voltar");
    }

    @Override public void onClick(View tela) {
        if (ocupado) return;
        if (acao(tela).equals("voltar")) { finish(); return; }
        try {
            Validacao.exigir(valor(assunto).trim().length() >= 3 && valor(assunto).trim().length() <= 150, "O assunto deve ter de 3 a 150 caracteres.");
            Validacao.exigir(valor(mensagemChamado).trim().length() >= 3 && valor(mensagemChamado).trim().length() <= 4000, "A mensagem deve ter de 3 a 4000 caracteres.");
            JSONObject dados = new JSONObject();
            dados.put("cursoId", cursoId); dados.put("assunto", valor(assunto).trim()); dados.put("texto", valor(mensagemChamado).trim());
            pedir("POST", "api/chamados", dados, "enviar");
        } catch (Exception falha) { erro(falha); }
    }

    @Override protected void receber(String operacao, String resposta) throws Exception {
        JSONObject chamado = new JSONObject(resposta);
        Toast.makeText(this, chamado.getString("mensagem"), Toast.LENGTH_LONG).show();
        abrir(ConversaActivity.class, chamado.getInt("id"));
        finish();
    }
}
