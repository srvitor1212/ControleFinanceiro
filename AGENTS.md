# ControleFinanceiro

## Arquitetura e dependências

Implemente respeitando a Clean Architecture e o sentido das dependências:

- `Domain`: núcleo do sistema; não referencia nenhum outro projeto ou detalhe externo.
    - Essa camada deve conter por exemplo: Models, Enums, Interface de Repository.
- `Application`: casos de uso; referencia `Domain`.
    - Essa camada deve conter por exemplo: Services, Dtos, Adapters.
- `Infra.Data`: persistência e acesso a dados; referencia `Domain`.
    - Essa camada deve conter por exemplo: Context, Configurações de entidades, Migrations, Repositorios.
- `Infra.Connect`: composition root; conecta e registra `Application`, `Domain` e `Infra.Data`.
    - Essa camada deve conter por exemplo: Injeção de dependecia, Injeção do banco de dados.
- `API`: entrada HTTP; referencia `Infra.Connect`.
    - Essa camada deve conter por exemplo: Controllers, Endpoints.

```text
API -> Infra.Connect -> Application -> Domain
                  -> Domain
                  -> Infra.Data -> Domain
```
