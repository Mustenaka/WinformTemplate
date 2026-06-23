using System.Linq.Expressions;
using WinformTemplate.Business.Template.Model;

namespace WinformTemplate.Business.Template.Repositories.Interface;

/// <summary>
/// 产品仓储接口
/// 定义产品数据访问的所有方法
/// </summary>
public interface IProductRepository
{
    // ==================== 基础CRUD ====================

    /// <summary>
    /// 获取所有产品
    /// </summary>
    IQueryable<ProductModel> GetAll();

    /// <summary>
    /// 根据条件获取产品
    /// </summary>
    IQueryable<ProductModel> GetByCondition(Expression<Func<ProductModel, bool>> filter);

    /// <summary>
    /// 根据ID获取产品
    /// </summary>
    Task<ProductModel?> GetByIdAsync(long id);

    /// <summary>
    /// 添加产品
    /// </summary>
    Task AddAsync(ProductModel product);

    /// <summary>
    /// 批量添加产品
    /// </summary>
    Task AddRangeAsync(IEnumerable<ProductModel> products);

    /// <summary>
    /// 更新产品
    /// </summary>
    void Update(ProductModel product);

    /// <summary>
    /// 批量更新产品
    /// </summary>
    void UpdateRange(IEnumerable<ProductModel> products);

    /// <summary>
    /// 删除产品
    /// </summary>
    void Delete(ProductModel product);

    /// <summary>
    /// 批量删除产品
    /// </summary>
    void DeleteRange(IEnumerable<ProductModel> products);

    /// <summary>
    /// 根据ID删除产品
    /// </summary>
    Task<bool> DeleteByIdAsync(long id);

    /// <summary>
    /// 根据ID列表批量删除产品
    /// </summary>
    Task<int> DeleteByIdsAsync(IEnumerable<long> ids);

    /// <summary>
    /// 检查产品是否存在
    /// </summary>
    Task<bool> ExistsAsync(Expression<Func<ProductModel, bool>> filter);

    /// <summary>
    /// 保存更改
    /// </summary>
    Task<int> SaveChangesAsync();

    // ==================== 高级查询 ====================

    /// <summary>
    /// 多条件搜索产品（支持分页和排序）
    /// </summary>
    /// <param name="keyword">关键词（搜索名称和编码）</param>
    /// <param name="categoryId">分类ID</param>
    /// <param name="status">状态</param>
    /// <param name="minPrice">最低价格</param>
    /// <param name="maxPrice">最高价格</param>
    /// <param name="startDate">开始日期</param>
    /// <param name="endDate">结束日期</param>
    /// <param name="pageIndex">页码（从1开始）</param>
    /// <param name="pageSize">每页大小</param>
    /// <param name="orderBy">排序字段（Name/Price/CreateAt/Stock）</param>
    /// <param name="ascending">是否升序</param>
    /// <returns>产品列表和总数</returns>
    Task<(List<ProductModel> Items, int TotalCount)> SearchProductsAsync(
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

    /// <summary>
    /// 获取所有产品（用于导出，不分页）
    /// </summary>
    Task<List<ProductModel>> GetAllForExportAsync(
        string? keyword = null,
        long? categoryId = null,
        int? status = null,
        decimal? minPrice = null,
        decimal? maxPrice = null,
        DateTime? startDate = null,
        DateTime? endDate = null);

    /// <summary>
    /// 检查产品编码是否已存在
    /// </summary>
    Task<bool> IsCodeExistsAsync(string code, long? excludeId = null);

    /// <summary>
    /// 根据分类ID获取产品数量
    /// </summary>
    Task<int> GetCountByCategoryAsync(long categoryId);

    /// <summary>
    /// 批量更新产品状态
    /// </summary>
    Task<int> BatchUpdateStatusAsync(IEnumerable<long> ids, int status);

    /// <summary>
    /// 批量更新产品分类
    /// </summary>
    Task<int> BatchUpdateCategoryAsync(IEnumerable<long> ids, long categoryId);
}
