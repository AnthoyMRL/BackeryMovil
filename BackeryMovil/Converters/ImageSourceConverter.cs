using System.Globalization;

namespace BackeryMovil.Converters
{
    public class ImageSourceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string imagePath && !string.IsNullOrEmpty(imagePath))
            {
                // Si es una URL (comienza con http o https)
                if (imagePath.StartsWith("http://") || imagePath.StartsWith("https://"))
                {
                    return ImageSource.FromUri(new Uri(imagePath));
                }

                // Si es una URL relativa de la API (comienza con /)
                if (imagePath.StartsWith("/"))
                {
                    // Construir la URL completa
                    var baseUrl = "https://localhost:7052"; // Cambia por tu URL base
                    return ImageSource.FromUri(new Uri(baseUrl + imagePath));
                }

                // Si es un archivo local
                if (File.Exists(imagePath))
                {
                    return ImageSource.FromFile(imagePath);
                }
            }

            // Imagen por defecto
            return ImageSource.FromFile("placeholder.png");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
