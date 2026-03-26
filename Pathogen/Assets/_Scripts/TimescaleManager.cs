using UnityEngine;
using System.Collections.Generic;
/// Manages Time.timeScale for multiple systems that may want to freeze time
public static class TimeScaleManager
{
    private static HashSet<object> freezers = new HashSet<object>();

    public static bool IsFrozen => freezers.Count > 0;

    public static void Freeze(object requester)
    {
        freezers.Add(requester);
        Apply();
    }

    public static void Unfreeze(object requester)
    {
        freezers.Remove(requester);
        Apply();
    }

    public static void UnfreezeAll()
    {
        freezers.Clear();
        Apply();
    }

    private static void Apply()
    {
        Time.timeScale = freezers.Count > 0 ? 0f : 1f;
        Debug.Log("[TimeScale] " + (freezers.Count > 0
            ? "Frozen by " + freezers.Count + " system(s)"
            : "Resumed"));
    }
}