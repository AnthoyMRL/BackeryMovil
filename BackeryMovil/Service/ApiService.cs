using System.Diagnostics; // Asegúrate de tener esto
using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization; // Añadir para JsonIgnoreCondition
using BackeryMovil.Models;

namespace BackeryMovil.Services
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl = "https://localhost:7052/api"; // Asegúrate de que el puerto coincida con tu API

        public ApiService()
        {
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
        }

        private async Task<T?> GetAsync<T>(string endpoint)
        {
            string url = $"{_baseUrl}/{endpoint}";
            Debug.WriteLine($"API: GET Request to {url}");
            try
            {
                var response = await _httpClient.GetAsync(url);
                Debug.WriteLine($"API: GET Response Status for {url}: {response.StatusCode}");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    Debug.WriteLine($"API: GET Response JSON for {url}: {json}");
                    return JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Debug.WriteLine($"API: GET Error Content for {url}: {errorContent}");
                }
            }
            catch (HttpRequestException httpEx)
            {
                Debug.WriteLine($"API GET HttpRequestException for {url}: {httpEx.Message}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"API GET General Error for {url}: {ex.Message}");
            }
            return default;
        }

        private async Task<List<T>> GetListAsync<T>(string endpoint)
        {
            string url = $"{_baseUrl}/{endpoint}";
            Debug.WriteLine($"API: GET List Request to {url}");
            try
            {
                var response = await _httpClient.GetAsync(url);
                Debug.WriteLine($"API: GET List Response Status for {url}: {response.StatusCode}");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    Debug.WriteLine($"API: GET List Response JSON for {url}: {json}");
                    return JsonSerializer.Deserialize<List<T>>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    }) ?? new List<T>();
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Debug.WriteLine($"API: GET List Error Content for {url}: {errorContent}");
                }
            }
            catch (HttpRequestException httpEx)
            {
                Debug.WriteLine($"API GET List HttpRequestException for {url}: {httpEx.Message}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"API GET List General Error for {url}: {ex.Message}");
            }
            return new List<T>();
        }

        private async Task<T?> PostAsync<T>(string endpoint, object data)
        {
            string url = $"{_baseUrl}/{endpoint}";
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = null,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault
            };
            string jsonInput = JsonSerializer.Serialize(data, options);
            Debug.WriteLine($"API: POST Request to {url} with data: {jsonInput}");
            try
            {
                var content = new StringContent(jsonInput, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(url, content);
                Debug.WriteLine($"API: POST Response Status for {url}: {response.StatusCode}");

                if (response.IsSuccessStatusCode)
                {
                    var responseJson = await response.Content.ReadAsStringAsync();
                    Debug.WriteLine($"API: POST Response JSON for {url}: {responseJson}");
                    return JsonSerializer.Deserialize<T>(responseJson, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Debug.WriteLine($"API: POST Error Content for {url}: {errorContent}");
                }
            }
            catch (HttpRequestException httpEx)
            {
                Debug.WriteLine($"API POST HttpRequestException for {url}: {httpEx.Message}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"API POST General Error for {url}: {ex.Message}");
            }
            return default;
        }

        private async Task<T?> PutAsync<T>(string endpoint, object data)
        {
            string url = $"{_baseUrl}/{endpoint}";
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = null,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault
            };
            string jsonInput = JsonSerializer.Serialize(data, options);
            Debug.WriteLine($"API: PUT Request to {url} with data: {jsonInput}");
            try
            {
                var content = new StringContent(jsonInput, Encoding.UTF8, "application/json");
                var response = await _httpClient.PutAsync(url, content);
                Debug.WriteLine($"API: PUT Response Status for {url}: {response.StatusCode}");

                if (response.IsSuccessStatusCode)
                {
                    var responseJson = await response.Content.ReadAsStringAsync();
                    Debug.WriteLine($"API: PUT Response JSON for {url}: {responseJson}");
                    return JsonSerializer.Deserialize<T>(responseJson, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Debug.WriteLine($"API: PUT Error Content for {url}: {errorContent}");
                }
            }
            catch (HttpRequestException httpEx)
            {
                Debug.WriteLine($"API PUT HttpRequestException: {httpEx.Message}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"API PUT General Error for {url}: {ex.Message}");
            }
            return default;
        }

        private async Task<bool> DeleteAsync(string endpoint)
        {
            string url = $"{_baseUrl}/{endpoint}";
            Debug.WriteLine($"API: DELETE Request to {url}");
            try
            {
                var response = await _httpClient.DeleteAsync(url);
                Debug.WriteLine($"API: DELETE Response Status for {url}: {response.StatusCode}");
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Debug.WriteLine($"API: DELETE Error Content for {url}: {errorContent}");
                }
                return response.IsSuccessStatusCode;
            }
            catch (HttpRequestException httpEx)
            {
                Debug.WriteLine($"API DELETE HttpRequestException: {httpEx.Message}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"API DELETE General Error for {url}: {ex.Message}");
            }
            return false;
        }

        public async Task<List<Category>> GetCategoriesAsync()
        {
            return await GetListAsync<Category>("CategoriaApi");
        }

        public async Task<Category?> GetCategoryAsync(int id)
        {
            return await GetAsync<Category>($"CategoriaApi/{id}");
        }

        // --- NUEVO MÉTODO PARA CREAR CATEGORÍAS USANDO multipart/form-data ---
        public async Task<Category?> CreateCategoryAsync(Category category)
        {
            string url = $"{_baseUrl}/CategoriaApi";
            Debug.WriteLine($"API: POST Request to {url} with multipart/form-data for category: {category.Name}");

            using var content = new MultipartFormDataContent();

            // Añadir campos de la categoría
            content.Add(new StringContent(category.Name ?? string.Empty), "nombre"); // Coincide con el nombre del parámetro en el backend
            content.Add(new StringContent(category.Description ?? string.Empty), "descripcion");
            content.Add(new StringContent(category.IconName ?? string.Empty), "iconName");

            // Si el backend espera el ID para un POST (aunque no es común para creación)
            if (category.Id != 0)
            {
                content.Add(new StringContent(category.Id.ToString()), "id");
            }
            // Si el backend espera ApiId para un POST (si se está re-creando un elemento sincronizado)
            if (category.ApiId.HasValue)
            {
                content.Add(new StringContent(category.ApiId.Value.ToString()), "apiId");
            }

            try
            {
                var response = await _httpClient.PostAsync(url, content);
                Debug.WriteLine($"API: POST Category Response Status for {url}: {response.StatusCode}");

                if (response.IsSuccessStatusCode)
                {
                    var responseJson = await response.Content.ReadAsStringAsync();
                    Debug.WriteLine($"API: POST Category Response JSON: {responseJson}");
                    return JsonSerializer.Deserialize<Category>(responseJson, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Debug.WriteLine($"API: POST Category Error Content: {errorContent}");
                }
            }
            catch (HttpRequestException httpEx)
            {
                Debug.WriteLine($"API POST Category HttpRequestException: {httpEx.Message}");
            }
            finally
            {
                // Asegúrate de que el contenido se deseche correctamente
                content.Dispose();
            }
            return default;
        }

        // --- NUEVO MÉTODO PARA ACTUALIZAR CATEGORÍAS USANDO multipart/form-data ---
        public async Task<Category?> UpdateCategoryAsync(Category category)
        {
            string url = $"{_baseUrl}/CategoriaApi/{category.ApiId}"; // Asumiendo que PUT usa el ApiId
            Debug.WriteLine($"API: PUT Request to {url} with multipart/form-data for category: {category.Name}");

            using var content = new MultipartFormDataContent();

            // Añadir campos de la categoría
            content.Add(new StringContent(category.Name ?? string.Empty), "nombre"); // Coincide con el nombre del parámetro en el backend
            content.Add(new StringContent(category.Description ?? string.Empty), "descripcion");
            content.Add(new StringContent(category.IconName ?? string.Empty), "iconName");

            // Si el backend espera el ID para un PUT
            if (category.Id != 0)
            {
                content.Add(new StringContent(category.Id.ToString()), "id");
            }
            // Si el backend espera ApiId para un PUT
            if (category.ApiId.HasValue)
            {
                content.Add(new StringContent(category.ApiId.Value.ToString()), "apiId");
            }

            try
            {
                var response = await _httpClient.PutAsync(url, content);
                Debug.WriteLine($"API: PUT Category Response Status for {url}: {response.StatusCode}");

                if (response.IsSuccessStatusCode)
                {
                    var responseJson = await response.Content.ReadAsStringAsync();
                    Debug.WriteLine($"API: PUT Category Response JSON: {responseJson}");
                    return JsonSerializer.Deserialize<Category>(responseJson, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Debug.WriteLine($"API: PUT Category Error Content: {errorContent}");
                }
            }
            catch (HttpRequestException httpEx)
            {
                Debug.WriteLine($"API PUT Category HttpRequestException: {httpEx.Message}");
            }
            finally
            {
                // Asegúrate de que el contenido se deseche correctamente
                content.Dispose();
            }
            return default;
        }

        public async Task<bool> DeleteCategoryAsync(int apiId)
        {
            return await DeleteAsync($"CategoriaApi/{apiId}");
        }

        public async Task<List<Product>> GetProductsAsync()
        {
            // ¡ESTA LÍNEA ES CRÍTICA! DEBE SER "ProductoApi"
            return await GetListAsync<Product>("ProductoApi");
        }

        public async Task<Product?> GetProductAsync(int id)
        {
            // ¡ESTA LÍNEA ES CRÍTICA! DEBE SER "ProductoApi"
            return await GetAsync<Product>($"ProductoApi/{id}");
        }

        // --- MÉTODO PARA CREAR PRODUCTOS CON IMAGEN (multipart/form-data) ---
        public async Task<Product?> CreateProductAsync(Product product, string? imagePath)
        {
            // ¡ESTA LÍNEA ES CRÍTICA! DEBE SER "ProductoApi"
            string url = $"{_baseUrl}/ProductoApi";
            Debug.WriteLine($"API: POST Request to {url} with multipart/form-data for product: {product.Name}");

            using var content = new MultipartFormDataContent();

            // Añadir campos del producto
            content.Add(new StringContent(product.Name ?? string.Empty), "nombre");
            content.Add(new StringContent(product.Description ?? string.Empty), "descripcion");
            content.Add(new StringContent(product.Price.ToString(CultureInfo.InvariantCulture)), "precio");
            content.Add(new StringContent(product.CategoryId.ToString()), "categoriaId");
            content.Add(new StringContent(product.IsAvailable.ToString()), "isAvailable");
            content.Add(new StringContent(product.StockQuantity.ToString()), "stockQuantity");
            // No añadir ProductId si es un nuevo producto (Id = 0)
            if (product.Id != 0)
            {
                content.Add(new StringContent(product.Id.ToString()), "Id");
            }
            // Si el producto tiene un ApiId, también enviarlo para actualizaciones
            if (product.ApiId.HasValue)
            {
                content.Add(new StringContent(product.ApiId.Value.ToString()), "ApiId");
            }

            // Añadir la imagen si existe
            if (!string.IsNullOrEmpty(imagePath) && File.Exists(imagePath))
            {
                var fileStream = File.OpenRead(imagePath);
                content.Add(new StreamContent(fileStream), "imagen", Path.GetFileName(imagePath));
                Debug.WriteLine($"API: Adding image file: {Path.GetFileName(imagePath)}");
            }
            else
            {
                Debug.WriteLine("API: No image file to attach or file does not exist.");
            }

            try
            {
                var response = await _httpClient.PostAsync(url, content);
                Debug.WriteLine($"API: POST Product Response Status for {url}: {response.StatusCode}");

                if (response.IsSuccessStatusCode)
                {
                    var responseJson = await response.Content.ReadAsStringAsync();
                    Debug.WriteLine($"API: POST Product Response JSON: {responseJson}");
                    return JsonSerializer.Deserialize<Product>(responseJson, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Debug.WriteLine($"API: POST Product Error Content: {errorContent}");
                }
            }
            catch (HttpRequestException httpEx)
            {
                Debug.WriteLine($"API POST Product HttpRequestException: {httpEx.Message}");
            }
            finally
            {
                // Asegúrate de cerrar el stream del archivo después de usarlo
                if (content.Contains(content.FirstOrDefault(c => c.Headers.ContentDisposition?.Name == "\"imagen\"")))
                {
                    var streamContent = content.FirstOrDefault(c => c.Headers.ContentDisposition?.Name == "\"imagen\"") as StreamContent;
                    streamContent?.ReadAsStream().Dispose();
                }
            }
            return default;
        }

        // --- MÉTODO PARA ACTUALIZAR PRODUCTOS CON IMAGEN (multipart/form-data) ---
        public async Task<Product?> UpdateProductAsync(Product product, string? imagePath)
        {
            // ¡ESTA LÍNEA ES CRÍTICA! DEBE SER "ProductoApi"
            string url = $"{_baseUrl}/ProductoApi/{product.ApiId}"; // Asumiendo que PUT usa el ApiId
            Debug.WriteLine($"API: PUT Request to {url} with multipart/form-data for product: {product.Name}");

            using var content = new MultipartFormDataContent();

            // Añadir campos del producto
            content.Add(new StringContent(product.Name ?? string.Empty), "nombre");
            content.Add(new StringContent(product.Description ?? string.Empty), "descripcion");
            content.Add(new StringContent(product.Price.ToString(CultureInfo.InvariantCulture)), "precio");
            content.Add(new StringContent(product.CategoryId.ToString()), "categoriaId");
            content.Add(new StringContent(product.IsAvailable.ToString()), "isAvailable");
            content.Add(new StringContent(product.StockQuantity.ToString()), "stockQuantity");
            // No añadir ProductId si es un nuevo producto (Id = 0)
            if (product.Id != 0)
            {
                content.Add(new StringContent(product.Id.ToString()), "Id");
            }
            // Si el producto tiene un ApiId, también enviarlo para actualizaciones
            if (product.ApiId.HasValue)
            {
                content.Add(new StringContent(product.ApiId.Value.ToString()), "productoId");
            }

            // Añadir la imagen si existe
            if (!string.IsNullOrEmpty(imagePath) && File.Exists(imagePath))
            {
                var fileStream = File.OpenRead(imagePath);
                content.Add(new StreamContent(fileStream), "imagen", Path.GetFileName(imagePath));
                Debug.WriteLine($"API: Adding image file: {Path.GetFileName(imagePath)}");
            }
            else
            {
                Debug.WriteLine("API: No image file to attach or file does not exist.");
            }

            try
            {
                var response = await _httpClient.PutAsync(url, content);
                Debug.WriteLine($"API: PUT Product Response Status for {url}: {response.StatusCode}");

                if (response.IsSuccessStatusCode)
                {
                    var responseJson = await response.Content.ReadAsStringAsync();
                    Debug.WriteLine($"API: PUT Product Response JSON: {responseJson}");
                    return JsonSerializer.Deserialize<Product>(responseJson, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Debug.WriteLine($"API: PUT Product Error Content: {errorContent}");
                }
            }
            catch (HttpRequestException httpEx)
            {
                Debug.WriteLine($"API PUT Product HttpRequestException: {httpEx.Message}");
            }
            finally
            {
                // Asegúrate de cerrar el stream del archivo después de usarlo
                if (content.Contains(content.FirstOrDefault(c => c.Headers.ContentDisposition?.Name == "\"imagen\"")))
                {
                    var streamContent = content.FirstOrDefault(c => c.Headers.ContentDisposition?.Name == "\"imagen\"") as StreamContent;
                    streamContent?.ReadAsStream().Dispose();
                }
            }
            return default;
        }

        public async Task<bool> DeleteProductAsync(int apiId)
        {
            // ¡ESTA LÍNEA ES CRÍTICA! DEBE SER "ProductoApi"
            return await DeleteAsync($"ProductoApi/{apiId}");
        }

        public async Task<List<Order>> GetOrdersAsync()
        {
            // ¡IMPORTANTE! Asegúrate de que tu controlador de órdenes en el backend se llame CarritoApi
            return await GetListAsync<Order>("CarritoApi");
        }

        public async Task<Order?> GetOrderAsync(int id)
        {
            return await GetAsync<Order>($"CarritoApi/{id}");
        }

        public async Task<Order?> CreateOrderAsync(Order order)
        {
            // Este método genérico PostAsync debería funcionar si CarritoApi espera JSON
            return await PostAsync<Order>("CarritoApi", order);
        }

        public async Task<Order?> UpdateOrderAsync(Order order)
        {
            // Este método genérico PutAsync debería funcionar si CarritoApi espera JSON
            return await PutAsync<Order>($"CarritoApi/{order.ApiId}", order);
        }

        public async Task<bool> DeleteOrderAsync(int apiId)
        {
            return await DeleteAsync($"CarritoApi/{apiId}");
        }
    }
}
