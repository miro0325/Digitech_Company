using System;
using System.Collections;
using UnityEngine;

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

    public static void Invoke(this MonoBehaviour mb, Action action, float t)
    {
        mb.StartCoroutine(Routine(action, t));
        static IEnumerator Routine(Action action, float t)
        {
            yield return new WaitForSeconds(t);
            action?.Invoke();    
        }
    }

}