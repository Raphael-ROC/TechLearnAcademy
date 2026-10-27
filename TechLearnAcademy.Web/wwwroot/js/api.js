// Funções comuns evitam repetir a comunicação e as mensagens.
function campo(id) { return document.getElementById(id); }

function elemento(tag, texto, classe) {
    const item = document.createElement(tag);
    if (texto !== undefined) item.textContent = texto;
    if (classe) item.className = classe;
    return item;
}

function aviso(texto, tipo = "informacao") {
    const mensagem = campo("mensagem");
    mensagem.textContent = texto;
    mensagem.className = "mensagem " + tipo;
    mensagem.setAttribute("role", tipo === "erro" ? "alert" : "status");
    if (tipo === "erro") {
        mensagem.tabIndex = -1;
        mensagem.focus({ preventScroll: true });
        mensagem.scrollIntoView({ block: "nearest", behavior: "smooth" });
    }
}

async function requisicao(caminho, metodo = "GET", dados) {
    const opcoes = { method: metodo, credentials: "include", cache: "no-store" };
    if (dados !== undefined) {
        if (dados instanceof FormData) opcoes.body = dados;
        else {
            opcoes.headers = { "Content-Type": "application/json" };
            opcoes.body = JSON.stringify(dados);
        }
    }
    let resposta;
    try { resposta = await fetch(URL_API + caminho, opcoes); }
    catch { throw new Error("Não foi possível conectar ao servidor. Verifique se a API está ligada."); }
    const texto = await resposta.text();
    let retorno = {};
    if (texto) {
        try { retorno = JSON.parse(texto); }
        catch { throw new Error("O servidor devolveu uma resposta inesperada."); }
    }
    if (!resposta.ok) {
        const erro = new Error(retorno.mensagem || "Não foi possível concluir a solicitação.");
        erro.status = resposta.status;
        throw erro;
    }
    return retorno;
}

function criarLink(texto, destino, classe = "botao secundario") {
    const link = elemento("a", texto, classe);
    link.href = destino;
    return link;
}

function cartao(titulo, texto) {
    const item = elemento("article", undefined, "cartao");
    item.append(elemento("h2", titulo));
    if (texto) item.append(elemento("p", texto));
    return item;
}

function vazio(area, texto = "Nenhum registro encontrado.") {
    area.replaceChildren(elemento("p", texto, "vazio"));
}

function progresso(valor) {
    const numero = Number(valor) || 0;
    const bloco = elemento("div", undefined, "progresso");
    const barra = elemento("progress");
    barra.max = 100;
    barra.value = numero;
    barra.setAttribute("aria-label", "Progresso: " + numero.toFixed(1) + "%");
    bloco.append(barra, elemento("span", numero.toFixed(1) + "% concluído"));
    return bloco;
}

function numeroId(nome = "id") {
    return Number(new URLSearchParams(location.search).get(nome)) || 0;
}

function dataBrasileira(valor) {
    // O SQL guarda datas em UTC; valores sem fuso recebem a indicação Z.
    const texto = String(valor);
    const utc = /Z$|[+-]\d{2}:\d{2}$/.test(texto) ? texto : texto + "Z";
    return new Date(utc).toLocaleString("pt-BR", { timeZone: "America/Sao_Paulo" });
}

function valor(id) { return campo(id).value; }
function textoCampo(id) { return valor(id).trim(); }

// Os textos vindos da API são exibidos com textContent, sem executar HTML.
function metricas(dados) {
    const area = campo("metricas");
    area.replaceChildren();
    for (const dado of dados) {
        const item = cartao(dado[0]);
        item.append(elemento("strong", dado[1], "numero"));
        area.append(item);
    }
}

function destinoPerfil(perfil) {
    if (perfil === "Aluno") return "painel-aluno.html";
    if (perfil === "Mentor") return "painel-mentor.html";
    if (perfil === "Gestor") return "painel-gestor.html";
    if (perfil === "Administrador") return "painel-administrador.html";
    return "";
}

function menu(perfil) {
    const links = [[destinoPerfil(perfil), "Visão geral"]];
    if (perfil === "Aluno") links.push(["cursos.html", "Catálogo"], ["meus-cursos.html", "Meus cursos"], ["resultado.html", "Resultados"], ["certificados.html", "Certificados"]);
    if (perfil === "Mentor") links.push(["alunos-mentor.html", "Alunos atendidos"]);
    if (perfil === "Gestor") links.push(["gerenciar-cursos.html", "Cursos"], ["gerenciar-conteudo.html", "Conteúdo"], ["gerenciar-avaliacoes.html", "Avaliações"], ["relatorios.html", "Relatórios"], ["certificados.html", "Certificados"], ["usuarios.html", "Usuários"]);
    if (perfil === "Administrador") links.push(["usuarios.html", "Usuários"]);
    if (perfil === "Administrador" || perfil === "Gestor") links.push(["cadastrar-usuario.html", "Cadastrar usuário"]);
    links.push(["chamados.html", perfil === "Aluno" || perfil === "Mentor" ? "Ajuda e ouvidoria" : "Chamados"], ["perfil.html", "Meu perfil"]);
    const area = campo("menu");
    for (const link of links) {
        const item = criarLink(link[1], link[0], "");
        if (location.pathname.endsWith(link[0])) item.setAttribute("aria-current", "page");
        area.append(item);
    }
}

