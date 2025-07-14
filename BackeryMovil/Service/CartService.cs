using BackeryMovil.Models; // Esto es correcto, ya que CartService usará el CartItem del Models

namespace BackeryMovil.Services
{
    public class CartService
    {
        private readonly List<CartItem> _cartItems = new();
        public event EventHandler? CartUpdated;
        public DatabaseService DatabaseService { get; }

        public CartService(DatabaseService databaseService)
        {
            DatabaseService = databaseService;
        }

        public List<CartItem> GetCartItems() => _cartItems;

        public void AddToCart(Product product, int quantity = 1)
        {
            var existingItem = _cartItems.FirstOrDefault(item => item.Product.Id == product.Id);

            if (existingItem != null)
            {
                existingItem.Quantity += quantity;
            }
            else
            {
                _cartItems.Add(new CartItem { Product = product, Quantity = quantity });
            }

            CartUpdated?.Invoke(this, EventArgs.Empty);
        }

        public void RemoveFromCart(int productId)
        {
            var item = _cartItems.FirstOrDefault(item => item.Product.Id == productId);
            if (item != null)
            {
                _cartItems.Remove(item);
                CartUpdated?.Invoke(this, EventArgs.Empty);
            }
        }

        public void UpdateQuantity(int productId, int newQuantity)
        {
            var item = _cartItems.FirstOrDefault(item => item.Product.Id == productId);
            if (item != null)
            {
                if (newQuantity <= 0)
                {
                    RemoveFromCart(productId);
                }
                else
                {
                    item.Quantity = newQuantity;
                    CartUpdated?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public decimal GetTotalAmount()
        {
            return _cartItems.Sum(item => item.TotalPrice);
        }

        public int GetItemCount()
        {
            return _cartItems.Sum(item => item.Quantity);
        }

        public void ClearCart()
        {
            _cartItems.Clear();
            CartUpdated?.Invoke(this, EventArgs.Empty);
        }
    }
}
