namespace WinformTemplate.Tools.Files;

public static class FilePathModifier
{
    /// <summary>
    /// 在文件路径中添加标识符，并保留原始扩展名
    /// </summary>
    /// <param name="filePath">原始文件路径</param>
    /// <param name="identifier">要添加的标识符</param>
    /// <returns>添加标识符后的文件路径</returns>
    public static string AddIdentifierToFilePath(string filePath, string identifier)
    {
        // 获取文件目录、文件名和扩展名
        var directory = Path.GetDirectoryName(filePath) ?? string.Empty;
        var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filePath);
        var extension = Path.GetExtension(filePath);

        // 创建新的文件名，包含标识符
        var newFileName = $"{fileNameWithoutExtension}_{identifier}{extension}";

        // 组合新的文件路径
        var newFilePath = Path.Combine(directory, newFileName);

        return newFilePath;
    }
}