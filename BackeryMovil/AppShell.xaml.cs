using BackeryMovil.Views;

namespace BackeryMovil
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute("editproduct", typeof(EditProductPage));
            Routing.RegisterRoute("createproduct", typeof(CreateProductPage));
            Routing.RegisterRoute("cart", typeof(CartPage)); // ¡Nueva ruta!
        }
    }
}
