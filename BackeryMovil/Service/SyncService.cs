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
                await SyncCategoriesAsync();
                await SyncProductsAsync();
                await SyncOrdersAsync();
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Sync error: {ex.Message}");
                return false;
            }
        }

        private async Task SyncCategoriesAsync()
        {
            var localCategories = await _databaseService.GetCategoriesAsync();
            var unsyncedCategories = localCategories.Where(c => !c.IsSynced).ToList();

            foreach (var category in unsyncedCategories)
            {
                if (category.ApiId == null)
                {
                    // Usar el nuevo método CreateCategoryAsync que envía multipart/form-data
                    var apiCategory = await _apiService.CreateCategoryAsync(category);
                    if (apiCategory != null)
                    {
                        category.ApiId = apiCategory.Id;
                        category.IsSynced = true;
                        category.LastSyncAt = DateTime.Now;
                        await _databaseService.SaveCategoryAsync(category);
                    }
                }
                else
                {
                    // Usar el nuevo método UpdateCategoryAsync que envía multipart/form-data
                    var apiCategory = await _apiService.UpdateCategoryAsync(category);
                    if (apiCategory != null)
                    {
                        category.IsSynced = true;
                        category.LastSyncAt = DateTime.Now;
                        await _databaseService.SaveCategoryAsync(category);
                    }
                }
            }

            var apiCategories = await _apiService.GetCategoriesAsync();
            foreach (var apiCategory in apiCategories)
            {
                var localCategory = localCategories.FirstOrDefault(c => c.ApiId == apiCategory.Id);
                if (localCategory == null)
                {
                    apiCategory.ApiId = apiCategory.Id;
                    apiCategory.Id = 0; // Asegurarse de que sea un nuevo registro local
                    apiCategory.IsSynced = true;
                    apiCategory.LastSyncAt = DateTime.Now;
                    await _databaseService.SaveCategoryAsync(apiCategory);
                }
                // Opcional: Si la categoría existe localmente pero ha cambiado en la API, actualizarla.
                // Esto requeriría lógica para detectar cambios y actualizar. Por ahora, solo se añade si no existe.
            }
        }

        private async Task SyncProductsAsync()
        {
            var localProducts = await _databaseService.GetProductsAsync();
            var unsyncedProducts = localProducts.Where(p => !p.IsSynced).ToList();

            foreach (var product in unsyncedProducts)
            {
                if (product.ApiId == null)
                {
                    var apiProduct = await _apiService.CreateProductAsync(product, product.ImagePath);
                    if (apiProduct != null)
                    {
                        product.ApiId = apiProduct.Id;
                        product.IsSynced = true;
                        product.LastSyncAt = DateTime.Now;
                        await _databaseService.SaveProductAsync(product);
                    }
                }
                else
                {
                    var apiProduct = await _apiService.UpdateProductAsync(product, product.ImagePath);
                    if (apiProduct != null)
                    {
                        product.IsSynced = true;
                        product.LastSyncAt = DateTime.Now;
                        await _databaseService.SaveProductAsync(product);
                    }
                }
            }

            var apiProducts = await _apiService.GetProductsAsync();
            foreach (var apiProduct in apiProducts)
            {
                var localProduct = localProducts.FirstOrDefault(p => p.ApiId == apiProduct.Id);
                if (localProduct == null)
                {
                    apiProduct.ApiId = apiProduct.Id;
                    apiProduct.Id = 0;
                    apiProduct.IsSynced = true;
                    apiProduct.LastSyncAt = DateTime.Now;
                    await _databaseService.SaveProductAsync(apiProduct);
                }
            }
        }

        private async Task SyncOrdersAsync()
        {
            var localOrders = await _databaseService.GetOrdersAsync();
            var unsyncedOrders = localOrders.Where(o => !o.IsSynced).ToList();

            foreach (var order in unsyncedOrders)
            {
                if (order.ApiId == null)
                {
                    var apiOrder = await _apiService.CreateOrderAsync(order);
                    if (apiOrder != null)
                    {
                        order.ApiId = apiOrder.Id;
                        order.IsSynced = true;
                        order.LastSyncAt = DateTime.Now;
                        await _databaseService.SaveOrderAsync(order);
                    }
                }
                else
                {
                    var apiOrder = await _apiService.UpdateOrderAsync(order);
                    if (apiOrder != null)
                    {
                        order.IsSynced = true;
                        order.LastSyncAt = DateTime.Now;
                        await _databaseService.SaveOrderAsync(order);
                    }
                }
            }
        }
    }
}
