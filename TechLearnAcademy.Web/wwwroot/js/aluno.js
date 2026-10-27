const pagina = document.body.dataset.pagina;

function botaoReiniciarCurso(cursoId) {
    return botaoAcao("Reiniciar curso", async function () {
        if (!await dialogo("Reiniciar curso", "O progresso de todos os módulos será zerado. Você começará uma nova tentativa do curso e poderá responder cada avaliação uma vez. As notas anteriores continuarão no histórico. Deseja reiniciar?", false, "Reiniciar curso")) return;
        await requisicao("/api/cursos/" + cursoId + "/reiniciar", "POST");
        location.href = "curso.html?id=" + cursoId;
    });
}

function cardCurso(curso, mostrarProgresso = false) {
    const item = cartao(curso.nome, curso.descricao);
    item.append(elemento("p", curso.cargaHoraria + " horas"));
    if (mostrarProgresso) item.append(progresso(curso.progresso));
    if (curso.reprovado) item.append(elemento("p", "Você reprovou em uma avaliação. Reinicie o curso para tentar novamente."));
    item.append(criarLink(mostrarProgresso ? "Continuar estudando" : "Ver curso", "curso.html?id=" + (curso.cursoId || curso.id)));
    return item;
}

async function catalogo(paginaNumero = 1) {
    const retorno = await requisicao("/api/cursos?busca=" + encodeURIComponent(textoCampo("busca")) + "&pagina=" + paginaNumero);
    const area = campo("lista");
    area.replaceChildren();
    if (!retorno.itens.length) vazio(area, "Nenhum curso disponível para esta pesquisa.");
    for (const curso of retorno.itens) area.append(cardCurso(curso));
    paginacao(retorno, catalogo);
}

async function detalheCurso() {
    const curso = await requisicao("/api/cursos/" + numeroId());
    const area = campo("detalhes");
    area.replaceChildren(elemento("h2", curso.nome), elemento("p", curso.descricao), elemento("p", curso.cargaHoraria + " horas"));
    area.append(elemento("span", curso.matriculado ? "Você está matriculado" : "Matrícula disponível", "selo"));
    const acoes = campo("acoes");
    acoes.replaceChildren();
    if (!curso.matriculado) {
        const botao = elemento("button", "Matricular-se");
        botao.onclick = async function () {
            botao.disabled = true;
            try {
                const retorno = await requisicao("/api/matriculas", "POST", { cursoId: curso.id });
                aviso(retorno.mensagem, "sucesso");
                await detalheCurso();
            } catch (erro) { aviso(erro.message, "erro"); }
            finally { botao.disabled = false; }
        };
        acoes.append(botao);
    }
    if (curso.matriculado) acoes.append(criarLink("Ajuda com mentor", "chamados.html?curso=" + curso.id), criarLink("Certificado do curso", "certificados.html?curso=" + curso.id));
    if (curso.reprovado) acoes.append(elemento("p", "Você reprovou em uma avaliação. Para fazer novas avaliações, reinicie o curso e revise todos os módulos."), botaoReiniciarCurso(curso.id));
    const lista = campo("lista");
    lista.replaceChildren();
    if (!curso.modulos.length) vazio(lista, "Este curso ainda não possui módulos disponíveis.");
    for (const modulo of curso.modulos) {
        const item = cartao(modulo.ordem + ". " + modulo.titulo);
        item.append(elemento("span", modulo.concluido ? "Concluído" : "Próxima etapa", "selo"));
        if (curso.matriculado) {
            item.append(criarLink("Estudar módulo", "estudar.html?id=" + modulo.id + "&curso=" + curso.id));
            const materiais = await requisicao("/api/modulos/" + modulo.id + "/materiais");
            for (const material of materiais) item.append(linkMaterial(material));
        } else item.append(elemento("p", "Matricule-se para acessar os materiais e as avaliações."));
        lista.append(item);
    }
}

