namespace Mango.Services.OrderAPI.Models
{
    public class OrderDetails
    {
        public int OrderDetailsId { get; set; }

        public int OrderHeaderId { get; set; }

        public OrderHeader OrderHeader { get; set; }

        public int ProductId { get; set; }       

        public int Count { get; set; }

        public string ProductName { get; set; }

        public double Price { get; set; }
    }
}
