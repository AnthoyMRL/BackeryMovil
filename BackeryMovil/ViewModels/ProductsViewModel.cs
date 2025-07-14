using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Input;
using BackeryMovil.Models;
using BackeryMovil.Service;
using BackeryMovil.Services;

namespace BackeryMovil.ViewModels
{
    public class ProductsViewModel : BaseViewModel
    {
        private readonly DatabaseService _databaseService;
        private readonly FileService _fileService;
        private readonly SyncService _syncService;
        private readonly ProductService _productService;
        private readonly CartService _cartService;

        private ObservableCollection<Product> _products = new();
        private ObservableCollection<Category> _categories = new();
        private Product? _selectedProduct;
        private string _searchText = string.Empty;

        public ProductsViewModel(DatabaseService databaseService, FileService fileService, SyncService syncService, ProductService productService, CartService cartService)
        {
            _databaseService = databaseService;
            _fileService = fileService;
            _syncService = syncService;
            _productService = productService;
            _cartService = cartService;

            Title = "Productos";

            LoadProductsCommand = new Command(async () => await LoadProductsAsync());
            AddProductCommand = new Command(async () => await AddProductAsync());
            EditProductCommand = new Command<Product>(async (product) => await EditProductAsync(product));
            DeleteProductCommand = new Command<Product>(async (product) => await DeleteProductAsync(product));
            SearchCommand = new Command<string>(async (searchText) => await SearchProductsAsync(searchText));
            SyncCommand = new Command(async () => await SyncDataAsync());
            TakePhotoCommand = new Command<Product>(async (product) => await TakePhotoAsync(product));
            PickPhotoCommand = new Command<Product>(async (product) => await PickPhotoAsync(product));
            AddToCartCommand = new Command<Product>(AddToCart); // ¡Nuevo comando!
        }

        public ProductService ProductService => _productService;
        public CartService CartService => _cartService;

        public ObservableCollection<Product> Products
        {
            get => _products;
            set => SetProperty(ref _products, value);
        }

        public ObservableCollection<Category> Categories
        {
            get => _categories;
            set => SetProperty(ref _categories, value);
        }

        public Product? SelectedProduct
        {
            get => _selectedProduct;
            set => SetProperty(ref _selectedProduct, value);
        }

        public string SearchText
        {
            get => _searchText;
            set => SetProperty(ref _searchText, value);
        }

        public ICommand LoadProductsCommand { get; }
        public ICommand AddProductCommand { get; }
        public ICommand EditProductCommand { get; }
        public ICommand DeleteProductCommand { get; }
        public ICommand SearchCommand { get; }
        public ICommand SyncCommand { get; }
        public ICommand TakePhotoCommand { get; }
        public ICommand PickPhotoCommand { get; }
        public ICommand AddToCartCommand { get; } // ¡Declaración del nuevo comando!

        public async Task InitializeAsync()
        {
            await LoadCategoriesAsync();
            await LoadProductsAsync();
        }

