# Configuração do banco de dados

Este projeto usa ADO.NET com o conector oficial `MySql.Data`. Não usa Entity Framework nem `DbContext`.

## Preparar o MySQL

1. Inicie o servidor MySQL.
2. No phpMyAdmin, abra a aba **SQL** e execute o conteúdo de [`database/schema.sql`](./database/schema.sql). Isso cria o banco `sistema_credenciamento` e a tabela `congressistas`.

## Configurar a conexão sem expor a senha

O projeto lê a connection string `ConnectionStrings:MySql`. Em desenvolvimento, configure-a com User Secrets no terminal, dentro da pasta do projeto:

```powershell
dotnet user-secrets set "ConnectionStrings:MySql" "Server=localhost;Port=3306;Database=sistema_credenciamento;User ID=SEU_USUARIO;Password=SUA_SENHA;"
```

Substitua `SEU_USUARIO` e `SUA_SENHA` pelos dados do seu MySQL. O segredo fica no perfil local do usuário, fora do repositório, e não deve ser adicionado ao GitHub.

## Acesso aos dados

`CongressistaRepository` é o exemplo de ADO.NET usado pelo projeto:

- `ListarAsync()` consulta os congressistas.
- `AdicionarAsync()` insere um congressista usando parâmetros SQL e retorna o ID criado.

O repositório pode ser injetado em um controller pelo construtor. As consultas usam parâmetros para que os valores recebidos não sejam concatenados diretamente no SQL.
