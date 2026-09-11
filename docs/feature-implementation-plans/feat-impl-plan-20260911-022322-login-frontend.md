# Plano de implementação: login no frontend

> **Estado da implementação:** Plano criado
> **Última atualização (UTC):** 2026-09-11 02:23:22 UTC
> **Documento de requisitos:** [feat-req-20260911-020935-login-frontend.md](../features/feat-req-20260911-020935-login-frontend.md)

## Objetivo e escopo

Adicionar ao Blazor WebAssembly do ControleFinanceiro uma página `/login` que
autentica no `auth-service`, persiste uma sessão local válida e protege apenas
a rota `/`. A sessão é válida quando há `accessToken` não vazio e um
`expiresAt` parseável como instante com deslocamento, estritamente posterior a
`DateTimeOffset.UtcNow`.

O plano não inclui alterações na API em `Api/`, no `auth-service`, CORS,
logout, cadastro, recuperação de senha, refresh/revogação de token ou guarda
de rotas diferentes de `/`.

## Contexto técnico atual

- `Frontend/Frontend.csproj` é uma aplicação Blazor WebAssembly em .NET 10 e
  não possui projeto ou infraestrutura de testes.
- `Frontend/Program.cs` registra somente um `HttpClient` scoped cuja base é a
  origem do próprio frontend. `Pages/Weather.razor` o consome para um recurso
  local; essa configuração deve permanecer inalterada.
- `Frontend/App.razor` já descobre componentes roteáveis da assembly com
  `Router` e lhes aplica `MainLayout`. Logo, criar uma página com `@page
  "/login"` basta para registrar a nova rota; não é necessária uma alteração
  global do roteador para proteger exclusivamente `/`.
- `Pages/Home.razor` é o único componente que atende `/` e atualmente mostra
  conteúdo sem ciclo de vida ou autenticação. Ele é o ponto de guarda da rota.
- Não há configuração web, modelos de autenticação, serviço de sessão,
  interoperabilidade com `localStorage` nem cliente para o `auth-service`.
- O contrato externo em `projects/auth-service/docs/api.md` define `POST
  /api/auth/login`, sucesso `200` com `accessToken`, `tokenType` e
  `expiresAt`, `400` com erros de validação (inclusive `errors` por campo) e
  `401` com `errors` como lista. O valor de `expiresAt` é UTC serializado com
  deslocamento.

## Decisões de implementação

| ID | Decisão | Origem | Impacto na implementação |
| --- | --- | --- | --- |
| IMP-DEC-001 | Manter o `HttpClient` existente para recursos da própria aplicação e criar `AuthService` que usa URI absoluta construída a partir de `AuthService:BaseUrl`. | RNF-001; `Frontend/Program.cs`; `Pages/Weather.razor`. | A integração não muda a base de `Weather.razor` nem depende de URL codificada no componente. |
| IMP-DEC-002 | Declarar `AuthService:BaseUrl` em `Frontend/wwwroot/appsettings.json`; a publicação fornece o valor da instância do ambiente. O valor de desenvolvimento será `https://localhost:7070/`, documentado pelo contrato do serviço. | RNF-001; `projects/auth-service/docs/api.md`; `Frontend/Properties/launchSettings.json`. | A configuração é pública, não contém segredo e pode ser substituída no artefato/configuração da implantação. |
| IMP-DEC-003 | Centralizar leitura, validação, gravação e limpeza da sessão em `SessionService`, usando `IJSRuntime` e as chaves estáveis `controle-financeiro.auth.access-token` e `controle-financeiro.auth.expires-at`. | RF-003, RF-005, RF-006, RF-008; DEC-001 e DEC-003. | Páginas não acessam `localStorage` diretamente; o token nunca é enviado para logs ou mensagens. |
| IMP-DEC-004 | Validar `accessToken` e `expiresAt` antes de gravar. Interpretar expiração com `DateTimeOffset.TryParse` e comparar estritamente com `DateTimeOffset.UtcNow`; em escrita parcial, remover ambas as chaves em melhor esforço. | RF-003, RF-005, RF-006; DEC-001; requisito sobre fuso horário. | Dados ausentes, inválidos, expirados ou uma resposta de sucesso incompleta não concedem acesso; não há estado de sessão parcialmente criado pela tentativa. |
| IMP-DEC-005 | Modelar o resultado da chamada de login como sucesso, erros recuperáveis do serviço ou falha genérica. Para `400` e `401`, extrair e achatar as mensagens de `errors`, aceitando lista e o objeto por campo do ASP.NET Core; quando não houver mensagem utilizável, usar uma mensagem genérica. | RF-004, RF-005; `projects/auth-service/docs/api.md`. | A página preserva as mensagens retornadas quando disponíveis, permanece em `/login` e não grava sessão em todos os caminhos de falha. |
| IMP-DEC-006 | Proteger `/` no ciclo de vida de `Home.razor`: ocultar o conteúdo enquanto a sessão é verificada e navegar para `/login` com substituição de histórico quando ela for inválida. | RF-006, RF-007; `App.razor`; `Pages/Home.razor`; DEC-002. | Apenas a rota requisitada é guardada e não há exibição de conteúdo de Home antes da verificação. |
| IMP-DEC-007 | Usar formulário HTML sem validação cliente bloqueante (`novalidate`), controles `email` e `password`, rótulos explícitos e região de erros com `role="alert"`/`aria-live`. | RF-001, RF-004, RNF-002. | Enter submete o formulário, o serviço continua sendo a fonte das mensagens de validação e tecnologias assistivas percebem os erros. |
| IMP-DEC-008 | Não introduzir projeto ou pacote de testes nesta mudança de autenticação: não há convenção de testes na base e os critérios serão verificados por build e cenários manuais contra uma instância configurada do `auth-service`. | `Frontend/Frontend.csproj`; escopo mínimo; requisitos de aceitação. | Evita incluir infraestrutura e dependências não existentes; os cenários têm resultado observável e rastreável abaixo. |

