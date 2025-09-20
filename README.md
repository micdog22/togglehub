
# ToggleHub (C# • .NET 8)

Serviço de Feature Flags simples, rápido e extensível. Útil para ativar/desativar recursos por rollout percentual, por usuário, ou por atributos (ex.: país, plano, appVersion). Inclui:
- Biblioteca de avaliação (`ToggleHub.Core`)
- API REST com Swagger (`ToggleHub.Api`)
- Persistência em arquivo JSON (opcional)
- Testes (xUnit), Dockerfile, CI via GitHub Actions
- Suporte opcional a API Key para rotas de administração

## Sumário
- Arquitetura
- Recursos
- Modelo de dados
- Como executar
- Endpoints
- Exemplos de uso
- Persistência
- Segurança
- Roadmap
- Contribuindo
- Licença

## Arquitetura

```
togglehub/
├─ src/ToggleHub.Core/
│  ├─ Models.cs
│  ├─ Engine.cs
│  └─ Store.cs
├─ src/ToggleHub.Api/
│  ├─ Program.cs
│  ├─ Controllers/
│  │  ├─ FlagsController.cs
│  │  └─ EvaluateController.cs
│  └─ Auth/ApiKeyAttribute.cs
├─ tests/ToggleHub.Tests/
│  ├─ CoreTests.cs
│  └─ ApiTests.cs
├─ Dockerfile
├─ .github/workflows/dotnet.yml
├─ ToggleHub.sln
└─ README.md
```

## Recursos
- Flags booleanas com `enabled`, `rolloutPercent` (0–100), `includeUserIds`, `excludeUserIds` e `matchAnyAttributes`.
- Avaliação determinística por hash (SHA-256).
- Motivos de decisão: `flag_disabled`, `excluded`, `included`, `attributes_match`, `rollout_on`, `rollout_off`, `flag_not_found`.

## Modelo de dados
Exemplo de Flag:
```json
{
  "key": "new-home",
  "description": "Nova homepage gradativa",
  "enabled": true,
  "rolloutPercent": 25,
  "includeUserIds": ["vip-123"],
  "excludeUserIds": ["blocked-999"],
  "matchAnyAttributes": {
    "country": ["BR", "PT"],
    "plan": ["pro"]
  }
}
```

Avaliação (`POST /api/evaluate`):
```json
{
  "flagKey": "new-home",
  "userId": "u-42",
  "attributes": { "country": "BR", "plan": "free" }
}
```

Retorno:
```json
{ "flagKey": "new-home", "enabled": true, "reason": "attributes_match" }
```

## Como executar

Pré-requisitos: .NET 8 SDK

```bash
dotnet restore
dotnet build
dotnet test

dotnet run --project src/ToggleHub.Api
# Swagger: http://localhost:5210/swagger
```

### Docker

```bash
docker build -t togglehub:latest .
docker run --rm -p 5210:8080 -v $(pwd)/data:/app/data togglehub:latest
```

## Endpoints
Admin (CRUD):
- GET /api/flags
- GET /api/flags/{key}
- POST /api/flags
- PUT /api/flags/{key}
- DELETE /api/flags/{key}
- POST /api/flags/{key}/toggle?enabled=true|false
- POST /api/flags/import
- GET /api/flags/export

Runtime:
- POST /api/evaluate

## Persistência
- Armazenamento em memória + arquivo JSON opcional em `./data/flags.json` (configurável via `Data:Path`).
- Salvamento automático a cada mudança (create/update/delete/toggle/import).

## Segurança
- Se `TOGGLEHUB_API_KEY` estiver definido, as rotas admin exigem header `X-API-Key`.
- `/api/evaluate` é pública por padrão.

## Roadmap
- Variantes com pesos (A/B/C).
- Segmentos nomeados.
- Provedor Redis/SQL.
- SDKs clientes.

## Licença
MIT. Veja `LICENSE`.
