namespace WinformTemplate.Tools.Files;

/// <summary>
/// 文件选择器
/// </summary>
public static class FileSelector
{
    /// <summary>
    /// 打开文件选择窗口，选择指定格式的文件
    /// </summary>
    /// <param name="filter">文件过滤器，例如 "Excel 文件 (*.xlsx)|*.xlsx|所有文件 (*.*)|*.*"</param>
    /// <returns>所选文件的路径，如果没有选择文件则返回 null</returns>
    public static string? SelectFile(string filter = "Excel 文件 (*.xlsx)|*.xlsx|所有文件 (*.*)|*.*")
    {
        using (var openFileDialog = new OpenFileDialog())
        {
            openFileDialog.Filter = filter;
            openFileDialog.Title = "请选择一个文件";

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                return openFileDialog.FileName;
            }
        }

        return null;
    }

    /// <summary>
    /// 打开文件夹选择窗口，选择目标路径，并将指定文件拷贝到该路径
    /// </summary>
    /// <param name="sourceFilePath">源文件路径</param>
    /// <returns>如果拷贝成功则返回目标文件路径，否则返回 null</returns>
    public static string? CopyFileToSelectedPath(string sourceFilePath)
    {
        using (var folderBrowserDialog = new FolderBrowserDialog())
        {
            folderBrowserDialog.Description = "请选择目标文件夹";

            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                string targetFolderPath = folderBrowserDialog.SelectedPath;
                string targetFilePath = Path.Combine(targetFolderPath, Path.GetFileName(sourceFilePath));

                try
                {
                    File.Copy(sourceFilePath, targetFilePath, overwrite: true);
                    return targetFilePath;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"文件拷贝失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        return null;
    }

    /// <summary>
    /// 打开文件夹选择窗口，并在选中的文件夹内创建一个指定名称的子文件夹
    /// </summary>
    /// <param name="folderName">要创建的子文件夹名称</param>
    /// <returns>新创建的子文件夹路径，如果没有选择文件夹或创建失败则返回 null</returns>
    public static string? CreateFolderInSelectedPath(string folderName)
    {
        using (var folderBrowserDialog = new FolderBrowserDialog())
        {
            folderBrowserDialog.Description = "请选择一个文件夹";

            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                string selectedFolderPath = folderBrowserDialog.SelectedPath;
                string newFolderPath = Path.Combine(selectedFolderPath, folderName);

                try
                {
                    if (!Directory.Exists(newFolderPath))
                    {
                        Directory.CreateDirectory(newFolderPath);
                    }
                    return newFolderPath;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"文件夹创建失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        return null;
    }

    /// <summary>
    /// 打开文件夹选择窗口，选择一个文件夹作为根目录，并根据传入的目录节点结构创建子文件夹
    /// </summary>
    /// <param name="rootDirectoryNode">要创建的目录结构的根节点</param>
    /// <returns>根目录路径，如果没有选择文件夹则返回 null</returns>
    public static string? CreateFoldersFromDirectoryNode(DirectoryNode rootDirectoryNode)
    {
        using (var folderBrowserDialog = new FolderBrowserDialog())
        {
            folderBrowserDialog.Description = "请选择一个文件夹作为根目录";

            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                string rootPath = folderBrowserDialog.SelectedPath;

                try
                {
                    CreateSubDirectories(rootPath, rootDirectoryNode);
                    return rootPath;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"文件夹创建失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        return null;
    }

    /// <summary>
    /// 递归创建子目录
    /// </summary>
    /// <param name="currentPath">当前路径</param>
    /// <param name="directoryNode">目录节点</param>
    private static void CreateSubDirectories(string currentPath, DirectoryNode directoryNode)
    {
        directoryNode.AbsPath = currentPath;

        foreach (var dir in directoryNode.SubDirectories)
        {
            string subDirPath = Path.Combine(currentPath, dir.Key);
            if (!Directory.Exists(subDirPath))
            {
                Directory.CreateDirectory(subDirPath);
            }
            CreateSubDirectories(subDirPath, dir.Value);
        }

        // 如果需要在目录节点中包含文件，可以在这里处理
        // foreach (var file in directoryNode.Files)
        // {
        //     // 文件处理逻辑，例如创建空文件
        //     File.Create(Path.Combine(currentPath, file)).Dispose();
        // }
    }
}