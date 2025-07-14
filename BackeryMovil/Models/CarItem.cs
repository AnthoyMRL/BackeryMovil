namespace BackeryMovil.Models // Asegúrate de que el namespace sea BackeryMovil.Models
{
    public class CartItem
    {
        public Product Product { get; set; } = new();
        public int Quantity { get; set; }
        public decimal TotalPrice => Product.Price * Quantity;
    }
}
