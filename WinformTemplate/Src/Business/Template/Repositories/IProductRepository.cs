using WinformTemplate.Business.Template.Model;
using WinformTemplate.Common.DataAccess;

namespace WinformTemplate.Business.Template.Repositories;

public interface IProductRepository : IRepository<ProductModel>
{
    Task<PagedResult<ProductModel>> SearchProductsAsync(
        string? keyword = null,
        long? categoryId = null,
        int? status = null,
        decimal? minPrice = null,
        decimal? maxPrice = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        int pageIndex = 1,
        int pageSize = 10,
        string? orderBy = null,
        bool ascending = true);

    Task<List<ProductModel>> GetAllForExportAsync(
        string? keyword = null,
        long? categoryId = null,
        int? status = null,
        decimal? minPrice = null,
        decimal? maxPrice = null,
        DateTime? startDate = null,
        DateTime? endDate = null);

    Task<bool> IsCodeExistsAsync(string code, long? excludeId = null);

    Task<int> GetCountByCategoryAsync(long categoryId);

    Task<int> DeleteByIdsAsync(IEnumerable<long> ids);

    Task<int> BatchUpdateStatusAsync(IEnumerable<long> ids, int status);

    Task<int> BatchUpdateCategoryAsync(IEnumerable<long> ids, long categoryId);
}
