using BackeryMovil.ViewModels;

namespace BackeryMovil.Views;

public partial class CreateProductPage : ContentPage
{
    private readonly CreateProductViewModel _viewModel;

    public CreateProductPage(CreateProductViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.InitializeAsync(); // Inicializa el ViewModel para un nuevo producto
    }
}
