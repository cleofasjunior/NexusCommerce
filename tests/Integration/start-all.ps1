Write-Host "Iniciando a Orquestra Nexus Commerce..." -ForegroundColor Green

# 1. Identity (Porta 5001)
Start-Process dotnet -ArgumentList "run --project src/Services/Nexus.Identity/Nexus.Identity.API/Nexus.Identity.API.csproj" -WorkingDirectory $PSScriptRoot

# 2. Stock (Porta 6001)
Start-Process dotnet -ArgumentList "run --project src/Services/Nexus.Stock/Nexus.Stock.API/Nexus.Stock.API.csproj" -WorkingDirectory $PSScriptRoot

# 3. Sales (Porta 5002)
Start-Process dotnet -ArgumentList "run --project src/Services/Nexus.Sales/Nexus.Sales.API/Nexus.Sales.API.csproj" -WorkingDirectory $PSScriptRoot

# 4. Gateway (Porta 5000)
Start-Process dotnet -ArgumentList "run --project src/Services/Nexus.Gateway/Nexus.Gateway.csproj" -WorkingDirectory $PSScriptRoot

Write-Host "Todos os serviços foram iniciados em janelas separadas!" -ForegroundColor Yellow