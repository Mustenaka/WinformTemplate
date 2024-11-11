namespace WinformTemplate.Common.Patterns;

/// <summary>
/// 工厂模式接口
/// </summary>
/// <typeparam name="T"></typeparam>
public interface IFactory<T>
{
    T CreateInstance(params object[] args);
}

/// <summary>
/// 抽象工厂基类
/// </summary>
/// <typeparam name="T"></typeparam>
public abstract class AbstractFactory<T> : IFactory<T>
{
    // 字典用于存储不同类型的创建方法
    private static readonly Dictionary<System.Type, Func<object[], T>> creators = new();

    // 注册创建方法，以便在子类中自定义创建逻辑
    protected static void RegisterType<U>(Func<object[], U> creator) where U : T
    {
        creators[typeof(U)] = args => creator(args);
    }

    // 默认的创建方法，如果没有指定类型的创建方法，将抛出异常
    public virtual T CreateInstance(params object[] args)
    {
        if (creators.TryGetValue(typeof(T), out var creator))
        {
            return creator(args);
        }
        throw new InvalidOperationException($"No registered creator for type {typeof(T)}");
    }
}