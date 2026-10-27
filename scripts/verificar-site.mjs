import { existsSync, readFileSync, readdirSync } from 'node:fs';
import { dirname, extname, join, relative, resolve } from 'node:path';
import { fileURLToPath } from 'node:url';
import { spawnSync } from 'node:child_process';

const raiz = resolve(dirname(fileURLToPath(import.meta.url)), '..');
const site = join(raiz, 'TechLearnAcademy.Web', 'wwwroot');
const falhas = [];

function listar(pasta) {
    return readdirSync(pasta, { withFileTypes: true }).flatMap(item => {
        const caminho = join(pasta, item.name);
        return item.isDirectory() ? listar(caminho) : [caminho];
    });
}

const arquivos = listar(site);
const scripts = arquivos.filter(arquivo => extname(arquivo) === '.js');
for (const arquivo of scripts) {
    const resultado = spawnSync(process.execPath, ['--check', arquivo], { encoding: 'utf8' });
    if (resultado.status !== 0) falhas.push(`${relative(raiz, arquivo)}: ${resultado.stderr || resultado.error}`);
}

let referencias = 0;
let downloads = 0;
const paginas = arquivos.filter(arquivo => extname(arquivo) === '.html');
for (const pagina of paginas) {
    const html = readFileSync(pagina, 'utf8');
    for (const [, endereco] of html.matchAll(/\b(?:src|href)\s*=\s*["']([^"']+)["']/gi)) {
        if (/^(?:https?:|mailto:|tel:|data:|blob:|#)/i.test(endereco)) continue;
        const caminho = decodeURIComponent(endereco.split(/[?#]/)[0]);
        if (!caminho || caminho.startsWith('/api/')) continue;
        // Instaladores são entregas de compilação, distribuídas separadamente dos fontes.
        if (/^\/?aplicativos\//.test(caminho)) { downloads++; continue; }
        const destino = caminho.startsWith('/') ? resolve(site, `.${caminho}`) : resolve(dirname(pagina), caminho);
        referencias++;
        if (!existsSync(destino)) falhas.push(`${relative(site, pagina)}: referência ausente ${endereco}`);
    }
}

if (falhas.length) {
    console.error(falhas.join('\n'));
    process.exitCode = 1;
} else {
    console.log(`OK: ${scripts.length} scripts JavaScript, ${paginas.length} páginas e ${referencias} referências locais.`);
    console.log(`${downloads} links de instaladores dependem dos pacotes distribuídos na página pública.`);
}
