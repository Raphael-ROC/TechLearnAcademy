package br.com.techlearn.academy;

import android.os.Bundle;
import android.view.View;
import android.widget.*;
import android.content.Intent;
import org.json.JSONObject;
import org.json.JSONArray;

public class CursoActivity extends TelaBase {
    private int cursoId;
    private JSONObject curso;
    private JSONArray modulos;

    @Override public void onCreate(Bundle estado) { super.onCreate(estado); if (isFinishing()) return; cursoId = getIntent().getIntExtra("id", 0); preparar("Curso"); }
    @Override public void onResume() { super.onResume(); carregar(); }
    private void carregar() { pedir("GET", "api/cursos/" + cursoId, null, "curso"); }

    @Override protected void receber(String operacao, String resposta) throws Exception {
        if (operacao.equals("excluir")) { Toast.makeText(this, new JSONObject(resposta).getString("mensagem"), Toast.LENGTH_LONG).show(); finish(); return; }
        if (!operacao.equals("curso")) { carregar(); return; }
        curso = new JSONObject(resposta);
        modulos = curso.getJSONArray("modulos");
        limparCorpo();
        texto(corpo, curso.getString("nome"), 24, true);
        texto(corpo, curso.getString("descricao"), 17, false);
        texto(corpo, "Carga horária: " + curso.getInt("cargaHoraria") + " horas", 16, false);
        if (Sessao.perfil("Gestor")) {
            botao(corpo, "Editar curso", "editar");
            botao(corpo, "Adicionar módulo", "novo");
            texto(corpo, "O curso pode ter até quatro módulos.", 15, false);
            botao(corpo, "Excluir curso", "excluir");
        } else if (!curso.optBoolean("matriculado")) {
            botao(corpo, "Matricular-me", "matricular");
        } else {
            botao(corpo, "Ajuda com mentor", "ajuda");
            if (curso.optBoolean("reprovado")) {
                texto(corpo, "Houve reprovação. Reinicie o curso para fazer uma nova tentativa.", 16, true);
                botao(corpo, "Reiniciar curso", "reiniciar");
            }
        }
        texto(corpo, "Módulos", 21, true);
        listaVazia(modulos);
        for (int i = 0; i < modulos.length(); i++) {
            JSONObject modulo = modulos.getJSONObject(i);
            LinearLayout item = cartao(corpo, modulo.getInt("ordem") + ". " + modulo.getString("titulo"));
            if (Sessao.perfil("Aluno")) texto(item, modulo.optBoolean("concluido") ? "Concluído" : "Em estudo", 15, false);
            else texto(item, modulo.optBoolean("ativo") ? "Ativo" : "Inativo", 15, false);
            if (Sessao.perfil("Gestor") || curso.optBoolean("matriculado"))
                botao(item, "Materiais e avaliações", "modulo/" + i);
            if (Sessao.perfil("Gestor")) botao(item, "Editar módulo", "editarModulo/" + i);
        }
        botao(corpo, "Atualizar", "atualizar");
        botao(corpo, "Voltar", "voltar");
    }

    @Override public void onClick(View tela) {
        if (ocupado) return;
        String acao = acao(tela);
        try {
            if (acao.equals("voltar")) { finish(); return; }
            if (acao.equals("atualizar")) { carregar(); return; }
            if (acao.equals("editar")) { abrir(CursoFormActivity.class, cursoId); return; }
            if (acao.equals("excluir")) { confirmar("Excluir este curso e retirar seu conteúdo das telas? O histórico será mantido.", "excluir"); return; }
            if (acao.equals("reiniciar")) { confirmar("Zerar o progresso de todos os módulos e iniciar uma nova tentativa? As notas anteriores ficam no histórico.", "reiniciar"); return; }
            if (acao.equals("matricular")) { JSONObject dados = new JSONObject(); dados.put("cursoId", cursoId); pedir("POST", "api/matriculas", dados, "matricular"); return; }
            if (acao.equals("ajuda")) {
                Intent pedido = new Intent(this, ChamadoFormActivity.class); pedido.putExtra("cursoId", cursoId); startActivity(pedido); return;
            }
            Intent pedido;
            if (acao.equals("novo")) pedido = new Intent(this, ModuloFormActivity.class);
            else {
                JSONObject modulo = modulos.getJSONObject(id(acao));
                pedido = new Intent(this, acao.startsWith("editarModulo/") ? ModuloFormActivity.class : ModuloActivity.class);
                pedido.putExtra("id", modulo.getInt("id"));
                pedido.putExtra("dados", modulo.toString());
            }
            pedido.putExtra("cursoId", cursoId);
            startActivity(pedido);
        } catch (Exception falha) { erro(falha); }
    }

    @Override protected void executarAcao(String acao) {
        if (acao.equals("excluir")) pedir("DELETE", "api/cursos/" + cursoId, null, "excluir");
        if (acao.equals("reiniciar")) pedir("POST", "api/cursos/" + cursoId + "/reiniciar", null, "reiniciar");
    }
}
