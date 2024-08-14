namespace WinformTemplate.Tools.Files;

/// <summary>
/// 文件结点数据结构
/// </summary>
public class DirectoryNode
{
    // Floder
    public Dictionary<string, DirectoryNode> SubDirectories { get; set; }

    // Files
    public List<string> Files { get; set; }

    // Absolute path
    public string AbsPath;

    public DirectoryNode()
    {
        SubDirectories = new Dictionary<string, DirectoryNode>();
        Files = new List<string>();
        AbsPath = "";
    }

    /// <summary>
    /// 添加文件到指定路径
    /// </summary>
    /// <param name="root">根节点</param>
    /// <param name="path">文件路径</param>
    public static void AddFile(DirectoryNode root, string path)
    {
        string[] parts = path.Split('/');
        DirectoryNode current = root;

        for (int i = 0; i < parts.Length - 1; i++)
        {
            if (!current.SubDirectories.ContainsKey(parts[i]))
            {
                current.SubDirectories[parts[i]] = new DirectoryNode();
            }
            current = current.SubDirectories[parts[i]];
        }

        current.Files.Add(parts[^1]);
    }

    /// <summary>
    /// 添加目录到指定路径
    /// </summary>
    /// <param name="root">根节点</param>
    /// <param name="path">文件路径</param>
    public static void AddDirectory(DirectoryNode root, string path)
    {
        string[] parts = path.Split('/');
        DirectoryNode current = root;

        foreach (var part in parts)
        {
            if (!current.SubDirectories.ContainsKey(part))
            {
                current.SubDirectories[part] = new DirectoryNode();
            }
            current = current.SubDirectories[part];
        }
    }

    /// <summary>
    /// 检查文件是否存在
    /// </summary>
    /// <param name="root">根节点</param>
    /// <param name="path">文件路径</param>
    /// <returns></returns>
    public static bool IsFileExists(DirectoryNode root, string path)
    {
        string[] parts = path.Split('/');
        DirectoryNode current = root;

        for (int i = 0; i < parts.Length - 1; i++)
        {
            if (!current.SubDirectories.ContainsKey(parts[i]))
            {
                return false;
            }
            current = current.SubDirectories[parts[i]];
        }

        return current.Files.Contains(parts[^1]);
    }

    /// <summary>
    /// 打印目录结构
    /// </summary>
    /// <param name="node">结点路径</param>
    /// <param name="indent">编号</param>
    public static void PrintDirectory(DirectoryNode node, string indent)
    {
        foreach (var dir in node.SubDirectories)
        {
            Console.WriteLine($"{indent}{dir.Key}/");
            PrintDirectory(dir.Value, indent + "  ");
        }

        foreach (var file in node.Files)
        {
            Console.WriteLine($"{indent}{file}");
        }
    }

    /// <summary>
    /// 查询包含特定名称的节点的绝对路径
    /// </summary>
    /// <param name="name">要查询的名称</param>
    /// <returns>包含特定名称的节点的绝对路径</returns>
    public string? FindNodeAbsPath(string name)
    {
        // 检查当前节点
        if (SubDirectories.ContainsKey(name))
        {
            return SubDirectories[name].AbsPath;
        }

        // 递归检查子节点
        foreach (var subDir in SubDirectories)
        {
            var result = subDir.Value.FindNodeAbsPath(name);
            if (result != null)
            {
                return result;
            }
        }

        return null;
    }
}