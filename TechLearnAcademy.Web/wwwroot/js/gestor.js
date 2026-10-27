const pagina = document.body.dataset.pagina;

async function todosCursos() {
    let paginaNumero = 1;
    let retorno;
    const cursos = [];
    do {
        retorno = await requisicao("/api/cursos?todos=true&pagina=" + paginaNumero);
        for (const curso of retorno.itens) cursos.push(curso);
        paginaNumero++;
    } while (paginaNumero <= retorno.totalPaginas);
    return cursos;
}

async function selecionarCursos() {
    for (const curso of await todosCursos()) {
        const opcao = elemento("option", curso.nome + (curso.ativo ? "" : " (inativo)"));
        opcao.value = curso.id;
        campo("curso-selecionado").append(opcao);
    }
}

function limparSelect(id, texto) {
    const opcao = elemento("option", texto);
    opcao.value = "";
    campo(id).replaceChildren(opcao);
}

async function carregarModulos() {
    limparSelect("modulo-selecionado", "Selecione um módulo");
    if (!valor("curso-selecionado")) return [];
    const curso = await requisicao("/api/cursos/" + valor("curso-selecionado"));
    for (const modulo of curso.modulos) {
        const opcao = elemento("option", modulo.ordem + ". " + modulo.titulo + (modulo.ativo ? "" : " (inativo)"));
        opcao.value = modulo.id;
        campo("modulo-selecionado").append(opcao);
    }
    return curso.modulos;
}

function editarBotao(tarefa) {
    const botao = elemento("button", "Editar", "secundario");
    botao.type = "button";
    botao.onclick = tarefa;
    return botao;
}

function excluirBotao(tipo, id, nome, atualizar) {
    return botaoAcao("Excluir", async function () {
        if (!await dialogo("Excluir " + tipo, "Deseja excluir " + nome + "? O conteúdo sairá do site. O histórico de notas e certificados será preservado.", false, "Excluir")) return;
        const rotas = { curso: "cursos", módulo: "modulos", material: "materiais", avaliação: "avaliacoes" };
        const retorno = await requisicao("/api/" + rotas[tipo] + "/" + id, "DELETE");
        aviso(retorno.mensagem, "sucesso"); await atualizar();
    }, "perigo");
}

async function listarCursos(paginaNumero = 1) {
    const retorno = await requisicao("/api/cursos?todos=true&busca=" + encodeURIComponent(textoCampo("busca")) + "&pagina=" + paginaNumero);
    const area = campo("lista");
    area.replaceChildren();
    if (!retorno.itens.length) vazio(area, "Nenhum curso encontrado. Cadastre o primeiro curso.");
    for (const curso of retorno.itens) {
        const item = cartao(curso.nome, curso.descricao);
        item.append(elemento("p", curso.cargaHoraria + " horas · " + (curso.ativo ? "Ativo" : "Inativo")));
        item.append(editarBotao(function () {
            campo("curso-id").value = curso.id;
            campo("nome").value = curso.nome;
            campo("descricao").value = curso.descricao;
            campo("carga").value = curso.cargaHoraria;
            campo("ativo").value = String(curso.ativo);
            campo("ativo").disabled = false;
            campo("titulo-form").textContent = "Editar curso";
            campo("nome").focus();
        }));
        item.append(excluirBotao("curso", curso.id, curso.nome, listarCursos));
        area.append(item);
    }
    paginacao(retorno, listarCursos);
}

async function configurarCursos() {
    // Novos registros são ativos; a situação muda durante a edição.
    campo("ativo").disabled = true;
    campo("cancelar").onclick = function () {
        campo("form-curso").reset();
        campo("curso-id").value = "0";
        campo("ativo").disabled = true;
        campo("titulo-form").textContent = "Novo curso";
        campo("nome").focus();
    };
    campo("form-busca").onsubmit = function (evento) {
        evento.preventDefault();
        listarCursos().catch(erro => aviso(erro.message, "erro"));
    };
    enviarFormulario("form-curso", async function () {
        const id = Number(valor("curso-id"));
        const dados = { nome: textoCampo("nome"), descricao: textoCampo("descricao"), cargaHoraria: Number(valor("carga")) };
        if (!dados.nome || !dados.descricao) throw new Error("Preencha o nome e a descrição do curso.");
        if (id) dados.ativo = valor("ativo") === "true";
        const retorno = await requisicao("/api/cursos" + (id ? "/" + id : ""), id ? "PUT" : "POST", dados);
        campo("cancelar").click();
        aviso(retorno.mensagem, "sucesso");
        await listarCursos();
    });
    await listarCursos();
}

