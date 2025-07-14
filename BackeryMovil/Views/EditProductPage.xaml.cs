using BackeryMovil.ViewModels;

namespace BackeryMovil.Views;

[QueryProperty(nameof(ProductId), "productId")]
public partial class EditProductPage : ContentPage
{
    private readonly EditProductViewModel _viewModel;
    private int _productId;

    public int ProductId
    {
        get => _productId;
        set
        {
            _productId = value;
            // Cuando el ProductId se establece, inicializa el ViewModel
            if (_viewModel != null)
            {
                Task.Run(async () => await _viewModel.InitializeAsync(value));
            }
        }
    }

    public EditProductPage(EditProductViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        // Asegúrate de que el ViewModel se inicialice si la página se carga sin un ProductId
        // o si el ProductId ya se estableció antes de que el ViewModel estuviera listo.
        if (_productId != 0 && _viewModel.Product == null)
        {
            await _viewModel.InitializeAsync(_productId);
        }
    }
}
