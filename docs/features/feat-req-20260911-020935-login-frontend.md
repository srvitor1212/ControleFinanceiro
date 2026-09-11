# Requisitos — login no frontend

## Contexto, objetivo e escopo

O `Frontend` do ControleFinanceiro é uma aplicação Blazor WebAssembly. No estado atual, a rota `/` mostra a página inicial sem autenticação e não existe uma tela de login, armazenamento de sessão nem proteção de rotas.

O objetivo é permitir que um usuário se autentique no `auth-service`, receba os erros retornados por ele e mantenha uma sessão local enquanto o token ainda for válido. Após uma autenticação bem-sucedida, o usuário deve ir para `/`; ao acessar `/` sem sessão válida, deve ser encaminhado para `/login`.

O escopo cobre somente o frontend e sua integração HTTP com o contrato de login já publicado pelo `auth-service`. Cadastro, recuperação de senha, renovação, revogação, logout, proteção de rotas além de `/` e alterações no `auth-service` não fazem parte deste levantamento.

## Fluxo atual, com evidências

1. O frontend registra apenas um `HttpClient` cuja URL-base é a origem da própria aplicação e inicia `App` em `Frontend/Program.cs`.
2. `Frontend/App.razor` usa um `Router` comum e renderiza todas as rotas sem verificação de autenticação.
3. `Frontend/Pages/Home.razor` atende a rota `/` e exibe a página padrão; não há página ou rota `/login`.
4. A documentação do projeto, em `AGENTS.md`, define que o frontend consulta o `auth-service` para autenticação.
5. O contrato externo em `projects/auth-service/docs/api.md` disponibiliza `POST /api/auth/login`. Ele recebe `email` e `password`, devolve `200` com `accessToken`, `tokenType` e `expiresAt`, devolve `400` para validações e `401` com `errors` para credenciais não aceitas.

## Fluxo desejado e comparação das mudanças

1. O usuário abre `/login`, informa e-mail e senha e envia o formulário.
2. O frontend envia os dados ao endpoint de login do `auth-service`.
3. Quando a resposta for bem-sucedida e trouxer um token e uma expiração futura, o frontend grava `accessToken` e `expiresAt` no `localStorage` da origem da aplicação e navega para `/`.
4. Quando o serviço retornar erro de validação ou de credenciais, o frontend permanece em `/login` e mostra as mensagens recebidas, sem criar ou renovar a sessão local.
5. Ao abrir `/`, inclusive após fechar e reabrir o navegador, o frontend lê a sessão persistida. Uma sessão é válida somente se houver token e se `expiresAt` representar um instante posterior ao momento da verificação.
6. Se não houver sessão válida, a navegação para `/` é redirecionada para `/login`. Dados ausentes, malformados ou expirados não podem conceder acesso a `/`.

Em relação ao estado atual, serão acrescentadas a rota de login, a chamada de integração, a persistência da sessão e a guarda da rota `/`. A página inicial e o contrato HTTP do serviço permanecem inalterados.

## Requisitos funcionais

| ID | Requisito | Critério de aceitação | Evidência ou origem |
| --- | --- | --- | --- |
| RF-001 | O frontend deve disponibilizar a rota `/login` com campos para e-mail e senha e uma ação explícita para enviar as credenciais. | Dado que o usuário acessa `/login`, quando a página terminar de carregar, então ele encontra os campos de e-mail e senha e pode submeter o login. | Solicitação do usuário; `projects/auth-service/docs/api.md`. |
| RF-002 | Ao submeter credenciais, o frontend deve enviar uma requisição `POST /api/auth/login` ao `auth-service`, com corpo JSON contendo `email` e `password`. | Dado que o usuário preenche e envia o formulário, quando a requisição for inspecionada, então o método, a rota e os dois campos do corpo correspondem ao contrato publicado. | `projects/auth-service/docs/api.md`. |
| RF-003 | Quando o login retornar `200 OK` com `accessToken` não vazio e `expiresAt` futuro, o frontend deve persistir ambos no `localStorage` e redirecionar o usuário para `/`. | Dado um retorno de login válido, quando a resposta for processada, então `localStorage` contém o token e a expiração recebidos; e a rota ativa passa a ser `/`. | Solicitação do usuário; `projects/auth-service/docs/api.md`; DEC-001. |
| RF-004 | Quando o `auth-service` devolver mensagens de erro de validação (`400`) ou de credenciais (`401`), o frontend deve apresentá-las ao usuário na tela de login e não deve persistir credenciais nem token. | Dado um retorno `400` ou `401` com mensagens em `errors`, quando a resposta for processada, então as mensagens recebidas ficam visíveis em `/login` e não há nova sessão válida gravada. | Solicitação do usuário; `projects/auth-service/docs/api.md`. |
| RF-005 | Quando a tentativa de login não puder ser concluída por falha de comunicação ou resposta sem o contrato esperado, o frontend deve manter o usuário em `/login`, não persistir sessão e informar que o login não foi concluído. | Dado que a requisição falha ou retorna sucesso sem token ou expiração futura utilizável, quando o processamento terminar, então a rota continua sendo `/login` e não há sessão válida criada. | Necessário para o objetivo de autenticação; contrato de sucesso em `projects/auth-service/docs/api.md`. |
| RF-006 | Ao acessar `/`, o frontend deve considerar a sessão válida somente se `accessToken` estiver presente e o instante de `expiresAt` armazenado for estritamente posterior ao instante atual. | Dado um token e uma expiração futura no `localStorage`, quando o usuário abre `/`, então permanece em `/`; dado token ausente, expiração inválida ou expirada, então a sessão é inválida. | Solicitação do usuário; DEC-001. |
| RF-007 | Ao acessar `/` sem sessão válida, o frontend deve redirecionar automaticamente para `/login`. | Dado que não há token válido conforme RF-006, quando o usuário acessa `/`, então a rota ativa passa a ser `/login`. | Solicitação do usuário; DEC-002. |
| RF-008 | A sessão válida persistida no `localStorage` deve ser reutilizada após o navegador ser fechado e aberto novamente, enquanto `expiresAt` ainda estiver no futuro. | Dado que um login bem-sucedido gravou a sessão e o navegador foi fechado, quando ele for reaberto e `/` for acessada antes da expiração, então o usuário permanece em `/` sem novo login. | Solicitação do usuário; DEC-003. |

