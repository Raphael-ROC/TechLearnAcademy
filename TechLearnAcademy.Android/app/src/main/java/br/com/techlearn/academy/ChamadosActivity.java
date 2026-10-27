package br.com.techlearn.academy;

import android.os.Bundle;
import android.view.View;
import android.widget.*;
import org.json.JSONObject;
import org.json.JSONArray;

public class ChamadosActivity extends TelaBase {
    private Spinner filtro;
    private LinearLayout lista;
    private JSONArray chamados = new JSONArray();

    @Override public void onCreate(Bundle estado) {
        super.onCreate(estado); if (isFinishing()) return;
        preparar("Chamados e conversas");
        filtro = opcoes(corpo, "Mostrar", new String[]{"Todos", "Abertos", "Meus atendimentos", "Disponíveis para atender"});
        botao(corpo, "Aplicar filtro / atualizar", "atualizar");
        if (!Sessao.perfil("Administrador")) botao(corpo, Sessao.perfil("Gestor") ? "Falar com o Administrador" : "Ouvidoria / ajuda com o Gestor", "novo");
        lista = new LinearLayout(this);
        lista.setOrientation(LinearLayout.VERTICAL);
        corpo.addView(lista);
        botao(corpo, "Voltar", "voltar");
    }

    @Override public void onResume() { super.onResume(); pedir("GET", "api/chamados", null, "chamados"); }

    @Override protected void receber(String operacao, String resposta) throws Exception {
        chamados = new JSONArray(resposta);
        lista.removeAllViews();
        int mostrados = 0;
        for (int i = 0; i < chamados.length(); i++) {
            JSONObject chamado = chamados.getJSONObject(i);
            int opcao = filtro.getSelectedItemPosition();
            if (opcao == 1 && chamado.getString("status").equals("Encerrado")) continue;
            if (opcao == 2 && chamado.optInt("responsavelId") != Sessao.usuario.optInt("id")) continue;
            if (opcao == 3 && !chamado.optBoolean("podeAtender")) continue;
            LinearLayout item = cartao(lista, "#" + chamado.getInt("id") + " · " + chamado.getString("assunto"));
            texto(item, "Solicitante: " + chamado.getString("solicitante") + "\nSituação: " + chamado.getString("status"), 16, false);
            if (!chamado.isNull("responsavel")) texto(item, "Responsável: " + chamado.getString("responsavel"), 15, false);
            botao(item, "Abrir conversa", "chamado/" + chamado.getInt("id"));
            mostrados++;
        }
        if (mostrados == 0) texto(lista, "Nenhum chamado neste filtro.", 16, false);
    }

    @Override public void onClick(View tela) {
        if (ocupado) return;
        String acao = acao(tela);
        if (acao.equals("voltar")) finish();
        else if (acao.equals("novo")) abrir(ChamadoFormActivity.class);
        else if (acao.startsWith("chamado/")) abrir(ConversaActivity.class, id(acao));
        else pedir("GET", "api/chamados", null, "chamados");
    }
}
