using System.Diagnostics; // Añadir para Debug.WriteLine
using BackeryMovil.Models;
using SQLite;

namespace BackeryMovil.Services
{
    public class DatabaseService
    {
        private SQLiteAsyncConnection? _database;

        public async Task InitializeAsync()
        {
            if (_database is not null)
            {
                Debug.WriteLine("DatabaseService: Database already initialized.");
                return;
            }

            var databasePath = Path.Combine(FileSystem.AppDataDirectory, "bakery.db");
            // ¡PARA DEPURACIÓN SOLAMENTE! Elimina la base de datos para forzar un re-seed
            // Comentar o eliminar la siguiente línea para que los datos persistan
            // if (File.Exists(databasePath))
            // {
            //     File.Delete(databasePath);
            //     Debug.WriteLine("DatabaseService: Existing database file deleted for re-seeding.");
            // }
            Debug.WriteLine($"DatabaseService: Initializing database at {databasePath}");
            _database = new SQLiteAsyncConnection(databasePath);

            await _database.CreateTableAsync<Category>();
            await _database.CreateTableAsync<Product>();
            await _database.CreateTableAsync<Order>();
            await _database.CreateTableAsync<OrderItem>();
            Debug.WriteLine("DatabaseService: Tables created.");

            await SeedDataAsync();
            Debug.WriteLine("DatabaseService: Seed data check completed.");
        }

        private async Task SeedDataAsync()
        {
            var categoryCount = await _database!.Table<Category>().CountAsync();
            if (categoryCount == 0)
            {
                Debug.WriteLine("DatabaseService: Seeding initial categories...");
                var categories = new List<Category>
             {
                 new Category { Name = "Panadería", Description = "Panes artesanales y de molde", IconName = "🍞" },
                 new Category { Name = "Pastelería Fina", Description = "Pasteles, tartas y postres elaborados", IconName = "🎂" },
                 new Category { Name = "Bollería", Description = "Croissants, donuts y hojaldre", IconName = "🥐" },
                 new Category { Name = "Cupcakes y Muffins", Description = "Variedad de cupcakes y muffins", IconName = "🧁" },
                 new Category { Name = "Galletas y Bizcochos", Description = "Galletas caseras y bizcochos", IconName = "🍪" }
             };

                await _database.InsertAllAsync(categories);
                Debug.WriteLine($"DatabaseService: {categories.Count} categories seeded.");

                Debug.WriteLine("DatabaseService: Seeding initial products...");
                var products = new List<Product>
             {
                 new Product { Name = "Pan Integral", Description = "Pan integral con semillas", Price = 3.50m, CategoryId = 1, StockQuantity = 20, ImagenUrl = "/images/placeholder.png" },
                 new Product { Name = "Croissant", Description = "Croissant de mantequilla", Price = 2.00m, CategoryId = 3, StockQuantity = 15, ImagenUrl = "/images/placeholder.png" },
                 new Product { Name = "Torta de Chocolate", Description = "Torta húmeda de chocolate", Price = 25.00m, CategoryId = 2, StockQuantity = 5, ImagenUrl = "/images/placeholder.png" },
                 new Product { Name = "Muffin de Arándanos", Description = "Muffin con arándanos frescos", Price = 2.75m, CategoryId = 4, StockQuantity = 12, ImagenUrl = "/images/placeholder.png" },
                 new Product { Name = "Galletas de Avena", Description = "Galletas caseras de avena", Price = 1.50m, CategoryId = 5, StockQuantity = 30, ImagenUrl = "/images/placeholder.png" }
             };

                await _database.InsertAllAsync(products);
                Debug.WriteLine($"DatabaseService: {products.Count} products seeded.");
            }
            else
            {
                Debug.WriteLine($"DatabaseService: Categories already exist ({categoryCount} found). Skipping seeding.");
            }
        }

        public async Task<List<Category>> GetCategoriesAsync()
        {
            await InitializeAsync();
            var categories = await _database!.Table<Category>().ToListAsync();
            Debug.WriteLine($"DatabaseService: Retrieved {categories.Count} categories from DB.");
            return categories;
        }

        public async Task<Category?> GetCategoryAsync(int id)
        {
            await InitializeAsync();
            var category = await _database!.Table<Category>().Where(c => c.Id == id).FirstOrDefaultAsync();
            Debug.WriteLine($"DatabaseService: Retrieved category ID {id}: {category?.Name ?? "Not Found"}");
            return category;
        }

        public async Task<int> SaveCategoryAsync(Category category)
        {
            await InitializeAsync();
            category.LastSyncAt = DateTime.Now;

            if (category.Id != 0)
            {
                Debug.WriteLine($"DatabaseService: Updating category: {category.Name} (ID: {category.Id})");
                return await _database!.UpdateAsync(category);
            }
            else
            {
                Debug.WriteLine($"DatabaseService: Inserting new category: {category.Name}");
                return await _database!.InsertAsync(category);
            }
        }

        public async Task<int> DeleteCategoryAsync(Category category)
        {
            await InitializeAsync();
            Debug.WriteLine($"DatabaseService: Deleting category: {category.Name} (ID: {category.Id})");
            return await _database!.DeleteAsync(category);
        }

        public async Task<List<Product>> GetProductsAsync()
        {
            await InitializeAsync();
            var products = await _database!.Table<Product>().ToListAsync();
            Debug.WriteLine($"DatabaseService: Retrieved {products.Count} products from DB.");

            foreach (var product in products)
            {
                product.Category = await GetCategoryAsync(product.CategoryId);
            }

            return products;
        }