async function estudar() {
    const curso = await requisicao("/api/cursos/" + numeroId("curso"));
    const modulo = curso.modulos.find(item => item.id === numeroId());
    if (!modulo) throw new Error("Módulo não encontrado neste curso.");
    campo("detalhes").append(elemento("h2", modulo.titulo), elemento("p", curso.nome), criarLink("Voltar ao curso", "curso.html?id=" + curso.id));
    const materiais = await requisicao("/api/modulos/" + modulo.id + "/materiais");
    const area = campo("materiais");
    if (!materiais.length) vazio(area, "Nenhum material disponível neste módulo.");
    for (const material of materiais) area.append(linkMaterial(material));
    campo("detalhes").append(criarLink("Pedir ajuda ao mentor", "chamados.html?curso=" + curso.id),
        elemento("p", "Você pode responder cada avaliação uma única vez por tentativa do curso. A nota mínima é 6. Se reprovar, será necessário reiniciar o curso inteiro."));
    if (curso.reprovado) campo("detalhes").append(elemento("p", "Novas avaliações estão bloqueadas por reprovação. Seus materiais continuam disponíveis para revisão."), botaoReiniciarCurso(curso.id));
    const botao = campo("concluir");
    if (modulo.concluido) { botao.disabled = true; botao.textContent = "Módulo concluído"; }
    if (curso.reprovado) { botao.disabled = true; botao.textContent = "Reinicie o curso para continuar"; }
    botao.onclick = async function () {
        botao.disabled = true;
        try {
            const retorno = await requisicao("/api/progresso", "POST", { moduloId: modulo.id });
            botao.textContent = "Módulo concluído";
            aviso(retorno.mensagem, "sucesso");
        } catch (erro) { aviso(erro.message, "erro"); botao.disabled = false; }
    };
    const avaliacoes = await requisicao("/api/modulos/" + modulo.id + "/avaliacoes");
    const lista = campo("lista");
    if (!avaliacoes.length) vazio(lista, "Nenhuma avaliação disponível neste módulo.");
    for (const avaliacao of avaliacoes) {
        const item = cartao(avaliacao.titulo, avaliacao.nota == null ? "Ainda não realizada nesta tentativa do curso" : "Nota: " + Number(avaliacao.nota).toFixed(2) + (avaliacao.aprovado ? " · Aprovado" : " · Reprovado: é necessário reiniciar o curso"));
        if (avaliacao.podeResponder) item.append(criarLink("Responder avaliação", "avaliacao.html?id=" + avaliacao.id));
        else if (avaliacao.resultadoId) item.append(criarLink("Ver resultado", "resultado.html?id=" + avaliacao.resultadoId));
        else item.append(elemento("p", "Avaliação bloqueada. Reinicie o curso após a reprovação."));
        lista.append(item);
    }
}

async function abrirAvaliacao() {
    const avaliacao = await requisicao("/api/avaliacoes/" + numeroId());
    campo("titulo-avaliacao").textContent = avaliacao.titulo;
    const area = campo("questoes");
    const botao = campo("form-avaliacao").querySelector("button");
    const acoes = campo("acoes-avaliacao");
    acoes.append(criarLink("Voltar ao curso", "curso.html?id=" + avaliacao.cursoId));
    if (!avaliacao.podeResponder) {
        campo("andamento").textContent = avaliacao.motivoBloqueio;
        campo("barra-prova").hidden = true;
        if (avaliacao.resultadoId) acoes.append(criarLink("Ver resultado", "resultado.html?id=" + avaliacao.resultadoId));
        return;
    }
    if (!avaliacao.questoes.length) {
        botao.disabled = true;
        campo("andamento").textContent = "Esta avaliação ainda não tem questões disponíveis.";
        return;
    }
    campo("form-avaliacao").hidden = false;
    for (const [indice, questao] of avaliacao.questoes.entries()) {
        const grupo = elemento("fieldset");
        grupo.append(elemento("legend", (indice + 1) + ". " + questao.enunciado));
        for (const [indiceAlternativa, alternativa] of questao.alternativas.entries()) {
            const label = elemento("label", undefined, "alternativa");
            const radio = elemento("input");
            radio.type = "radio";
            radio.name = "questao-" + questao.id;
            radio.value = alternativa.id;
            radio.required = true;
            label.append(radio, elemento("span", "ABCDE"[indiceAlternativa] + ") " + alternativa.texto));
            grupo.append(label);
        }
        area.append(grupo);
    }
    function atualizarAndamento() {
        const respondidas = area.querySelectorAll('input:checked').length;
        campo("andamento").textContent = respondidas + " de " + avaliacao.questoes.length + " questões respondidas";
        campo("barra-prova").value = respondidas * 100 / avaliacao.questoes.length;
    }
    area.addEventListener("change", atualizarAndamento);
    atualizarAndamento();
    enviarFormulario("form-avaliacao", async function (formulario) {
        const dados = new FormData(formulario);
        const respostas = avaliacao.questoes.map(questao => ({
            questaoId: questao.id, alternativaId: Number(dados.get("questao-" + questao.id))
        }));
        let retorno;
        try { retorno = await requisicao("/api/avaliacoes/" + avaliacao.id + "/respostas", "POST", { respostas, tentativaCurso: avaliacao.tentativaCurso }); }
        catch (erro) {
            if (erro.status === 409) {
                formulario.hidden = true;
                campo("barra-prova").hidden = true;
                campo("andamento").textContent = erro.message;
            }
            throw erro;
        }
        location.href = "resultado.html?id=" + retorno.id;
    }, "Enviando respostas...");
}

