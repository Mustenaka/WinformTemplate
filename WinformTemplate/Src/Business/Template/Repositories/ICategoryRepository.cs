using WinformTemplate.Business.Template.Model;
using WinformTemplate.Common.DataAccess;

namespace WinformTemplate.Business.Template.Repositories;

public interface ICategoryRepository : IRepository<CategoryModel>
{
    Task<List<CategoryModel>> GetActiveCategoriesAsync();

    Task<List<CategoryModel>> GetCategoryTreeAsync();

    Task<List<CategoryModel>> GetChildrenAsync(long? parentId);

    Task<bool> IsNameExistsAsync(string name, long? excludeId = null);
}
