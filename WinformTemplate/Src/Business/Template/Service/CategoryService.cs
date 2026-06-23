using Microsoft.EntityFrameworkCore;
using WinformTemplate.Business.Template.Model;
using WinformTemplate.Business.Template.Repositories.Interface;
using WinformTemplate.Business.Template.Service.Interface;
using WinformTemplate.Logger;

namespace WinformTemplate.Business.Template.Service;

/// <summary>
/// 分类服务实现
/// </summary>
public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IProductRepository _productRepository;

    public CategoryService(
        ICategoryRepository categoryRepository,
        IProductRepository productRepository)
    {
        _categoryRepository = categoryRepository ?? throw new ArgumentNullException(nameof(categoryRepository));
        _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
    }

    public async Task<List<CategoryModel>> GetAllCategoriesAsync()
    {
        try
        {
            return await _categoryRepository.GetAll().ToListAsync();
        }
        catch (Exception ex)
        {
            Debug.Error("获取所有分类失败", ex);
            throw;
        }
    }

    public async Task<List<CategoryModel>> GetActiveCategoriesAsync()
    {
        try
        {
            return await _categoryRepository.GetActiveCategoriesAsync();
        }
        catch (Exception ex)
        {
            Debug.Error("获取启用的分类失败", ex);
            throw;
        }
    }

    public async Task<List<CategoryModel>> GetCategoryTreeAsync()
    {
        try
        {
            return await _categoryRepository.GetCategoryTreeAsync();
        }
        catch (Exception ex)
        {
            Debug.Error("获取分类树失败", ex);
            throw;
        }
    }

    public async Task<CategoryModel?> GetCategoryByIdAsync(long id)
    {
        try
        {
            return await _categoryRepository.GetByIdAsync(id);
        }
        catch (Exception ex)
        {
            Debug.Error($"获取分类失败，ID: {id}", ex);
            throw;
        }
    }

    public async Task<(bool Success, string Message)> AddCategoryAsync(CategoryModel category)
    {
        try
        {
            // 验证分类名称
            if (string.IsNullOrWhiteSpace(category.Name))
            {
                return (false, "分类名称不能为空");
            }

            // 检查名称是否重复
            var nameExists = await _categoryRepository.IsNameExistsAsync(category.Name);
            if (nameExists)
            {
                return (false, $"分类名称 '{category.Name}' 已存在");
            }

            // 设置默认值
            category.Status ??= 0;
            category.Level ??= 0;
            category.SortOrder ??= 0;

            await _categoryRepository.AddAsync(category);
            await _categoryRepository.SaveChangesAsync();

            Debug.Info($"添加分类成功: {category.Name}");
            return (true, "添加成功");
        }
        catch (Exception ex)
        {
            Debug.Error("添加分类失败", ex);
            return (false, $"添加失败: {ex.Message}");
        }
    }

    public async Task<(bool Success, string Message)> UpdateCategoryAsync(CategoryModel category)
    {
        try
        {
            // 验证分类名称
            if (string.IsNullOrWhiteSpace(category.Name))
            {
                return (false, "分类名称不能为空");
            }

            // 检查分类是否存在
            var existing = await _categoryRepository.GetByIdAsync(category.Id);
            if (existing == null)
            {
                return (false, "分类不存在");
            }

            // 检查名称是否重复
            var nameExists = await _categoryRepository.IsNameExistsAsync(category.Name, category.Id);
            if (nameExists)
            {
                return (false, $"分类名称 '{category.Name}' 已存在");
            }

            _categoryRepository.Update(category);
            await _categoryRepository.SaveChangesAsync();

            Debug.Info($"更新分类成功: {category.Name}");
            return (true, "更新成功");
        }
        catch (Exception ex)
        {
            Debug.Error("更新分类失败", ex);
            return (false, $"更新失败: {ex.Message}");
        }
    }

    public async Task<(bool Success, string Message)> DeleteCategoryAsync(long id)
    {
        try
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null)
            {
                return (false, "分类不存在");
            }

            // 检查该分类下是否有产品
            var productCount = await _productRepository.GetCountByCategoryAsync(id);
            if (productCount > 0)
            {
                return (false, $"该分类下还有 {productCount} 个产品，无法删除");
            }

            _categoryRepository.Delete(category);
            await _categoryRepository.SaveChangesAsync();

            Debug.Info($"删除分类成功: {category.Name}");
            return (true, "删除成功");
        }
        catch (Exception ex)
        {
            Debug.Error($"删除分类失败，ID: {id}", ex);
            return (false, $"删除失败: {ex.Message}");
        }
    }
}
