const botaoApk = document.getElementById("baixar-apk");
const mensagemDownload = document.getElementById("mensagem-download");
let baixando = false;

botaoApk.addEventListener("click", async function (evento) {
    evento.preventDefault();
    if (baixando) return;
    baixando = true;
    botaoApk.setAttribute("aria-disabled", "true");
    botaoApk.textContent = "Preparando APK...";
    mensagemDownload.textContent = "Preparando o arquivo para baixar.";
    try {
        const resposta = await fetch(botaoApk.href);
        if (!resposta.ok) throw new Error("Falha no download.");
        // Informa ao navegador o tipo correto do instalador Android.
        const arquivo = new Blob([await resposta.arrayBuffer()], {
            type: "application/vnd.android.package-archive"
        });
        const endereco = URL.createObjectURL(arquivo);
        const link = document.createElement("a");
        link.href = endereco;
        link.download = "TechLearnAcademy-Android.apk";
        document.body.appendChild(link);
        link.click();
        link.remove();
        setTimeout(function () { URL.revokeObjectURL(endereco); }, 60000);
        mensagemDownload.textContent = "Download solicitado. Abra o APK no celular para instalar.";
    } catch (erro) {
        mensagemDownload.textContent = "Não foi possível baixar o APK. Tente novamente ou utilize o arquivo ZIP.";
    } finally {
        baixando = false;
        botaoApk.removeAttribute("aria-disabled");
        botaoApk.textContent = "Baixar APK Android";
    }
});
