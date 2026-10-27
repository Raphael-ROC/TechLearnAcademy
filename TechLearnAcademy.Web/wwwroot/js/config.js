// Vazio usa a mesma origem da API que hospeda o site.
// Na prévia Web separada, informe aqui a URL HTTPS da API.
const URL_API = "";

// O VLibras adiciona o botão de tradução para Libras em todas as páginas.
const scriptLibras = document.createElement("script");
scriptLibras.src = "https://vlibras.gov.br/app/vlibras-plugin.js";
scriptLibras.async = true;
document.head.append(scriptLibras);

