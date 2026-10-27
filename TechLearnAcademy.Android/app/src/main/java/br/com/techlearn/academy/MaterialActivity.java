package br.com.techlearn.academy;

import android.os.Bundle;
import android.view.View;
import android.widget.*;
import android.text.InputType;
import android.content.Intent;
import android.net.Uri;
import org.json.JSONObject;

public class MaterialActivity extends TelaBase {
    private int materialId, moduloId;
    private boolean arquivoExistente;
    private Uri arquivo;
    private EditText titulo, url;
    private TextView nomeArquivo;
    private Spinner tipo;
    private CheckBox ativo;

    @Override public void onCreate(Bundle estado) {
        super.onCreate(estado); if (isFinishing()) return;
        moduloId = getIntent().getIntExtra("moduloId", 0);
        preparar("Material de estudo");
        titulo = campo(corpo, "Título", InputType.TYPE_CLASS_TEXT);
        tipo = opcoes(corpo, "Tipo", new String[]{"Link", "PDF"});
        url = campo(corpo, "Endereço do material (para links e PDFs externos)", InputType.TYPE_CLASS_TEXT | InputType.TYPE_TEXT_VARIATION_URI);
        texto(corpo, "Para enviar um documento, selecione PDF e escolha um arquivo de até 10 MB.", 15, false);
        nomeArquivo = texto(corpo, "Nenhum arquivo selecionado.", 15, false);
        botao(corpo, "Selecionar PDF", "arquivo");
        ativo = marcar(corpo, "Material ativo", true);
        try {
            String recebido = getIntent().getStringExtra("dados");
            if (recebido != null) {
                JSONObject dados = new JSONObject(recebido);
                materialId = dados.getInt("id");
                titulo.setText(dados.getString("titulo"));
                tipo.setSelection(dados.getString("tipo").equals("PDF") ? 1 : 0);
                url.setText(dados.optString("url"));
                ativo.setChecked(dados.getBoolean("ativo"));
                arquivoExistente = dados.optBoolean("arquivo");
                if (arquivoExistente) nomeArquivo.setText("PDF atual: " + dados.optString("arquivoNome") + ". Se não escolher outro, o arquivo será mantido.");
                botao(corpo, "Excluir material", "excluir");
            } else ativo.setVisibility(View.GONE);
        } catch (Exception falha) { erro(falha); }
        botao(corpo, "Salvar material", "salvar");
        botao(corpo, "Cancelar", "voltar");
    }

    @Override public void onClick(View tela) {
        if (ocupado) return;
        String acao = acao(tela);
        if (acao.equals("voltar")) { finish(); return; }
        if (acao.equals("excluir")) { confirmar("Retirar este material das telas?", "excluir"); return; }
        if (acao.equals("arquivo")) {
            Intent pedido = new Intent(Intent.ACTION_OPEN_DOCUMENT);
            pedido.addCategory(Intent.CATEGORY_OPENABLE);
            pedido.setType("application/pdf");
            startActivityForResult(pedido, 1);
            return;
        }
        try {
            Validacao.exigir(!valor(titulo).trim().isEmpty(), "Informe o título do material.");
            JSONObject dados = new JSONObject();
            dados.put("moduloId", moduloId); dados.put("titulo", valor(titulo).trim()); dados.put("ativo", ativo.isChecked());
            boolean pdf = tipo.getSelectedItem().toString().equals("PDF");
            if (pdf && arquivo != null) {
                String caminho = materialId == 0 ? "api/materiais/pdf" : "api/materiais/" + materialId + "/pdf";
                pedirPdf(materialId == 0 ? "POST" : "PUT", caminho, dados, arquivo, "salvar");
            } else {
                Validacao.exigir(!valor(url).trim().isEmpty() || (pdf && arquivoExistente), "Informe o endereço ou selecione um PDF.");
                dados.put("tipo", pdf ? "PDF" : "Link");
                dados.put("url", valor(url).trim());
                pedir(materialId == 0 ? "POST" : "PUT", "api/materiais" + (materialId == 0 ? "" : "/" + materialId), dados, "salvar");
            }
        } catch (Exception falha) { erro(falha); }
    }

    @Override protected void onActivityResult(int pedido, int resultado, Intent dados) {
        super.onActivityResult(pedido, resultado, dados);
        if (pedido == 1 && resultado == RESULT_OK && dados != null) {
            arquivo = dados.getData();
            tipo.setSelection(1);
            nomeArquivo.setText("PDF selecionado. O tamanho e o conteúdo serão conferidos no envio.");
        }
    }

    @Override protected void executarAcao(String acao) { pedir("DELETE", "api/materiais/" + materialId, null, "excluir"); }
    @Override protected void receber(String operacao, String resposta) throws Exception {
        Toast.makeText(this, new JSONObject(resposta).getString("mensagem"), Toast.LENGTH_LONG).show();
        finish();
    }
}
