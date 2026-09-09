# API atual

Este documento descreve somente o que está implementado hoje em `Api/`. Apesar
de o projeto ter como objetivo o controle de dívidas pessoais, a API ainda
expõe apenas o endpoint de exemplo de previsão do tempo.

## Execução e configuração

O projeto HTTP é `Api/Backend.csproj`, direcionado ao .NET 10 (`net10.0`). A
execução local pode ser iniciada, a partir da raiz de `controlefinanceiro`, com:

```powershell
dotnet run --project Api/Backend.csproj
```

O perfil `backend` de `Api/Properties/launchSettings.json` configura o ambiente
`Development` e as URLs `https://localhost:7171` e `http://localhost:7272`.
Essas portas pertencem ao perfil de execução; podem ser diferentes se a
aplicação for iniciada com outra configuração ou por hospedagem externa.

A aplicação usa redirecionamento para HTTPS. Portanto, uma chamada recebida
pela URL HTTP configurada é redirecionada para HTTPS. A configuração atual não
define connection strings, provedores de persistência, CORS, autenticação ou
parâmetros de negócio para a API.

O arquivo `Api/Backend.http` ainda usa `http://localhost:5029` como endereço de
exemplo, que não corresponde às URLs do perfil `backend` atualmente
versionado.

## Endpoint disponível

### `GET /WeatherForecast`

Retorna uma lista JSON com cinco previsões geradas no momento da requisição.
Não recebe parâmetros, não exige cabeçalho de autenticação e não há controle de
autorização aplicado ao controlador.

Resposta de sucesso: `200 OK`.

Exemplo de forma da resposta (os valores variam a cada chamada):

```json
[
  {
    "date": "2026-09-09",
    "temperatureC": 18,
    "temperatureF": 64,
    "summary": "Mild"
  }
]
```

Cada objeto possui os campos abaixo:

| Campo | Tipo JSON | Descrição |
| --- | --- | --- |
| `date` | string | Data no formato ISO `YYYY-MM-DD`. São produzidas cinco datas, do dia seguinte ao momento local do servidor até cinco dias posteriores. |
| `temperatureC` | número inteiro | Temperatura em Celsius, sorteada no intervalo de `-20` a `54`, inclusive. |
| `temperatureF` | número inteiro | Valor calculado a partir de `temperatureC` pela implementação atual: `32 + (int)(temperatureC / 0.5556)`. |
| `summary` | string | Uma descrição sorteada dentre `Freezing`, `Bracing`, `Chilly`, `Cool`, `Mild`, `Warm`, `Balmy`, `Hot`, `Sweltering` e `Scorching`. |

Como a temperatura e o resumo usam seleção aleatória e a data depende do relógio
local do servidor, a resposta não é determinística. O serializador JSON padrão
do ASP.NET Core expõe as propriedades em `camelCase`, como no exemplo.

## HTTPS e Swagger

O middleware de redirecionamento HTTPS é sempre configurado. O Swagger e a
interface Swagger UI são configurados apenas quando o ambiente é
`Development`. No perfil local padrão, a interface é aberta em
`https://localhost:7171/swagger` ao iniciar o projeto; o perfil também define
`swagger` como a página inicial do navegador.

Em ambientes que não sejam `Development`, o código não registra os middlewares
de Swagger e Swagger UI, portanto esses recursos não são disponibilizados por
essa configuração.

## Estado atual e diretrizes do projeto

O `AGENTS.md` define diretrizes para que a API concentre regras de negócio,
validações, persistência e contratos HTTP de controle financeiro, além de
validar tokens emitidos pelo `auth-service` para rotas protegidas. Essas são
diretrizes de desenvolvimento, não funcionalidades presentes nesta versão.

Atualmente não existem endpoints para dívidas, usuários ou outros recursos
financeiros; tampouco há persistência, validação de token, emissão de `401` ou
`403` por rotas protegidas implementadas no controlador disponível. A única API
documentável é `GET /WeatherForecast`.