## Requisitos não funcionais

| ID | Requisito | Critério de aceitação | Evidência ou origem |
| --- | --- | --- | --- |
| RNF-001 | A URL do `auth-service` usada pelo frontend deve ser definida conforme o ambiente de execução, sem incorporar segredos no código cliente. | Em ambiente local, a requisição é enviada à instância configurada do serviço; em outro ambiente, a URL pode ser alterada por configuração de implantação. | Integração externa; `projects/auth-service/docs/api.md`. |
| RNF-002 | A tela de login deve expor rótulos associados aos campos, permitir envio por teclado e tornar as mensagens de erro percebíveis por tecnologias assistivas. | Com navegação por teclado, o foco alcança e opera e-mail, senha e envio; leitor de tela anuncia os rótulos e o resultado de erro apresentado. | Necessidade de acessibilidade da nova tela. |
| RNF-003 | O frontend não deve registrar `password` nem `accessToken` em mensagens, telemetria ou logs de cliente. | Ao revisar os mecanismos de mensagem e diagnóstico introduzidos, não há valor de senha ou token em suas saídas. | Segurança da integração de autenticação. |

## Decisões tomadas

| ID | Data e hora (UTC) | Decisão | Origem | Impacto |
| --- | --- | --- | --- | --- |
| DEC-001 | 2026-09-11 02:09:35 UTC | A validade da sessão no frontend é definida somente pela presença do token e por `expiresAt` posterior ao instante atual. | Usuário | RF-003, RF-005 e RF-006 não exigem validação remota ou inspeção adicional do JWT. |
| DEC-002 | 2026-09-11 02:09:35 UTC | O acesso a `/` sem token válido deve ser redirecionado automaticamente para `/login`. | Usuário | RF-007 adiciona a guarda da rota inicial. |
| DEC-003 | 2026-09-11 02:09:35 UTC | Token e expiração devem ser persistidos em `localStorage` para sobreviver ao fechamento do navegador. | Usuário | RF-003 e RF-008 definem a persistência e a restauração de sessão. |

## Impactos, dependências e migrações

- O `auth-service` deve estar acessível a partir do navegador e atender ao contrato `POST /api/auth/login` documentado.
- A política CORS do `auth-service` precisa permitir a origem do frontend. A documentação operacional do serviço registra que as origens de desenvolvimento atualmente configuradas não incluem `https://localhost:7270` nem `http://localhost:7271`, que são as portas do perfil local do frontend. Esse ajuste é uma dependência externa e não altera o escopo deste projeto.
- Não há migração de dados de servidor. A mudança introduz duas informações de sessão no armazenamento local do navegador: token e expiração.
- O `auth-service` não fornece renovação nem revogação de token no estado documentado; por isso, a expiração local encerra a validade considerada neste escopo.

## Fora do escopo e suposições validadas

- Não há logout, cadastro, recuperação de senha, refresh token ou revogação.
- A proteção solicitada é exclusivamente da rota `/`; outras rotas existentes não são alteradas por este requisito.
- A expiração recebida é um instante serializado com deslocamento e deve ser comparada como instante, não como horário local. O contrato a informa em UTC.
- Um token cujo `expiresAt` seja igual ao instante atual é inválido, pois a condição solicitada é estar no futuro.

## Rastreabilidade e critérios de aceitação por cenário

| Cenário | Requisitos cobertos | Resultado esperado |
| --- | --- | --- |
| Login com credenciais aceitas | RF-001, RF-002, RF-003 | A sessão é persistida e o usuário chega a `/`. |
| Login com credenciais recusadas | RF-001, RF-002, RF-004 | A mensagem do serviço é exibida e a sessão não é criada. |
| Login com dados inválidos | RF-001, RF-002, RF-004 | Os erros de validação retornados são exibidos em `/login`. |
| Falha de rede ou retorno malformado | RF-005 | O usuário permanece em `/login` e nenhuma sessão válida é persistida. |
| Acesso a `/` sem sessão, com expiração inválida ou expirada | RF-006, RF-007 | O usuário é direcionado a `/login`. |
| Reabertura com sessão ainda válida | RF-006, RF-008 | O usuário acessa `/` sem realizar novo login. |

## Histórico de alterações

| Data e hora (UTC) | Alteração |
| --- | --- |
| 2026-09-11 02:09:35 UTC | Criado levantamento de requisitos para login persistente no frontend. |

