# Extração da configuração do frontend

Data e hora da implementação: 2026-09-12T03:41:12Z

## Alteração

O código específico do controle financeiro que estava no `Frontend/Program.cs` foi movido para `Frontend/Configure.cs`.

Foram centralizados:

- validação de `AuthService:BaseUrl`;
- registro do `HttpClient` do frontend;
- registro de `IAuthService` e `ISessionService`.

O `Program.cs` permanece responsável pela criação do host Blazor WebAssembly, inclusão dos componentes raiz e execução da aplicação.

## Validação

Compilar o projeto `Frontend/Frontend.csproj` após a alteração.
