namespace BackeryMovil.Service
{
    public class FileService
    {
        private readonly string _imagesDirectory;

        public FileService()
        {
            _imagesDirectory = Path.Combine(FileSystem.AppDataDirectory, "Images");
            Directory.CreateDirectory(_imagesDirectory);
        }

        public async Task<string> SaveImageAsync(Stream imageStream, string fileName)
        {
            try
            {
                var fileExtension = Path.GetExtension(fileName);
                var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
                var filePath = Path.Combine(_imagesDirectory, uniqueFileName);

                using var fileStream = File.Create(filePath);
                await imageStream.CopyToAsync(fileStream);

                return filePath;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving image: {ex.Message}");
                return string.Empty;
            }
        }

        public async Task<string> SaveImageFromCameraAsync()
        {
            try
            {
                var photo = await MediaPicker.CapturePhotoAsync();
                if (photo != null)
                {
                    using var stream = await photo.OpenReadAsync();
                    return await SaveImageAsync(stream, photo.FileName);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error capturing photo: {ex.Message}");
            }
            return string.Empty;
        }

        public async Task<string> SaveImageFromGalleryAsync()
        {
            try
            {
                var photo = await MediaPicker.PickPhotoAsync();
                if (photo != null)
                {
                    using var stream = await photo.OpenReadAsync();
                    return await SaveImageAsync(stream, photo.FileName);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error picking photo: {ex.Message}");
            }
            return string.Empty;
        }

        public bool DeleteImage(string imagePath)
        {
            try
            {
                if (File.Exists(imagePath))
                {
                    File.Delete(imagePath);
                    return true;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error deleting image: {ex.Message}");
            }
            return false;
        }

        public ImageSource GetImageSource(string imagePath)
        {
            if (string.IsNullOrEmpty(imagePath) || !File.Exists(imagePath))
            {
                return ImageSource.FromFile("placeholder.png");
            }
            return ImageSource.FromFile(imagePath);
        }

        public async Task<string> ExportDataToJsonAsync<T>(List<T> data, string fileName)
        {
            try
            {
                var json = System.Text.Json.JsonSerializer.Serialize(data, new System.Text.Json.JsonSerializerOptions
                {
                    WriteIndented = true
                });

                var filePath = Path.Combine(FileSystem.AppDataDirectory, fileName);
                await File.WriteAllTextAsync(filePath, json);
                return filePath;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error exporting data: {ex.Message}");
                return string.Empty;
            }
        }

        public async Task<List<T>?> ImportDataFromJsonAsync<T>(string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    var json = await File.ReadAllTextAsync(filePath);
                    return System.Text.Json.JsonSerializer.Deserialize<List<T>>(json);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error importing data: {ex.Message}");
            }
            return null;
        }
    }
}
