﻿using Microsoft.EntityFrameworkCore;
using WinformTemplate.Business.Sys.Model;
using WinformTemplate.Serialize;

namespace WinformTemplate.Business.Sys.Context;

/// <summary>
/// 系统参数模型 数据库上下文
/// </summary>
/// <param name="options"></param>
public class SysParamContext(DbContextOptions<SysParamContext> options) : DbContext(options)
{
    /// <summary>
    /// 系统参数模型
    /// </summary>
    public DbSet<SysParamModel> SysParams { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (optionsBuilder.IsConfigured)
        {
            return;
        }

        var connectionString = GlobalProjectConfig.Instance.Config?.DB;
        optionsBuilder.UseMySql(connectionString,
            new MySqlServerVersion(new Version(8, 0, 21))
        );
    }

    /// <summary>
    /// 数据库模型配置
    /// </summary>
    /// <param name="modelBuilder"></param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
    }
}

/// <summary>
/// 系统参数模型 数据库上下文工厂
/// </summary>
public class SysParamContextFactory : IDbContextFactory<SysParamContext>
{
    public SysParamContext CreateDbContext()
    {
        var connectionString = GlobalProjectConfig.Instance.Config?.DB;
        var options = new DbContextOptionsBuilder<SysParamContext>()
            .UseMySql(connectionString!, new MySqlServerVersion(new Version(8, 0, 21)))
            .Options;

        return new SysParamContext(options);
    }
}