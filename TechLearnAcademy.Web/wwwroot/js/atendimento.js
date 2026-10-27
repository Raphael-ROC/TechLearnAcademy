// Cada página usa as mesmas funções de comunicação e de mensagens de api.js.
const pagina = document.body.dataset.pagina;
let usuarioAtual;
let chamadoAtual = 0;
let ultimaConversa = "";
let atualizandoChamados = false;

function nomeTipo(tipo) {
    if (tipo === "AjudaCurso") return "Dúvida do curso";
    if (tipo === "ContatoAdministracao") return "Contato com a administração";
    return "Ouvidoria / ajuda com o gestor";
}

async function listarChamados() {
    const lista = await requisicao("/api/chamados");
    const filtro = valor("filtro-chamados");
    const area = campo("lista"); area.replaceChildren();
    let exibidos = 0;
    for (const chamado of lista) {
        const meu = chamado.solicitanteId === usuarioAtual.id || chamado.responsavelId === usuarioAtual.id;
        const disponivel = chamado.podeAtender === true;
        if (filtro === "abertos" && chamado.status === "Encerrado") continue;
        if (filtro === "meus" && !meu) continue;
        if (filtro === "disponiveis" && !disponivel) continue;
        const card = cartao("#" + chamado.id + " · " + chamado.assunto, nomeTipo(chamado.tipo));
        card.append(elemento("p", "Solicitante: " + chamado.solicitante + (chamado.curso ? " · Curso: " + chamado.curso : "")),
            elemento("p", "Responsável: " + (chamado.responsavel || "Aguardando atendimento")),
            elemento("span", chamado.status, "selo"),
            botaoAcao(chamado.solicitanteId === usuarioAtual.id ? "Acompanhar meu chamado" : "Abrir conversa", () => abrirConversa(chamado.id)));
        if (disponivel) card.append(botaoAcao("Atender chamado", async function () {
            await requisicao("/api/chamados/" + chamado.id + "/assumir", "POST");
            aviso("Você assumiu o chamado.", "sucesso");
            await listarChamados(); await abrirConversa(chamado.id);
        }));
        area.append(card); exibidos++;
    }
    if (!exibidos) vazio(area, "Nenhum chamado neste filtro.");
}

async function abrirConversa(id, automatico = false) {
    chamadoAtual = id;
    const chamado = await requisicao("/api/chamados/" + id);
    // Uma resposta atrasada não pode trocar o chamado que acabou de ser aberto.
    if (chamadoAtual !== id) return;
    const versao = JSON.stringify(chamado);
    if (automatico && chamadoAtual === id && ultimaConversa === versao) return;
    ultimaConversa = versao;
    const detalhe = campo("detalhe-chamado"); detalhe.hidden = false;
    campo("titulo-chamado").textContent = "#" + id + " · " + chamado.assunto;
    campo("dados-chamado").textContent = chamado.solicitante + " · " + nomeTipo(chamado.tipo) + " · " + chamado.status +
        " · Responsável: " + (chamado.responsavel || "Aguardando") + (chamado.curso ? " · " + chamado.curso : "");
    campo("descricao-chamado").textContent = chamado.descricao;
    const conversa = campo("conversa"); conversa.replaceChildren();
    for (const mensagem of chamado.mensagens) {
        const item = elemento("article");
        item.append(elemento("strong", mensagem.nome + " · " + mensagem.perfil),
            elemento("p", mensagem.texto), elemento("small", dataBrasileira(mensagem.data)));
        conversa.append(item);
    }
    if (!chamado.mensagens.length) vazio(conversa, "Ainda não há respostas. Acompanhe por aqui.");
    conversa.scrollTop = conversa.scrollHeight;
    campo("form-mensagem").hidden = !chamado.podeResponder;
    campo("situacao-conversa").textContent = chamado.status === "Encerrado" ? "Chamado encerrado. A conversa permanece no histórico." :
        chamado.solicitanteId === usuarioAtual.id && usuarioAtual.perfil !== "Aluno" ? "Este é o seu chamado. Acompanhe aqui o status e a resposta do responsável." :
        chamado.solicitanteId === usuarioAtual.id && !chamado.responsavelId ? "Seu chamado foi enviado. Aguarde um responsável assumir o atendimento. Você pode acompanhar o status por aqui." : "";
    const acoes = campo("acoes-chamado"); acoes.replaceChildren();
    acoes.append(botaoAcao("Atualizar conversa", () => abrirConversa(id)));
    if (chamado.podeAtender)
        acoes.append(botaoAcao("Atender chamado", async function () {
            await requisicao("/api/chamados/" + id + "/assumir", "POST"); await abrirConversa(id); await listarChamados();
        }));
    if (chamado.podeEncerrar) acoes.append(botaoAcao("Encerrar chamado", async function () {
        if (!await dialogo("Encerrar chamado", "A conversa será mantida no histórico. Deseja encerrar este atendimento?")) return;
        await requisicao("/api/chamados/" + id + "/encerrar", "POST");
        aviso("Chamado encerrado.", "sucesso"); await abrirConversa(id); await listarChamados();
    }));
    if (!automatico) detalhe.scrollIntoView({ block: "start", behavior: "smooth" });
}

