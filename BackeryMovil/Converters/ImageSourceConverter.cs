using System.Globalization;

namespace BackeryMovil.Converters
{
    public class ImageSourceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string imagePath && !string.IsNullOrEmpty(imagePath) && File.Exists(imagePath))
            {
                return ImageSource.FromFile(imagePath);
            }
            return ImageSource.FromFile("placeholder.png");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
