using WinformTemplate.Business.Template.Model;

namespace WinformTemplate.Business.Template.Service.Interface;

/// <summary>
/// 分类服务接口
/// </summary>
public interface ICategoryService
{
    /// <summary>
    /// 获取所有分类
    /// </summary>
    Task<List<CategoryModel>> GetAllCategoriesAsync();

    /// <summary>
    /// 获取所有启用的分类
    /// </summary>
    Task<List<CategoryModel>> GetActiveCategoriesAsync();

    /// <summary>
    /// 获取树形结构的分类
    /// </summary>
    Task<List<CategoryModel>> GetCategoryTreeAsync();

    /// <summary>
    /// 根据ID获取分类
    /// </summary>
    Task<CategoryModel?> GetCategoryByIdAsync(long id);

    /// <summary>
    /// 添加分类
    /// </summary>
    Task<(bool Success, string Message)> AddCategoryAsync(CategoryModel category);

    /// <summary>
    /// 更新分类
    /// </summary>
    Task<(bool Success, string Message)> UpdateCategoryAsync(CategoryModel category);

    /// <summary>
    /// 删除分类
    /// </summary>
    Task<(bool Success, string Message)> DeleteCategoryAsync(long id);
}
