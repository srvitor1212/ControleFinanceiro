# ControleFinanceiro

Este projeto possui duas aplicações:

- API: `Api/`
- Frontend: `Frontend/`

## API

A aplicação em `Api/` é o backend do sistema. Ela concentra as regras de negócio, o acesso e as consultas ao banco de dados, além de expor as operações necessárias para o frontend.

## Frontend

A aplicação em `Frontend/` é a interface do usuário. Para autenticação, ela consulta o `auth-service`; para as demais operações do sistema, seu backend é a API em `Api/`.
