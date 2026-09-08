# Controle Financeiro

## Objetivo

Aplicativo para registrar e controlar dívidas pessoais.

## Estrutura da solução

A solução é composta por serviços separados por responsabilidade:

- `Api/`: API HTTP construída com ASP.NET Core.
- `Frontend/`: aplicação PWA construída com Blazor WebAssembly.

Os arquivos de solução ficam na raiz do projeto, em `ControleFinanceiro.slnx`.

## Regras de desenvolvimento

- Mantenha a separação entre apresentação, comunicação HTTP e regras de negócio.
- A `Api/` deve concentrar regras de negócio, validações, persistência e contratos expostos por HTTP.
- O `Frontend/` deve concentrar a interface, o estado da tela e a experiência do usuário; não replique nele regras de negócio da API.
- Atualize o `AGENTS.md` ou a documentação em `docs/` quando um novo serviço for incluido no projeto.

## Autenticação e autorização

- O `auth-service` é responsável por autenticar usuários e emitir os tokens de acesso.
- A `Api/` não deve confiar apenas no frontend. Toda requisição protegida deve validar o token recebido, incluindo assinatura, emissor, audiência e expiração.
- A API deve aplicar autorização nos endpoints com base nas claims/permissões do token.
- Responda `401 Unauthorized` quando o token estiver ausente ou inválido e `403 Forbidden` quando o usuário não tiver permissão.
- O `Frontend/` deve obter e renovar o token por meio do `auth-service` e enviá-lo nas chamadas à API usando:
