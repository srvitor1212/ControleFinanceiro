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
