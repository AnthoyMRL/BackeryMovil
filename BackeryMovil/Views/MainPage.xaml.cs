namespace BackeryMovil.Views;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
    }

    private async void OnBrowseProductsClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//products");
    }

    private async void OnViewCartClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("cart"); // ¡Navegar a la página del carrito con ruta relativa!
    }

    private async void OnCategoryTapped(object sender, EventArgs e)
    {
        if (sender is TapGestureRecognizer tapGesture && tapGesture.CommandParameter is string category)
        {
            await Shell.Current.GoToAsync($"//products?category={category}");
        }
    }
}
