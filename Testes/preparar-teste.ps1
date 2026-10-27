$ErrorActionPreference = "Stop"
$raizTeste = Split-Path -Parent $PSScriptRoot
$instanciaTeste = "TechLearnPimIVTeste20261004"

# Esta ferramenta nunca usa a instância do projeto anterior.
& sqllocaldb start $instanciaTeste
if ($LASTEXITCODE -ne 0) { throw "Não foi possível iniciar a instância exclusiva de teste." }

& sqlcmd -S "(localdb)\TechLearnPimIVTeste20261004" -E -C -b -i (Join-Path $raizTeste "Banco\01-criar-banco.sql")
if ($LASTEXITCODE -ne 0) { throw "O script recusou a criação. Nenhum banco existente será apagado." }

Write-Output "Banco de teste criado. Execute testar-integracao.py --executar para verificar a API."
