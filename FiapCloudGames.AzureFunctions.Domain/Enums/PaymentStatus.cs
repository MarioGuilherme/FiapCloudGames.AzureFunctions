namespace FiapCloudGames.AzureFunctions.Domain.Enums;

public enum PaymentStatus : byte
{
    Created = 1,
    PendingPayment,
    Paid,
    Canceled
}
