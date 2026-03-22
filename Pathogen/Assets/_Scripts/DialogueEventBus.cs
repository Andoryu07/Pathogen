using System;
using System.Collections.Generic;

/// Simple static event bus for dialogue custom actions
/// Subscribe to named events and fire them from dialogue lines

public static class DialogueEventBus
{
    private static Dictionary<string, Action> events = new Dictionary<string, Action>();

    public static void Subscribe(string eventName, Action callback)
    {
        if (!events.ContainsKey(eventName)) events[eventName] = null;
        events[eventName] += callback;
    }

    public static void Unsubscribe(string eventName, Action callback)
    {
        if (events.ContainsKey(eventName)) events[eventName] -= callback;
    }

    public static void Trigger(string eventName)
    {
        if (events.TryGetValue(eventName, out Action action))
            action?.Invoke();
        else
            UnityEngine.Debug.LogWarning("[DialogueEventBus] No listeners for: " + eventName);
    }
}