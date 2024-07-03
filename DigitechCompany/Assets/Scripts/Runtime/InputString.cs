using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public static class InputString
{
    public static string GetString(InputAction action, int index)
    {
        return InputControlPath.ToHumanReadableString
            (
                action.bindings[index].effectivePath,
                InputControlPath.HumanReadableStringOptions.OmitDevice | InputControlPath.HumanReadableStringOptions.UseShortNames
            );
    }

    public static List<string> GetStrings(InputAction action)
    {
        var list = new List<string>();
        foreach(var binding in action.bindings)
        {
            InputControlPath.ToHumanReadableString
            (
                binding.effectivePath,
                InputControlPath.HumanReadableStringOptions.OmitDevice | InputControlPath.HumanReadableStringOptions.UseShortNames
            );
        }
        return list;
    }
}