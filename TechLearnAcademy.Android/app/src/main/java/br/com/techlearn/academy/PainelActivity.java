package br.com.techlearn.academy;

import android.os.Bundle;
import android.view.View;
import android.content.Intent;
import org.json.JSONObject;
import org.json.JSONArray;

public class PainelActivity extends TelaBase {
    private boolean saindo;

    @Override public void onCreate(Bundle estado) {
        super.onCreate(estado); if (isFinishing()) return;
        preparar("Painel");
    }

    @Override public void onResume() {
        super.onResume();
        if (!Sessao.token.isEmpty()) carregar();
    }

    private void carregar() {
        String rota = "api/mentor/alunos";
        if (Sessao.perfil("Aluno")) rota = "api/aluno/painel";
        if (Sessao.perfil("Gestor")) rota = "api/relatorios";
        if (Sessao.perfil("Administrador")) rota = "api/administracao/painel";
        pedir("GET", rota, null, "painel");
    }

    @Override protected void receber(String operacao, String resposta) throws Exception {
        if (operacao.equals("sair")) { voltarAoLogin(); return; }
        if (operacao.equals("banco")) { aviso(new JSONObject(resposta).getString("mensagem")); return; }
        limparCorpo();
        texto(corpo, "Olá, " + Sessao.usuario.optString("nome"), 23, true);
        texto(corpo, "Perfil: " + Sessao.usuario.optString("perfil"), 16, false);
        if (Sessao.perfil("Aluno")) {
            JSONObject painel = new JSONObject(resposta);
            texto(corpo, "Cursos matriculados: " + painel.optInt("cursosMatriculados") + "\nProgresso geral: " + painel.optDouble("progressoGeral") + "%", 18, false);
            botao(corpo, "Cursos disponíveis", "cursos");
            botao(corpo, "Meus cursos", "meus");
            botao(corpo, "Resultados das avaliações", "resultados");
            botao(corpo, "Certificados", "certificados");
            botao(corpo, "Orientações dos mentores", "mentoria");
        } else if (Sessao.perfil("Mentor")) {
            texto(corpo, "Alunos acompanhados: " + new JSONArray(resposta).length(), 18, false);
            botao(corpo, "Alunos e orientações", "mentoria");
        } else if (Sessao.perfil("Gestor")) {
            JSONObject painel = new JSONObject(resposta);
            texto(corpo, "Cursos: " + painel.optInt("quantidadeCursos") + "\nMatrículas: " + painel.optInt("totalMatriculas"), 18, false);
            botao(corpo, "Gerenciar cursos e conteúdo", "cursos");
            botao(corpo, "Usuários", "usuarios");
            botao(corpo, "Certificados", "certificados");
            botao(corpo, "Relatórios", "relatorios");
        } else if (Sessao.perfil("Administrador")) {
            JSONObject painel = new JSONObject(resposta);
            texto(corpo, "Usuários ativos: " + painel.optInt("ativos") + "\nUsuários inativos: " + painel.optInt("inativos"), 18, false);
            JSONArray perfis = painel.optJSONArray("perfis");
            if (perfis != null) for (int i = 0; i < perfis.length(); i++) {
                JSONObject perfil = perfis.getJSONObject(i);
                texto(corpo, perfil.getString("perfil") + ": " + perfil.getInt("quantidade"), 16, false);
            }
            botao(corpo, "Gerenciar usuários", "usuarios");
            botao(corpo, "Verificar banco pela API", "banco");
        }
        botao(corpo, "Chamados e conversas", "chamados");
        botao(corpo, "Meu perfil", "perfil");
        botao(corpo, "Atualizar painel", "atualizar");
        botao(corpo, "Sair", "sair");
    }

    @Override public void onClick(View tela) {
        if (ocupado) return;
        String acao = acao(tela);
        if (acao.equals("cursos")) abrir(CursosActivity.class);
        if (acao.equals("meus")) {
            Intent pedido = new Intent(this, CursosActivity.class);
            pedido.putExtra("meus", true);
            startActivity(pedido);
        }
        if (acao.equals("resultados")) abrir(ResultadosActivity.class);
        if (acao.equals("certificados")) abrir(CertificadosActivity.class);
        if (acao.equals("mentoria")) abrir(MentoriaActivity.class);
        if (acao.equals("usuarios")) abrir(UsuariosActivity.class);
        if (acao.equals("relatorios")) abrir(RelatoriosActivity.class);
        if (acao.equals("chamados")) abrir(ChamadosActivity.class);
        if (acao.equals("perfil")) abrir(PerfilActivity.class);
        if (acao.equals("atualizar")) carregar();
        if (acao.equals("banco")) pedir("GET", "api/status/banco", null, "banco");
        if (acao.equals("sair")) { saindo = true; pedir("POST", "api/usuarios/logout", null, "sair"); }
    }

    @Override protected void falhou(Exception falha) {
        if (saindo) { voltarAoLogin(); return; }
        super.falhou(falha);
        if (corpo.getChildCount() == 0) { botao(corpo, "Tentar novamente", "atualizar"); botao(corpo, "Sair", "sair"); }
    }
}
