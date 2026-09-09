# Frontend Blazor WebAssembly/PWA

## Escopo atual

O diretório `Frontend/` contém uma aplicação Blazor WebAssembly direcionada ao .NET 10. Ela é o cliente executado no navegador. A rota inicial implementa o login no `auth-service`; as demais páginas de exemplo do template ainda permanecem no projeto.

O ponto de entrada é `Frontend/Program.cs`. Ele registra `App` no elemento `#app`, adiciona o `HeadOutlet`, disponibiliza um `HttpClient` cujo endereço-base é a própria origem em que o frontend foi servido e registra `AuthServiceClient`. Este último usa a URL configurada em `AuthService:BaseUrl`, cujo valor padrão é `https://localhost:7070/`.

## Páginas e rotas

| Rota | Componente | Comportamento atual |
| --- | --- | --- |
| `/` | `Pages/Home.razor` | Exibe o formulário de login e é a tela inicial da aplicação. |
| `/counter` | `Pages/Counter.razor` | Exibe um contador local iniciado em zero e um botão que o incrementa. O estado não é persistido. |
| `/weather` | `Pages/Weather.razor` | Carrega e mostra uma tabela de previsões de exemplo. |
| `/not-found` | `Pages/NotFound.razor` | Exibe a mensagem de conteúdo não encontrado. |
| Qualquer rota não mapeada | `Pages/NotFound.razor` | O `Router` de `App.razor` direciona a página não encontrada para esse componente. |

As páginas são renderizadas com `MainLayout`, exceto quando um componente definir outro layout. O roteador também move o foco para o primeiro `h1` após a navegação.

## Login

A página inicial solicita e-mail e senha e executa validação local para ambos os campos antes de enviar um JSON para `POST /api/auth/login` do `auth-service`. Erros de credenciais, de validação HTTP ou de conexão são mostrados na própria tela. Quando a resposta é `200 OK`, o navegador apresenta o alerta “Login realizado com sucesso.”.

O `accessToken` retornado no sucesso não é lido, persistido nem enviado a outras chamadas neste estágio. Portanto, ainda não há sessão autenticada, renovação de token ou autorização de rotas no frontend.

## Layout e navegação

`Layout/MainLayout.razor` organiza a interface em uma barra lateral e uma área principal. A barra lateral usa `Layout/NavMenu.razor`, que apresenta os links **Home**, **Counter** e **Weather**. Em telas menores, o botão de menu alterna a abertura e o fechamento da navegação.

O layout também inclui um link externo “About” para a documentação do ASP.NET Core. O HTML base, em `wwwroot/index.html`, carrega Bootstrap, os estilos próprios da aplicação, o CSS isolado compilado (`Frontend.styles.css`) e uma interface de carregamento/erro do Blazor.

## Dados exibidos em Weather

A página `/weather` injeta o `HttpClient` local e faz uma requisição GET relativa para `sample-data/weather.json`. Portanto, o endereço resolvido é o mesmo host e caminho-base do frontend, e o conteúdo vem de `wwwroot/sample-data/weather.json`.

Esse arquivo JSON estático contém cinco previsões de exemplo, com data, temperatura em Celsius e resumo em inglês. A temperatura em Fahrenheit é calculada pelo próprio componente. Não há chamada à `Api/`, endpoint financeiro, autenticação, nem integração remota nessa página ou nos demais componentes do frontend atual.

## Manifesto e instalação

`wwwroot/manifest.webmanifest` é referenciado por `index.html` e define o aplicativo como:

- nome e nome curto: `Frontend`;
- URL inicial e identificador: `./`;
- modo de exibição: `standalone`;
- cores de fundo e tema: `#ffffff` e `#03173d`;
- ícones PNG de 192×192 e 512×512 pixels.

O documento HTML também referencia esses ícones como `apple-touch-icon`. A disponibilidade de instalação depende do navegador e de seus critérios para PWAs, além de a aplicação estar servida em um contexto seguro quando isso for exigido pelo navegador.

## Service workers e uso offline

`index.html` registra sempre `service-worker.js`, com `updateViaCache: 'none'`. O arquivo efetivamente distribuído varia conforme o modo de build definido em `Frontend.csproj`:

| Modo | Arquivo usado | Comportamento |
| --- | --- | --- |
| Desenvolvimento (`dotnet run`) | `wwwroot/service-worker.js` | Instala um listener de `fetch` que não responde a requisições. O navegador usa a rede normalmente e não há suporte offline deliberadamente. |
| Publicação (`dotnet publish`) | `wwwroot/service-worker.published.js` | Importa o manifesto gerado `service-worker-assets.js`, cria um cache versionado e armazena os ativos publicados compatíveis antes da ativação. |

No worker de publicação, DLLs, WASM, HTML, JavaScript, JSON, CSS, fontes e imagens compatíveis são pré-carregados conforme o manifesto de ativos; o próprio `service-worker.js` fica fora desse conjunto. Na ativação, caches antigos com o prefixo `offline-cache-` são removidos.

Para requisições GET, o worker publicado tenta servir a resposta do cache e recorre à rede se ela não estiver lá. Em navegações de rotas do cliente, ele pode devolver o `index.html` em cache, permitindo que o roteador do Blazor trate a rota. Assim, após uma instalação/publicação bem-sucedida e o cache inicial, a interface e os dados estáticos incluídos no manifesto — como `sample-data/weather.json` — podem funcionar sem rede. Recursos ausentes do cache continuam dependendo da rede; não existe uma página offline específica nem sincronização de dados.

## Execução local

Com o SDK .NET compatível com o `TargetFramework` `net10.0` instalado, execute a partir da raiz do projeto:

```powershell
dotnet run --project Frontend
```

O perfil `frontend` em `Frontend/Properties/launchSettings.json` abre o navegador e usa, no ambiente `Development`, estas URLs:

- `https://localhost:8181`
- `http://localhost:8282`

Essas portas são valores do perfil de desenvolvimento e podem ser substituídas por argumentos ou configurações do ambiente ao executar o projeto. O `HttpClient` configurado para arquivos locais continuará apontando para a origem em uso. Para login, `AuthServiceClient` usa `AuthService:BaseUrl`; mantenha esse valor e as origens CORS do `auth-service` alinhados ao ambiente em que o frontend for servido.

## Limitações conhecidas

As páginas Counter e Weather, a nomenclatura do projeto e a descrição textual de Weather são herdadas do template. Não há telas ou dados financeiros, armazenamento/renovação de token, nem integração com a API financeira do projeto.

## Histórico de alterações

| Data e hora (UTC) | Alteração |
| --- | --- |
| 2026-09-08 00:00:00 UTC | Documentado o login inicial integrado ao auth-service e a ausência intencional de persistência do token. |