async function listarMateriais() {
    const area = campo("materiais");
    area.replaceChildren();
    if (!valor("modulo-selecionado")) { vazio(area, "Selecione um módulo para consultar seus materiais."); return; }
    const materiais = await requisicao("/api/modulos/" + valor("modulo-selecionado") + "/materiais");
    if (!materiais.length) vazio(area, "Este módulo ainda não tem materiais.");
    for (const material of materiais) {
        const item = cartao(material.titulo, material.tipo + " · " + (material.ativo ? "Ativo" : "Inativo"));
        item.append(linkMaterial(material));
        item.append(editarBotao(function () {
            campo("material-id").value = material.id;
            campo("material-titulo").value = material.titulo;
            campo("tipo").value = material.tipo;
            campo("url").value = material.url;
            materialTemArquivo = material.arquivo;
            campo("origem-material").value = material.arquivo ? "arquivo" : "link";
            campo("arquivo-pdf").value = "";
            configurarOrigemMaterial();
            campo("material-ativo").value = String(material.ativo);
            campo("material-ativo").disabled = false;
            campo("titulo-material").textContent = "Editar material";
            campo("material-titulo").focus();
        }));
        item.append(excluirBotao("material", material.id, material.titulo, listarMateriais));
        area.append(item);
    }
}

async function listarModulos() {
    const modulos = await carregarModulos();
    campo("limite-modulos").textContent = modulos.length + " de 4 módulos cadastrados. É possível adicionar vários materiais em cada módulo.";
    const area = campo("modulos");
    area.replaceChildren();
    if (!modulos.length) vazio(area, "Nenhum módulo neste curso.");
    for (const modulo of modulos) {
        const item = cartao(modulo.ordem + ". " + modulo.titulo, modulo.ativo ? "Ativo" : "Inativo");
        item.append(editarBotao(function () {
            campo("modulo-id").value = modulo.id;
            campo("titulo").value = modulo.titulo;
            campo("ordem").value = modulo.ordem;
            campo("modulo-ativo").value = String(modulo.ativo);
            campo("modulo-ativo").disabled = false;
            campo("titulo-modulo").textContent = "Editar módulo";
            campo("titulo").focus();
        }));
        item.append(excluirBotao("módulo", modulo.id, modulo.titulo, listarModulos));
        area.append(item);
    }
    await listarMateriais();
}

let materialTemArquivo = false;

function configurarOrigemMaterial() {
    const arquivo = valor("origem-material") === "arquivo";
    campo("campo-link").hidden = arquivo;
    campo("campo-arquivo").hidden = !arquivo;
    campo("url").required = !arquivo;
    campo("arquivo-pdf").required = arquivo && !materialTemArquivo;
    campo("tipo").disabled = arquivo;
    if (arquivo) campo("tipo").value = "PDF";
}

