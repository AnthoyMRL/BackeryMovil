using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Input;
using BackeryMovil.Models;
using BackeryMovil.Services;

namespace BackeryMovil.ViewModels
{
    public partial class CartViewModel : BaseViewModel
    {
        private readonly CartService _cartService;
        private ObservableCollection<CartItem> _cartItems = new();
        private decimal _totalAmount;

        public CartViewModel(CartService cartService)
        {
            _cartService = cartService;
            Title = "Mi Carrito";
            Debug.WriteLine("CartViewModel: Constructor called.");

            _cartService.CartUpdated += OnCartUpdated;

            try
            {
                LoadCartItems();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"CartViewModel ERROR in constructor LoadCartItems: {ex.Message}");
            }

            RemoveItemCommand = new Command<CartItem>(RemoveItem);
            UpdateQuantityCommand = new Command<CartItem>(UpdateQuantity);
            ClearCartCommand = new Command(ClearCart);
            CheckoutCommand = new Command(async () => await CheckoutAsync());
            IncreaseQuantityCommand = new Command<CartItem>(IncreaseQuantity); // Nuevo comando
            DecreaseQuantityCommand = new Command<CartItem>(DecreaseQuantity); // Nuevo comando
        }

        public ObservableCollection<CartItem> CartItems
        {
            get => _cartItems;
            set => SetProperty(ref _cartItems, value);
        }

        public decimal TotalAmount
        {
            get => _totalAmount;
            set => SetProperty(ref _totalAmount, value);
        }

        public ICommand RemoveItemCommand { get; }
        public ICommand UpdateQuantityCommand { get; }
        public ICommand ClearCartCommand { get; }
        public ICommand CheckoutCommand { get; }
        public ICommand IncreaseQuantityCommand { get; } // Declaración del nuevo comando
        public ICommand DecreaseQuantityCommand { get; } // Declaración del nuevo comando

        public void LoadCartItems()
        {
            Debug.WriteLine("CartViewModel: LoadCartItems method called.");
            try
            {
                CartItems.Clear();
                var items = _cartService.GetCartItems();
                if (items != null)
                {
                    foreach (var item in items)
                    {
                        if (item.Product != null)
                        {
                            CartItems.Add(item);
                        }
                        else
                        {
                            Debug.WriteLine($"CartViewModel: Skipping CartItem with null Product. ProductId: {item.Product?.Id ?? -1}");
                        }
                    }
                    TotalAmount = _cartService.GetTotalAmount();
                    Debug.WriteLine($"CartViewModel: Loaded {CartItems.Count} items. Total: {TotalAmount:C}");
                }
                else
                {
                    Debug.WriteLine("CartViewModel: _cartService.GetCartItems() returned null or empty list.");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"CartViewModel ERROR in LoadCartItems: {ex.Message}");
            }
        }

        private void OnCartUpdated(object? sender, EventArgs e)
        {
            Debug.WriteLine("CartViewModel: CartUpdated event received.");
            LoadCartItems();
        }