        public async Task<List<Product>> GetProductsByCategoryAsync(int categoryId)
        {
            await InitializeAsync();
            var products = await _database!.Table<Product>().Where(p => p.CategoryId == categoryId).ToListAsync();
            Debug.WriteLine($"DatabaseService: Retrieved {products.Count} products for category ID {categoryId}.");

            foreach (var product in products)
            {
                product.Category = await GetCategoryAsync(product.CategoryId);
            }

            return products;
        }

        public async Task<Product?> GetProductAsync(int id)
        {
            await InitializeAsync();
            var product = await _database!.Table<Product>().Where(p => p.Id == id).FirstOrDefaultAsync();
            if (product != null)
            {
                product.Category = await GetCategoryAsync(product.CategoryId);
            }
            Debug.WriteLine($"DatabaseService: Retrieved product ID {id}: {product?.Name ?? "Not Found"}");
            return product;
        }

        public async Task<int> SaveProductAsync(Product product)
        {
            await InitializeAsync();
            product.UpdatedAt = DateTime.Now;
            product.LastSyncAt = DateTime.Now;

            if (product.Id != 0)
            {
                Debug.WriteLine($"DatabaseService: Updating product: {product.Name} (ID: {product.Id}), Desc: {product.Description}, Price: {product.Price}");
                return await _database!.UpdateAsync(product);
            }
            else
            {
                Debug.WriteLine($"DatabaseService: Inserting new product: {product.Name} (ID: {product.Id}), Desc: {product.Description}, Price: {product.Price}");
                return await _database!.InsertAsync(product);
            }
        }

        public async Task<int> DeleteProductAsync(Product product)
        {
            await InitializeAsync();
            Debug.WriteLine($"DatabaseService: Deleting product: {product.Name} (ID: {product.Id})");
            return await _database!.DeleteAsync(product);
        }

        public async Task<List<Order>> GetOrdersAsync()
        {
            await InitializeAsync();
            var orders = await _database!.Table<Order>().OrderByDescending(o => o.OrderDate).ToListAsync();
            Debug.WriteLine($"DatabaseService: Retrieved {orders.Count} orders from DB.");
            return orders;
        }

        public async Task<Order?> GetOrderAsync(int id)
        {
            await InitializeAsync();
            var order = await _database!.Table<Order>().Where(o => o.Id == id).FirstOrDefaultAsync();
            Debug.WriteLine($"DatabaseService: Retrieved order ID {id}: {order?.CustomerName ?? "Not Found"}");
            return order;
        }

        public async Task<int> SaveOrderAsync(Order order)
        {
            await InitializeAsync();
            order.LastSyncAt = DateTime.Now;

            if (order.Id != 0)
            {
                Debug.WriteLine($"DatabaseService: Updating order: {order.CustomerName} (ID: {order.Id})");
                return await _database!.UpdateAsync(order);
            }
            else
            {
                Debug.WriteLine($"DatabaseService: Inserting new order: {order.CustomerName}");
                return await _database!.InsertAsync(order);
            }
        }

        public async Task<int> DeleteOrderAsync(Order order)
        {
            await InitializeAsync();
            Debug.WriteLine($"DatabaseService: Deleting order: {order.CustomerName} (ID: {order.Id})");
            return await _database!.DeleteAsync(order);
        }

        public async Task<List<OrderItem>> GetOrderItemsAsync(int orderId)
        {
            await InitializeAsync();
            var items = await _database!.Table<OrderItem>().Where(oi => oi.OrderId == orderId).ToListAsync();
            Debug.WriteLine($"DatabaseService: Retrieved {items.Count} order items for order ID {orderId}.");

            foreach (var item in items)
            {
                item.Product = await GetProductAsync(item.ProductId);
            }

            return items;
        }

        public async Task<OrderItem?> GetOrderItemAsync(int id)
        {
            await InitializeAsync();
            var item = await _database!.Table<OrderItem>().Where(oi => oi.Id == id).FirstOrDefaultAsync();
            if (item != null)
            {
                item.Product = await GetProductAsync(item.ProductId);
            }
            Debug.WriteLine($"DatabaseService: Retrieved order item ID {id}: {item?.Product?.Name ?? "Not Found"}");
            return item;
        }

        public async Task<int> SaveOrderItemAsync(OrderItem orderItem)
        {
            await InitializeAsync();

            if (orderItem.Id != 0)
            {
                Debug.WriteLine($"DatabaseService: Updating order item: Product ID {orderItem.ProductId}, Quantity {orderItem.Quantity} (ID: {orderItem.Id})");
                return await _database!.UpdateAsync(orderItem);
            }
            else
            {
                Debug.WriteLine($"DatabaseService: Inserting new order item: Product ID {orderItem.ProductId}, Quantity {orderItem.Quantity}");
                return await _database!.InsertAsync(orderItem);
            }
        }

        public async Task<int> DeleteOrderItemAsync(OrderItem orderItem)
        {
            await InitializeAsync();
            Debug.WriteLine($"DatabaseService: Deleting order item: Product ID {orderItem.ProductId}, Quantity {orderItem.Quantity} (ID: {orderItem.Id})");
            return await _database!.DeleteAsync(orderItem);
        }
    }
}
