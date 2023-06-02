namespace Mango.Services.PaymentAPI.Messages
{
    public class UpdatePaymentResultMessage
    {
        public int OrderId { get; set; }

        public bool Status { get; set; }
    }
}
