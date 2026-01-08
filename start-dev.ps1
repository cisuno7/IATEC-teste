Write-Host "Iniciando desenvolvimento Agenda Manager..." -ForegroundColor Green
Write-Host ""

# Verificar .NET SDK
Write-Host "[1/4] Verificando .NET SDK..." -NoNewline
try {
    $dotnetVersion = dotnet --version
    Write-Host " OK ($dotnetVersion)" -ForegroundColor Green
} catch {
    Write-Host " ERRO: .NET SDK não encontrado. Instale o .NET 10 SDK." -ForegroundColor Red
    Write-Host "Execute: winget install Microsoft.DotNet.SDK.10" -ForegroundColor Yellow
    Read-Host "Pressione Enter para sair"
    exit 1
}

# Verificar Node.js
Write-Host "[2/4] Verificando Node.js..." -NoNewline
try {
    $nodeVersion = node --version
    Write-Host " OK ($nodeVersion)" -ForegroundColor Green
} catch {
    Write-Host " ERRO: Node.js não encontrado. Instale o Node.js 18+." -ForegroundColor Red
    Read-Host "Pressione Enter para sair"
    exit 1
}

# Verificar Angular CLI
Write-Host "[3/4] Verificando Angular CLI..." -NoNewline
try {
    $ngVersion = ng version --version 2>$null
    Write-Host " OK" -ForegroundColor Green
} catch {
    Write-Host " ERRO: Angular CLI não encontrado. Execute: npm install -g @angular/cli" -ForegroundColor Red
    Read-Host "Pressione Enter para sair"
    exit 1
}

# Instalar dependências
Write-Host ""
Write-Host "[4/4] Instalando dependências..." -ForegroundColor Yellow

Write-Host "Instalando dependências .NET..." -NoNewline
dotnet restore
if ($LASTEXITCODE -eq 0) {
    Write-Host " OK" -ForegroundColor Green
} else {
    Write-Host " ERRO" -ForegroundColor Red
    exit 1
}

Write-Host "Instalando dependências Node.js..." -NoNewline
npm install
if ($LASTEXITCODE -eq 0) {
    Write-Host " OK" -ForegroundColor Green
} else {
    Write-Host " ERRO" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "============================================" -ForegroundColor Cyan
Write-Host "Dependências instaladas com sucesso!" -ForegroundColor Green
Write-Host ""
Write-Host "Para executar o projeto:" -ForegroundColor Yellow
Write-Host ""
Write-Host "1. Terminal 1 - Backend:" -ForegroundColor White
Write-Host "   dotnet run --project AgendaManager.Api.csproj" -ForegroundColor Gray
Write-Host ""
Write-Host "2. Terminal 2 - Frontend:" -ForegroundColor White
Write-Host "   npm start" -ForegroundColor Gray
Write-Host ""
Write-Host "============================================" -ForegroundColor Cyan

Read-Host "Pressione Enter para continuar"
