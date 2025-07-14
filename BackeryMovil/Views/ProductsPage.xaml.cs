using BackeryMovil.ViewModels;

namespace BackeryMovil.Views;

[QueryProperty(nameof(Category), "category")]
public partial class ProductsPage : ContentPage
{
    private readonly ProductsViewModel _viewModel;
    private string _category = string.Empty;

    public string Category
    {
        get => _category;
        set
        {
            _category = value;
            if (!string.IsNullOrEmpty(value) && _viewModel != null)
            {
                Task.Run(async () => await _viewModel.FilterByCategory(value));
            }
        }
    }

    public ProductsPage(ProductsViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.InitializeAsync();
    }

    // Nuevo método para el ToolbarItem
    private async void OnHomeToolbarItemClicked(object sender, EventArgs e)
    {
        // Navega a la ruta principal de la aplicación (MainPage)
        await Shell.Current.GoToAsync("//main");
    }
}