        private void RemoveItem(CartItem item)
        {
            if (item == null)
            {
                Debug.WriteLine("CartViewModel: RemoveItem called with null item.");
                return;
            }
            try
            {
                _cartService.RemoveFromCart(item.Product.Id);
                Debug.WriteLine($"CartViewModel: Removed item {item.Product.Name}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"CartViewModel ERROR removing item: {ex.Message}");
                Application.Current!.MainPage!.DisplayAlert("Error", $"Error al eliminar producto del carrito: {ex.Message}", "OK");
            }
        }

        private void UpdateQuantity(CartItem item)
        {
            if (item == null)
            {
                Debug.WriteLine("CartViewModel: UpdateQuantity called with null item.");
                return;
            }
            try
            {
                _cartService.UpdateQuantity(item.Product.Id, item.Quantity);
                Debug.WriteLine($"CartViewModel: Updated quantity for {item.Product.Name} to {item.Quantity}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"CartViewModel ERROR updating quantity: {ex.Message}");
                Application.Current!.MainPage!.DisplayAlert("Error", $"Error al actualizar cantidad: {ex.Message}", "OK");
            }
        }

        // Nuevo método para aumentar la cantidad
        private void IncreaseQuantity(CartItem item)
        {
            if (item == null) return;
            item.Quantity++;
            UpdateQuantity(item);
            Debug.WriteLine($"CartViewModel: Increased quantity for {item.Product.Name} to {item.Quantity}");
        }

        // Nuevo método para disminuir la cantidad
        private void DecreaseQuantity(CartItem item)
        {
            if (item == null) return;
            if (item.Quantity > 1) // No permitir cantidad menor a 1
            {
                item.Quantity--;
                UpdateQuantity(item);
                Debug.WriteLine($"CartViewModel: Decreased quantity for {item.Product.Name} to {item.Quantity}");
            }
            else
            {
                // Si la cantidad es 1 y se intenta disminuir, se elimina el producto
                RemoveItem(item);
                Debug.WriteLine($"CartViewModel: Removed {item.Product.Name} as quantity reached 0.");
            }
        }

        private void ClearCart()
        {
            try
            {
                _cartService.ClearCart();
                Debug.WriteLine("CartViewModel: Cart cleared.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"CartViewModel ERROR clearing cart: {ex.Message}");
                Application.Current!.MainPage!.DisplayAlert("Error", $"Error al vaciar el carrito: {ex.Message}", "OK");
            }
        }

        private async Task CheckoutAsync()
        {
            Debug.WriteLine("CartViewModel: CheckoutAsync called.");
            if (!CartItems.Any())
            {
                await Application.Current!.MainPage!.DisplayAlert("Carrito Vacío", "No hay productos en el carrito para procesar.", "OK");
                Debug.WriteLine("CartViewModel: Checkout attempted with empty cart.");
                return;
            }

            var confirm = await Application.Current!.MainPage!.DisplayAlert(
                "Confirmar Pedido",
                $"¿Desea confirmar su pedido por un total de {TotalAmount:C}?",
                "Sí", "No");

            if (confirm)
            {
                try
                {
                    var newOrder = new Order
                    {
                        CustomerName = "Cliente App",
                        CustomerPhone = "555-1234",
                        TotalAmount = TotalAmount,
                        Status = OrderStatus.Pending,
                        OrderDate = DateTime.Now
                    };
                    await _cartService.DatabaseService.SaveOrderAsync(newOrder);
                    Debug.WriteLine($"CartViewModel: Order saved with ID: {newOrder.Id}");

                    foreach (var item in CartItems)
                    {
                        var orderItem = new OrderItem
                        {
                            OrderId = newOrder.Id,
                            ProductId = item.Product.Id,
                            Quantity = item.Quantity,
                            UnitPrice = item.Product.Price
                        };
                        await _cartService.DatabaseService.SaveOrderItemAsync(orderItem);
                        Debug.WriteLine($"CartViewModel: OrderItem saved for Product ID: {item.Product.Id}");
                    }

                    _cartService.ClearCart();
                    await Application.Current!.MainPage!.DisplayAlert("Pedido Confirmado", "Su pedido ha sido procesado con éxito.", "OK");
                    Debug.WriteLine("CartViewModel: Checkout successful.");
                    await Shell.Current.GoToAsync("//main");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"CartViewModel ERROR during checkout: {ex.Message}");
                    await Application.Current!.MainPage!.DisplayAlert("Error de Pedido", $"Error al procesar el pedido: {ex.Message}", "OK");
                }
            }
            else
            {
                Debug.WriteLine("CartViewModel: Checkout cancelled by user.");
            }
        }

        ~CartViewModel()
        {
            Debug.WriteLine("CartViewModel: Destructor called. Unsubscribing from CartUpdated.");
            _cartService.CartUpdated -= OnCartUpdated;
        }
    }
}