async function configurarConteudo() {
    campo("origem-material").onchange = configurarOrigemMaterial;
    configurarOrigemMaterial();
    campo("modulo-ativo").disabled = true;
    campo("material-ativo").disabled = true;
    await selecionarCursos();
    campo("curso-selecionado").onchange = function () {
        campo("novo-modulo").click();
        campo("novo-material").click();
        listarModulos().catch(erro => aviso(erro.message, "erro"));
    };
    campo("modulo-selecionado").onchange = function () {
        campo("novo-material").click();
        listarMateriais().catch(erro => aviso(erro.message, "erro"));
    };
    campo("novo-modulo").onclick = function () {
        campo("form-modulo").reset();
        campo("modulo-id").value = "0";
        campo("modulo-ativo").disabled = true;
        campo("titulo-modulo").textContent = "Novo módulo";
    };
    campo("novo-material").onclick = function () {
        delete campo("form-material").dataset.pendente;
        campo("material-id").value = "0";
        campo("material-titulo").value = "";
        campo("url").value = "";
        campo("tipo").value = "PDF";
        campo("material-ativo").value = "true";
        campo("material-ativo").disabled = true;
        campo("titulo-material").textContent = "Novo material";
        materialTemArquivo = false;
        campo("origem-material").value = "arquivo";
        campo("arquivo-pdf").value = "";
        configurarOrigemMaterial();
    };
    enviarFormulario("form-modulo", async function () {
        const id = Number(valor("modulo-id"));
        const dados = { titulo: textoCampo("titulo"), ordem: Number(valor("ordem")) };
        if (!dados.titulo) throw new Error("Informe o título do módulo.");
        if (!valor("curso-selecionado")) throw new Error("Selecione um curso.");
        if (id) dados.ativo = valor("modulo-ativo") === "true";
        else dados.cursoId = Number(valor("curso-selecionado"));
        const retorno = await requisicao("/api/modulos" + (id ? "/" + id : ""), id ? "PUT" : "POST", dados);
        campo("novo-modulo").click();
        aviso(retorno.mensagem, "sucesso");
        await listarModulos();
    });
    enviarFormulario("form-material", async function () {
        const id = Number(valor("material-id"));
        if (!valor("modulo-selecionado")) throw new Error("Selecione um módulo.");
        const titulo = textoCampo("material-titulo");
        const ativo = !id || valor("material-ativo") === "true";
        let retorno;
        if (valor("origem-material") === "arquivo" && campo("arquivo-pdf").files.length) {
            const arquivo = campo("arquivo-pdf").files[0];
            if (arquivo.size > 10 * 1024 * 1024) throw new Error("O PDF deve ter até 10 MB.");
            const dados = new FormData();
            dados.append("arquivo", arquivo); dados.append("titulo", titulo);
            dados.append("moduloId", valor("modulo-selecionado")); dados.append("ativo", ativo);
            retorno = await requisicao(id ? "/api/materiais/" + id + "/pdf" : "/api/materiais/pdf", id ? "PUT" : "POST", dados);
        } else {
            if (valor("origem-material") === "arquivo" && !materialTemArquivo) throw new Error("Selecione um PDF.");
            const dados = { titulo, ativo, tipo: valor("tipo"), url: valor("origem-material") === "arquivo" ? "" : textoCampo("url"), moduloId: Number(valor("modulo-selecionado")) };
            retorno = await requisicao("/api/materiais" + (id ? "/" + id : ""), id ? "PUT" : "POST", dados);
        }
        campo("novo-material").click();
        aviso(retorno.mensagem, "sucesso");
        await listarMateriais();
    });
    vazio(campo("modulos"), "Selecione um curso para começar.");
    vazio(campo("materiais"), "Selecione um módulo para consultar seus materiais.");
}

let avaliacaoAtual = 0;
let contadorAlternativas = 0;

function adicionarAlternativa(texto = "", correta = false) {
    if (contadorAlternativas >= 5) { aviso("Uma questão pode ter no máximo cinco alternativas: A a E.", "erro"); return; }
    const indice = contadorAlternativas++;
    campo("adicionar-alternativa").disabled = contadorAlternativas >= 5;
    const bloco = elemento("div", undefined, "linha");
    const id = "alternativa-" + indice;
    const label = elemento("label", "Alternativa " + "ABCDE"[indice]);
    label.htmlFor = id;
    const entrada = elemento("input");
    entrada.id = id;
    entrada.type = "text";
    entrada.className = "texto-alternativa";
    entrada.value = texto;
    entrada.maxLength = 1000;
    const gabarito = elemento("label", undefined, "alternativa");
    const radio = elemento("input");
    radio.type = "radio";
    radio.name = "gabarito";
    radio.value = indice;
    radio.checked = correta;
    gabarito.append(radio, elemento("span", "Esta é a alternativa correta"));
    bloco.append(label, entrada, gabarito);
    campo("alternativas").append(bloco);
}

function novaQuestao() {
    campo("form-questao").reset();
    campo("questao-id").value = "0";
    campo("questao-ativo").disabled = true;
    campo("titulo-form-questao").textContent = "Nova questão";
    campo("alternativas").replaceChildren();
    contadorAlternativas = 0;
    campo("adicionar-alternativa").disabled = false;
    adicionarAlternativa();
    adicionarAlternativa();
}

