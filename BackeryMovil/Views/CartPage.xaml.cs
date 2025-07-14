using System.Diagnostics; // Añadir para Debug.WriteLine
using BackeryMovil.Models;
using BackeryMovil.ViewModels;

namespace BackeryMovil.Views;

public partial class CartPage : ContentPage
{
    private readonly CartViewModel _viewModel;

    public CartPage(CartViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
        Debug.WriteLine("CartPage: Constructor called.");
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        Debug.WriteLine("CartPage: OnAppearing called.");
        try
        {
            if (BindingContext is CartViewModel vm)
            {
                vm.LoadCartItems();
                Debug.WriteLine("CartPage: LoadCartItems called from OnAppearing.");
            }
            else
            {
                Debug.WriteLine("CartPage: BindingContext is not CartViewModel in OnAppearing.");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"CartPage ERROR in OnAppearing: {ex.Message}");
            // Opcional: Mostrar un alert al usuario si el error es crítico
            // Application.Current?.MainPage?.DisplayAlert("Error", $"Error al cargar el carrito: {ex.Message}", "OK");
        }
    }

    private void OnQuantityCompleted(object sender, EventArgs e)
    {
        Debug.WriteLine("CartPage: OnQuantityCompleted event fired.");
        try
        {
            if (sender is Entry entry && entry.BindingContext is CartItem cartItem)
            {
                if (int.TryParse(entry.Text, out int newQuantity))
                {
                    cartItem.Quantity = newQuantity; // Actualizar la propiedad del modelo directamente
                    if (BindingContext is CartViewModel vm)
                    {
                        vm.UpdateQuantityCommand.Execute(cartItem); // Ejecutar el comando para notificar al servicio
                        Debug.WriteLine($"CartPage: Quantity updated for {cartItem.Product.Name} to {newQuantity}");
                    }
                }
                else
                {
                    Debug.WriteLine($"CartPage: Invalid quantity entered: {entry.Text}. Reverting.");
                    // Si la entrada no es un número válido, revertir al valor anterior o mostrar un error
                    if (BindingContext is CartViewModel vm)
                    {
                        vm.LoadCartItems(); // Recargar para revertir el valor inválido en la UI
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"CartPage ERROR in OnQuantityCompleted: {ex.Message}");
            // Opcional: Mostrar un alert al usuario
            // Application.Current?.MainPage?.DisplayAlert("Error", $"Error al actualizar cantidad: {ex.Message}", "OK");
        }
    }
}
