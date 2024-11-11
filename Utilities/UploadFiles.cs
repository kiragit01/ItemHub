
namespace ItemHub.Utilities;

public static class UploadFiles
{
    public static async Task<string> UploadAvatarAsync(IFormFile? file, string userLogin)
    {
        if (file == null)
        {
            return "images/NoAvatar.png";
        }

        // Путь для хранения аватарки пользователя
        var folderPath = Path.Combine("images", userLogin);

        // Создаем папку для хранения файлов
        var uploadPath = Path.Combine(WebRootPath.Path, folderPath);
        Directory.CreateDirectory(uploadPath);

        var type = Path.GetExtension(file.FileName).ToLower();
        var fileName = "Avatar" + type;

        var pathForList = Path.Combine(folderPath, fileName);
        var fullPath = Path.Combine(WebRootPath.Path, pathForList);

        // Сохраняем файл
        await using var fileStream = new FileStream(fullPath, FileMode.Create);
        await file.CopyToAsync(fileStream);

        return pathForList;
    }

    public static async Task<List<string>> UploadItemImagesAsync(IFormFileCollection? files, string userLogin, Guid id)
    {
        if (files == null || files.Count == 0)
        {
            return ["images/NoImage.png"];
        }

        // Путь для хранения изображений товаров
        var folderPath = Path.Combine("images", userLogin, id.ToString());

        // Создаем папку для хранения файлов
        var uploadPath = Path.Combine(WebRootPath.Path, folderPath);
        Directory.CreateDirectory(uploadPath);

        var uploadedPaths = new List<string>();
        foreach (var file in files)
        {
            var type = Path.GetExtension(file.FileName).ToLower();
            var fileName = $"{Guid.NewGuid()}{type}";

            var pathForList = Path.Combine(folderPath, fileName);
            var fullPath = Path.Combine(WebRootPath.Path, pathForList);
            uploadedPaths.Add(pathForList);

            // Сохраняем файл
            await using var fileStream = new FileStream(fullPath, FileMode.Create);
            await file.CopyToAsync(fileStream);
        }

        return uploadedPaths;
    }

}