async function abrirQuestoes(id, titulo) {
    avaliacaoAtual = id;
    campo("avaliacao-atual").textContent = titulo;
    novaQuestao();
    const avaliacao = await requisicao("/api/avaliacoes/" + id);
    const area = campo("questoes");
    area.replaceChildren();
    if (!avaliacao.questoes.length) vazio(area, "Esta avaliação ainda não tem questões.");
    for (const questao of avaliacao.questoes) {
        const item = cartao(questao.enunciado, questao.ativo === false ? "Inativa" : "Ativa");
        for (const [i, alternativa] of questao.alternativas.entries()) item.append(elemento("p", "ABCDE"[i] + ") " + (alternativa.correta ? "✓ " : "") + alternativa.texto));
        item.append(editarBotao(function () {
            campo("questao-id").value = questao.id;
            campo("enunciado").value = questao.enunciado;
            campo("questao-ativo").value = String(questao.ativo !== false);
            campo("questao-ativo").disabled = false;
            campo("titulo-form-questao").textContent = "Editar questão";
            campo("alternativas").replaceChildren();
            contadorAlternativas = 0;
            campo("adicionar-alternativa").disabled = false;
            for (const alternativa of questao.alternativas) adicionarAlternativa(alternativa.texto, alternativa.correta);
            campo("enunciado").focus();
        }));
        area.append(item);
    }
}

async function listarAvaliacoes() {
    const area = campo("avaliacoes");
    area.replaceChildren();
    avaliacaoAtual = 0;
    campo("avaliacao-atual").textContent = "Selecione uma avaliação.";
    campo("questoes").replaceChildren();
    if (!valor("modulo-selecionado")) { vazio(area, "Selecione um módulo."); return; }
    const lista = await requisicao("/api/modulos/" + valor("modulo-selecionado") + "/avaliacoes");
    if (!lista.length) vazio(area, "Este módulo ainda não possui avaliações.");
    for (const avaliacao of lista) {
        const item = cartao(avaliacao.titulo, avaliacao.ativo ? "Ativa" : "Inativa");
        item.append(editarBotao(function () {
            campo("avaliacao-id").value = avaliacao.id;
            campo("titulo").value = avaliacao.titulo;
            campo("avaliacao-ativo").value = String(avaliacao.ativo);
            campo("avaliacao-ativo").disabled = false;
            campo("titulo-form-avaliacao").textContent = "Editar avaliação";
            abrirQuestoes(avaliacao.id, avaliacao.titulo).catch(erro => aviso(erro.message, "erro"));
        }));
        const botao = elemento("button", "Questões e gabarito", "secundario");
        botao.type = "button";
        botao.onclick = () => abrirQuestoes(avaliacao.id, avaliacao.titulo).catch(erro => aviso(erro.message, "erro"));
        item.append(botao, excluirBotao("avaliação", avaliacao.id, avaliacao.titulo, listarAvaliacoes));
        area.append(item);
    }
}

async function configurarAvaliacoes() {
    campo("avaliacao-ativo").disabled = true;
    await selecionarCursos();
    novaQuestao();
    campo("curso-selecionado").onchange = async function () {
        campo("nova-avaliacao").click();
        try { await carregarModulos(); await listarAvaliacoes(); }
        catch (erro) { aviso(erro.message, "erro"); }
    };
    campo("modulo-selecionado").onchange = function () {
        campo("nova-avaliacao").click();
        listarAvaliacoes().catch(erro => aviso(erro.message, "erro"));
    };
    campo("nova-avaliacao").onclick = function () {
        campo("form-avaliacao").reset();
        campo("avaliacao-id").value = "0";
        campo("avaliacao-ativo").disabled = true;
        campo("titulo-form-avaliacao").textContent = "Nova avaliação";
    };
    campo("nova-questao").onclick = novaQuestao;
    campo("adicionar-alternativa").onclick = () => adicionarAlternativa();
    enviarFormulario("form-avaliacao", async function () {
        if (!valor("modulo-selecionado")) throw new Error("Selecione um módulo.");
        const id = Number(valor("avaliacao-id"));
        const dados = { titulo: textoCampo("titulo") };
        if (!dados.titulo) throw new Error("Informe o título da avaliação.");
        if (id) dados.ativo = valor("avaliacao-ativo") === "true";
        else dados.moduloId = Number(valor("modulo-selecionado"));
        const retorno = await requisicao("/api/avaliacoes" + (id ? "/" + id : ""), id ? "PUT" : "POST", dados);
        campo("nova-avaliacao").click();
        aviso(retorno.mensagem, "sucesso");
        await listarAvaliacoes();
    });
    enviarFormulario("form-questao", async function () {
        if (!avaliacaoAtual) throw new Error("Selecione uma avaliação.");
        const marcada = campo("alternativas").querySelector('input[name="gabarito"]:checked');
        if (!marcada) throw new Error("Marque uma alternativa correta.");
        const entradas = campo("alternativas").querySelectorAll(".texto-alternativa");
        const alternativas = [];
        for (const [indice, entrada] of Array.from(entradas).entries()) {
            const texto = entrada.value.trim();
            if (texto) alternativas.push({ texto: texto, correta: indice === Number(marcada.value) });
        }
        if (alternativas.length < 2) throw new Error("Preencha pelo menos duas alternativas.");
        if (!alternativas.some(item => item.correta)) throw new Error("A alternativa correta não pode estar vazia.");
        const id = Number(valor("questao-id"));
        const dados = { enunciado: textoCampo("enunciado"), alternativas: alternativas };
        if (!dados.enunciado) throw new Error("Escreva o enunciado.");
        if (id) dados.ativo = valor("questao-ativo") === "true";
        else dados.avaliacaoId = avaliacaoAtual;
        const retorno = await requisicao("/api/questoes" + (id ? "/" + id : ""), id ? "PUT" : "POST", dados);
        aviso(retorno.mensagem, "sucesso");
        await abrirQuestoes(avaliacaoAtual, campo("avaliacao-atual").textContent);
    });
    vazio(campo("avaliacoes"), "Selecione um curso e um módulo.");
    vazio(campo("questoes"), "Selecione uma avaliação para consultar suas questões.");
}

