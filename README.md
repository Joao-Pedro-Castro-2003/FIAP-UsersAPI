# FIAP Cloud Games - UsersAPI

Microsservico responsavel pelo cadastro de usuarios, autenticacao com JWT e controle basico de autorizacao da plataforma Cloud Games.

Este servico faz parte do Tech Challenge Fase 2 da FIAP e foi separado do monolito original para compor uma arquitetura baseada em microsservicos.

## Responsabilidades

- Cadastrar usuarios comuns e administradores.
- Autenticar usuarios.
- Gerar token JWT.
- Proteger rotas administrativas.
- Publicar evento de usuario criado para o RabbitMQ.

## Tecnologias

- .NET 8
- ASP.NET Core Web API
- Entity Framework Core
- SQLite
- JWT Bearer Authentication
- RabbitMQ
- MassTransit
- Swagger
- Docker

## Principais rotas

| Metodo | Rota | Descricao |
| --- | --- | --- |
| POST | `/api/users` | Cadastra um novo usuario |
| GET | `/api/users/{id}` | Consulta usuario por id, rota restrita a admin |
| POST | `/api/auth/login` | Autentica usuario e retorna token JWT |

## Exemplo de cadastro

```json
{
  "name": "Admin",
  "email": "admin@fiap.com",
  "password": "123456",
  "isAdmin": true
}
```

Para usuario comum, envie `isAdmin` como `false`.

## Evento publicado

Ao cadastrar um usuario, a API publica o evento:

```text
UserCreatedEvent
```

Esse evento e consumido pela NotificationsAPI para simular o envio de e-mail de boas-vindas.

## Variaveis de ambiente

| Variavel | Descricao | Exemplo |
| --- | --- | --- |
| `ASPNETCORE_ENVIRONMENT` | Ambiente da aplicacao | `Development` |
| `ConnectionStrings__DefaultConnection` | String de conexao do SQLite | `Data Source=/data/users.db` |
| `Jwt__Key` | Chave usada para assinar o token JWT | `fiap-cloud-games-secret-key` |
| `Jwt__Issuer` | Emissor do token JWT | `FIAP.CloudGames` |
| `Jwt__Audience` | Audiencia do token JWT | `FIAP.CloudGames` |
| `RabbitMq__Host` | Host do RabbitMQ | `rabbitmq` |
| `RabbitMq__Username` | Usuario do RabbitMQ | `guest` |
| `RabbitMq__Password` | Senha do RabbitMQ | `guest` |

## Executando localmente

```bash
dotnet restore
dotnet run --project src/UsersAPI/UsersAPI.csproj
```

Swagger:

```text
http://localhost:5001/swagger
```

## Executando com Docker

```bash
docker build -t fiap-users-api:latest .
docker run -p 5001:8080 fiap-users-api:latest
```

No projeto completo, a execucao recomendada e pelo repositorio de orquestracao, usando Docker Compose ou Kubernetes.

