using System.Collections.ObjectModel;
using System.Diagnostics; // Añadir para Debug.WriteLine
using System.Text.Json;
using System.Windows.Input;
using BackeryMovil.Models;
using BackeryMovil.Service; // Para serializar/deserializar Category
using BackeryMovil.Services;

namespace BackeryMovil.ViewModels
{
    [QueryProperty(nameof(SelectedCategoryJson), "selectedCategory")] // Para recibir la categoría seleccionada
    public class EditProductViewModel : BaseViewModel
    {
        private readonly DatabaseService _databaseService;
        private readonly FileService _fileService;

        private Product _product = new();
        private ObservableCollection<Category> _categories = new();
        private Category? _selectedCategory;

        // Propiedad para recibir la categoría seleccionada como JSON
        public string SelectedCategoryJson
        {
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    try
                    {
                        // Deserializar la categoría recibida
                        var category = JsonSerializer.Deserialize<Category>(value, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                        if (category != null)
                        {
                            SelectedCategory = category;
                            Product.CategoryId = category.Id; // Asegúrate de que el Product.CategoryId se actualice
                            Debug.WriteLine($"EditProductViewModel: Received selected category: {SelectedCategory?.Name}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"EditProductViewModel Error deserializing selected category: {ex.Message}");
                    }
                }
            }
        }

        public EditProductViewModel(DatabaseService databaseService, FileService fileService)
        {
            _databaseService = databaseService;
            _fileService = fileService;
            Title = "Editar Producto";

            SaveProductCommand = new Command(async () => await SaveProductAsync());
            CancelCommand = new Command(async () => await CancelAsync());
            TakePhotoCommand = new Command(async () => await TakePhotoAsync());
            PickPhotoCommand = new Command(async () => await PickPhotoAsync());
            SelectCategoryCommand = new Command(async () => await SelectCategoryAsync()); // Nuevo comando
        }

        public Product Product
        {
            get => _product;
            set => SetProperty(ref _product, value);
        }

        public ObservableCollection<Category> Categories
        {
            get => _categories;
            set => SetProperty(ref _categories, value);
        }

        public Category? SelectedCategory
        {
            get => _selectedCategory;
            set => SetProperty(ref _selectedCategory, value);
        }

        public ICommand SaveProductCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand TakePhotoCommand { get; }
        public ICommand PickPhotoCommand { get; }
        public ICommand SelectCategoryCommand { get; } // Declaración del nuevo comando

        public async Task InitializeAsync(int productId)
        {
            if (IsBusy) return;

            try
            {
                IsBusy = true;
                Debug.WriteLine("EditProductViewModel: Initializing...");
                await LoadCategoriesAsync(); // Asegura que las categorías se carguen primero

                if (!Categories.Any())
                {
                    Debug.WriteLine("EditProductViewModel: No categories found after loading.");
                    await Application.Current!.MainPage!.DisplayAlert("Advertencia", "No hay categorías disponibles. Por favor, agregue categorías primero.", "OK");
                }
                else
                {
                    Debug.WriteLine($"EditProductViewModel: Loaded {Categories.Count} categories.");
                }

                if (productId != 0)
                {
                    Debug.WriteLine($"EditProductViewModel: Loading product with ID: {productId}");
                    var product = await _databaseService.GetProductAsync(productId);
                    if (product != null)
                    {
                        Product = product;
                        // Intenta encontrar la categoría por ID
                        SelectedCategory = Categories.FirstOrDefault(c => c.Id == Product.CategoryId);
                        // Si no se encuentra la categoría o si Product.CategoryId es 0 (para un producto nuevo que aún no tiene categoría asignada),
                        // y hay categorías disponibles, asigna la primera como predeterminada.
                        if (SelectedCategory == null && Categories.Any())
                        {
                            SelectedCategory = Categories.First();
                            Product.CategoryId = SelectedCategory.Id; // Asegura que el ID del producto también se actualice
                            Debug.WriteLine("EditProductViewModel: Product category not found or null, defaulting to first available category.");
                        }
                        Debug.WriteLine($"EditProductViewModel: Product loaded: {Product.Name}, Category: {SelectedCategory?.Name}");
                    }
                    else
                    {
                        Debug.WriteLine($"EditProductViewModel: Product with ID {productId} not found.");
                    }
                }
                else
                {
                    Debug.WriteLine("EditProductViewModel: Initializing for new product.");
                    // Para un nuevo producto
                    Product = new Product
                    {
                        Name = "Nuevo Producto",
                        Description = "",
                        Price = 0m,
                        StockQuantity = 0,
                        IsAvailable = true,
                    };
                    // Asegura que SelectedCategory siempre sea la primera categoría si hay alguna
                    if (Categories.Any())
                    {
                        SelectedCategory = Categories.First();
                        Product.CategoryId = SelectedCategory.Id; // Asigna el ID de la primera categoría al producto
                    }
                    else
                    {
                        SelectedCategory = null; // No hay categorías, deja SelectedCategory como null
                        Product.CategoryId = 0; // Asegura que CategoryId sea 0 si no hay categorías
                    }
                    Debug.WriteLine($"EditProductViewModel: New product initialized. Default category: {SelectedCategory?.Name ?? "None"}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"EditProductViewModel Error: {ex.Message}");
                await Application.Current!.MainPage!.DisplayAlert("Error", $"Error al cargar producto: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
                Debug.WriteLine("EditProductViewModel: Initialization finished.");
            }
        }

        private async Task LoadCategoriesAsync()
        {
            try
            {
                Debug.WriteLine("EditProductViewModel: Loading categories from DatabaseService...");
                var categories = await _databaseService.GetCategoriesAsync();
                Categories.Clear();
                foreach (var category in categories)
                {
                    Categories.Add(category);
                }
                Debug.WriteLine($"EditProductViewModel: DatabaseService returned {categories.Count} categories. ObservableCollection now has {Categories.Count} items.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"EditProductViewModel Error loading categories: {ex.Message}");
                await Application.Current!.MainPage!.DisplayAlert("Error", $"Error al cargar categorías: {ex.Message}", "OK");
            }
        }

        private async Task SaveProductAsync()
        {
            if (IsBusy) return;

            try
            {
                IsBusy = true;
                Debug.WriteLine("EditProductViewModel: Saving product...");
                if (SelectedCategory != null)
                {
                    Product.CategoryId = SelectedCategory.Id;
                    Debug.WriteLine($"EditProductViewModel: Product category set to {SelectedCategory.Name} (ID: {SelectedCategory.Id})");
                }
                else if (Categories.Any())
                {
                    Product.CategoryId = Categories.First().Id;
                    Debug.WriteLine($"EditProductViewModel: No category selected, defaulting to first category: {Categories.First().Name} (ID: {Categories.First().Id})");
                }
                else
                {
                    await Application.Current!.MainPage!.DisplayAlert("Error", "Debe seleccionar una categoría.", "OK");
                    Debug.WriteLine("EditProductViewModel: No categories available to select.");
                    return;
                }

                await _databaseService.SaveProductAsync(Product);
                await Application.Current!.MainPage!.DisplayAlert("Éxito", "Producto guardado correctamente.", "OK");
                Debug.WriteLine("EditProductViewModel: Product saved successfully. Navigating back.");
                await Shell.Current.GoToAsync(".."); // Volver a la página anterior
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"EditProductViewModel Error saving product: {ex.Message}");
                await Application.Current!.MainPage!.DisplayAlert("Error", $"Error al guardar producto: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task CancelAsync()
        {
            Debug.WriteLine("EditProductViewModel: Cancelled. Navigating back.");
            await Shell.Current.GoToAsync(".."); // Volver a la página anterior
        }

        private async Task SelectCategoryAsync()
        {
            Debug.WriteLine("EditProductViewModel: Navigating to CategorySelectionPage.");
            // Navegar a la página de selección de categorías
            await Shell.Current.GoToAsync("categoryselection");
        }

        private async Task TakePhotoAsync()
        {
            if (IsBusy) return;
            IsBusy = true;
            try
            {
                Debug.WriteLine("EditProductViewModel: Taking photo...");
                var imagePath = await _fileService.SaveImageFromCameraAsync();
                if (!string.IsNullOrEmpty(imagePath))
                {
                    if (!string.IsNullOrEmpty(Product.ImagePath))
                    {
                        _fileService.DeleteImage(Product.ImagePath);
                        Debug.WriteLine($"EditProductViewModel: Deleted old image: {Product.ImagePath}");
                    }
                    Product.ImagePath = imagePath;
                    Product.IsSynced = false;
                    OnPropertyChanged(nameof(Product)); // Notificar a la UI del cambio de imagen
                    Debug.WriteLine($"EditProductViewModel: New image path: {imagePath}");
                }
                else
                {
                    Debug.WriteLine("EditProductViewModel: No photo taken from camera.");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"EditProductViewModel Error taking photo: {ex.Message}");
                await Application.Current!.MainPage!.DisplayAlert("Error", $"Error al tomar foto: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task PickPhotoAsync()
        {
            if (IsBusy) return;
            IsBusy = true;
            try
            {
                Debug.WriteLine("EditProductViewModel: Picking photo...");
                var imagePath = await _fileService.SaveImageFromGalleryAsync();
                if (!string.IsNullOrEmpty(imagePath))
                {
                    if (!string.IsNullOrEmpty(Product.ImagePath))
                    {
                        _fileService.DeleteImage(Product.ImagePath);
                        Debug.WriteLine($"EditProductViewModel: Deleted old image: {Product.ImagePath}");
                    }
                    Product.ImagePath = imagePath;
                    Product.IsSynced = false;
                    OnPropertyChanged(nameof(Product)); // Notificar a la UI del cambio de imagen
                    Debug.WriteLine($"EditProductViewModel: New image path: {imagePath}");
                }
                else
                {
                    Debug.WriteLine("EditProductViewModel: No photo picked from gallery.");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"EditProductViewModel Error picking photo: {ex.Message}");
                await Application.Current!.MainPage!.DisplayAlert("Error", $"Error al seleccionar foto: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
