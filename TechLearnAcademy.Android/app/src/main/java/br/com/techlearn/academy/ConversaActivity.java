package br.com.techlearn.academy;

import android.os.Bundle;
import android.os.Handler;
import android.os.Looper;
import android.view.View;
import android.widget.*;
import android.text.InputType;
import org.json.JSONObject;
import org.json.JSONArray;

public class ConversaActivity extends TelaBase {
    private int chamadoId;
    private TextView titulo, detalhes, historico;
    private EditText resposta;
    private Button atender, enviar, encerrar;
    private JSONObject chamado;
    private final Handler relogio = new Handler(Looper.getMainLooper());
    private final Runnable atualizar = new Runnable() {
        @Override public void run() {
            if (!ocupado) carregar();
            relogio.postDelayed(this, 5000);
        }
    };

    @Override public void onCreate(Bundle estado) {
        super.onCreate(estado); if (isFinishing()) return;
        chamadoId = getIntent().getIntExtra("id", 0);
        preparar("Conversa do chamado");
        titulo = texto(corpo, "Carregando chamado...", 22, true);
        detalhes = texto(corpo, "", 16, false);
        LinearLayout conversa = cartao(corpo, "Histórico");
        historico = texto(conversa, "", 16, false);
        historico.setTextIsSelectable(true);
        resposta = campo(corpo, "Sua mensagem", InputType.TYPE_CLASS_TEXT | InputType.TYPE_TEXT_FLAG_MULTI_LINE);
        resposta.setFilters(new android.text.InputFilter[]{new android.text.InputFilter.LengthFilter(4000)});
        atender = botao(corpo, "Atender chamado", "atender");
        enviar = botao(corpo, "Enviar mensagem", "enviar");
        encerrar = botao(corpo, "Encerrar atendimento", "encerrar");
        botao(corpo, "Atualizar conversa", "atualizar");
        botao(corpo, "Voltar", "voltar");
        resposta.setEnabled(false); atender.setEnabled(false); enviar.setEnabled(false); encerrar.setEnabled(false);
    }

    @Override public void onResume() {
        super.onResume();
        carregar();
        relogio.postDelayed(atualizar, 5000);
    }
    @Override public void onPause() { relogio.removeCallbacks(atualizar); super.onPause(); }
    private void carregar() { pedir("GET", "api/chamados/" + chamadoId, null, "conversa"); }

    @Override protected void receber(String operacao, String retorno) throws Exception {
        if (!operacao.equals("conversa")) {
            if (operacao.equals("enviar")) resposta.setText("");
            carregar();
            return;
        }
        chamado = new JSONObject(retorno);
        titulo.setText("#" + chamadoId + " · " + chamado.getString("assunto"));
        String responsavel = chamado.isNull("responsavel") ? "Aguardando atendimento" : chamado.getString("responsavel");
        detalhes.setText("Solicitante: " + chamado.getString("solicitante") + "\nResponsável: " + responsavel + "\nSituação: " + chamado.getString("status"));
        StringBuilder conversa = new StringBuilder("Descrição\n" + chamado.getString("descricao") + "\n\n");
        JSONArray mensagens = chamado.getJSONArray("mensagens");
        for (int i = 0; i < mensagens.length(); i++) {
            JSONObject item = mensagens.getJSONObject(i);
            conversa.append(item.getString("nome")).append(" · ").append(item.getString("perfil")).append("\n")
                .append(item.getString("data").replace("T", " ")).append("\n").append(item.getString("texto")).append("\n\n");
        }
        if (mensagens.length() == 0) conversa.append("Ainda não há respostas.");
        // A atualização altera o histórico e preserva a resposta em digitação.
        historico.setText(conversa.toString());
        atender.setEnabled(chamado.optBoolean("podeAtender"));
        enviar.setEnabled(chamado.optBoolean("podeResponder"));
        resposta.setEnabled(chamado.optBoolean("podeResponder"));
        encerrar.setEnabled(chamado.optBoolean("podeEncerrar"));
        aviso(chamado.optString("status").equals("Encerrado") ? "Atendimento encerrado. O histórico permanece disponível." :
            chamado.optBoolean("podeAtender") ? "Você pode assumir este chamado." : "Conversa atualizada.");
    }

    @Override public void onClick(View tela) {
        if (ocupado) return;
        String acao = acao(tela);
        if (acao.equals("voltar")) { finish(); return; }
        if (acao.equals("atualizar")) { carregar(); return; }
        if (acao.equals("atender")) { pedir("POST", "api/chamados/" + chamadoId + "/assumir", null, "atender"); return; }
        if (acao.equals("encerrar")) { confirmar("Encerrar este atendimento? O histórico será mantido.", "encerrar"); return; }
        try {
            Validacao.exigir(!valor(resposta).trim().isEmpty(), "Escreva uma mensagem.");
            JSONObject dados = new JSONObject(); dados.put("texto", valor(resposta).trim());
            resposta.setEnabled(false);
            pedir("POST", "api/chamados/" + chamadoId + "/mensagens", dados, "enviar");
        } catch (Exception falha) { erro(falha); }
    }

    @Override protected void executarAcao(String acao) { pedir("POST", "api/chamados/" + chamadoId + "/encerrar", null, "encerrar"); }
    @Override protected void falhou(Exception falha) {
        if (chamado != null) resposta.setEnabled(chamado.optBoolean("podeResponder"));
        super.falhou(falha);
    }
}