## Arquivos afetados

| Caminho | Ação | Responsabilidade da mudança | Requisitos |
| --- | --- | --- | --- |
| `Frontend/wwwroot/appsettings.json` | Criar | URL pública e configurável do `auth-service`. | RNF-001 |
| `Frontend/Program.cs` | Alterar | Registrar serviços de autenticação e sessão, preservando o cliente HTTP atual. | RF-002, RF-003, RF-006, RNF-001 |
| `Frontend/Models/LoginRequest.cs` | Criar | Corpo `email`/`password` do contrato de login. | RF-002 |
| `Frontend/Models/LoginResponse.cs` | Criar | Contrato de sucesso e dados necessários para validar a resposta. | RF-003, RF-005 |
| `Frontend/Models/LoginResult.cs` | Criar | Resultado sem exceções para sucesso, erros retornados e falha não recuperável. | RF-004, RF-005 |
| `Frontend/Services/IAuthService.cs` | Criar | Contrato consumido pela página de login. | RF-002, RF-004, RF-005 |
| `Frontend/Services/AuthService.cs` | Criar | POST, interpretação segura da resposta e mapeamento de erros. | RF-002, RF-003, RF-004, RF-005, RNF-003 |
| `Frontend/Services/ISessionService.cs` | Criar | Contrato de persistência e consulta da sessão atual. | RF-003, RF-006, RF-008 |
| `Frontend/Services/SessionService.cs` | Criar | Acesso encapsulado ao `localStorage` e regra de validade temporal. | RF-003, RF-005, RF-006, RF-008, RNF-003 |
| `Frontend/Pages/Login.razor` | Criar | Rota, formulário, estado de envio, erros e redirecionamento pós-login. | RF-001 a RF-005, RNF-002, RNF-003 |
| `Frontend/Pages/Home.razor` | Alterar | Guarda da rota `/` e renderização condicional da página inicial. | RF-006, RF-007, RF-008 |

## Ordem de implementação

### 1. Configurar e registrar as dependências de autenticação

- [ ] Implementado
- **Objetivo:** disponibilizar uma URL do serviço por ambiente e injetar as
  abstrações de autenticação sem alterar o `HttpClient` já consumido pelo
  frontend.
- **Requisitos atendidos:** RF-002, RF-003, RF-006, RNF-001.
- **Arquivos:** `Frontend/wwwroot/appsettings.json` (criar),
  `Frontend/Program.cs` (alterar), `Frontend/Services/IAuthService.cs`
  (criar), `Frontend/Services/ISessionService.cs` (criar).
- **Alterações:** criar a seção pública `AuthService` com `BaseUrl`. Em
  `Program.cs`, obter e validar a URI absoluta configurada uma única vez,
  registrar `IAuthService` e `ISessionService` como scoped e manter o
  registro atual de `HttpClient` com base na origem do frontend. A URL não é
  um segredo e a implantação deve substituir apenas essa configuração para
  apontar para sua instância; nenhuma credencial entra em arquivos cliente.
