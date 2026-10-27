package br.com.techlearn.academy;

import android.os.Bundle;
import android.view.View;
import android.widget.*;
import android.text.InputType;
import org.json.JSONObject;
import org.json.JSONArray;

public class CursosActivity extends TelaBase {
    private boolean meus;
    private int pagina = 1;
    private int totalPaginas = 1;
    private String busca = "";
    private EditText pesquisa;

    @Override public void onCreate(Bundle estado) {
        super.onCreate(estado); if (isFinishing()) return;
        meus = getIntent().getBooleanExtra("meus", false);
        preparar(meus ? "Meus cursos" : Sessao.perfil("Gestor") ? "Gerenciar cursos" : "Cursos disponíveis");
    }

    @Override public void onResume() { super.onResume(); carregar(); }

    private void carregar() {
        try {
            String caminho = meus ? "api/matriculas/minhas" : "api/cursos?pagina=" + pagina + "&busca=" + codificar(busca) + "&todos=" + Sessao.perfil("Gestor");
            pedir("GET", caminho, null, "cursos");
        } catch (Exception falha) { erro(falha); }
    }

    @Override protected void receber(String operacao, String resposta) throws Exception {
        limparCorpo();
        JSONArray cursos;
        if (meus) cursos = new JSONArray(resposta);
        else {
            JSONObject lista = new JSONObject(resposta);
            cursos = lista.getJSONArray("itens");
            pagina = lista.getInt("pagina");
            totalPaginas = lista.getInt("totalPaginas");
            pesquisa = campo(corpo, "Buscar cursos", InputType.TYPE_CLASS_TEXT);
            pesquisa.setText(busca);
            botao(corpo, "Buscar", "buscar");
            if (Sessao.perfil("Gestor")) botao(corpo, "Cadastrar curso", "novo");
        }
        listaVazia(cursos);
        for (int i = 0; i < cursos.length(); i++) {
            JSONObject curso = cursos.getJSONObject(i);
            LinearLayout item = cartao(corpo, curso.getString("nome"));
            texto(item, curso.optString("descricao"), 16, false);
            texto(item, "Carga horária: " + curso.optInt("cargaHoraria") + " horas", 15, false);
            if (meus) texto(item, "Progresso: " + curso.optDouble("progresso") + "%", 16, true);
            if (Sessao.perfil("Gestor")) texto(item, curso.optBoolean("ativo") ? "Ativo" : "Inativo", 15, false);
            botao(item, "Abrir curso", "abrir/" + curso.getInt(meus ? "cursoId" : "id"));
        }
        if (!meus) {
            texto(corpo, "Página " + pagina + " de " + totalPaginas, 16, false);
            botao(corpo, "Anterior", "anterior").setEnabled(pagina > 1);
            botao(corpo, "Próxima", "proxima").setEnabled(pagina < totalPaginas);
        }
        botao(corpo, "Voltar", "voltar");
    }

    @Override public void onClick(View tela) {
        if (ocupado) return;
        String acao = acao(tela);
        if (acao.equals("voltar")) { finish(); return; }
        if (acao.equals("novo")) { abrir(CursoFormActivity.class); return; }
        if (acao.startsWith("abrir/")) { abrir(CursoActivity.class, id(acao)); return; }
        if (acao.equals("buscar")) { pagina = 1; busca = valor(pesquisa).trim(); }
        if (acao.equals("anterior")) pagina--;
        if (acao.equals("proxima")) pagina++;
        carregar();
    }
}
