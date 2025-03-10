using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using WinformTemplate.Business.Sys.Context;
using WinformTemplate.Common.MVVM;

namespace WinformTemplate.Common.Repository;


/// <summary>
/// 仓储基类，提供基本的CRUD操作
/// </summary>
/// <typeparam name="TEntity">实体类型</typeparam>
public abstract class BaseRepository<TEntity> where TEntity : BaseModel
{
    protected readonly SysDbContext dbContext;
    protected readonly DbSet<TEntity> dbSet;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="dbContext">数据库上下文</param>
    protected BaseRepository(SysDbContext dbContext)
    {
        this.dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        dbSet = this.dbContext.Set<TEntity>();
    }

    /// <summary>
    /// 获取所有实体
    /// </summary>
    /// <returns>查询结果</returns>
    public virtual IQueryable<TEntity> GetAll()
    {
        return dbSet;
    }

    /// <summary>
    /// 根据条件获取实体
    /// </summary>
    /// <param name="filter">过滤条件</param>
    /// <returns>查询结果</returns>
    public virtual IQueryable<TEntity> GetByCondition(Expression<Func<TEntity, bool>> filter)
    {
        return dbSet.Where(filter);
    }

    /// <summary>
    /// 根据ID获取实体
    /// </summary>
    /// <param name="id">实体ID</param>
    /// <returns>实体</returns>
    public virtual async Task<TEntity?> GetByIdAsync(object id)
    {
        return await dbSet.FindAsync(id);
    }

    /// <summary>
    /// 添加实体
    /// </summary>
    /// <param name="entity">实体</param>
    public virtual async Task AddAsync(TEntity entity)
    {
        await dbSet.AddAsync(entity);
    }

    /// <summary>
    /// 批量添加实体
    /// </summary>
    /// <param name="entities">实体集合</param>
    public virtual async Task AddRangeAsync(IEnumerable<TEntity> entities)
    {
        await dbSet.AddRangeAsync(entities);
    }

    /// <summary>
    /// 更新实体
    /// </summary>
    /// <param name="entity">实体</param>
    public virtual void Update(TEntity entity)
    {
        dbContext.Entry(entity).State = EntityState.Modified;
    }

    /// <summary>
    /// 批量更新实体
    /// </summary>
    /// <param name="entities">实体集合</param>
    public virtual void UpdateRange(IEnumerable<TEntity> entities)
    {
        foreach (var entity in entities)
        {
            dbContext.Entry(entity).State = EntityState.Modified;
        }
    }

    /// <summary>
    /// 删除实体
    /// </summary>
    /// <param name="entity">实体</param>
    public virtual void Delete(TEntity entity)
    {
        dbSet.Remove(entity);
    }

    /// <summary>
    /// 批量删除实体
    /// </summary>
    /// <param name="entities">实体集合</param>
    public virtual void DeleteRange(IEnumerable<TEntity> entities)
    {
        dbSet.RemoveRange(entities);
    }

    /// <summary>
    /// 根据ID删除实体
    /// </summary>
    /// <param name="id">实体ID</param>
    /// <returns>是否删除成功</returns>
    public virtual async Task<bool> DeleteByIdAsync(object id)
    {
        var entity = await GetByIdAsync(id);
        if (entity == null)
            return false;

        Delete(entity);
        return true;
    }

    /// <summary>
    /// 检查实体是否存在
    /// </summary>
    /// <param name="filter">过滤条件</param>
    /// <returns>是否存在</returns>
    public virtual async Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> filter)
    {
        return await dbSet.AnyAsync(filter);
    }

    /// <summary>
    /// 保存更改到数据库
    /// </summary>
    /// <returns>受影响的行数</returns>
    public virtual async Task<int> SaveChangesAsync()
    {
        return await dbContext.SaveChangesAsync();
    }

    /// <summary>
    /// 分页查询
    /// </summary>
    /// <param name="filter">过滤条件</param>
    /// <param name="orderBy">排序字段</param>
    /// <param name="ascending">是否升序</param>
    /// <param name="pageIndex">页码（从1开始）</param>
    /// <param name="pageSize">每页大小</param>
    /// <returns>分页结果</returns>
    public virtual async Task<(List<TEntity> Items, int TotalCount)> GetPagedAsync(
        Expression<Func<TEntity, bool>>? filter = null,
        Expression<Func<TEntity, object>>? orderBy = null,
        bool ascending = true,
        int pageIndex = 1,
        int pageSize = 10)
    {
        var query = dbSet.AsQueryable();

        if (filter != null)
            query = query.Where(filter);

        // 计算总记录数
        var totalCount = await query.CountAsync();

        // 添加排序
        if (orderBy != null)
        {
            query = ascending
                ? query.OrderBy(orderBy)
                : query.OrderByDescending(orderBy);
        }

        // 执行分页查询
        var skip = (pageIndex - 1) * pageSize;
        var items = await query.Skip(skip).Take(pageSize).ToListAsync();

        return (items, totalCount);
    }
}