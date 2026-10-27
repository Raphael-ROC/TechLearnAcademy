package br.com.techlearn.academy;

import android.os.Bundle;
import android.view.View;
import android.widget.*;
import android.app.AlertDialog;
import android.content.Intent;
import android.content.DialogInterface;
import android.graphics.*;
import android.graphics.pdf.PdfDocument;
import org.json.JSONObject;
import org.json.JSONArray;
import java.io.OutputStream;

public class CertificadosActivity extends TelaBase {
    private int certificadoId;
    private JSONObject certificado;

    @Override public void onCreate(Bundle estado) {
        super.onCreate(estado); if (isFinishing()) return;
        certificadoId = getIntent().getIntExtra("id", 0);
        preparar("Certificados");
    }

    @Override public void onResume() { super.onResume(); carregar(); }
    private void carregar() { pedir("GET", "api/certificados" + (certificadoId > 0 ? "/" + certificadoId : ""), null, "certificados"); }

    @Override protected void receber(String operacao, String resposta) throws Exception {
        if (!operacao.equals("certificados")) { Toast.makeText(this, new JSONObject(resposta).getString("mensagem"), Toast.LENGTH_LONG).show(); carregar(); return; }
        limparCorpo();
        if (certificadoId > 0) {
            certificado = new JSONObject(resposta);
            texto(corpo, "Certificado de conclusão", 25, true);
            texto(corpo, "Certificamos que", 18, false);
            texto(corpo, certificado.getString("aluno"), 24, true);
            texto(corpo, "concluiu o curso", 18, false);
            texto(corpo, certificado.getString("curso"), 23, true);
            texto(corpo, "Carga horária: " + certificado.getInt("carga") + " horas\nGestor: " + certificado.getString("gestor") +
                "\nEmissão: " + certificado.getString("data").replace("T", " ") + "\nCódigo: " + certificado.getString("codigo"), 17, false);
            botao(corpo, "Salvar certificado em PDF", "pdf");
        } else {
            JSONArray lista = new JSONArray(resposta);
            listaVazia(lista);
            for (int i = 0; i < lista.length(); i++) {
                JSONObject item = lista.getJSONObject(i);
                LinearLayout cartao = cartao(corpo, item.getString("curso"));
                String status = item.isNull("status") ? "Ainda não solicitado" : item.optString("status");
                if (Sessao.perfil("Gestor")) texto(cartao, "Aluno: " + item.optString("aluno"), 17, true);
                texto(cartao, "Situação: " + status, 16, false);
                if (!item.isNull("motivo") && !item.optString("motivo").isEmpty()) texto(cartao, "Motivo: " + item.getString("motivo"), 16, false);
                if (status.equals("Aprovado")) botao(cartao, "Ver certificado", "abrir/" + item.getInt("id"));
                if (Sessao.perfil("Aluno") && item.optBoolean("podeSolicitar") && (status.equals("Recusado") || status.equals("Ainda não solicitado")))
                    botao(cartao, "Solicitar certificado", "solicitar/" + item.getInt("cursoId"));
                if (Sessao.perfil("Aluno") && !item.optBoolean("podeSolicitar") && !status.equals("Aprovado"))
                    texto(cartao, "Conclua e seja aprovado em todos os módulos para solicitar.", 15, false);
                if (Sessao.perfil("Gestor") && status.equals("Pendente")) {
                    botao(cartao, "Aprovar certificado", "aprovar/" + item.getInt("id"));
                    botao(cartao, "Recusar com motivo", "recusar/" + item.getInt("id"));
                }
            }
            botao(corpo, "Atualizar", "atualizar");
        }
        botao(corpo, "Voltar", "voltar");
    }

