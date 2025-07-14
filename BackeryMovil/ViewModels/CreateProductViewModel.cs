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
    public class CreateProductViewModel : BaseViewModel
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
                            Debug.WriteLine($"CreateProductViewModel: Received selected category: {SelectedCategory?.Name}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"CreateProductViewModel Error deserializing selected category: {ex.Message}");
                    }
                }
            }
        }

        public CreateProductViewModel(DatabaseService databaseService, FileService fileService)
        {
            _databaseService = databaseService;
            _fileService = fileService;
            Title = "Crear Nuevo Producto";

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

        public async Task InitializeAsync()
        {
            if (IsBusy) return;

            try
            {
                IsBusy = true;
                Debug.WriteLine("CreateProductViewModel: Initializing for new product...");
                await LoadCategoriesAsync(); // Asegura que las categorías se carguen primero

                if (!Categories.Any())
                {
                    Debug.WriteLine("CreateProductViewModel: No categories found after loading.");
                    await Application.Current!.MainPage!.DisplayAlert("Advertencia", "No hay categorías disponibles. Por favor, agregue categorías primero.", "OK");
                    // Si no hay categorías, no tiene sentido crear un producto. Podrías deshabilitar el botón de guardar o navegar de vuelta.
                    // await Shell.Current.GoToAsync("..");
                    // return;
                }
                else
                {
                    Debug.WriteLine($"CreateProductViewModel: Loaded {Categories.Count} categories.");
                }

                // Inicializar un nuevo objeto Product
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
                Debug.WriteLine($"CreateProductViewModel: New product initialized. Default category: {SelectedCategory?.Name ?? "None"}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"CreateProductViewModel Error: {ex.Message}");
                await Application.Current!.MainPage!.DisplayAlert("Error", $"Error al inicializar la creación de producto: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
                Debug.WriteLine("CreateProductViewModel: Initialization finished.");
            }
        }

        private async Task LoadCategoriesAsync()
        {
            try
            {
                Debug.WriteLine("CreateProductViewModel: Loading categories from DatabaseService...");
                var categories = await _databaseService.GetCategoriesAsync();
                Categories.Clear();
                foreach (var category in categories)
                {
                    Categories.Add(category);
                }
                Debug.WriteLine($"CreateProductViewModel: DatabaseService returned {categories.Count} categories. ObservableCollection now has {Categories.Count} items.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"CreateProductViewModel Error loading categories: {ex.Message}");
                await Application.Current!.MainPage!.DisplayAlert("Error", $"Error al cargar categorías: {ex.Message}", "OK");
            }
        }

        private async Task SaveProductAsync()
        {
            if (IsBusy) return;

            try
            {
                IsBusy = true;
                Debug.WriteLine("CreateProductViewModel: Saving new product...");
                if (SelectedCategory != null)
                {
                    Product.CategoryId = SelectedCategory.Id;
                    Debug.WriteLine($"CreateProductViewModel: Product category set to {SelectedCategory.Name} (ID: {SelectedCategory.Id})");
                }
                else if (Categories.Any())
                {
                    Product.CategoryId = Categories.First().Id;
                    Debug.WriteLine($"CreateProductViewModel: No category selected, defaulting to first category: {Categories.First().Name} (ID: {Categories.First().Id})");
                }
                else
                {
                    await Application.Current!.MainPage!.DisplayAlert("Error", "Debe seleccionar una categoría.", "OK");
                    Debug.WriteLine("CreateProductViewModel: No categories available to select.");
                    return;
                }

                await _databaseService.SaveProductAsync(Product);
                await Application.Current!.MainPage!.DisplayAlert("Éxito", "Producto creado correctamente.", "OK");
                Debug.WriteLine("CreateProductViewModel: Product created successfully. Navigating back.");
                await Shell.Current.GoToAsync(".."); // Volver a la página anterior (ProductsPage)
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"CreateProductViewModel Error saving product: {ex.Message}");
                await Application.Current!.MainPage!.DisplayAlert("Error", $"Error al guardar producto: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task CancelAsync()
        {
            Debug.WriteLine("CreateProductViewModel: Creation cancelled. Navigating back.");
            await Shell.Current.GoToAsync(".."); // Volver a la página anterior
        }

        private async Task SelectCategoryAsync()
        {
            Debug.WriteLine("CreateProductViewModel: Navigating to CategorySelectionPage.");
            // Navegar a la página de selección de categorías
            await Shell.Current.GoToAsync("categoryselection");
        }

        private async Task TakePhotoAsync()
        {
            if (IsBusy) return;
            IsBusy = true;
            try
            {
                Debug.WriteLine("CreateProductViewModel: Taking photo...");
                var imagePath = await _fileService.SaveImageFromCameraAsync();
                if (!string.IsNullOrEmpty(imagePath))
                {
                    if (!string.IsNullOrEmpty(Product.ImagePath))
                    {
                        _fileService.DeleteImage(Product.ImagePath);
                        Debug.WriteLine($"CreateProductViewModel: Deleted old image: {Product.ImagePath}");
                    }
                    Product.ImagePath = imagePath;
                    Product.IsSynced = false;
                    OnPropertyChanged(nameof(Product)); // Notificar a la UI del cambio de imagen
                    Debug.WriteLine($"CreateProductViewModel: New image path: {imagePath}");
                }
                else
                {
                    Debug.WriteLine("CreateProductViewModel: No photo taken from camera.");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"CreateProductViewModel Error taking photo: {ex.Message}");
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
                Debug.WriteLine("CreateProductViewModel: Picking photo...");
                var imagePath = await _fileService.SaveImageFromGalleryAsync();
                if (!string.IsNullOrEmpty(imagePath))
                {
                    if (!string.IsNullOrEmpty(Product.ImagePath))
                    {
                        _fileService.DeleteImage(Product.ImagePath);
                        Debug.WriteLine($"CreateProductViewModel: Deleted old image: {Product.ImagePath}");
                    }
                    Product.ImagePath = imagePath;
                    Product.IsSynced = false;
                    OnPropertyChanged(nameof(Product)); // Notificar a la UI del cambio de imagen
                    Debug.WriteLine($"CreateProductViewModel: New image path: {imagePath}");
                }
                else
                {
                    Debug.WriteLine("CreateProductViewModel: No photo picked from gallery.");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"CreateProductViewModel Error picking photo: {ex.Message}");
                await Application.Current!.MainPage!.DisplayAlert("Error", $"Error al seleccionar foto: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