- **Trecho decisivo:**

  ```csharp
  var authBaseUrl = builder.Configuration["AuthService:BaseUrl"];
  // Validar URI absoluta com esquema HTTP(S) antes de registrar IAuthService.
  // O HttpClient existente continua com builder.HostEnvironment.BaseAddress.
  ```

- **Dependências:** N/A.
- **Verificação:** iniciar o frontend localmente e confirmar que a URL é lida
  de `wwwroot/appsettings.json`; alterar somente o valor configurado para uma
  URL de teste e confirmar que a aplicação não exige mudança de código.

### 2. Implementar contratos e cliente do `auth-service`

- [ ] Implementado
- **Objetivo:** enviar exatamente o contrato de login e transformar as
  respostas HTTP em resultado seguro para a interface.
- **Requisitos atendidos:** RF-002, RF-003, RF-004, RF-005, RNF-003.
- **Arquivos:** `Frontend/Models/LoginRequest.cs` (criar),
  `Frontend/Models/LoginResponse.cs` (criar),
  `Frontend/Models/LoginResult.cs` (criar),
  `Frontend/Services/AuthService.cs` (criar).
- **Alterações:** `LoginRequest` contém somente `Email` e `Password` para
  serialização JSON camelCase. `LoginResponse` mapeia `accessToken`,
  `tokenType` e `expiresAt`; a expiração permanece disponível como texto até
  a validação de sessão, preservando o valor recebido. `AuthService` envia
  `POST` para a URI absoluta `api/auth/login`, com `application/json`, sem
  registrar corpo, senha, cabeçalhos ou token. Em `200`, retorna sucesso
  apenas com corpo desserializado; token vazio, expiração ausente ou formato
  inesperado vira falha genérica. Em `400`/`401`, lê o JSON e achata
  `errors` quando ele for vetor de textos ou objeto de vetores por campo. Em
  falha de rede, desserialização, URL inválida ou outro status, retorna uma
  mensagem genérica de login não concluído, sem vazar detalhes técnicos.
- **Trecho decisivo:**

  ```csharp
  using var response = await httpClient.PostAsJsonAsync(loginUri, request, cancellationToken);
  if (response.IsSuccessStatusCode && HasUsableLoginContract(responseBody))
      return LoginResult.Success(responseBody);

  if (response.StatusCode is HttpStatusCode.BadRequest or HttpStatusCode.Unauthorized)
      return LoginResult.Rejected(FlattenErrors(responseContent));

  return LoginResult.Failure();
  ```

- **Dependências:** passo 1; `auth-service` acessível e CORS liberado para a
  origem do frontend.
- **Verificação:** com um interceptador/proxy de desenvolvimento ou instância
  de teste, inspecionar `POST /api/auth/login`, `Content-Type` e JSON com
  `email` e `password`; simular `200`, `400`, `401`, falha de rede e corpo
  inválido, confirmando o resultado de domínio esperado sem log de segredo.

### 3. Implementar persistência e validação local de sessão

- [ ] Implementado
- **Objetivo:** encapsular no browser a sessão persistente e sua validade,
  evitando que dados malformados concedam acesso.
- **Requisitos atendidos:** RF-003, RF-005, RF-006, RF-008, RNF-003.
- **Arquivos:** `Frontend/Services/SessionService.cs` (criar).
- **Alterações:** usar `IJSRuntime` para obter/gravar/remover os dois valores
  de `localStorage`. A operação de gravação recebe token e expiração apenas
  depois que `AuthService` informou sucesso, verifica token não vazio e
  `DateTimeOffset.TryParse(expiresAt, ...) > DateTimeOffset.UtcNow`, e então
  grava ambas as chaves. Se qualquer gravação falhar, remove as duas chaves
  em melhor esforço e retorna falha. A consulta de sessão lê ambas, aplica a
  mesma regra estrita de tempo e devolve inválida para valores ausentes,
  ilegíveis ou expirados; nesse caso limpa os dados inválidos em melhor
  esforço. Não há decodificação de JWT, validação remota, renovação ou
  revogação neste escopo.
- **Trecho decisivo:**

  ```csharp
  var isValid = !string.IsNullOrWhiteSpace(token)
      && DateTimeOffset.TryParse(expiresAt, out var expiresAtInstant)
      && expiresAtInstant > DateTimeOffset.UtcNow;
  ```

