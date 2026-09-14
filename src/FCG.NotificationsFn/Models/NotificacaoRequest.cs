namespace FCG.NotificationsFn.Models;

public record NotificacaoRequest(
    Guid UsuarioId,
    string Email,
    string Tipo,       // "BemVindo" | "PagamentoConfirmado" | "PagamentoRecusado"
    string Mensagem
);

public record NotificacaoResponse(
    bool Enviado,
    string Tipo,
    string Destinatario,
    DateTime ProcessadoEm
);
