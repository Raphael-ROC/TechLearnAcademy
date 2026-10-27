# Verificações de interface do desktop

O programa usa os formulários reais de Windows Forms e confere campos, máscara de senha, colagem, redimensionamento e maximização. Não faz login nem cria registros no banco.

No Windows:

```powershell
dotnet run --project Testes/Desktop/VerificarDesktop.csproj -c Release
```

É necessário Windows com interface gráfica e área de transferência. O programa restaura o conteúdo anterior da área de transferência ao concluir.

`DesktopColagem` preserva uma ferramenta anterior. As integrações SQL exigem uma instância exclusiva de teste. Leia os comentários antes de executá-las.

Os registros antigos estão em `Documentacao/historico`. Os resultados desta organização ficam em `Documentacao/verificacao.md`.
