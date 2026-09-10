# Requisitos — tela de login no frontend

## Contexto, objetivo e escopo

O `Frontend/` é uma aplicação Blazor WebAssembly que ainda contém apenas as telas padrão do template. O `AGENTS.md` do projeto determina que a autenticação é realizada pelo `auth-service`; porém, não há no repositório uma integração, contrato ou configuração desse serviço.

O objetivo é disponibilizar uma tela de login no frontend para que o usuário se autentique antes de acessar as funcionalidades financeiras futuras. Este levantamento cobre o comportamento observável da tela e sua integração com o serviço de autenticação. Não cobre cadastro, recuperação de senha, gestão de usuários, renovação de sessão nem autorização das funcionalidades de negócio.

## Fluxo atual

Não existe fluxo de login (`N/A`). Evidências consultadas:

- `Frontend/Pages/Home.razor` apresenta a página padrão na rota `/`;
- `Frontend/App.razor` aplica `MainLayout` a todas as rotas conhecidas;
- `Frontend/Program.cs` registra somente um `HttpClient` com base na origem do frontend;
- `Frontend/` não contém rota de login, estado de autenticação, armazenamento de sessão, cliente do `auth-service` ou controle de acesso;
- `AGENTS.md` do projeto informa que o frontend deve consultar o `auth-service` para autenticação.

Não foi localizado contrato, endpoint, ambiente ou credencial de desenvolvimento do `auth-service`. Portanto, qualquer formato de requisição, resposta e token permanece pendente.

## Fluxo desejado e mudanças

1. Um visitante abre a rota de login e vê os campos de identificação e senha, além da ação para entrar.
2. O frontend valida os dados obrigatórios localmente. Dados inválidos não são enviados.
3. Com dados válidos, o frontend envia as credenciais ao `auth-service` usando o contrato acordado.
4. Enquanto a autenticação está em andamento, a tela evita reenvios e informa o estado ao usuário.
5. Em sucesso, o frontend estabelece a sessão conforme o contrato, redireciona o usuário ao destino permitido e não expõe a senha.
6. Em falha de credenciais ou indisponibilidade, a tela mostra uma mensagem adequada sem revelar informações sensíveis e permite nova tentativa.

Em relação ao estado atual, serão adicionados: uma rota/tela de login, validação de formulário, integração com o `auth-service`, estado de sessão e navegação pós-login. A navegação padrão e a API de negócio devem ser preservadas até que regras de proteção de rotas sejam definidas.

## Requisitos funcionais

| ID | Requisito | Critério de aceitação | Evidência ou origem |
| --- | --- | --- | --- |
| RF-001 | O frontend deve disponibilizar uma rota de login acessível a usuários não autenticados. | Dado um visitante, quando acessar a rota de login, então deve visualizar o formulário sem depender de sessão prévia. | Mudança solicitada; fluxo atual não tem rota de login. |
| RF-002 | O formulário deve solicitar identificador de acesso e senha, marcando ambos como obrigatórios. | Dado o envio com qualquer campo vazio, quando o usuário tentar entrar, então a tela deve indicar o campo obrigatório e não chamar o serviço de autenticação. | Mudança solicitada. |
| RF-003 | A tela deve ocultar visualmente o valor da senha durante a digitação. | Dado o preenchimento da senha, quando o usuário digitar caracteres, então eles não devem ser exibidos em texto aberto. | Proteção mínima de dados de autenticação. |
| RF-004 | Com dados válidos, o frontend deve encaminhar as credenciais ao `auth-service` conforme contrato de autenticação aprovado. | Dado identificador e senha preenchidos, quando o usuário enviar o formulário, então deve ocorrer uma única solicitação ao endpoint e no formato definidos no contrato. | `AGENTS.md` do projeto define o `auth-service` como responsável pela autenticação. |
| RF-005 | Durante a solicitação de autenticação, a tela deve informar o processamento e impedir envios duplicados. | Dado um envio válido em andamento, quando o usuário tentar enviar novamente, então uma nova solicitação não deve ser criada até a conclusão da anterior. | Mudança solicitada. |
| RF-006 | Quando o `auth-service` confirmar a autenticação, o frontend deve criar o estado de sessão com os artefatos previstos no contrato e redirecionar o usuário a um destino permitido. | Dado retorno de sucesso do serviço, quando a resposta for processada, então a sessão deve ficar disponível à aplicação e a navegação deve ocorrer sem expor a senha. | Mudança solicitada; formato da sessão pendente. |
| RF-007 | Quando as credenciais forem rejeitadas, a tela deve informar que não foi possível autenticar e permitir nova tentativa. | Dado retorno de credenciais inválidas, quando a resposta for processada, então a tela deve apresentar mensagem não enumerativa, manter o identificador preenchido e limpar ou não persistir a senha após a tentativa. | Mudança solicitada; proteção contra enumeração de contas. |
| RF-008 | Quando o serviço de autenticação estiver indisponível ou retornar erro não tratado, a tela deve apresentar mensagem de indisponibilidade e permitir nova tentativa. | Dado falha de rede ou resposta inesperada, quando a solicitação terminar, então a tela deve sair do estado de processamento e não deve criar sessão. | Mudança solicitada. |

