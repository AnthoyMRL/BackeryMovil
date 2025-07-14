using BackeryMovil.Service;
using BackeryMovil.Services;
using BackeryMovil.ViewModels;
using BackeryMovil.Views;

namespace BackeryMovil;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            });

        // Register Services
        builder.Services.AddSingleton<DatabaseService>();
        builder.Services.AddSingleton<ApiService>();
        builder.Services.AddSingleton<FileService>();
        builder.Services.AddSingleton<CartService>();
        builder.Services.AddSingleton<ProductService>();
        builder.Services.AddSingleton<SyncService>();

        // Register ViewModels
        builder.Services.AddTransient<ProductsViewModel>();
        builder.Services.AddTransient<EditProductViewModel>();
        builder.Services.AddTransient<CreateProductViewModel>();
        builder.Services.AddTransient<CartViewModel>(); // ¡Nuevo registro!

        // Register Views
        builder.Services.AddTransient<ProductsPage>();
        builder.Services.AddTransient<MainPage>();
        builder.Services.AddTransient<EditProductPage>();
        builder.Services.AddTransient<CreateProductPage>();
        builder.Services.AddTransient<CartPage>(); // ¡Nuevo registro!

        var app = builder.Build();

        // Inicializar DatabaseService explícitamente para asegurar que los datos se siembren
        var databaseService = app.Services.GetRequiredService<DatabaseService>();
        Task.Run(async () => await databaseService.InitializeAsync()).Wait();

        return app;
    }
}
