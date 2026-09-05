# ControleFinanceiro

## Objetivo

Implementar e evoluir o sistema respeitando a arquitetura Clean Architecture, mantendo as responsabilidades e o sentido das dependências entre as camadas.

## Arquitetura e dependências

- `Domain` é o núcleo do sistema. Não deve referenciar nenhum outro projeto, framework de infraestrutura ou mecanismo externo.
- `Application` contém os casos de uso e regras de orquestração da aplicação. Pode depender de `Domain`, mas não deve depender de `API`, `Infra.Data` ou outros detalhes de infraestrutura.
- `Infra.Data` implementa persistência e acesso a dados. Pode depender de `Domain`, mas não deve ser referenciada diretamente por `API`.
- `Infra.Connect` é o composition root da aplicação. É responsável por conectar e registrar as implementações de `Application`, `Domain` e `Infra.Data`, incluindo a configuração de injeção de dependência.
- `API` é a camada de entrada e exposição HTTP. Deve depender de `Infra.Connect` para obter a composição da aplicação e não deve criar dependências diretas com detalhes de infraestrutura.
- As dependências devem apontar para dentro: detalhes externos podem depender do núcleo, mas o núcleo nunca deve depender de detalhes externos.
- Não introduza dependências circulares nem referências entre camadas que violem essas regras.