// Esta conferência organiza a navegação. A API controla as permissões reais.
async function iniciar(perfis, tarefa) {
    const area = campo("conteudo");
    if (area) area.inert = true;
    aviso("Carregando...");
    try {
        const usuario = await requisicao("/api/usuarios/perfil");
        menu(usuario.perfil);
        campo("nome-usuario").textContent = usuario.nome + " · " + usuario.perfil;
        campo("sair").addEventListener("click", async function () {
            this.disabled = true;
            try {
                await requisicao("/api/usuarios/logout", "POST");
                location.href = "index.html";
            } catch (erro) { aviso(erro.message, "erro"); this.disabled = false; }
        });
        if (perfis.length && !perfis.includes(usuario.perfil)) {
            aviso("Seu perfil não tem acesso a esta página.", "erro");
            campo("conteudo").hidden = true;
            return;
        }
        await tarefa(usuario);
        if (campo("mensagem").textContent === "Carregando...") aviso("");
    } catch (erro) {
        if (erro.status === 401) {
            location.href = "index.html";
            return;
        }
        aviso(erro.message, "erro");
    } finally {
        if (area) area.inert = false;
    }
}

// O botão volta ao estado normal mesmo quando o servidor falha.
function enviarFormulario(id, tarefa, texto = "Salvando...") {
    const formulario = campo(id);
    formulario.addEventListener("submit", async function (evento) {
        evento.preventDefault();
        if (!formulario.reportValidity()) return;
        const botao = formulario.querySelector('button[type="submit"]');
        const anterior = botao.textContent;
        botao.disabled = true;
        botao.textContent = texto;
        aviso(texto);
        try { await tarefa(formulario); delete formulario.dataset.pendente; aplicarAtualizacao(); }
        catch (erro) { aviso(erro.message, "erro"); }
        finally { botao.disabled = false; botao.textContent = anterior; }
    });
}

function verificarSenha(senha, confirmacao) {
    if (Array.from(senha).length < 8) throw new Error("A senha deve ter pelo menos 8 caracteres.");
    if (/\s/u.test(senha)) throw new Error("A senha não pode conter espaços.");
    // As regras Unicode também aceitam letras e números de outros idiomas.
    if (!/\p{Lu}/u.test(senha)) throw new Error("Inclua pelo menos uma letra maiúscula.");
    if (!/\p{N}/u.test(senha)) throw new Error("Inclua pelo menos um número.");
    if (!/[^\p{L}\p{N}\s]/u.test(senha)) throw new Error("Inclua pelo menos um caractere especial.");
    if (senha !== confirmacao) throw new Error("A confirmação deve ser exatamente igual à senha.");
}

function verificarEmail(email) {
    if (email.length > 254 || !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email)) throw new Error("Digite um e-mail válido de até 254 caracteres.");
}

function verificarNome(nome) {
    const tamanho = nome.length;
    if (tamanho < 2 || tamanho > 100) throw new Error("O nome deve ter de 2 a 100 caracteres.");
}

function paginacao(retorno, carregar) {
    const area = campo("paginacao");
    area.replaceChildren();
    if (retorno.totalPaginas < 2) return;
    const anterior = elemento("button", "Anterior", "secundario");
    anterior.disabled = retorno.pagina <= 1;
    anterior.onclick = () => carregar(retorno.pagina - 1).catch(erro => aviso(erro.message, "erro"));
    const proximo = elemento("button", "Próxima", "secundario");
    proximo.disabled = retorno.pagina >= retorno.totalPaginas;
    proximo.onclick = () => carregar(retorno.pagina + 1).catch(erro => aviso(erro.message, "erro"));
    area.append(anterior, elemento("span", "Página " + retorno.pagina + " de " + retorno.totalPaginas), proximo);
}

function linkMaterial(material) {
    if (material.arquivo) {
        const link = criarLink("PDF · " + material.titulo, URL_API + "/api/materiais/" + material.id + "/arquivo", "material");
        link.target = "_blank"; link.rel = "noopener";
        return link;
    }
    const url = new URL(material.url);
    if (url.protocol !== "https:" && url.protocol !== "http:") throw new Error("O material tem um endereço inválido.");
    const link = criarLink(material.tipo + " · " + material.titulo, url.href, "material");
    link.target = "_blank";
    link.rel = "noopener noreferrer";
    return link;
}