async function configurarChamados() {
    if (usuarioAtual.perfil === "Administrador") {
        campo("novo-chamado").hidden = true;
        campo("destino-chamado").textContent = "";
    } else {
        campo("destino-chamado").textContent = usuarioAtual.perfil === "Gestor" ?
            "Sua solicitação será recebida pelo administrador." : "Ajuda e ouvidoria serão recebidas pelo gestor.";
        if (usuarioAtual.perfil === "Aluno") {
            campo("campo-curso").hidden = false;
            const cursos = await requisicao("/api/matriculas/minhas");
            for (const curso of cursos) {
                const opcao = elemento("option", "Ajuda com mentor · " + curso.nome); opcao.value = curso.cursoId;
                campo("curso-chamado").append(opcao);
            }
            if (cursos.some(c => c.cursoId === numeroId("curso"))) campo("curso-chamado").value = numeroId("curso");
            campo("curso-chamado").onchange = function () {
                campo("destino-chamado").textContent = Number(valor("curso-chamado")) ?
                    "Sua dúvida será recebida pelos mentores. Um deles assumirá seu chamado." : "Sua ouvidoria será recebida pelos gestores.";
            };
            campo("curso-chamado").onchange();
        }
        enviarFormulario("form-chamado", async function (form) {
            const retorno = await requisicao("/api/chamados", "POST", {
                cursoId: usuarioAtual.perfil === "Aluno" ? Number(valor("curso-chamado")) : 0,
                assunto: textoCampo("assunto"), texto: textoCampo("texto-chamado")
            });
            campo("assunto").value = ""; campo("texto-chamado").value = "";
            aviso(retorno.mensagem, "sucesso"); await listarChamados(); await abrirConversa(retorno.id);
        });
    }
    campo("filtro-chamados").onchange = () => listarChamados().catch(e => aviso(e.message, "erro"));
    campo("atualizar-lista").onclick = () => listarChamados().catch(e => aviso(e.message, "erro"));
    enviarFormulario("form-mensagem", async function (form) {
        await requisicao("/api/chamados/" + chamadoAtual + "/mensagens", "POST", { texto: textoCampo("texto-mensagem") });
        form.reset(); aviso("Mensagem enviada.", "sucesso"); await abrirConversa(chamadoAtual);
    });
    await listarChamados();
    if (numeroId()) await abrirConversa(numeroId());
    // Atualiza a fila e as respostas, preservando a mensagem em digitação.
    setInterval(async function () {
        if (document.hidden || atualizandoChamados || document.querySelector("dialog[open]")) return;
        atualizandoChamados = true;
        try { await listarChamados(); if (chamadoAtual) await abrirConversa(chamadoAtual, true); }
        catch (erro) { aviso(erro.message, "erro"); }
        finally { atualizandoChamados = false; }
    }, 5000);
}

