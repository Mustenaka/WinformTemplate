using System.Linq.Expressions;
using WinformTemplate.Business.Template.Model;

namespace WinformTemplate.Business.Template.Repositories.Interface;

/// <summary>
/// 分类仓储接口
/// </summary>
public interface ICategoryRepository
{
    // ==================== 基础CRUD ====================

    IQueryable<CategoryModel> GetAll();

    IQueryable<CategoryModel> GetByCondition(Expression<Func<CategoryModel, bool>> filter);

    Task<CategoryModel?> GetByIdAsync(long id);

    Task AddAsync(CategoryModel category);

    Task AddRangeAsync(IEnumerable<CategoryModel> categories);

    void Update(CategoryModel category);

    void Delete(CategoryModel category);

    Task<bool> DeleteByIdAsync(long id);

    Task<bool> ExistsAsync(Expression<Func<CategoryModel, bool>> filter);

    Task<int> SaveChangesAsync();

    // ==================== 高级查询 ====================

    /// <summary>
    /// 获取所有启用的分类
    /// </summary>
    Task<List<CategoryModel>> GetActiveCategoriesAsync();

    /// <summary>
    /// 获取树形结构的分类列表
    /// </summary>
    Task<List<CategoryModel>> GetCategoryTreeAsync();

    /// <summary>
    /// 根据父ID获取子分类
    /// </summary>
    Task<List<CategoryModel>> GetChildrenAsync(long? parentId);

    /// <summary>
    /// 检查分类名称是否已存在
    /// </summary>
    Task<bool> IsNameExistsAsync(string name, long? excludeId = null);
}
