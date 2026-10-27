package br.com.techlearn.academy;

import android.os.Bundle;
import android.view.View;
import android.widget.LinearLayout;
import android.content.Intent;
import android.net.Uri;
import org.json.JSONObject;
import org.json.JSONArray;

public class ModuloActivity extends TelaBase {
    private int moduloId;
    private JSONArray materiais;

    @Override public void onCreate(Bundle estado) { super.onCreate(estado); if (isFinishing()) return; moduloId = getIntent().getIntExtra("id", 0); preparar("Materiais e avaliações"); }
    @Override public void onResume() { super.onResume(); pedir("GET", "api/modulos/" + moduloId + "/materiais", null, "materiais"); }

    @Override protected void receber(String operacao, String resposta) throws Exception {
        if (operacao.equals("materiais")) {
            materiais = new JSONArray(resposta);
            limparCorpo();
            texto(corpo, "Materiais de estudo", 22, true);
            if (Sessao.perfil("Gestor")) botao(corpo, "Adicionar material", "novoMaterial");
            listaVazia(materiais);
            for (int i = 0; i < materiais.length(); i++) {
                JSONObject material = materiais.getJSONObject(i);
                LinearLayout item = cartao(corpo, material.getString("titulo"));
                texto(item, material.getString("tipo"), 15, false);
                botao(item, "Abrir material", "material/" + i);
                if (Sessao.perfil("Gestor")) {
                    texto(item, material.optBoolean("ativo") ? "Ativo" : "Inativo", 15, false);
                    botao(item, "Editar material", "editarMaterial/" + i);
                }
            }
            pedir("GET", "api/modulos/" + moduloId + "/avaliacoes", null, "avaliacoes");
            return;
        }
        texto(corpo, "Avaliações", 22, true);
        if (Sessao.perfil("Gestor")) botao(corpo, "Adicionar avaliação", "novaAvaliacao");
        JSONArray avaliacoes = new JSONArray(resposta);
        listaVazia(avaliacoes);
        for (int i = 0; i < avaliacoes.length(); i++) {
            JSONObject avaliacao = avaliacoes.getJSONObject(i);
            LinearLayout item = cartao(corpo, avaliacao.getString("titulo"));
            if (avaliacao.optInt("resultadoId") > 0) {
                texto(item, "Nota: " + avaliacao.optDouble("nota"), 16, true);
                botao(item, "Ver resultado", "resultado/" + avaliacao.optInt("resultadoId"));
            } else if (Sessao.perfil("Aluno") && !avaliacao.optBoolean("podeResponder")) {
                texto(item, "Avaliação bloqueada. Confira a situação do curso.", 16, false);
            }
            if (Sessao.perfil("Gestor") || avaliacao.optBoolean("podeResponder"))
                botao(item, Sessao.perfil("Gestor") ? "Gerenciar avaliação" : "Responder avaliação", "avaliacao/" + avaliacao.getInt("id"));
        }
        botao(corpo, "Voltar", "voltar");
    }

    @Override public void onClick(View tela) {
        if (ocupado) return;
        String acao = acao(tela);
        try {
            if (acao.equals("voltar")) { finish(); return; }
            if (acao.startsWith("avaliacao/")) { abrir(AvaliacaoActivity.class, id(acao)); return; }
            if (acao.startsWith("resultado/")) { abrir(ResultadosActivity.class, id(acao)); return; }
            if (acao.equals("novaAvaliacao")) {
                Intent pedido = new Intent(this, AvaliacaoFormActivity.class);
                pedido.putExtra("moduloId", moduloId); startActivity(pedido); return;
            }
            if (acao.equals("novoMaterial") || acao.startsWith("editarMaterial/")) {
                Intent pedido = new Intent(this, MaterialActivity.class);
                pedido.putExtra("moduloId", moduloId);
                if (acao.startsWith("editarMaterial/")) pedido.putExtra("dados", materiais.getJSONObject(id(acao)).toString());
                startActivity(pedido); return;
            }
            JSONObject material = materiais.getJSONObject(id(acao));
            if (material.optBoolean("arquivo")) abrir(PdfActivity.class, material.getInt("id"));
            else {
                Uri endereco = Uri.parse(material.getString("url"));
                Validacao.exigir(("https".equals(endereco.getScheme()) || "http".equals(endereco.getScheme())) && endereco.getHost() != null, "Endereço de material inválido.");
                startActivity(new Intent(Intent.ACTION_VIEW, endereco));
            }
        } catch (android.content.ActivityNotFoundException falha) { erro(new Exception("Não há um aplicativo disponível para abrir este material.")); }
        catch (Exception falha) { erro(falha); }
    }
}