async function listarCertificados() {
    const lista = await requisicao("/api/certificados");
    const area = campo("lista"); area.replaceChildren();
    if (!lista.length) { vazio(area, "Nenhuma solicitação ou curso matriculado encontrado."); return; }
    for (const certificado of lista) {
        const item = cartao(certificado.curso, certificado.aluno ? "Aluno: " + certificado.aluno : "");
        item.append(elemento("span", certificado.status || "Ainda não solicitado", "selo"));
        if (certificado.motivo) item.append(elemento("p", "Motivo: " + certificado.motivo));
        if (certificado.codigo) item.append(elemento("p", "Código: " + certificado.codigo));
        if (certificado.gestor) item.append(elemento("p", "Gestor: " + certificado.gestor));
        if (certificado.status === "Aprovado") {
            const link = criarLink("Ver certificado / salvar PDF", "certificado.html?id=" + certificado.id);
            link.target = "_blank"; link.rel = "noopener"; item.append(link);
        } else if (usuarioAtual.perfil === "Aluno") {
            if (certificado.status === "Pendente") item.append(elemento("p", "Aguardando análise do gestor."));
            else if (certificado.podeSolicitar) item.append(botaoAcao("Solicitar certificado", async function () {
                const resposta = await requisicao("/api/certificados", "POST", { cursoId: certificado.cursoId });
                aviso(resposta.mensagem, "sucesso"); await listarCertificados();
            }));
            else item.append(elemento("p", "Conclua todos os módulos e alcance nota 6 em cada avaliação."));
        } else if (certificado.status === "Pendente") {
            item.append(botaoAcao("Aprovar e gerar", async function () {
                if (!await dialogo("Aprovar certificado", "O sistema conferirá a aprovação em todos os módulos antes da emissão.")) return;
                const r = await requisicao("/api/certificados/" + certificado.id + "/decisao", "POST", { aprovar: true });
                aviso(r.mensagem, "sucesso"); await listarCertificados();
            }));
            item.append(botaoAcao("Recusar", async function () {
                const motivo = await dialogo("Recusar certificado", "Explique ao aluno o motivo da recusa.", true, "Enviar recusa");
                if (!motivo) return;
                const r = await requisicao("/api/certificados/" + certificado.id + "/decisao", "POST", { aprovar: false, motivo });
                aviso(r.mensagem, "sucesso"); await listarCertificados();
            }, "perigo"));
        }
        area.append(item);
    }
}

