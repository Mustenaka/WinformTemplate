using WinformTemplate.Common.Patterns;

namespace WinformTemplate.Business.Sys.Context.Factory;

/// <summary>
/// 所有工厂类上下文
/// </summary>
public class AllFactoryContext : SingletonBase<AllFactoryContext>
{
    /// <summary>
    /// 系统账户类数据库上下文工厂
    /// </summary>
    public SysAccountContextFactory? SysAccount { get; set; }

    /// <summary>
    /// 初始化构造
    /// </summary>
    public AllFactoryContext()
    {
        SysAccount = new SysAccountContextFactory();
    }
}