namespace WinformTemplate.Tools.Calc;

/// <summary>
/// 处理公式模板
/// </summary>
public class Formula
{
    private const double Pi = 3.1415926f;

    /// <summary>
    /// 以某一个数字为基准，以maxOffset为范围随机生成一个范围内的随机数
    /// </summary>
    /// <param name="input">基准数字</param>
    /// <param name="maxOffset">最大偏移数字</param>
    /// <param name="decimalPlace">小数点后位数，默认 = 2</param>
    /// <returns>生成结果</returns>
    public static double GenerateValueByOffset(double? input, double maxOffset, int decimalPlace = 2)
    {
        if (input == null)
        {
            return 0.0;
        }

        var random = new Random();
        var offset = (random.NextDouble() * 2 - 1) * maxOffset;
        var number = Math.Round(offset, decimalPlace);

        return Math.Abs(input.Value + number);
    }

    /// <summary>
    /// 以某一个数字为基准，以maxOffset为范围随机生成一个范围内的随机数
    /// </summary>
    /// <param name="input">基准数字</param>
    /// <param name="maxOffset">最大偏移数字</param>
    /// <param name="decimalPlace">小数点后位数，默认 = 2</param>
    /// <returns>生成结果</returns>
    public static double GenerateValueByOffset(double input, double maxOffset, int decimalPlace = 2)
    {
        var random = new Random();
        var offset = (random.NextDouble() * 2 - 1) * maxOffset;
        var number = Math.Round(offset, decimalPlace);

        return Math.Abs(input + number);
    }

    /// <summary>
    /// 以input为基准，生成两个值value1和value2，value1、value2的最大差值为±maxOffset。
    /// 并确保(value1+value2)/2 = input
    /// </summary>
    /// <param name="input">输入值</param>
    /// <param name="maxOffset">偏移值（请输入正数）</param>
    /// <returns>包含两个结果值的元组</returns>
    public static (double value1, double value2) Generate2ValuesByOffset(double input, double maxOffset)
    {
        var random = new Random();
        var adjustOffset = maxOffset / 2;
        var offset = Math.Round(random.NextDouble() * adjustOffset, 2); // 生成一个0~maxOffset的随机数，保留两位小数

        var value1 = Math.Round(input - offset, 2);
        var value2 = Math.Round(input + offset, 2);

        return (value1, value2);
    }

    /// <summary>
    /// 以input为基准，生成两个值value1和value2，value1、value2的最大差值为±maxOffset。
    /// 并确保(multiple+value2)/2 = input
    /// </summary>
    /// <param name="input">输入值</param>
    /// <param name="maxOffset">偏移值（请输入正数）</param>
    /// <param name="places"></param>
    /// <returns>包含两个结果值的元组</returns>
    public static (double? value1, double? value2) Generate2ValuesByOffset(double? input, double maxOffset, int places = 2)
    {
        if (input is null)
        {
            return (null, null);
        }

        var random = new Random();
        var adjustOffset = maxOffset / 2;
        var offset = Math.Round(random.NextDouble() * adjustOffset, places); // 生成一个0~maxOffset的随机数，保留两位小数

        var value1 = Math.Round(input.Value - offset, places);
        var value2 = Math.Round(input.Value + offset, places);

        return (value1, value2);
    }

    /// <summary>
    /// 取小数点后位数
    /// </summary>
    /// <param name="value"></param>
    /// <param name="digits"></param>
    /// <returns></returns>
    public static double? Round(double? value, int digits)
    {
        if (value == null)
        {
            return null;
        }

        return Math.Round(value.Value, digits);
    }


    /// <summary>
    /// 带误差的乘法
    /// </summary>
    /// <param name="value">基数值</param>
    /// <param name="multiple">乘法倍数值</param>
    /// <param name="maxOffset">最大误差</param>
    /// <param name="decimalPlace">小数点后位数，默认 = 2</param>
    /// <returns></returns>
    public static double? ErrorMultiplication(double value, double multiple, double maxOffset, int decimalPlace = 2)
    {
        var random = new Random();
        var offset = Math.Round(random.NextDouble() * maxOffset * 2 - maxOffset, decimalPlace); // 生成一个0~maxOffset的随机数

        return Math.Round(value * multiple + offset, decimalPlace);
    }

    /// <summary>
    /// 带误差的乘法
    /// </summary>
    /// <param name="value">基数值</param>
    /// <param name="multiple">乘法倍数值</param>
    /// <param name="maxOffset">最大误差</param>
    /// <param name="decimalPlace">小数点后位数，默认 = 2</param>
    /// <returns></returns>
    public static double? ErrorMultiplication(double? value, double? multiple, double maxOffset, int decimalPlace = 2)
    {
        if (value is null || multiple is null)
        {
            return null;
        }

        var random = new Random();
        var offset = Math.Round(random.NextDouble() * maxOffset * 2 - maxOffset, decimalPlace); // 生成一个0~maxOffset的随机数

        var result = Math.Round(value.Value * multiple.Value + offset, decimalPlace);
        return result;
    }
}