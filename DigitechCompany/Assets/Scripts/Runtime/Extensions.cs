using System;

public static class GameExtensions
{
    /// <summary>
    /// Do for command
    /// </summary>
    /// <param name="action">Action to invoke on each element(index, element)</param>
    public static void For<T>(this T[] array, Action<int, T> action)
    {
        for(int i = 0; i < array.Length; i++)
            action?.Invoke(i, array[i]);
    }
}