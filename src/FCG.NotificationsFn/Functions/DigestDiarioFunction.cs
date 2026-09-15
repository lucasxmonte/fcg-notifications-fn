using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace FCG.NotificationsFn.Functions;

/// <summary>
/// Função de timer: dispara diariamente às 08:00 UTC e envia
/// o digest de novos jogos/promoções para os usuários.
/// Demonstra o modelo serverless agendado (cron).
/// </summary>
public class DigestDiarioFunction
{
    private readonly ILogger<DigestDiarioFunction> _logger;

    public DigestDiarioFunction(ILogger<DigestDiarioFunction> logger)
        => _logger = logger;

    // Executa todo dia às 08:00 UTC  →  "0 0 8 * * *"
    [Function("DigestDiario")]
    public void Run([TimerTrigger("0 0 8 * * *", UseMonitor = false)] TimerInfo timer)
    {
        _logger.LogInformation(
            "📰 DigestDiario executado em {Timestamp} UTC. " +
            "Próxima execução: {Proxima}",
            DateTime.UtcNow,
            timer.ScheduleStatus?.Next);

        // Em produção: consulta CatalogAPI, agrega novidades e envia e-mail
        _logger.LogInformation(
            "📧 Enviando digest de novos jogos e promoções para todos os usuários ativos...");
    }
}
