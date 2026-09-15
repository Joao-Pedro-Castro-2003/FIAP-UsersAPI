# UsersAPI — Fase 3

Cadastro de usuários comuns, login JWT e evento UserCreatedEvent. Banco SQLite e hash de senha com PasswordHasher. O cadastro público rejeita isAdmin=true.

O administrador inicial é criado somente em banco vazio, usando Bootstrap__AdminEmail e Bootstrap__AdminPassword. Esses valores são gerados pelo script de orquestração e não são versionados.

## Configuração

| Variável | Uso |
|---|---|
| ConnectionStrings__Db | Ex.: Data Source=/app/data/users.db |
| Jwt__Key | Chave de pelo menos 32 bytes, compartilhada com Kong e CatalogAPI |
| Jwt__Issuer / Jwt__Audience | FiapCloudGames no ambiente local |
| RabbitMq__Host / RabbitMq__Username / RabbitMq__Password | Conexão configurável |
| Bootstrap__AdminEmail / Bootstrap__AdminPassword | Admin inicial; senha de pelo menos 12 caracteres |

Métricas em /metrics e logs JSON no console. Liveness em /health/live e consulta do SQLite em /health/ready. Esses caminhos são internos e não publicados pelo gateway.

## Executar

Use o [guia central de orquestração](../FIAP-CloudGames-Orchestration/README.md) no workspace. No GitHub, consulte o repositório FIAP-CloudGames-Orchestration na mesma conta.

```powershell
dotnet test UsersAPI.sln
```

Os fontes Data são versionados; arquivos *.db e auxiliares SQLite são ignorados. A criação de um banco vazio não recupera os registros antigos.
