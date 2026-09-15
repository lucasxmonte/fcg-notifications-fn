# FCG Notifications Function

**Azure Function serverless** para envio de notificações aos usuários da plataforma FCG (Full Cycle Gaming). Implementada com o modelo **Isolated Worker** do .NET 8 e Azure Functions v4.

## Visão Geral

| | |
|---|---|
| **Runtime** | .NET 8 — Azure Functions v4 (Isolated Worker) |
| **Porta local** | `7071` |
| **Trigger HTTP** | `POST /api/notificar` |
| **Trigger Timer** | Diário às 08:00 UTC |
| **Pré-requisito local** | Azurite (emulador Azure Storage) |

## Por que Serverless?

O envio de notificações é **esporádico** — acontece quando um pagamento é confirmado, quando um usuário se cadastra. Um servidor dedicado 24h para esse fim seria desperdício. Com Azure Functions:

- Escala sob demanda (zero instâncias quando ocioso)
- Custo por execução, não por uptime
- Isolamento da lógica de notificação dos microsserviços principais

## Estrutura do Projeto

```
fcg-notifications-fn/
├── src/
│   └── FCG.NotificationsFn/
│       ├── Functions/
│       │   ├── NotificarUsuarioFunction.cs  # HTTP trigger POST + GET health
│       │   └── DigestDiarioFunction.cs      # Timer trigger (cron diário)
│       ├── Models/
│       │   └── NotificacaoRequest.cs        # Request e Response records
│       ├── Program.cs                       # HostBuilder + DI
│       ├── host.json
│       └── local.settings.json             # Configuração local (não versionado)
└── .gitignore
```

## Funções

### NotificarUsuario — HTTP Trigger

```
POST http://localhost:7071/api/notificar
```

**Body:**
```json
{
  "usuarioId": "00000000-0000-0000-0000-000000000001",
  "email":     "usuario@fcg.com",
  "tipo":      "PagamentoConfirmado",
  "mensagem":  "Seu pagamento foi aprovado! O jogo já está na sua biblioteca."
}
```

**Tipos válidos:** `BemVindo` · `PagamentoConfirmado` · `PagamentoRecusado`

**Resposta (200 OK):**
```json
{
  "enviado":      true,
  "tipo":         "PagamentoConfirmado",
  "destinatario": "usuario@fcg.com",
  "processadoEm": "2026-09-15T10:30:00Z"
}
```

### NotificarHealth — HTTP Trigger

```
GET http://localhost:7071/api/notificar/health
```

Resposta:
```json
{ "status": "healthy", "service": "FCG.NotificationsFn", "timestamp": "..." }
```

### DigestDiario — Timer Trigger

Cron: `0 0 8 * * *` — executa todo dia às **08:00 UTC**.

Em produção: consulta CatalogAPI, agrega novidades e envia e-mail de digest para usuários ativos.

## Executar Localmente

### 1. Subir o Azurite (emulador de Azure Storage)

```bash
docker run -d --name azurite \
  -p 10000:10000 -p 10001:10001 -p 10002:10002 \
  mcr.microsoft.com/azure-storage/azurite
```

### 2. Rodar a function

```bash
cd src/FCG.NotificationsFn
dotnet run
```

> **Importante:** use `dotnet run`, não F5 no Visual Studio. O debugger do VS não inicializa corretamente o canal gRPC do Functions Worker.

## Variáveis de Ambiente (`local.settings.json`)

```json
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "DefaultEndpointsProtocol=http;AccountName=devstoreaccount1;...",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated"
  }
}
```

> Em produção no Azure, configure `APPLICATIONINSIGHTS_CONNECTION_STRING` no Function App para habilitar Application Insights. Localmente, o App Insights é ignorado automaticamente se a variável não estiver presente.

## Deploy no Azure

```bash
# Publicar
dotnet publish -c Release -o ./publish

# Deploy via Azure CLI
az functionapp deployment source config-zip \
  --resource-group rg-fcg \
  --name fcg-notifications-fn \
  --src publish.zip
```

---

> **FIAP Pós-Tech — Software Architecture | Tech Challenge — Fase 3**
> - Lucas Monte Ferreri Castilho
