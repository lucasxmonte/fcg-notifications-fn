using FCG.NotificationsFn.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace FCG.NotificationsFn.Functions;

/// <summary>
/// Função serverless para envio de notificações aos usuários da plataforma FCG.
/// Recebe eventos via HTTP (chamados pela NotificationsAPI ou diretamente) e
/// simula o envio de e-mail/SMS.
/// </summary>
public class NotificarUsuarioFunction
{
    private readonly ILogger<NotificarUsuarioFunction> _logger;

    public NotificarUsuarioFunction(ILogger<NotificarUsuarioFunction> logger)
        => _logger = logger;

    // ── POST /api/notificar ──────────────────────────────────────────
    [Function("NotificarUsuario")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "notificar")] HttpRequest req)
    {
        _logger.LogInformation("📨 NotificarUsuario disparado em {Timestamp}", DateTime.UtcNow);

        NotificacaoRequest? notificacao;
        try
        {
            var body = await new StreamReader(req.Body).ReadToEndAsync();
            notificacao = JsonSerializer.Deserialize<NotificacaoRequest>(body,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (notificacao is null)
                return new BadRequestObjectResult(new { erro = "Payload inválido." });
        }
        catch (JsonException ex)
        {
            _logger.LogWarning("JSON inválido: {Msg}", ex.Message);
            return new BadRequestObjectResult(new { erro = "JSON inválido." });
        }

        // Simula envio de notificação (em produção: SendGrid, SES, Twilio, etc.)
        _logger.LogInformation(
            "✉️  [{Tipo}] Para: {Email} (UsuarioId: {Id}) — {Mensagem}",
            notificacao.Tipo,
            notificacao.Email,
            notificacao.UsuarioId,
            notificacao.Mensagem);

        await SimularEnvioAsync(notificacao);

        var response = new NotificacaoResponse(
            Enviado: true,
            Tipo: notificacao.Tipo,
            Destinatario: notificacao.Email,
            ProcessadoEm: DateTime.UtcNow);

        return new OkObjectResult(response);
    }

    // ── GET /api/notificar/health ────────────────────────────────────
    [Function("NotificarHealth")]
    public IActionResult Health(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "notificar/health")] HttpRequest req)
    {
        return new OkObjectResult(new
        {
            status = "healthy",
            service = "FCG.NotificationsFn",
            timestamp = DateTime.UtcNow
        });
    }

    private static async Task SimularEnvioAsync(NotificacaoRequest notificacao)
    {
        // Simula latência de provedor externo (SendGrid, AWS SES, etc.)
        await Task.Delay(50);

        // Em produção: integrar com provedor real de e-mail/SMS
        _ = notificacao.Tipo switch
        {
            "BemVindo"             => $"E-mail de boas-vindas para {notificacao.Email}",
            "PagamentoConfirmado"  => $"Confirmação de pagamento para {notificacao.Email}",
            "PagamentoRecusado"    => $"Aviso de recusa de pagamento para {notificacao.Email}",
            _                     => $"Notificação genérica para {notificacao.Email}"
        };
    }
}