async function mostrarResultado(usuario) {
    const resultado = await requisicao("/api/resultados/" + numeroId());
    metricas([["Acertos", resultado.acertos], ["Erros", resultado.total - resultado.acertos],
        ["Percentual", Number(resultado.percentual).toFixed(1) + "%"], ["Nota", Number(resultado.nota).toFixed(1) + " / 10"]]);
    const detalhe = cartao(resultado.titulo, resultado.aprovado ? "Aprovado: você atingiu pelo menos 60% de acertos." : "Reprovado nesta avaliação. Uma nova avaliação só será permitida após reiniciar o curso.");
    detalhe.append(elemento("p", "Tentativa do curso: " + resultado.tentativaCurso));
    detalhe.append(progresso(resultado.progresso));
    if (resultado.cursoId && usuario.perfil === "Aluno") {
        detalhe.append(criarLink("Voltar ao curso", "curso.html?id=" + resultado.cursoId));
        if (resultado.podeReiniciar) detalhe.append(botaoReiniciarCurso(resultado.cursoId));
        detalhe.append(criarLink("Certificados", "certificados.html?curso=" + resultado.cursoId));
    }
    campo("detalhes").append(detalhe);
    for (const resposta of resultado.respostas) {
        const item = cartao(resposta.enunciado, "Sua resposta: " + resposta.texto);
        item.append(elemento("span", resposta.correta ? "Acerto" : "Erro", "selo"));
        if (!resposta.correta) item.append(elemento("p", "Resposta correta: " + resposta.respostaCorreta));
        campo("lista").append(item);
    }
}

iniciar(pagina === "resultado" && numeroId() ? ["Aluno", "Mentor", "Gestor"] : ["Aluno"], async function (usuario) {
    if (pagina === "painel") {
        const painel = await requisicao("/api/aluno/painel");
        metricas([["Cursos matriculados", painel.cursosMatriculados], ["Progresso geral", Number(painel.progressoGeral).toFixed(1) + "%"]]);
        listarResultados(painel.avaliacoesRecentes, campo("lista"));
        const orientacoes = await requisicao("/api/aluno/orientacoes");
        if (!orientacoes.length) vazio(campo("orientacoes"), "Seu mentor ainda não registrou orientações.");
        for (const orientacao of orientacoes) {
            const item = cartao(orientacao.mentor, orientacao.texto);
            item.append(elemento("p", dataBrasileira(orientacao.data), "ajuda"));
            campo("orientacoes").append(item);
        }
    }
    if (pagina === "cursos") {
        campo("form-busca").addEventListener("submit", function (evento) {
            evento.preventDefault();
            catalogo().catch(erro => aviso(erro.message, "erro"));
        });
        await catalogo();
    }
    if (pagina === "curso") await detalheCurso();
    if (pagina === "meus") {
        const cursos = await requisicao("/api/matriculas/minhas");
        if (!cursos.length) vazio(campo("lista"), "Você ainda não está matriculado. Explore o catálogo para começar.");
        for (const curso of cursos) campo("lista").append(cardCurso(curso, true));
    }
    if (pagina === "estudar") await estudar();
    if (pagina === "avaliacao") await abrirAvaliacao();
    if (pagina === "resultado") {
        if (numeroId()) await mostrarResultado(usuario);
        else listarResultados(await requisicao("/api/resultados"), campo("lista"));
    }
});