## Requisitos não funcionais

| ID | Requisito | Critério de aceitação | Evidência ou origem |
| --- | --- | --- | --- |
| RNF-001 | A tela deve ser utilizável por teclado, com rótulos associados aos campos, foco visível e mensagens de erro anunciáveis por tecnologias assistivas. | Verificação manual por navegação apenas com teclado e inspeção de associação entre rótulos, campos e mensagens. | Aplicável a todo formulário de acesso. |
| RNF-002 | A senha não deve ser registrada em logs, exibida em mensagens de erro ou armazenada pelo frontend após a solicitação. | Inspeção do fluxo de login e teste de falha confirmam ausência da senha no console, tela e armazenamento do navegador. | Proteção de dados de autenticação. |
| RNF-003 | A comunicação com o `auth-service` deve usar HTTPS em ambientes publicados. | Inspeção da configuração publicada confirma URL HTTPS; tentativas HTTP devem ser rejeitadas ou não configuradas. | Proteção de credenciais em trânsito. |
| RNF-004 | O tempo máximo aceitável para a resposta visual de processamento e o tempo limite da solicitação devem ser definidos antes da implementação. | Pendência resolvida com valores mensuráveis e teste automatizável ou de integração. | Não há contrato nem SLO do `auth-service` no repositório. |

## Rastreabilidade e validação

- RF-001 a RF-003: validar a apresentação e as validações locais do formulário.
- RF-004 a RF-006: validar com um ambiente controlado do `auth-service` e respostas de sucesso documentadas.
- RF-007: validar resposta de credenciais inválidas, confirmando a ausência de detalhes sobre a conta.
- RF-008: simular indisponibilidade e resposta inesperada; confirmar que não há sessão criada.
- RNF-001 a RNF-003: executar verificações manuais de acessibilidade, inspeção de armazenamento/logs e configuração de transporte.

## Fora do escopo, suposições e questões em aberto

Fora do escopo: cadastro, recuperação/alteração de senha, MFA, logout, renovação de token, proteção das rotas existentes, controle de perfis, criação de API ou mudança do `auth-service`.

Suposição de trabalho: o identificador de acesso pode ser tratado como e-mail ou nome de usuário até a definição do contrato, sem impor validação de formato no cliente.

Questões em aberto que bloqueiam a integração efetiva:

1. Qual é a URL por ambiente, endpoint, método HTTP e esquema de autenticação do `auth-service`?
2. Quais são os nomes/formato dos campos de entrada e a estrutura das respostas de sucesso, credenciais inválidas e erro?
3. Qual artefato de sessão é devolvido, onde pode ser armazenado e como deve ser renovado ou removido?
4. Qual rota deve receber o usuário após o login e como deve ser preservado um destino originalmente solicitado?
5. Quais mensagens aprovadas e qual tempo limite devem ser usados?

## Impactos, dependências, migrações e decisões pendentes

A implementação depende do contrato e da configuração por ambiente do `auth-service`. Também exige uma decisão de sessão e de redirecionamento pós-login. Não há migração de dados identificada. A documentação do frontend deverá registrar a configuração necessária quando o contrato estiver disponível.

## Histórico de alterações

| Data e hora (UTC) | Alteração |
| --- | --- |
| 2026-09-10 04:10:05 UTC | Levantamento inicial dos requisitos da tela de login no frontend. |
