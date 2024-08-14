namespace WinformTemplate.Tools.Util;

public static class ToolExtend
{
    /// <summary>
    /// Fisher-Yates 洗牌算法，用于将一个List完全打乱顺序，时间复杂度为O(n)
    /// </summary>
    /// <typeparam name="T">任意List类型</typeparam>
    /// <param name="list">List</param>
    public static void Shuffle<T>(List<T> list)
    {
        Random rnd = new Random();
        int n = list.Count;

        for (int i = n - 1; i > 0; i--)
        {
            int j = rnd.Next(i + 1);

            // 交换位置
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

    /// <summary>
    ///  Fisher-Yates 洗牌算法，用于将2个List完全打乱顺序，时间复杂度为O(n)
    /// </summary>
    /// <typeparam name="T1">任意List类型</typeparam>
    /// <typeparam name="T2">任意List类型</typeparam>
    /// <param name="list1">List</param>
    /// <param name="list2">List</param>
    public static void Shuffle<T1, T2>(List<T1> list1, List<T2> list2)
    {
        Random rnd = new Random();
        int n = list1.Count;

        for (int i = n - 1; i > 0; i--)
        {
            int j = rnd.Next(i + 1);

            // 交换位置
            (list1[i], list1[j]) = (list1[j], list1[i]);
            (list2[i], list2[j]) = (list2[j], list2[i]);
        }
    }

    /// <summary>
    /// Fisher-Yates 洗牌算法，用于将3个List完全打乱顺序，时间复杂度为O(n)
    /// </summary>
    /// <typeparam name="T1">任意List类型</typeparam>
    /// <typeparam name="T2">任意List类型</typeparam>
    /// <typeparam name="T3">任意List类型</typeparam>
    /// <param name="list1">List</param>
    /// <param name="list2">List</param>
    /// <param name="list3">List</param>
    public static void Shuffle<T1, T2, T3>(List<T1> list1, List<T2> list2, List<T3> list3)
    {
        Random rnd = new Random();
        int n = list1.Count;

        for (int i = n - 1; i > 0; i--)
        {
            int j = rnd.Next(i + 1);

            // 交换位置
            (list1[i], list1[j]) = (list1[j], list1[i]);
            (list2[i], list2[j]) = (list2[j], list2[i]);
            (list3[i], list3[j]) = (list3[j], list3[i]);
        }
    }

    /// <summary>
    /// Fisher-Yates 洗牌算法，用于将3个List完全打乱顺序，时间复杂度为O(n)
    /// </summary>
    /// <typeparam name="T1">任意List类型</typeparam>
    /// <typeparam name="T2">任意List类型</typeparam>
    /// <typeparam name="T3">任意List类型</typeparam>
    /// <typeparam name="T4">任意List类型</typeparam>
    /// <param name="list1">List</param>
    /// <param name="list2">List</param>
    /// <param name="list3">List</param>
    /// <param name="list4">List</param>
    public static void Shuffle<T1, T2, T3, T4>(List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4)
    {
        Random rnd = new Random();
        int n = list1.Count;

        for (int i = n - 1; i > 0; i--)
        {
            int j = rnd.Next(i + 1);

            // 交换位置
            (list1[i], list1[j]) = (list1[j], list1[i]);
            (list2[i], list2[j]) = (list2[j], list2[i]);
            (list3[i], list3[j]) = (list3[j], list3[i]);
            (list4[i], list4[j]) = (list4[j], list4[i]);
        }
    }
}