namespace Mango.Services.ShoppingCartAPI.Models
{
    public class CartHeader
    {
        public int CartHeaderId { get; set; }

        public string UserId { get; set; }

        public string CouponCode { get; set; }

        public CartDetails CartDetails { get; set; }
    }
}