        public async Task FilterByCategory(string categoryName)
        {
            try
            {
                if (categoryName == "All")
                {
                    await LoadProductsAsync();
                }
                else
                {
                    var category = Categories.FirstOrDefault(c => c.Name == categoryName);
                    if (category != null)
                    {
                        var filteredProducts = await _databaseService.GetProductsByCategoryAsync(category.Id);
                        Products.Clear();
                        foreach (var product in filteredProducts)
                        {
                            Products.Add(product);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                await Application.Current!.MainPage!.DisplayAlert("Error", $"Error al filtrar productos: {ex.Message}", "OK");
            }
        }

        private async Task LoadProductsAsync()
        {
            if (IsBusy) return;

            try
            {
                IsBusy = true;
                var products = await _databaseService.GetProductsAsync();
                Products.Clear();

                foreach (var product in products)
                {
                    Products.Add(product);
                    Debug.WriteLine($"ProductsViewModel: Loaded product from DB: {product.Name}, Desc: {product.Description}, Price: {product.Price}, Stock: {product.StockQuantity}");
                }
            }
            catch (Exception ex)
            {
                await Application.Current!.MainPage!.DisplayAlert("Error", $"Error al cargar productos: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task LoadCategoriesAsync()
        {
            try
            {
                var categories = await _databaseService.GetCategoriesAsync();
                Categories.Clear();

                foreach (var category in categories)
                {
                    Categories.Add(category);
                }
            }
            catch (Exception ex)
            {
                await Application.Current!.MainPage!.DisplayAlert("Error", $"Error al cargar categorías: {ex.Message}", "OK");
            }
        }

        private async Task AddProductAsync()
        {
            await Shell.Current.GoToAsync("createproduct");
        }

        private async Task EditProductAsync(Product product)
        {
            if (product == null) return;

            try
            {
                SelectedProduct = product;
                await Shell.Current.GoToAsync($"editproduct?productId={product.Id}");
            }
            catch (Exception ex)
            {
                await Application.Current!.MainPage!.DisplayAlert("Error", $"Error al editar producto: {ex.Message}", "OK");
            }
        }

        private async Task DeleteProductAsync(Product product)
        {
            if (product == null) return;

            try
            {
                var result = await Application.Current!.MainPage!.DisplayAlert(
                    "Confirmar",
                    $"¿Está seguro de eliminar {product.Name}?",
                    "Sí",
                    "No");

                if (result)
                {
                    if (!string.IsNullOrEmpty(product.ImagePath))
                    {
                        _fileService.DeleteImage(product.ImagePath);
                    }

                    await _databaseService.DeleteProductAsync(product);
                    Products.Remove(product);
                }
            }
            catch (Exception ex)
            {
                await Application.Current!.MainPage!.DisplayAlert("Error", $"Error al eliminar producto: {ex.Message}", "OK");
            }
        }

        private async Task SearchProductsAsync(string searchText)
        {
            if (string.IsNullOrWhiteSpace(searchText))
            {
                await LoadProductsAsync();
                return;
            }

            try
            {
                var allProducts = await _databaseService.GetProductsAsync();
                var filteredProducts = allProducts.Where(p =>
                    p.Name.Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
                    p.Description.Contains(searchText, StringComparison.OrdinalIgnoreCase)).ToList();

                Products.Clear();
                foreach (var product in filteredProducts)
                {
                    Products.Add(product);
                }
            }
            catch (Exception ex)
            {
                await Application.Current!.MainPage!.DisplayAlert("Error", $"Error en búsqueda: {ex.Message}", "OK");
            }
        }

        private async Task SyncDataAsync()
        {
            if (IsBusy) return;

            try
            {
                IsBusy = true;
                var success = await _syncService.SyncAllDataAsync();

                if (success)
                {
                    await Application.Current!.MainPage!.DisplayAlert("Éxito", "Datos sincronizados correctamente", "OK");
                    await LoadProductsAsync();
                }
                else
                {
                    await Application.Current!.MainPage!.DisplayAlert("Error", "Error al sincronizar datos", "OK");
                }
            }
            catch (Exception ex)
            {
                await Application.Current!.MainPage!.DisplayAlert("Error", $"Error de sincronización: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void AddToCart(Product product)
        {
            if (product == null) return;
            _cartService.AddToCart(product);
            Application.Current!.MainPage!.DisplayAlert("Carrito", $"{product.Name} añadido al carrito.", "OK");
            Debug.WriteLine($"ProductsViewModel: Added {product.Name} to cart.");
        }

        private async Task TakePhotoAsync(Product product)
        {
            if (product == null) return;

            try
            {
                var imagePath = await _fileService.SaveImageFromCameraAsync();
                if (!string.IsNullOrEmpty(imagePath))
                {
                    if (!string.IsNullOrEmpty(product.ImagePath))
                    {
                        _fileService.DeleteImage(product.ImagePath);
                    }

                    product.ImagePath = imagePath;
                    product.IsSynced = false;
                    await _databaseService.SaveProductAsync(product);

                    var index = Products.IndexOf(product);
                    if (index >= 0)
                    {
                        Products[index] = product;
                    }
                }
            }
            catch (Exception ex)
            {
                await Application.Current!.MainPage!.DisplayAlert("Error", $"Error al tomar foto: {ex.Message}", "OK");
            }
        }

        private async Task PickPhotoAsync(Product product)
        {
            if (product == null) return;

            try
            {
                var imagePath = await _fileService.SaveImageFromGalleryAsync();
                if (!string.IsNullOrEmpty(imagePath))
                {
                    if (!string.IsNullOrEmpty(product.ImagePath))
                    {
                        _fileService.DeleteImage(product.ImagePath);
                    }

                    product.ImagePath = imagePath;
                    product.IsSynced = false;
                    await _databaseService.SaveProductAsync(product);

                    var index = Products.IndexOf(product);
                    if (index >= 0)
                    {
                        Products[index] = product;
                    }
                }
            }
            catch (Exception ex)
            {
                await Application.Current!.MainPage!.DisplayAlert("Error", $"Error al seleccionar foto: {ex.Message}", "OK");
            }
        }
    }
}
