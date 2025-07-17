using System.Diagnostics;

namespace BackeryMovil.Services
{
    public class SyncService
    {
        private readonly DatabaseService _databaseService;
        private readonly ApiService _apiService;

        public SyncService(DatabaseService databaseService, ApiService apiService)
        {
            _databaseService = databaseService;
            _apiService = apiService;
        }

        public async Task<bool> SyncAllDataAsync()
        {
            try
            {
                Debug.WriteLine("SyncService: Starting full sync...");

                // Sincronizar productos desde la API
                await SyncProductsFromApiAsync();

                // Sincronizar productos locales no sincronizados hacia la API
                await SyncLocalProductsToApiAsync();

                Debug.WriteLine("SyncService: Full sync completed successfully");
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"SyncService: Sync error: {ex.Message}");
                return false;
            }
        }

        private async Task SyncProductsFromApiAsync()
        {
            try
            {
                Debug.WriteLine("SyncService: Syncing products from API...");

                var apiProducts = await _apiService.GetProductsAsync();
                Debug.WriteLine($"SyncService: Retrieved {apiProducts.Count} products from API");

                foreach (var apiProduct in apiProducts)
                {
                    // Buscar si el producto ya existe localmente
                    var localProducts = await _databaseService.GetProductsAsync();
                    var existingProduct = localProducts.FirstOrDefault(p => p.ProductoId == apiProduct.ProductoId);

                    if (existingProduct == null)
                    {
                        // Producto nuevo desde la API
                        apiProduct.Id = 0; // Asegurar que sea un nuevo registro local
                        apiProduct.IsSynced = true;
                        apiProduct.LastSyncAt = DateTime.Now;

                        await _databaseService.SaveProductAsync(apiProduct);
                        Debug.WriteLine($"SyncService: Added new product from API: {apiProduct.Name}");
                    }
                    else
                    {
                        // Actualizar producto existente con datos de la API
                        existingProduct.Name = apiProduct.Name;
                        existingProduct.Description = apiProduct.Description;
                        existingProduct.Price = apiProduct.Price;
                        existingProduct.ImagenUrl = apiProduct.ImagenUrl;
                        existingProduct.CategoryId = apiProduct.CategoryId;
                        existingProduct.IsAvailable = apiProduct.IsAvailable;
                        existingProduct.StockQuantity = apiProduct.StockQuantity;
                        existingProduct.IsSynced = true;
                        existingProduct.LastSyncAt = DateTime.Now;

                        await _databaseService.SaveProductAsync(existingProduct);
                        Debug.WriteLine($"SyncService: Updated existing product: {existingProduct.Name}");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"SyncService: Error syncing from API: {ex.Message}");
                throw;
            }
        }

        private async Task SyncLocalProductsToApiAsync()
        {
            try
            {
                Debug.WriteLine("SyncService: Syncing local products to API...");

                var localProducts = await _databaseService.GetProductsAsync();
                var unsyncedProducts = localProducts.Where(p => !p.IsSynced || p.ProductoId == null).ToList();

                Debug.WriteLine($"SyncService: Found {unsyncedProducts.Count} unsynced local products");

                foreach (var product in unsyncedProducts)
                {
                    try
                    {
                        if (product.ProductoId == null)
                        {
                            Debug.WriteLine($"SyncService: Attempting to create product in API: {product.Name}");

                            // Validar datos antes de enviar
                            if (string.IsNullOrEmpty(product.Name))
                            {
                                product.Name = "Producto sin nombre";
                            }

                            if (product.CategoryId <= 0)
                            {
                                product.CategoryId = 1; // Categoría por defecto
                            }

                            // Crear nuevo producto en la API
                            var apiProduct = await _apiService.CreateProductAsync(product, product.ImagenUrl);
                            if (apiProduct != null)
                            {
                                product.ProductoId = apiProduct.ProductoId;
                                product.IsSynced = true;
                                product.LastSyncAt = DateTime.Now;

                                // Actualizar la URL de la imagen si la API devolvió una nueva
                                if (!string.IsNullOrEmpty(apiProduct.ImagenUrl))
                                {
                                    product.ImagenUrl = apiProduct.ImagenUrl;
                                }

                                await _databaseService.SaveProductAsync(product);
                                Debug.WriteLine($"SyncService: Successfully created product in API: {product.Name}");
                            }
                            else
                            {
                                Debug.WriteLine($"SyncService: Failed to create product in API: {product.Name}");
                            }
                        }
                        // Nota: No implementamos UPDATE porque la API actual no tiene PUT
                        // Si agregas un endpoint PUT, puedes implementar la actualización aquí
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"SyncService: Error syncing product {product.Name}: {ex.Message}");
                        // Continuar con el siguiente producto en caso de error
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"SyncService: Error syncing to API: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> SyncProductsAsync()
        {
            try
            {
                await SyncProductsFromApiAsync();
                await SyncLocalProductsToApiAsync();
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"SyncService: Product sync error: {ex.Message}");
                return false;
            }
        }

        // Métodos placeholder para categorías y órdenes
        private async Task SyncCategoriesAsync()
        {
            Debug.WriteLine("SyncService: Category sync not implemented yet");
        }

        private async Task SyncOrdersAsync()
        {
            Debug.WriteLine("SyncService: Order sync not implemented yet");
        }
    }
}