- **Dependências:** passo 1; `IJSRuntime` já é disponibilizado pelo Blazor
  WebAssembly.
- **Verificação:** no DevTools, testar token sem expiração, expiração não
  parseável, expiração igual ao instante atual, expiração passada e expiração
  futura. Somente o último caso deve ser considerado válido; falha durante a
  persistência não pode deixar uma nova sessão válida.

### 4. Criar a rota e a experiência de login

- [ ] Implementado
- **Objetivo:** oferecer o formulário acessível que consome os serviços e
  conduz o usuário à página inicial somente após concluir a sessão.
- **Requisitos atendidos:** RF-001, RF-003, RF-004, RF-005, RNF-002,
  RNF-003.
- **Arquivos:** `Frontend/Pages/Login.razor` (criar).
- **Alterações:** declarar `@page "/login"` e injetar
  `IAuthService`, `ISessionService` e `NavigationManager`. Implementar um
  `<form novalidate>` com `<label for>` associado a cada controle,
  `input type="email" autocomplete="username"` e `input type="password"
  autocomplete="current-password"`, além de botão `type="submit"`.
  O submit desabilita reenvio concorrente, limpa erros anteriores, chama o
  serviço e somente persiste/navega quando o resultado e `SessionService`
  forem bem-sucedidos. Renderizar os erros em região anunciável
  (`role="alert"` e `aria-live`) e manter o usuário na rota em todas as
  falhas. Não escrever senha nem token em UI diagnóstica, console ou logs.
- **Trecho decisivo:**

  ```csharp
  var result = await authService.LoginAsync(email, password);
  if (result.IsSuccess && await sessionService.StoreAsync(result.AccessToken, result.ExpiresAt))
      navigation.NavigateTo("/", replace: true);
  else
      errors = result.ErrorsOrGenericFailure();
  ```

- **Dependências:** passos 1, 2 e 3.
- **Verificação:** abrir diretamente `/login`; percorrer os controles por
  Tab e submeter por Enter. Em sucesso, conferir as duas chaves no
  `localStorage` e a navegação para `/`; em `400`, `401`, rede ou sucesso
  malformado, conferir mensagens, permanência em `/` não ocorrer e ausência
  de nova sessão válida.

### 5. Aplicar a guarda somente à página inicial

- [ ] Implementado
- **Objetivo:** impedir que conteúdo de `/` seja renderizado para sessão
  local inválida e reutilizar a sessão persistida válida.
- **Requisitos atendidos:** RF-006, RF-007, RF-008.
- **Arquivos:** `Frontend/Pages/Home.razor` (alterar).
- **Alterações:** injetar `ISessionService` e `NavigationManager`; durante a
  inicialização assíncrona, não renderizar o conteúdo existente. Após
  `GetValidSessionAsync`, renderizar a Home apenas se o resultado for válido;
  caso contrário, navegar para `/login` com `replace: true`. Não alterar
  `App.razor`, `NavMenu.razor`, `Counter.razor` ou `Weather.razor`, pois a
  proteção exigida se limita à rota `/`.
- **Trecho decisivo:**

  ```csharp
  var hasValidSession = await sessionService.HasValidSessionAsync();
  if (!hasValidSession)
      navigation.NavigateTo("/login", replace: true);
  else
      showHome = true;
  ```

- **Dependências:** passo 3.
- **Verificação:** abrir `/` em aba nova com armazenamento vazio, inválido e
  expirado e confirmar redirecionamento. Repetir com token e expiração futura,
  fechar/reabrir o navegador e acessar `/`; a Home deve aparecer sem novo
  login.

### 6. Executar validação integrada e registrar os resultados

- [ ] Implementado
- **Objetivo:** confirmar compilação, contrato, persistência, proteção e
  acessibilidade antes de considerar a implementação concluída.
- **Requisitos atendidos:** RF-001 a RF-008, RNF-001 a RNF-003.
- **Arquivos:** nenhum arquivo de produto; atualizar este plano somente após
  implementação, conforme o estado e histórico definidos pela skill.
- **Alterações:** executar `dotnet build Frontend/Frontend.csproj` e os
  cenários da seção de validação. Não incluir tokens, senhas ou cabeçalhos de
  autorização em capturas, logs ou documentação de evidências.
- **Dependências:** passos 1 a 5; instância de teste do `auth-service` e CORS
  adequadamente configurado.
- **Verificação:** build bem-sucedido e todos os cenários de aceitação
  concluídos com os resultados esperados.

## Estratégia de testes e validação

