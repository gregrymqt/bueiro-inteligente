# Testes do Backend

Este diretório concentra a suíte de testes automatizados do backend do projeto Bueiro Inteligente.

A suíte usa `xUnit`, `Moq`, `FluentAssertions` e `coverlet.collector`, com referência direta ao projeto principal em [backend/backend.csproj](../backend/backend.csproj).

## Estrutura

```text
Tests/
├── backend.Tests.csproj
├── GlobalUsings.cs
└── Features/
    ├── Auth/
    ├── Drains/
    ├── Home/
    ├── Monitoring/
    └── Rows/
```

## O que esta suite cobre

- autenticação e autorização
- gerenciamento de bueiros
- conteúdo da home
- monitoramento e recebimento de medições
- sincronização com Rows

Os testes seguem a convenção de namespace `backend.Tests.Features.<Feature>` e costumam separar o comportamento por serviço, controller ou job.

## Como executar

### Rodando tudo a partir da raiz do repositório

```bash
dotnet test Tests/backend.Tests.csproj
```

### Rodando a partir da pasta `Tests`

```bash
dotnet test
```

### Rodando um arquivo ou um caso específico

```bash
dotnet test --filter FullyQualifiedName~AuthControllerTests
```

### Coletando cobertura

```bash
dotnet test Tests/backend.Tests.csproj --collect:"XPlat Code Coverage"
```

## Padrões usados nos testes

- `Arrange / Act / Assert` em praticamente todos os cenários
- `MockBehavior.Strict` para evitar chamadas inesperadas
- `FluentAssertions` para asserts mais legíveis
- `VerifyNoOtherCalls()` quando a interação com dependências precisa ser validada com rigor
- `GlobalUsings.cs` para reduzir repetição de imports entre os arquivos de teste

## Convenções de criação

Ao adicionar novos testes, siga a mesma organização por feature e mantenha o foco em comportamento observável.

- coloque testes novos em `Tests/Features/<Feature>/`
- nomeie arquivos de forma descritiva, como `AuthServiceTests.cs` ou `RowsSyncJobTests.cs`
- prefira mocks para isolar a unidade testada
- evite dependência de banco real quando um mock ou uma fixture cobrir o cenário

## Arquivos importantes

- [backend.Tests.csproj](backend.Tests.csproj)
- [GlobalUsings.cs](GlobalUsings.cs)
