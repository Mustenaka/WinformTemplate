using System.ComponentModel;
using WinformTemplate.Common.MVVM.Command.Interface;

namespace WinformTemplate.Common.MVVM.Extensions;

/// <summary>
/// 为WinForm控件提供数据绑定功能的扩展方法
/// </summary>
public static class DefaultBindingExtensions
{
    /// <summary>
    /// 绑定文本框到ViewModel的属性
    /// </summary>
    /// <typeparam name="T">ViewModel类型</typeparam>
    /// <param name="textBox">文本框</param>
    /// <param name="viewModel">ViewModel实例</param>
    /// <param name="propertySelector">属性选择器</param>
    /// <param name="updateMode">更新模式</param>
    public static void BindTo<T>(this TextBox textBox, T viewModel, Func<T, string> propertySelector,
        DataSourceUpdateMode updateMode = DataSourceUpdateMode.OnPropertyChanged) where T : INotifyPropertyChanged
    {
        var property = propertySelector.Target.GetType().GetProperty(propertySelector.Method.Name.Substring(4));
        if (property == null)
            throw new ArgumentException("Invalid property selector");

        var binding = new Binding("Text", viewModel, property.Name, true, updateMode);
        textBox.DataBindings.Add(binding);
    }

    /// <summary>
    /// 绑定复选框到ViewModel的属性
    /// </summary>
    /// <typeparam name="T">ViewModel类型</typeparam>
    /// <param name="checkBox">复选框</param>
    /// <param name="viewModel">ViewModel实例</param>
    /// <param name="propertySelector">属性选择器</param>
    /// <param name="updateMode">更新模式</param>
    public static void BindTo<T>(this CheckBox checkBox, T viewModel, Func<T, bool> propertySelector,
        DataSourceUpdateMode updateMode = DataSourceUpdateMode.OnPropertyChanged) where T : INotifyPropertyChanged
    {
        var property = propertySelector.Target.GetType().GetProperty(propertySelector.Method.Name.Substring(4));
        if (property == null)
            throw new ArgumentException("Invalid property selector");

        var binding = new Binding("Checked", viewModel, property.Name, true, updateMode);
        checkBox.DataBindings.Add(binding);
    }

    /// <summary>
    /// 绑定按钮到ViewModel的命令
    /// </summary>
    /// <param name="button">按钮</param>
    /// <param name="command">命令</param>
    public static void BindCommand(this Button button, IRelayCommand command)
    {
        // 移除之前的事件处理程序
        var prevHandler = button.Tag as EventHandler;
        if (prevHandler != null)
            button.Click -= prevHandler;

        // 创建新的事件处理程序
        EventHandler handler = (sender, e) =>
        {
            if (command.CanExecute())
                command.Execute();
        };

        // 存储并添加新的事件处理程序
        button.Tag = handler;
        button.Click += handler;
    }

    /// <summary>
    /// 绑定数据网格视图到ViewModel的集合属性
    /// </summary>
    /// <typeparam name="T">ViewModel类型</typeparam>
    /// <typeparam name="TItem">集合项类型</typeparam>
    /// <param name="dataGridView">数据网格视图</param>
    /// <param name="viewModel">ViewModel实例</param>
    /// <param name="collectionSelector">集合属性选择器</param>
    public static void BindTo<T, TItem>(this DataGridView dataGridView, T viewModel,
        Func<T, IEnumerable<TItem>> collectionSelector) where T : INotifyPropertyChanged
    {
        var property = collectionSelector.Target.GetType().GetProperty(collectionSelector.Method.Name.Substring(4));
        if (property == null)
            throw new ArgumentException("Invalid property selector");

        var binding = new Binding("DataSource", viewModel, property.Name, true);
        dataGridView.DataBindings.Add(binding);
    }
}