async function relatorio() {
    const retorno = await requisicao("/api/relatorios");
    if (!Array.isArray(retorno.cursos) || !Array.isArray(retorno.atendimentos) || !Array.isArray(retorno.chamadosMentores))
        throw new Error("A API aberta está em uma versão antiga. Pare e inicie o projeto TechLearnAcademy.Api desta solução.");
    metricas([["Cursos", retorno.quantidadeCursos], ["Matrículas", retorno.totalMatriculas],
        ["Média das tentativas", Number(retorno.mediaGeral).toFixed(1)], ["Aprovação nas tentativas", Number(retorno.aprovacao).toFixed(1) + "%"]]);
    if (pagina !== "relatorios") return;
    campo("lista").replaceChildren();
    if (!retorno.cursos.length) vazio(campo("lista"), "Nenhum curso cadastrado.");
    for (const curso of retorno.cursos) {
        const item = cartao(curso.nome, curso.matriculas + " matrículas");
        item.append(elemento("p", "Média " + Number(curso.media).toFixed(1) + " · Aprovação " + Number(curso.aprovacao).toFixed(1) + "%"),
            criarLink("Ver alunos e notas", "desempenho-curso.html?id=" + curso.cursoId));
        campo("lista").append(item);
    }
    const tabela = elemento("table"), head = elemento("thead"), linha = elemento("tr");
    tabela.append(elemento("caption", "Chamados assumidos e encerrados por mentor ou gestor"));
    for (const titulo of ["Responsável", "Perfil", "Assumidos", "Encerrados"]) linha.append(elemento("th", titulo));
    head.append(linha); tabela.append(head);
    const body = elemento("tbody");
    for (const a of retorno.atendimentos) {
        const tr = elemento("tr");
        for (const v of [a.nome, a.perfil, a.assumidos, a.encerrados]) tr.append(elemento("td", v));
        body.append(tr);
    }
    tabela.append(body); campo("atendimentos").replaceChildren(tabela);
    campo("chamados-mentores").replaceChildren();
    if (!retorno.chamadosMentores.length) vazio(campo("chamados-mentores"), "Nenhum chamado de aluno foi assumido por um mentor.");
    for (const c of retorno.chamadosMentores) {
        const item = cartao("Mentor: " + c.responsavel, "Aluno: " + c.solicitante + " · Curso: " + c.curso);
        item.append(elemento("p", "#" + c.id + " · " + c.assunto + " · " + c.status), criarLink("Ver conversa", "chamados.html?id=" + c.id));
        campo("chamados-mentores").append(item);
    }
}

iniciar(["Gestor"], async function () {
    if (pagina === "painel" || pagina === "relatorios") await relatorio();
    if (pagina === "cursos") await configurarCursos();
    if (pagina === "conteudo") await configurarConteudo();
    if (pagina === "avaliacoes") await configurarAvaliacoes();
});
