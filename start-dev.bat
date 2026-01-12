@echo off
echo Iniciando desenvolvimento Agenda Manager...
echo.

echo [1/4] Verificando .NET SDK...
dotnet --version >nul 2>&1
if errorlevel 1 (
    echo ERRO: .NET SDK nao encontrado. Instale o .NET 8 SDK.
    pause
    exit /b 1
)
echo .NET SDK encontrado.

echo.
echo [2/4] Verificando Node.js...
node --version >nul 2>&1
if errorlevel 1 (
    echo ERRO: Node.js nao encontrado. Instale o Node.js 18+.
    pause
    exit /b 1
)
echo Node.js encontrado.

echo.
echo [3/4] Verificando Angular CLI...
ng version >nul 2>&1
if errorlevel 1 (
    echo ERRO: Angular CLI nao encontrado. Execute: npm install -g @angular/cli
    pause
    exit /b 1
)
echo Angular CLI encontrado.

echo.
echo [4/4] Instalando dependencias...
echo Instalando dependencias .NET...
dotnet restore

echo.
echo Instalando dependencias Node.js...
npm install

echo.
echo ============================================
echo Dependencias instaladas com sucesso!
echo.
echo Para executar o projeto:
echo.
echo 1. Terminal 1 - Backend (com Hot Reload):
echo    dotnet watch run --project AgendaManager.Api.csproj
echo.
echo 2. Terminal 2 - Frontend:
echo    npm start
echo.
echo ============================================
pause
