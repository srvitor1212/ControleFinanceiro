# Arquitetura e estado atual

## Visão geral

O `ControleFinanceiro.slnx` referencia dois projetos .NET 10:

| Projeto | Tipo | Papel observado |
| --- | --- | --- |
| `Api/Backend.csproj` | ASP.NET Core Web API | Expõe controllers HTTP; no estado atual, somente a previsão do tempo de exemplo. |
| `Frontend/Frontend.csproj` | Blazor WebAssembly PWA | Executa no navegador e contém as telas e o estado de interface de exemplo. |

Os dois projetos são independentes: não há referência de projeto entre eles. Em desenvolvimento, os perfis de inicialização usam, respectivamente, `https://localhost:7171`/`http://localhost:7272` para a API e `https://localhost:8181`/`http://localhost:8282` para o frontend.

## Composição técnica

### API

O projeto `Api` usa o SDK `Microsoft.NET.Sdk.Web`, tem `TargetFramework` `net10.0`, nullable habilitado e usings implícitos. Sua única dependência NuGet declarada é `Swashbuckle.AspNetCore` 10.2.3.

Em `Program.cs`, a aplicação:

- registra controllers e Swagger;
- disponibiliza Swagger e Swagger UI apenas no ambiente `Development`;
- aplica redirecionamento HTTPS;
- chama `UseAuthorization()` e mapeia controllers.

O único controller é `WeatherForecastController`, exposto em `GET /WeatherForecast`. Ele produz cinco itens gerados em memória, com data, temperaturas e um resumo em inglês. O modelo `WeatherForecast` também pertence ao projeto `Api`.

Os arquivos de configuração da API possuem apenas logging e `AllowedHosts`; não há configurações de conexão, JWT ou serviços externos.

### Frontend

O projeto `Frontend` usa o SDK `Microsoft.NET.Sdk.BlazorWebAssembly`, `net10.0` e as dependências `Microsoft.AspNetCore.Components.WebAssembly` e `Microsoft.AspNetCore.Components.WebAssembly.DevServer`, ambas na versão 10.0.11. Ele contém configuração de PWA, manifest, ícones e service worker.

O bootstrap registra `App` e `HeadOutlet` e disponibiliza um `HttpClient` scoped cuja base é a mesma origem que serve o frontend. As rotas de interface são:

- `/`: página inicial do template;
- `/counter`: contador mantido localmente no componente;
- `/weather`: demonstração de previsão do tempo;
- `/not-found`: página de não encontrado.

Embora a página `/weather` injete `HttpClient`, ela requisita `sample-data/weather.json` sob os arquivos estáticos do próprio frontend. Ela não consulta `GET /WeatherForecast` da API. As configurações do frontend também contêm somente logging e `AllowedHosts`.

## Separação prevista e implementação observada

O `AGENTS.md` prescreve a separação entre apresentação, comunicação HTTP e regras de negócio: a API deve concentrar regras, validações, persistência e contratos HTTP; o frontend deve concentrar interface, estado de tela e experiência do usuário, sem reproduzir regras de negócio.

No estado atual, essa divisão existe apenas na estrutura dos dois projetos e na natureza de seus templates. A API tem um contrato HTTP de demonstração e o frontend tem componentes de demonstração, mas ainda não há um fluxo de negócio que atravesse essa fronteira. Em particular, a tela de clima usa conteúdo local, e não o controller existente.

O mesmo arquivo atribui ao `auth-service` a autenticação e emissão de tokens, determina que a API valide assinatura, emissor, audiência e expiração, e que aplique autorização baseada em claims ou permissões. Ele também prevê que o frontend obtenha, renove e envie o token à API. A última instrução do arquivo termina em `usando:`; não há, no conteúdo atual, a especificação do mecanismo após esse ponto.

## Lacunas verificáveis no estado atual

As observações abaixo descrevem o que não está presente no código e nas configurações versionados; não representam funcionalidades já entregues nem uma lista fechada de requisitos futuros.

- **Domínio financeiro:** não há entidades, endpoints, telas, contratos ou regras para registrar ou controlar dívidas pessoais, apesar de esse ser o objetivo declarado do projeto.
- **Persistência:** não há pacote ou configuração de acesso a banco, `DbContext`, migrations, repositórios ou connection string. O único dado retornado pela API é criado em memória para cada requisição.
- **Autenticação e autorização JWT:** a API chama `UseAuthorization()`, mas não registra autenticação JWT (`AddAuthentication`/`AddJwtBearer`), não chama `UseAuthentication()` e não define parâmetros de emissor, audiência, assinatura ou expiração. Também não há endpoints ou atributos de autorização no controller atual. Portanto, o comportamento de validação de token, `401` e `403` prescrito no `AGENTS.md` não está configurado.
- **Integração com o `auth-service`:** não há URL, cliente HTTP, fluxo de login, renovação de token, armazenamento de token ou cabeçalho `Authorization: Bearer` no frontend; tampouco há configuração correspondente na API.
- **Integração frontend--API:** as origens configuradas para desenvolvimento são diferentes e não há `HttpClient` apontando para a URL da API, política CORS, proxy, nem código que chame seus endpoints. A única chamada da tela de clima é ao JSON estático local.
- **Validação e testes:** não há validações de entrada ou testes automatizados entre os arquivos-fonte e projetos presentes na solução.

## Limites desta leitura

Este documento representa os arquivos versionados encontrados em `Api/`, `Frontend/`, `ControleFinanceiro.slnx` e `AGENTS.md`. Artefatos gerados, diretórios de IDE e dependências instaladas não foram tratados como fonte de arquitetura. A existência ou a configuração de serviços externos, como o `auth-service`, não pode ser inferida deste repositório.
