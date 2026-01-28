# ComparePrice 

Aplicação web para comparação e gestão de preços, desenvolvida no âmbito do projeto ESII(Engenharia Software).

## Stack / Tecnologias

- C# + .NET 8
- ASP.NET Core Web API (Backend)
- ASP.NET Core Razor Pages (Frontend)
- Entity Framework Core (EF Core)
- PostgreSQL
- HTML / CSS

## Estrutura do Projeto

ES-2/
├── backend/
│   └── SistemaPrecos.API.csproj
├── frontend/
│   └── SistemaPrecos.Web.csproj
└── ES-2.sln

## Requisitos

- .NET SDK 8.0
- PostgreSQL (ex.: versão 17)
- EF Core Tools (opcional)

## Instalar EF Core Tools

dotnet tool install --global dotnet-ef

## Base de Dados

Criar manualmente no PostgreSQL uma base de dados com o nome:
ComparePrice

## Connection String

Ficheiro: backend/appsettings.json

{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=ComparePrice;Username=postgres;Password=YOUR_PASSWORD"
  }
}

## Como Correr o Projeto

dotnet restore .\ES-2.sln

dotnet ef database update --project .\backend\SistemaPrecos.API.csproj

## Backend(API)

dotnet run --project .\backend\SistemaPrecos.API.csproj

## Frontend(Web)

dotnet run --project .\frontend\SistemaPrecos.Web.csproj

## URLs da Aplicação

Frontend: http://localhost:5002  
Backend: http://localhost:5000  

## Autenticação e Perfis

A aplicação suporta dois tipos de utilizador:
- Administrador
- Utilizador

O tipo de utilizador é controlado através de cookies e redirecionamento automático após login.

## Comandos Úteis

dotnet ef migrations list --project .\backend\SistemaPrecos.API.csproj

dotnet clean .\ES-2.sln

dotnet ef database update --project .\backend\SistemaPrecos.API.csproj
