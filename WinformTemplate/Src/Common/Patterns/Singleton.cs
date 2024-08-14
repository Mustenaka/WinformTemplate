namespace WinformTemplate.Common.Patterns;

/// <summary>
/// 单例模式基类
/// </summary>
/// <typeparam name="T">要实现单例模式的类类型</typeparam>
public abstract class SingletonBase<T> where T : SingletonBase<T>, new()
{
    // 用于存储单例实例
    private static readonly Lazy<T> instance = new Lazy<T>(() => new T());

    /// <summary>
    /// 获取单例实例
    /// </summary>
    public static T Instance => instance.Value;

    // 保护构造函数，防止外部实例化
    protected SingletonBase()
    {
        if (instance.IsValueCreated)
        {
            throw new InvalidOperationException("不能创建单例类的另一个实例。");
        }
    }

    /// <summary>
    /// 初始化单例实例的方法，可以由派生类重写
    /// </summary>
    protected virtual void Initialize()
    {
        // 可由派生类实现
    }
}