Não há infraestrutura de testes automatizados no frontend atual; a mudança
será validada por build e pelos seguintes cenários manuais em navegador, usando
contas e tokens descartáveis de ambiente de desenvolvimento:

| Validação | Requisitos | Evidência esperada |
| --- | --- | --- |
| `dotnet build Frontend/Frontend.csproj` | Todos | Compilação sem erros. |
| Abrir `/login`, navegar por teclado e submeter por Enter | RF-001, RNF-002 | Campos possuem rótulos, foco e ação de envio operáveis. |
| Login aceito, seguido de inspeção de rede e `localStorage` | RF-002, RF-003 | `POST`/JSON corretos, duas chaves persistidas e navegação a `/`. |
| Respostas `400` com erros por campo e `401` com lista de erros | RF-004, RNF-002 | Mensagens retornadas visíveis e anunciáveis; nenhuma nova sessão gravada. |
| Falha de rede, status inesperado e `200` sem token ou expiração futura | RF-005 | Mensagem genérica, permanência em `/login` e nenhuma sessão válida criada. |
| Abrir `/` com armazenamento vazio, token vazio, texto de expiração inválido, instante igual ao atual e instante passado | RF-006, RF-007 | Redirecionamento a `/login`, sem exibição da Home. |
| Reabrir navegador com valores futuros válidos | RF-006, RF-008 | Acesso a `/` sem repetir o login. |
| Revisar console, captura de rede e pontos de diagnóstico adicionados | RNF-003 | Nenhuma senha ou token exposto fora do armazenamento e da requisição indispensável. |
| Executar com URL do serviço alterada por configuração de implantação | RNF-001 | A chamada usa a instância configurada, sem alteração do código cliente. |

## Rastreabilidade dos requisitos

| Requisito ou critério | Passos | Validação |
| --- | --- | --- |
| RF-001 | 4 | Abrir `/login`, teclado e Enter. |
| RF-002 | 1, 2, 4 | Inspeção de método, URI e JSON da requisição. |
| RF-003 | 2, 3, 4 | Login aceito, chaves persistidas e navegação. |
| RF-004 | 2, 4 | Cenários `400` e `401` com erros visíveis e sem nova sessão. |
| RF-005 | 2, 3, 4 | Rede, status/corpo inválido e sucesso incompleto. |
| RF-006 | 3, 5 | Cenários de sessão ausente, malformada, expirada e futura. |
| RF-007 | 5 | Acesso direto a `/` sem sessão válida. |
| RF-008 | 3, 5 | Reabertura do navegador com expiração futura. |
| RNF-001 | 1, 2, 6 | URL configurável por ambiente. |
| RNF-002 | 4, 6 | Navegação por teclado e anúncio de erros. |
| RNF-003 | 2, 3, 4, 6 | Revisão de ausência de segredos nos diagnósticos. |

## Riscos, compatibilidade e implantação

- A aplicação depende de o `auth-service` estar acessível do navegador e
  permitir a origem do frontend por CORS. Conforme os requisitos, as portas
  locais atuais do frontend (`https://localhost:7270` e
  `http://localhost:7271`) ainda não estão incluídas nas origens de
  desenvolvimento documentadas pelo serviço. Esse ajuste externo deve ocorrer
  antes da validação integrada.
- `localStorage` é específico da origem. Alterar domínio, esquema ou porta em
  uma implantação não migra sessões; o usuário fará login novamente, o que é
  seguro e compatível com o escopo.
- Não há migração de servidor. O rollback consiste em publicar a versão
  anterior do frontend; as duas chaves locais remanescentes são inofensivas
  para essa versão e expiram conforme seu valor. A nova versão descarta dados
  inválidos ao verificá-los.
- Não há telemetria nova. Erros exibidos são mensagens de negócio do serviço
  ou texto genérico, sem conteúdo sensível.

## Itens fora do escopo

- Logout, cadastro, recuperação de senha, refresh token e revogação.
- Proteção de quaisquer rotas além de `/`.
- Alterações no contrato, configuração, CORS ou implementação do
  `auth-service` e no backend `Api/`.
- Validação remota da assinatura do JWT ou inspeção de seus claims pelo
  frontend.
- Criação de infraestrutura de testes automatizados.

## Histórico de alterações

| Data e hora (UTC) | Alteração |
| --- | --- |
| 2026-09-11 02:23:22 UTC | Plano criado a partir de `docs/features/feat-req-20260911-020935-login-frontend.md`. |
