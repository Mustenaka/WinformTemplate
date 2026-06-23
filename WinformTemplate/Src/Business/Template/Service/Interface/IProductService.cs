using WinformTemplate.Business.Template.Model;

namespace WinformTemplate.Business.Template.Service.Interface;

/// <summary>
/// 产品服务接口
/// 提供产品相关的业务逻辑
/// </summary>
public interface IProductService
{
    /// <summary>
    /// 获取所有产品
    /// </summary>
    Task<List<ProductModel>> GetAllProductsAsync();

    /// <summary>
    /// 根据ID获取产品
    /// </summary>
    Task<ProductModel?> GetProductByIdAsync(long id);

    /// <summary>
    /// 搜索产品（支持分页）
    /// </summary>
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
    /// 获取所有产品（用于导出）
    /// </summary>
    Task<List<ProductModel>> GetProductsForExportAsync(
        string? keyword = null,
        long? categoryId = null,
        int? status = null,
        decimal? minPrice = null,
        decimal? maxPrice = null,
        DateTime? startDate = null,
        DateTime? endDate = null);

    /// <summary>
    /// 添加产品
    /// </summary>
    /// <param name="product">产品信息</param>
    /// <returns>添加是否成功的结果和消息</returns>
    Task<(bool Success, string Message)> AddProductAsync(ProductModel product);

    /// <summary>
    /// 更新产品
    /// </summary>
    Task<(bool Success, string Message)> UpdateProductAsync(ProductModel product);

    /// <summary>
    /// 删除产品
    /// </summary>
    Task<(bool Success, string Message)> DeleteProductAsync(long id);

    /// <summary>
    /// 批量删除产品
    /// </summary>
    Task<(bool Success, string Message, int DeletedCount)> BatchDeleteProductsAsync(IEnumerable<long> ids);

    /// <summary>
    /// 批量更新产品状态
    /// </summary>
    Task<(bool Success, string Message, int UpdatedCount)> BatchUpdateStatusAsync(IEnumerable<long> ids, int status);

    /// <summary>
    /// 批量更新产品分类
    /// </summary>
    Task<(bool Success, string Message, int UpdatedCount)> BatchUpdateCategoryAsync(IEnumerable<long> ids, long categoryId);

    /// <summary>
    /// 验证产品数据
    /// </summary>
    /// <param name="product">产品信息</param>
    /// <param name="isUpdate">是否是更新操作</param>
    /// <returns>验证结果和错误消息</returns>
    Task<(bool IsValid, List<string> Errors)> ValidateProductAsync(ProductModel product, bool isUpdate = false);
}
