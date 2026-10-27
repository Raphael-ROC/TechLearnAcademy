# Conferência da organização do repositório

Conferência realizada em **27/10/2026**. Esta página separa o que foi verificado nesta organização dos registros históricos.

## Resultado verificado

| Verificação | Resultado |
| --- | --- |
| Solução principal .NET em Release | API, Web, Desktop e testes compilados; 0 erros e 0 avisos. |
| Verificações C# sem banco | 58 aprovadas, incluindo regras de senha, hash, token, modelos e permissões dos chamados. |
| Arquivos do site | 10 scripts com sintaxe válida, 30 páginas e 167 referências locais conferidas. |
| Comparação com o site público | 42 arquivos HTTP 200; três JavaScripts diferentes foram incorporados. |
| GitHub Actions em cópia limpa | API e site, Desktop Windows e Android Java aprovados na [execução inicial](https://github.com/Raphael-ROC/TechLearnAcademy/actions/runs/40539451010). |
| Android no CI | APK de desenvolvimento compilado e Android Lint executado. |
| Separação de arquivos privados | Backups, registros do banco, configuração privada e chaves de assinatura fora do histórico. |

Os três links de instaladores dependem dos pacotes distribuídos no site. Esses binários não integram os fontes; a limitação é explícita no verificador e no guia de publicação.

## Relação com a API do Azure

O pacote de referência consultado no Azure tinha SHA-256:

```text
74addd67b68a60d99763c0b892322f84e2d5f81553222e57ea2bdedc9ed3b77b
```

Os símbolos de compilação permitiram comparar os hashes dos 24 arquivos de código da API: 21 correspondiam às fontes locais. Nos três restantes foram identificados o comando de manutenção administrativa, a integração Azure/SMTP e as regras atualizadas dos chamados.

Esses trechos foram conferidos por análise estática e incorporados aos fontes legíveis. Isso não recupera a formatação e os comentários originais nem prova, por si só, o comportamento em produção. O tratamento local de erros SQL foi mantido e `System.Security.Cryptography.Pkcs` foi atualizado para `10.0.14` após o alerta da restauração inicial.

O conjunto final foi compilado e passou pelas verificações sem banco. Não foi anunciado como uma reprodução byte a byte da DLL da produção.

## Histórico Git

Os commits foram divididos por configuração, banco, camadas da API, funcionalidades web, desktop, Android, testes e documentação. Todos usam a data real da organização, com o nome público da conta e o endereço de privacidade do GitHub.

Consulte o [histórico de commits](https://github.com/Raphael-ROC/TechLearnAcademy/commits/main/). A separação facilita a revisão, mas não representa uma cronologia retroativa do desenvolvimento.

## Limites

- Não foram criados usuários, cursos ou outros dados de demonstração nesta organização.
- Não foram executadas migrações, restaurações ou comandos de manutenção contra o banco do Azure.
- Não foram trocadas senhas nem enviados e-mails de recuperação para testar o serviço.
- A hospedagem e o banco de produção não foram atualizados.
- Instalação e interação do Android em aparelho físico continuam sendo uma validação separada.
- O protótipo MAUI foi preservado como referência, sem fazer parte da solução e do CI atuais.
- Os testes de integração SQL e os relatórios da pasta `historico` não foram reexecutados como parte desta organização.
