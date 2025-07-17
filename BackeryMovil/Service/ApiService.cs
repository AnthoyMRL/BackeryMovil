using System.Diagnostics;
using System.Globalization;
using System.Text.Json;
using BackeryMovil.Models;

namespace BackeryMovil.Services
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl = "https://localhost:7024/api"; // Tu URL actualizada

        public ApiService()
        {
            // Configurar para desarrollo (ignorar certificados SSL)
            var handler = new HttpClientHandler()
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            };
            _httpClient = new HttpClient(handler);
            _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
        }

        // DTO simplificado que coincide con tu modelo web
        public class ProductoApiDto
        {
            public int ProductoId { get; set; }
            public string Nombre { get; set; } = string.Empty;
            public decimal Precio { get; set; }
            public string ImagenUrl { get; set; } = string.Empty;
        }

        private Product MapFromApiDto(ProductoApiDto dto)
        {
            return new Product
            {
                ProductoId = dto.ProductoId,
                Name = dto.Nombre,
                Description = "Descripción no disponible", // Valor por defecto
                Price = dto.Precio,
                ImagenUrl = dto.ImagenUrl,
                CategoryId = 1, // Categoría por defecto
                IsAvailable = true, // Valor por defecto
                StockQuantity = 0, // Valor por defecto
                IsSynced = true,
                LastSyncAt = DateTime.Now
            };
        }

        private ProductoApiDto MapToApiDto(Product product)
        {
            return new ProductoApiDto
            {
                ProductoId = product.ProductoId ?? 0,
                Nombre = product.Name,
                Precio = product.Price,
                ImagenUrl = product.ImagenUrl
            };
        }

        public async Task<List<Product>> GetProductsAsync()
        {
            string url = $"{_baseUrl}/ProductoApi";
            Debug.WriteLine($"API: GET Request to {url}");

            try
            {
                var response = await _httpClient.GetAsync(url);
                Debug.WriteLine($"API: GET Response Status: {response.StatusCode}");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    Debug.WriteLine($"API: Response JSON: {json}");

                    var dtos = JsonSerializer.Deserialize<List<ProductoApiDto>>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    }) ?? new List<ProductoApiDto>();

                    return dtos.Select(MapFromApiDto).ToList();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"API Error: {ex.Message}");
            }

            return new List<Product>();
        }

        public async Task<Product?> CreateProductAsync(Product product, string? imagePath)
        {
            string url = $"{_baseUrl}/ProductoApi";
            Debug.WriteLine($"API: POST Request to {url} for product: {product.Name}");

            using var content = new MultipartFormDataContent();

            // Solo enviar los campos que existen en tu modelo web
            content.Add(new StringContent(product.Name), "Nombre");
            content.Add(new StringContent(product.Price.ToString(CultureInfo.InvariantCulture)), "Precio");

            // Manejar imagen
            if (!string.IsNullOrEmpty(imagePath) && File.Exists(imagePath))
            {
                try
                {
                    var fileStream = File.OpenRead(imagePath);
                    var fileName = Path.GetFileName(imagePath);
                    content.Add(new StreamContent(fileStream), "imagen", fileName);
                    Debug.WriteLine($"API: Adding image file: {fileName}");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"API: Error reading image: {ex.Message}");
                }
            }

            try
            {
                var response = await _httpClient.PostAsync(url, content);
                Debug.WriteLine($"API: POST Response Status: {response.StatusCode}");

                if (response.IsSuccessStatusCode)
                {
                    var responseJson = await response.Content.ReadAsStringAsync();
                    Debug.WriteLine($"API: POST Response JSON: {responseJson}");

                    var responseDto = JsonSerializer.Deserialize<ProductoApiDto>(responseJson, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    return responseDto != null ? MapFromApiDto(responseDto) : null;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Debug.WriteLine($"API: POST Error Content: {errorContent}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"API POST Error: {ex.Message}");
            }

            return null;
        }

        public async Task<Product?> GetProductAsync(int id)
        {
            string url = $"{_baseUrl}/ProductoApi/{id}";
            Debug.WriteLine($"API: GET Request to {url}");

            try
            {
                var response = await _httpClient.GetAsync(url);
                Debug.WriteLine($"API: GET Response Status: {response.StatusCode}");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    Debug.WriteLine($"API: Response JSON: {json}");

                    var dto = JsonSerializer.Deserialize<ProductoApiDto>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    return dto != null ? MapFromApiDto(dto) : null;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"API Error getting product: {ex.Message}");
            }

            return null;
        }

        public async Task<bool> DeleteProductAsync(int productoId)
        {
            string url = $"{_baseUrl}/ProductoApi/{productoId}";
            Debug.WriteLine($"API: DELETE Request to {url}");

            try
            {
                var response = await _httpClient.DeleteAsync(url);
                Debug.WriteLine($"API: DELETE Response Status: {response.StatusCode}");

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Debug.WriteLine($"API: DELETE Error Content: {errorContent}");
                }

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"API DELETE Error: {ex.Message}");
                return false;
            }
        }

        // Métodos para categorías (implementación básica)
        public async Task<List<Category>> GetCategoriesAsync()
        {
            return new List<Category>();
        }

        public async Task<Category?> CreateCategoryAsync(Category category)
        {
            return null;
        }

        public async Task<Category?> UpdateCategoryAsync(Category category)
        {
            return null;
        }

        public async Task<bool> DeleteCategoryAsync(int apiId)
        {
            return false;
        }

        // Métodos para órdenes (implementación básica)
        public async Task<List<Order>> GetOrdersAsync()
        {
            return new List<Order>();
        }

        public async Task<Order?> GetOrderAsync(int id)
        {
            return null;
        }

        public async Task<Order?> CreateOrderAsync(Order order)
        {
            return null;
        }

        public async Task<Order?> UpdateOrderAsync(Order order)
        {
            return null;
        }

        public async Task<bool> DeleteOrderAsync(int apiId)
        {
            return false;
        }
    }
}
