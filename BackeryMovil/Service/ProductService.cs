using BackeryMovil.Models;
using BackeryMovil.Services;

namespace BackeryMovil.Service
{
    public class ProductService
    {
        private readonly DatabaseService _databaseService;

        public ProductService(DatabaseService databaseService)
        {
            _databaseService = databaseService;
        }

        public async Task<List<Product>> GetProductsAsync()
        {
            return await _databaseService.GetProductsAsync();
        }

        public async Task<List<Product>> GetProductsByCategoryAsync(int categoryId)
        {
            return await _databaseService.GetProductsByCategoryAsync(categoryId);
        }

        public async Task<Product?> GetProductByIdAsync(int id)
        {
            return await _databaseService.GetProductAsync(id);
        }

        public List<string> GetCategories()
        {
            // This should be async, but keeping it sync for compatibility
            var task = _databaseService.GetCategoriesAsync();
            task.Wait();
            return task.Result.Select(c => c.Name).ToList();
        }

        public async Task<int> SaveProductAsync(Product product)
        {
            return await _databaseService.SaveProductAsync(product);
        }

        public async Task<int> DeleteProductAsync(Product product)
        {
            return await _databaseService.DeleteProductAsync(product);
        }
    }
}
