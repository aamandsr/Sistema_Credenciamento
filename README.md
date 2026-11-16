# Sistema de Credenciamento

Aplicação web para organizar o credenciamento de eventos: cadastre congressistas, importe inscrições por CSV e acompanhe a impressão de etiquetas em uma tela de busca e paginação.

> **Tecnologias:** ASP.NET Core MVC · .NET 11 · MySQL · ADO.NET · Razor

## ✨ Funcionalidades

- Cadastro, edição e exclusão de congressistas.
- Importação em lote por CSV UTF-8 (até 5 MB), com validação de cabeçalho, campos e valores.
- Busca por nome ou nome no crachá, filtro por tipo e paginação no navegador.
- Impressão permitida para pagamentos confirmados ou cortesia; pagamentos pendentes são bloqueados.
- Registro das etiquetas em `etiquetas.txt` na Área de Trabalho do usuário que executa a aplicação.
- Proteção contra registro duplicado de impressão.

## 🧰 Requisitos

- .NET SDK 11.
- MySQL em execução.
- Acesso ao banco e permissão para criar a tabela `congressista`.

## 🚀 Começando

### 1. Prepare o banco

Conecte-se ao MySQL por um cliente de sua preferência e execute:

```sql
CREATE DATABASE sistema_credenciamento
    CHARACTER SET utf8mb4
    COLLATE utf8mb4_unicode_ci;

USE sistema_credenciamento;

CREATE TABLE congressista (
    id INT NOT NULL AUTO_INCREMENT,
    nome VARCHAR(255) NOT NULL,
    nome_cracha VARCHAR(255) NOT NULL,
    tipo VARCHAR(50) NOT NULL,
    pagamento VARCHAR(50) NOT NULL,
    material_impresso BOOLEAN NOT NULL DEFAULT FALSE,
    empresa VARCHAR(255) NULL,
    PRIMARY KEY (id)
);
```

O projeto usa o conector `MySql.Data` e ADO.NET diretamente; não utiliza Entity Framework. A estrutura acima corresponde às colunas consultadas pelo repositório em [`Data/CongressistaRepository.cs`](./Data/CongressistaRepository.cs).

### 2. Configure a conexão

Na pasta do projeto, salve a connection string com .NET User Secrets. Assim, a senha fica fora dos arquivos versionados:

```bash
dotnet user-secrets set "ConnectionStrings:MySql" "Server=localhost;Port=3306;Database=sistema_credenciamento;User ID=SEU_USUARIO;Password=SUA_SENHA;"
```

Substitua `SEU_USUARIO` e `SUA_SENHA` pelos dados do seu ambiente MySQL. A aplicação espera a chave `ConnectionStrings:MySql` e informa explicitamente quando ela não está configurada.

### 3. Restaure e execute

```bash
dotnet restore
dotnet run
```

Abra no navegador o endereço informado pelo `dotnet run`. O perfil local do projeto, quando usado, inicia em `http://localhost:5074`.

## 📄 Importação CSV

O arquivo deve estar em UTF-8 e pode usar vírgula ou ponto e vírgula como delimitador. Os cabeçalhos não diferenciam maiúsculas de minúsculas nem acentos, espaços e underscores. As colunas obrigatórias são `nome`, `nome_cracha`, `tipo` e `pagamento`; `empresa` é opcional.

Tipos aceitos: `Congressista` e `Palestrante`. Situações aceitas: `Sim`, `Não` e `Cortesia`.

```csv
nome,nome_cracha,tipo,pagamento,empresa
Maria da Silva,Maria Silva,Congressista,Sim,Empresa Exemplo
João Souza,João Souza,Palestrante,Cortesia,
```

A aplicação valida o arquivo inteiro antes de gravar os registros. Novos cadastros importados começam com a etiqueta marcada como não impressa.

## 🗂️ Organização do projeto

| Pasta/arquivo | Responsabilidade |
| --- | --- |
| [`Controllers/`](./Controllers/) | Recebe as requisições MVC e coordena as operações da aplicação. |
| [`Data/`](./Data/) | Cria conexões MySQL e executa as consultas parametrizadas. |
| [`Models/`](./Models/) | Define os dados de congressistas e da página de erro. |
| [`Services/`](./Services/) | Interpreta CSV e registra as etiquetas no arquivo local. |
| [`Views/`](./Views/) | Renderiza as páginas Razor e os formulários. |
| [`wwwroot/`](./wwwroot/) | Contém CSS, JavaScript e bibliotecas estáticas da interface. |
| [`Program.cs`](./Program.cs) | Configura serviços, pipeline HTTP e rota padrão. |

## 🖨️ Como funciona a impressão

Ao solicitar uma impressão, o sistema busca o congressista, verifica a situação do pagamento e se a etiqueta já foi impressa. Em seguida, acrescenta uma linha com o nome do crachá e data/hora a `etiquetas.txt` na Área de Trabalho do usuário que está executando o servidor e atualiza `material_impresso` no MySQL.

Em uma implantação hospedada, essa Área de Trabalho pertence ao servidor, não necessariamente ao computador do operador. A aplicação registra a impressão em arquivo; o envio físico à impressora não é automatizado.

### Objetivo final

O objetivo final do sistema é enviar os nomes diretamente para uma impressora térmica de etiquetas, automatizando a impressão a partir do cadastro dos congressistas.

### Próxima melhoria

A próxima evolução planejada é permitir controlar pelo próprio sistema algumas configurações da impressão, começando pelo tamanho da etiqueta. Esse controle ainda não está disponível na versão atual.