    @Override public void onClick(View tela) {
        if (ocupado) return;
        final String acao = acao(tela);
        if (acao.equals("voltar")) { finish(); return; }
        if (acao.equals("atualizar")) { carregar(); return; }
        if (acao.startsWith("abrir/")) { abrir(CertificadosActivity.class, id(acao)); return; }
        if (acao.startsWith("solicitar/")) { confirmar("Enviar a solicitação de certificado ao Gestor?", acao); return; }
        if (acao.startsWith("aprovar/")) { confirmar("Aprovar a emissão? A API conferirá novamente os requisitos.", acao); return; }
        if (acao.startsWith("recusar/")) {
            final EditText motivo = new EditText(this);
            motivo.setHint("Motivo da recusa");
            new AlertDialog.Builder(this).setTitle("Recusar certificado").setView(motivo)
                .setNegativeButton("Cancelar", null)
                .setPositiveButton("Recusar", new DialogInterface.OnClickListener() {
                    @Override public void onClick(DialogInterface dialogo, int botao) {
                        try {
                            String texto = motivo.getText().toString().trim();
                            Validacao.exigir(texto.length() >= 3 && texto.length() <= 1000, "O motivo deve ter de 3 a 1000 caracteres.");
                            JSONObject dados = new JSONObject(); dados.put("aprovar", false); dados.put("motivo", texto);
                            pedir("POST", "api/certificados/" + id(acao) + "/decisao", dados, "decisao");
                        } catch (Exception falha) { erro(falha); }
                    }
                }).show();
            return;
        }
        if (acao.equals("pdf")) {
            Intent pedido = new Intent(Intent.ACTION_CREATE_DOCUMENT);
            pedido.addCategory(Intent.CATEGORY_OPENABLE); pedido.setType("application/pdf");
            pedido.putExtra(Intent.EXTRA_TITLE, "Certificado-" + certificado.optString("codigo") + ".pdf");
            startActivityForResult(pedido, 1);
        }
    }

    @Override protected void executarAcao(String acao) {
        try {
            JSONObject dados = new JSONObject();
            if (acao.startsWith("solicitar/")) {
                dados.put("cursoId", id(acao));
                pedir("POST", "api/certificados", dados, "solicitar");
            } else {
                dados.put("aprovar", true);
                pedir("POST", "api/certificados/" + id(acao) + "/decisao", dados, "decisao");
            }
        } catch (Exception falha) { erro(falha); }
    }

    @Override protected void onActivityResult(int pedido, int resultado, Intent dados) {
        super.onActivityResult(pedido, resultado, dados);
        if (pedido != 1 || resultado != RESULT_OK || dados == null) return;
        // O PDF usa somente os dados do certificado aprovado pela API.
        PdfDocument documento = new PdfDocument();
        try (OutputStream saida = getContentResolver().openOutputStream(dados.getData())) {
            PdfDocument.Page pagina = documento.startPage(new PdfDocument.PageInfo.Builder(842, 595, 1).create());
            Canvas tela = pagina.getCanvas();
            Paint pincel = new Paint(Paint.ANTI_ALIAS_FLAG);
            tela.drawColor(Color.WHITE);
            pincel.setColor(VERDE); pincel.setStyle(Paint.Style.STROKE); pincel.setStrokeWidth(3);
            tela.drawRect(24, 24, 818, 571, pincel);
            pincel.setStyle(Paint.Style.FILL); pincel.setColor(AZUL);
            linha(tela, pincel, "TechLearn Academy", 90, 30);
            linha(tela, pincel, "CERTIFICADO DE CONCLUSÃO", 153, 27);
            linha(tela, pincel, "Certificamos que", 208, 17);
            linha(tela, pincel, certificado.getString("aluno"), 251, 27);
            linha(tela, pincel, "concluiu o curso", 299, 17);
            linha(tela, pincel, certificado.getString("curso"), 343, 25);
            linha(tela, pincel, "Carga horária: " + certificado.getInt("carga") + " horas", 389, 18);
            linha(tela, pincel, "Emissão: " + certificado.getString("data").substring(0, 10), 434, 15);
            linha(tela, pincel, "Gestor: " + certificado.getString("gestor"), 465, 16);
            linha(tela, pincel, "Código: " + certificado.getString("codigo"), 523, 13);
            documento.finishPage(pagina);
            documento.writeTo(saida);
            aviso("Certificado salvo em PDF no local escolhido.");
        } catch (Exception falha) { erro(falha); }
        finally { documento.close(); }
    }

    private void linha(Canvas tela, Paint pincel, String texto, int altura, int tamanho) {
        pincel.setTextSize(tamanho);
        while (pincel.measureText(texto) > 738 && pincel.getTextSize() > 10) pincel.setTextSize(pincel.getTextSize() - 1);
        tela.drawText(texto, (842 - pincel.measureText(texto)) / 2, altura, pincel);
    }
}
