using WinformTemplate.Business.Template.Model;
using WinformTemplate.Business.Template.Repositories;
using WinformTemplate.Business.Template.Service.Interface;
using WinformTemplate.Common.DataAccess;
using WinformTemplate.Logger;

namespace WinformTemplate.Business.Template.Service;

/// <summary>
/// 产品服务实现
/// </summary>
public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;
    private readonly ICategoryRepository _categoryRepository;

    public ProductService(
        IProductRepository productRepository,
        ICategoryRepository categoryRepository)
    {
        _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
        _categoryRepository = categoryRepository ?? throw new ArgumentNullException(nameof(categoryRepository));
    }

    public async Task<List<ProductModel>> GetAllProductsAsync()
    {
        try
        {
            var result = await _productRepository.QueryAsync(new QueryRequest
            {
                Page = 1,
                PageSize = int.MaxValue
            });
            return result.Items.ToList();
        }
        catch (Exception ex)
        {
            Debug.Error("获取所有产品失败", ex);
            throw;
        }
    }

    public async Task<ProductModel?> GetProductByIdAsync(long id)
    {
        try
        {
            return await _productRepository.GetByIdAsync(id);
        }
        catch (Exception ex)
        {
            Debug.Error($"获取产品失败，ID: {id}", ex);
            throw;
        }
    }

    public async Task<(List<ProductModel> Items, int TotalCount)> SearchProductsAsync(
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
        bool ascending = true)
    {
        try
        {
            var result = await _productRepository.SearchProductsAsync(
                keyword, categoryId, status, minPrice, maxPrice,
                startDate, endDate, pageIndex, pageSize, orderBy, ascending);
            return (result.Items.ToList(), result.Total);
        }
        catch (Exception ex)
        {
            Debug.Error("搜索产品失败", ex);
            throw;
        }
    }

    public async Task<List<ProductModel>> GetProductsForExportAsync(
        string? keyword = null,
        long? categoryId = null,
        int? status = null,
        decimal? minPrice = null,
        decimal? maxPrice = null,
        DateTime? startDate = null,
        DateTime? endDate = null)
    {
        try
        {
            return await _productRepository.GetAllForExportAsync(
                keyword, categoryId, status, minPrice, maxPrice, startDate, endDate);
        }
        catch (Exception ex)
        {
            Debug.Error("获取导出数据失败", ex);
            throw;
        }
    }

    public async Task<(bool Success, string Message)> AddProductAsync(ProductModel product)
    {
        try
        {
            // 验证数据
            var (isValid, errors) = await ValidateProductAsync(product, isUpdate: false);
            if (!isValid)
            {
                return (false, string.Join("; ", errors));
            }

            await _productRepository.AddAsync(product);

            Debug.Info($"添加产品成功: {product.Name} ({product.Code})");
            return (true, "添加成功");
        }
        catch (Exception ex)
        {
            Debug.Error("添加产品失败", ex);
            return (false, $"添加失败: {ex.Message}");
        }
    }

    public async Task<(bool Success, string Message)> UpdateProductAsync(ProductModel product)
    {
        try
        {
            // 验证数据
            var (isValid, errors) = await ValidateProductAsync(product, isUpdate: true);
            if (!isValid)
            {
                return (false, string.Join("; ", errors));
            }

            // 检查产品是否存在
            var existing = await _productRepository.GetByIdAsync(product.Id);
            if (existing == null)
            {
                return (false, "产品不存在");
            }

            if (!await _productRepository.UpdateAsync(product))
            {
                return (false, "产品不存在");
            }

            Debug.Info($"更新产品成功: {product.Name} ({product.Code})");
            return (true, "更新成功");
        }
        catch (Exception ex)
        {
            Debug.Error("更新产品失败", ex);
            return (false, $"更新失败: {ex.Message}");
        }
    }

    public async Task<(bool Success, string Message)> DeleteProductAsync(long id)
    {
        try
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
            {
                return (false, "产品不存在");
            }

            if (!await _productRepository.DeleteAsync(id))
            {
                return (false, "产品不存在");
            }

            Debug.Info($"删除产品成功: {product.Name} ({product.Code})");
            return (true, "删除成功");
        }
        catch (Exception ex)
        {
            Debug.Error($"删除产品失败，ID: {id}", ex);
            return (false, $"删除失败: {ex.Message}");
        }
    }

    public async Task<(bool Success, string Message, int DeletedCount)> BatchDeleteProductsAsync(IEnumerable<long> ids)
    {
        try
        {
            var idList = ids.ToList();
            if (!idList.Any())
            {
                return (false, "未选择要删除的产品", 0);
            }

            var deletedCount = await _productRepository.DeleteByIdsAsync(idList);

            Debug.Info($"批量删除产品成功，删除数量: {deletedCount}");
            return (true, $"成功删除 {deletedCount} 个产品", deletedCount);
        }
        catch (Exception ex)
        {
            Debug.Error("批量删除产品失败", ex);
            return (false, $"删除失败: {ex.Message}", 0);
        }
    }

    public async Task<(bool Success, string Message, int UpdatedCount)> BatchUpdateStatusAsync(IEnumerable<long> ids, int status)
    {
        try
        {
            var idList = ids.ToList();
            if (!idList.Any())
            {
                return (false, "未选择要更新的产品", 0);
            }

            var updatedCount = await _productRepository.BatchUpdateStatusAsync(idList, status);

            Debug.Info($"批量更新产品状态成功，更新数量: {updatedCount}，状态: {status}");
            return (true, $"成功更新 {updatedCount} 个产品的状态", updatedCount);
        }
        catch (Exception ex)
        {
            Debug.Error("批量更新产品状态失败", ex);
            return (false, $"更新失败: {ex.Message}", 0);
        }
    }

    public async Task<(bool Success, string Message, int UpdatedCount)> BatchUpdateCategoryAsync(IEnumerable<long> ids, long categoryId)
    {
        try
        {
            var idList = ids.ToList();
            if (!idList.Any())
            {
                return (false, "未选择要更新的产品", 0);
            }

            // 验证分类是否存在
            var category = await _categoryRepository.GetByIdAsync(categoryId);
            if (category == null)
            {
                return (false, "分类不存在", 0);
            }

            var updatedCount = await _productRepository.BatchUpdateCategoryAsync(idList, categoryId);

            Debug.Info($"批量更新产品分类成功，更新数量: {updatedCount}，分类: {category.Name}");
            return (true, $"成功更新 {updatedCount} 个产品的分类", updatedCount);
        }
        catch (Exception ex)
        {
            Debug.Error("批量更新产品分类失败", ex);
            return (false, $"更新失败: {ex.Message}", 0);
        }
    }

    public async Task<(bool IsValid, List<string> Errors)> ValidateProductAsync(ProductModel product, bool isUpdate = false)
    {
        var errors = new List<string>();

        // 验证产品名称
        if (string.IsNullOrWhiteSpace(product.Name))
        {
            errors.Add("产品名称不能为空");
        }
        else if (product.Name.Length > 128)
        {
            errors.Add("产品名称长度不能超过128个字符");
        }

        // 验证产品编码
        if (string.IsNullOrWhiteSpace(product.Code))
        {
            errors.Add("产品编码不能为空");
        }
        else if (product.Code.Length > 64)
        {
            errors.Add("产品编码长度不能超过64个字符");
        }
        else
        {
            // 检查编码是否重复
            var codeExists = await _productRepository.IsCodeExistsAsync(
                product.Code,
                isUpdate ? product.Id : null);

            if (codeExists)
            {
                errors.Add($"产品编码 '{product.Code}' 已存在");
            }
        }

        // 验证价格
        if (product.Price.HasValue && product.Price.Value < 0)
        {
            errors.Add("价格不能为负数");
        }

        // 验证库存
        if (product.Stock.HasValue && product.Stock.Value < 0)
        {
            errors.Add("库存不能为负数");
        }

        // 验证分类（如果指定了分类）
        if (product.CategoryId.HasValue)
        {
            var category = await _categoryRepository.GetByIdAsync(product.CategoryId.Value);
            if (category == null)
            {
                errors.Add("指定的分类不存在");
            }
        }

        // 验证状态值
        if (product.Status.HasValue && (product.Status.Value < 0 || product.Status.Value > 2))
        {
            errors.Add("状态值无效（0-正常, 1-停用, 2-缺货）");
        }

        return (errors.Count == 0, errors);
    }
}
