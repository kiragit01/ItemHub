namespace ItemHub.Utilities;

public static class UploadFiles
{
    public static async Task<string> UploadAvatar(IFormFile? file, string userLogin, string webRootPath)
    {
        if (file == null)
        {
            return "images/NoAvatar.png";
        }

        // Путь для хранения аватарки пользователя
        var folderPath = Path.Combine("images", userLogin);

        // Создаем папку для хранения файлов
        var uploadPath = Path.Combine(webRootPath, folderPath);
        Directory.CreateDirectory(uploadPath);

        var type = Path.GetExtension(file.FileName).ToLower();
        var fileName = "Avatar" + type;

        var pathForList = Path.Combine(folderPath, fileName);
        var fullPath = Path.Combine(webRootPath, pathForList);

        // Сохраняем файл
        await using var fileStream = new FileStream(fullPath, FileMode.Create);
        await file.CopyToAsync(fileStream);

        return pathForList;
    }

    public static async Task<List<string>> UploadItemImages(IFormFileCollection? files, string userLogin, string webRootPath, Guid id)
    {
        if (files == null || files.Count == 0)
        {
            return ["images/NoImage.png"];
        }

        // Путь для хранения изображений товаров
        var folderPath = Path.Combine("images", userLogin, id.ToString());

        // Создаем папку для хранения файлов
        var uploadPath = Path.Combine(webRootPath, folderPath);
        Directory.CreateDirectory(uploadPath);

        var uploadedPaths = new List<string>();
        foreach (var file in files)
        {
            var type = Path.GetExtension(file.FileName).ToLower();
            var fileName = $"{Guid.NewGuid()}{type}";

            var pathForList = Path.Combine(folderPath, fileName);
            var fullPath = Path.Combine(webRootPath, pathForList);
            uploadedPaths.Add(pathForList);

            // Сохраняем файл
            await using var fileStream = new FileStream(fullPath, FileMode.Create);
            await file.CopyToAsync(fileStream);
        }

        return uploadedPaths;
    }

}