using System.ComponentModel;
using System.Reflection;
using System.Windows.Input;
using AntdUI;

namespace WinformTemplate.Common.MVVM.Extensions;

/// <summary>
/// 为 AntdUI 控件提供数据绑定功能的扩展方法
/// </summary>
public static class AntdUIBindingExtensions
{
    public static void BindText<TViewModel>(this Input input, TViewModel viewModel, string propertyName)
        where TViewModel : INotifyPropertyChanged
    {
        ArgumentNullException.ThrowIfNull(input);
        var property = GetWritableProperty(typeof(TViewModel), propertyName);

        input.Text = Convert.ToString(property.GetValue(viewModel)) ?? string.Empty;
        input.TextChanged += (_, _) => SetIfChanged(viewModel, property, input.Text);
        viewModel.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == propertyName)
            {
                SetControlText(input, Convert.ToString(property.GetValue(viewModel)) ?? string.Empty);
            }
        };
    }

    public static void BindChecked<TViewModel>(this Switch control, TViewModel viewModel, string propertyName)
        where TViewModel : INotifyPropertyChanged
    {
        ArgumentNullException.ThrowIfNull(control);
        var property = GetWritableProperty(typeof(TViewModel), propertyName);

        control.Checked = property.GetValue(viewModel) is true;
        control.CheckedChanged += (_, _) => SetIfChanged(viewModel, property, control.Checked);
        viewModel.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == propertyName && property.GetValue(viewModel) is bool value)
            {
                SetControlChecked(control, value);
            }
        };
    }

    public static void BindCommand(this AntdUI.Button button, ICommand command, object? parameter = null)
    {
        ArgumentNullException.ThrowIfNull(button);
        ArgumentNullException.ThrowIfNull(command);

        void RefreshEnabled()
        {
            button.Enabled = command.CanExecute(parameter);
        }

        button.Click += (_, _) =>
        {
            if (command.CanExecute(parameter))
            {
                command.Execute(parameter);
            }
        };
        command.CanExecuteChanged += (_, _) => RefreshEnabled();
        RefreshEnabled();
    }

    private static PropertyInfo GetWritableProperty(System.Type viewModelType, string propertyName)
    {
        var property = viewModelType.GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public);
        if (property == null || !property.CanWrite)
        {
            throw new ArgumentException($"Property '{propertyName}' was not found or is read-only.", nameof(propertyName));
        }

        return property;
    }

    private static void SetIfChanged<TViewModel>(TViewModel viewModel, PropertyInfo property, object? value)
    {
        var convertedValue = ConvertValue(value, property.PropertyType);
        if (!Equals(property.GetValue(viewModel), convertedValue))
        {
            property.SetValue(viewModel, convertedValue);
        }
    }

    private static object? ConvertValue(object? value, System.Type targetType)
    {
        var nullableType = Nullable.GetUnderlyingType(targetType);
        var effectiveType = nullableType ?? targetType;
        if (value == null || value is string text && string.IsNullOrWhiteSpace(text))
        {
            return nullableType != null || !effectiveType.IsValueType ? null : Activator.CreateInstance(effectiveType);
        }

        if (effectiveType == typeof(string))
        {
            return Convert.ToString(value) ?? string.Empty;
        }

        if (effectiveType == typeof(bool))
        {
            return value is bool boolValue && boolValue;
        }

        return Convert.ChangeType(value, effectiveType);
    }

    private static void SetControlText(Input input, string value)
    {
        if (input.IsDisposed)
        {
            return;
        }

        void Apply()
        {
            if (input.Text != value)
            {
                input.Text = value;
            }
        }

        if (input.InvokeRequired)
        {
            input.BeginInvoke(Apply);
            return;
        }

        Apply();
    }

    private static void SetControlChecked(Switch control, bool value)
    {
        if (control.IsDisposed)
        {
            return;
        }

        void Apply()
        {
            if (control.Checked != value)
            {
                control.Checked = value;
            }
        }

        if (control.InvokeRequired)
        {
            control.BeginInvoke(Apply);
            return;
        }

        Apply();
    }
}