async function abrirDesempenho(alunoId) {
    const r = await requisicao("/api/relatorios/cursos/" + numeroId() + "/alunos/" + alunoId);
    campo("relatorio-aluno").hidden = false;
    campo("nome-aluno").textContent = r.nome;
    campo("situacao-aluno").textContent = "Tentativa atual do curso: " + r.tentativaCurso + ". " + (r.aprovadoCurso ? "Aprovado em todos os módulos do curso." : "Ainda há módulos ou avaliações pendentes.");
    campo("modulos-relatorio").replaceChildren();
    for (const m of r.modulos) campo("modulos-relatorio").append(cartao(m.ordem + ". " + m.titulo,
        !m.ativo ? "Módulo inativo" : m.aprovado ? "Aprovado" : "Pendente na tentativa atual do curso"));
    campo("avaliacoes-relatorio").replaceChildren();
    for (const a of r.avaliacoes) {
        const item = cartao(a.modulo + " · " + a.titulo,
            a.nota == null ? "Não realizada" : "Nota " + Number(a.nota).toFixed(2) + " · " + (a.aprovado ? "Aprovado" : "Reprovado"));
        item.append(elemento("p", "Tentativas: " + a.tentativas + (a.nota == null ? "" : " · Acertos: " + a.acertos + "/" + a.total + " · " + Number(a.percentual).toFixed(2) + "%")));
        if (!a.ativo) item.append(elemento("p", "Avaliação inativa."));
        if (a.resultadoId) item.append(criarLink("Ver respostas e gabarito", "resultado.html?id=" + a.resultadoId));
        campo("avaliacoes-relatorio").append(item);
    }
    const tabela = elemento("table"); tabela.append(elemento("caption", "Todas as tentativas deste aluno no curso"));
    const cabecalho = elemento("thead"), tr = elemento("tr");
    for (const texto of ["Data", "Tentativa do curso", "Módulo / avaliação", "Acertos", "Percentual", "Nota", "Situação", "Respostas"]) tr.append(elemento("th", texto));
    cabecalho.append(tr); tabela.append(cabecalho);
    const corpo = elemento("tbody");
    for (const h of r.historico) {
        const linha = elemento("tr");
        for (const texto of [dataBrasileira(h.data), h.tentativaCurso, h.modulo + " / " + h.titulo, h.acertos + "/" + h.total,
            Number(h.percentual).toFixed(2) + "%", Number(h.nota).toFixed(2), h.aprovado ? "Aprovado" : "Reprovado"])
            linha.append(elemento("td", texto));
        const link = elemento("td"); link.append(criarLink("Ver", "resultado.html?id=" + h.id, "")); linha.append(link); corpo.append(linha);
    }
    tabela.append(corpo);
    campo("historico-relatorio").replaceChildren(tabela);
    if (!r.historico.length) vazio(campo("historico-relatorio"), "Este aluno ainda não realizou avaliações.");
    campo("relatorio-aluno").scrollIntoView({ block: "start", behavior: "smooth" });
}

iniciar((pagina === "certificados" || pagina === "certificado") ? ["Aluno", "Gestor"] : pagina === "desempenho" ? ["Gestor"] : [], async function (usuario) {
    usuarioAtual = usuario;
    if (pagina === "chamados") document.querySelector(".introducao .subtitulo").textContent =
        usuario.perfil === "Administrador" ? "Receba e responda às solicitações dos gestores." :
        usuario.perfil === "Gestor" ? "Atenda alunos e mentores na ouvidoria. Contate o administrador pelo novo chamado." :
        usuario.perfil === "Mentor" ? "Escolha um chamado de aluno para atender. Para falar com o gestor, abra uma ouvidoria." :
        "Peça ajuda ao mentor dentro do curso ou fale com o gestor na ouvidoria.";
    if (pagina === "chamados") await configurarChamados();
    if (pagina === "certificados") {
        if (usuario.perfil === "Gestor") campo("regra-certificado").textContent = "Analise as solicitações. A aprovação emite o certificado para o aluno imprimir ou salvar em PDF.";
        campo("atualizar-certificados").onclick = () => listarCertificados().catch(e => aviso(e.message, "erro"));
        await listarCertificados();
    }
    if (pagina === "certificado") {
        const certificado = await requisicao("/api/certificados/" + numeroId());
        campo("certificado-aluno").textContent = certificado.aluno;
        campo("certificado-curso").textContent = certificado.curso;
        campo("certificado-carga").textContent = certificado.carga + " horas";
        campo("certificado-data").textContent = new Date(certificado.data).toLocaleDateString("pt-BR");
        campo("certificado-codigo").textContent = certificado.codigo;
        campo("certificado-gestor").textContent = certificado.gestor;
        campo("imprimir-certificado").disabled = false;
        campo("imprimir-certificado").onclick = () => window.print();
    }
    if (pagina === "desempenho") {
        const curso = await requisicao("/api/relatorios/cursos/" + numeroId() + "/alunos");
        campo("nome-curso").textContent = curso.nome;
        if (!curso.alunos.length) vazio(campo("lista"), "Nenhum aluno matriculado neste curso.");
        for (const aluno of curso.alunos) {
            const item = cartao(aluno.nome, aluno.email); item.append(progresso(aluno.progresso),
                botaoAcao("Ver desempenho", () => abrirDesempenho(aluno.id))); campo("lista").append(item);
        }
    }
});
