package br.com.techlearn.academy;

import android.os.Bundle;
import android.view.View;
import android.widget.*;
import android.text.InputType;
import org.json.JSONObject;
import org.json.JSONArray;

public class UsuariosActivity extends TelaBase {
    private int pagina = 1;
    private int totalPaginas = 1;
    private String busca = "";
    private EditText pesquisa;

    @Override public void onCreate(Bundle estado) { super.onCreate(estado); if (isFinishing()) return; preparar("Usuários"); }
    @Override public void onResume() { super.onResume(); carregar(); }

    private void carregar() {
        try { pedir("GET", "api/administracao/usuarios?pagina=" + pagina + "&busca=" + codificar(busca), null, "lista"); }
        catch (Exception falha) { erro(falha); }
    }

    @Override protected void receber(String operacao, String resposta) throws Exception {
        JSONObject lista = new JSONObject(resposta);
        pagina = lista.getInt("pagina");
        totalPaginas = lista.getInt("totalPaginas");
        limparCorpo();
        pesquisa = campo(corpo, "Buscar por nome ou e-mail", InputType.TYPE_CLASS_TEXT);
        pesquisa.setText(busca);
        botao(corpo, "Buscar", "buscar");
        botao(corpo, "Cadastrar usuário", "novo");
        JSONArray usuarios = lista.getJSONArray("itens");
        listaVazia(usuarios);
        for (int i = 0; i < usuarios.length(); i++) {
            JSONObject usuario = usuarios.getJSONObject(i);
            LinearLayout item = cartao(corpo, usuario.getString("nome"));
            texto(item, usuario.getString("email") + "\n" + usuario.getString("perfil") + " · " + (usuario.getBoolean("ativo") ? "Ativo" : "Inativo"), 16, false);
            botao(item, "Editar dados", "editar/" + usuario.getInt("id"));
        }
        texto(corpo, "Página " + pagina + " de " + totalPaginas, 16, false);
        botao(corpo, "Anterior", "anterior").setEnabled(pagina > 1);
        botao(corpo, "Próxima", "proxima").setEnabled(pagina < totalPaginas);
        botao(corpo, "Voltar", "voltar");
    }

    @Override public void onClick(View tela) {
        if (ocupado) return;
        String acao = acao(tela);
        if (acao.equals("voltar")) { finish(); return; }
        if (acao.equals("novo")) { abrir(UsuarioActivity.class); return; }
        if (acao.startsWith("editar/")) { abrir(UsuarioActivity.class, id(acao)); return; }
        if (acao.equals("buscar")) { busca = valor(pesquisa).trim(); pagina = 1; }
        if (acao.equals("anterior")) pagina--;
        if (acao.equals("proxima")) pagina++;
        carregar();
    }
}
