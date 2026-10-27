param(
    [string]$Sdk = 'C:\Program Files (x86)\Android\android-sdk',
    [string]$Java = 'C:\Program Files\Android\openjdk\jdk-21.0.8'
)
$ErrorActionPreference = 'Stop'
$raiz = $PSScriptRoot
$local = Join-Path $raiz '.local'
$tools = Join-Path $Sdk 'build-tools\36.0.0'
$android = Join-Path $Sdk 'platforms\android-36\android.jar'
$env:JAVA_HOME = $Java
$env:PATH = (Join-Path $Java 'bin') + ';' + $env:PATH
function Conferir { if ($LASTEXITCODE -ne 0) { throw "A ferramenta de compilação retornou erro $LASTEXITCODE." } }
foreach ($pasta in 'classes','dex','gerado') {
    New-Item -ItemType Directory -Path (Join-Path $local $pasta) -Force | Out-Null
}
$manifesto = [IO.File]::ReadAllText((Join-Path $raiz 'app\src\main\AndroidManifest.xml')).Replace('<manifest ', '<manifest package="br.com.techlearn.academy" ')
[IO.File]::WriteAllText((Join-Path $local 'AndroidManifest.xml'), $manifesto, [Text.UTF8Encoding]::new($false))
# Compila os recursos e o manifesto do aplicativo.
& (Join-Path $tools 'aapt2.exe') compile --dir (Join-Path $raiz 'app\src\main\res') -o (Join-Path $local 'recursos.zip')
Conferir
& (Join-Path $tools 'aapt2.exe') link -o (Join-Path $local 'sem-assinatura.apk') -I $android --manifest (Join-Path $local 'AndroidManifest.xml') --java (Join-Path $local 'gerado') --min-sdk-version 26 --target-sdk-version 35 --version-code 1 --version-name '1.0' (Join-Path $local 'recursos.zip')
Conferir
$fontes = Get-ChildItem -LiteralPath (Join-Path $raiz 'app\src\main\java'),(Join-Path $local 'gerado') -Filter '*.java' -Recurse | ForEach-Object { '"' + $_.FullName.Replace('\','/') + '"' }
$listaFontes = Join-Path $local 'fontes.txt'
[IO.File]::WriteAllLines($listaFontes, $fontes, [Text.UTF8Encoding]::new($false))
& (Join-Path $Java 'bin\javac.exe') -encoding UTF-8 -source 8 -target 8 -bootclasspath $android -d (Join-Path $local 'classes') ('@' + $listaFontes)
Conferir
& (Join-Path $Java 'bin\jar.exe') cf (Join-Path $local 'classes.jar') -C (Join-Path $local 'classes') .
Conferir
# Converte o Java compilado para o formato de execução do Android.
& (Join-Path $tools 'd8.bat') --release --lib $android --min-api 26 --output (Join-Path $local 'dex') (Join-Path $local 'classes.jar')
Conferir
& (Join-Path $Java 'bin\jar.exe') uf (Join-Path $local 'sem-assinatura.apk') -C (Join-Path $local 'dex') classes.dex
Conferir
& (Join-Path $tools 'zipalign.exe') -f -p 4 (Join-Path $local 'sem-assinatura.apk') (Join-Path $local 'alinhado.apk')
Conferir
$chave = Join-Path $local 'chave-teste.keystore'
if (!(Test-Path -LiteralPath $chave)) {
    # Esta é a chave padrão de desenvolvimento Android, não uma senha de usuário.
    & (Join-Path $Java 'bin\keytool.exe') -genkeypair -noprompt -keystore $chave -storepass android -keypass android -alias androiddebugkey -keyalg RSA -keysize 2048 -validity 10000 -dname 'CN=TechLearn Academy,OU=Projeto Academico,O=TechLearn,C=BR'
    Conferir
}
$saida = Join-Path (Split-Path $raiz) 'entregas\Android'
New-Item -ItemType Directory -Path $saida -Force | Out-Null
$apk = Join-Path $saida 'TechLearnAcademy-Android.apk'
& (Join-Path $tools 'apksigner.bat') sign --ks $chave --ks-key-alias androiddebugkey --ks-pass pass:android --key-pass pass:android --out $apk (Join-Path $local 'alinhado.apk')
Conferir
& (Join-Path $tools 'apksigner.bat') verify --verbose $apk
Conferir
Write-Output "APK gerado: $apk"