function listarResultados(lista, area) {
    area.replaceChildren();
    if (!lista.length) { vazio(area, "Você ainda não tem avaliações realizadas."); return; }
    for (const resultado of lista) {
        const item = cartao(resultado.titulo, "Nota " + Number(resultado.nota).toFixed(1) + " · " + (resultado.aprovado ? "Aprovado" : "Não aprovado"));
        item.append(criarLink("Ver resultado", "resultado.html?id=" + resultado.id));
        area.append(item);
    }
}


// Confirmação e motivo usam a mesma janela, com contraste e foco visíveis.
function dialogo(titulo, texto, pedirTexto = false, confirmar = "Confirmar") {
    return new Promise(function (resolver) {
        const janela = elemento("dialog", undefined, "dialogo");
        const form = elemento("form");
        const cabecalho = elemento("h2", titulo);
        cabecalho.id = "titulo-dialogo"; janela.setAttribute("aria-labelledby", cabecalho.id);
        form.append(cabecalho, elemento("p", texto));
        let entrada;
        if (pedirTexto) {
            const label = elemento("label", "Motivo"); label.htmlFor = "texto-dialogo";
            entrada = elemento("textarea"); entrada.id = label.htmlFor;
            entrada.required = true; entrada.minLength = 3; entrada.maxLength = 1000;
            form.append(label, entrada);
        }
        const acoes = elemento("div", undefined, "acoes");
        const cancelar = elemento("button", "Cancelar", "secundario");
        cancelar.type = "button"; cancelar.onclick = () => janela.close();
        cancelar.hidden = confirmar === "Entendi";
        const aceitar = elemento("button", confirmar); aceitar.type = "submit";
        acoes.append(cancelar, aceitar); form.append(acoes); janela.append(form);
        form.onsubmit = function (evento) {
            evento.preventDefault();
            if (entrada && entrada.value.trim().length < 3) {
                entrada.setCustomValidity("Escreva um motivo com pelo menos 3 caracteres."); entrada.reportValidity(); return;
            }
            janela.close("confirmado");
        };
        if (entrada) entrada.oninput = () => entrada.setCustomValidity("");
        janela.onclose = function () {
            resolver(janela.returnValue === "confirmado" ? (entrada ? entrada.value.trim() : true) : null); janela.remove();
        };
        document.body.append(janela); janela.showModal(); (entrada || (cancelar.hidden ? aceitar : cancelar)).focus();
    });
}

function botaoAcao(texto, tarefa, classe = "secundario") {
    const botao = elemento("button", texto, classe); botao.type = "button";
    botao.onclick = async function () {
        botao.disabled = true;
        try { await tarefa(); } catch (erro) { aviso(erro.message, "erro"); }
        finally { botao.disabled = false; }
    };
    return botao;
}

// O servidor informa a versão dos arquivos. Uma página aberta recebe as mudanças.
let versaoPagina = document.querySelector('meta[name="versao-site"]')?.content;
let atualizacaoDisponivel = false;
document.addEventListener("input", function (evento) {
    if (evento.target.form) evento.target.form.dataset.pendente = "true";
});
document.addEventListener("reset", function (evento) {
    delete evento.target.dataset.pendente;
});
function aplicarAtualizacao() {
    if (!atualizacaoDisponivel) return;
    const digitando = document.querySelector('form[data-pendente="true"]');
    if (!digitando && !document.querySelector("dialog[open]")) location.reload();
    else if (!campo("aviso-atualizacao")) {
        const avisoAtualizacao = elemento("aside", "O site recebeu uma atualização. Salve o que está digitando para aplicá-la automaticamente. ", "mensagem informacao");
        avisoAtualizacao.id = "aviso-atualizacao"; avisoAtualizacao.setAttribute("role", "status");
        const botao = elemento("button", "Atualizar agora", "secundario");
        botao.onclick = () => location.reload(); avisoAtualizacao.append(botao);
        document.body.prepend(avisoAtualizacao);
    }
}
async function conferirAtualizacao() {
    if (!versaoPagina || document.hidden) return;
    try {
        const resposta = await fetch("/versao-site", { cache: "no-store" });
        if (!resposta.ok) return;
        const nova = (await resposta.json()).versao;
        if (nova !== versaoPagina) { atualizacaoDisponivel = true; aplicarAtualizacao(); }
    } catch { /* Uma reinicialização breve do servidor não interrompe a página. */ }
}
setInterval(conferirAtualizacao, 5000);
document.addEventListener("visibilitychange", conferirAtualizacao);
window.addEventListener("pageshow", conferirAtualizacao);

