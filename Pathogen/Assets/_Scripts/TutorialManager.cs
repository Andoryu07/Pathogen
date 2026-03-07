using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class TutorialEntry
{
    public string title;
    public string body;

    public TutorialEntry(string t, string b) { title = t; body = b; }
}

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance { get; private set; }

    private List<TutorialEntry> tutorials = new List<TutorialEntry>();
    private HashSet<string> seen = new HashSet<string>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SeedDefaultTutorials();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// Adds a tutorial only once (ignores duplicates by title).
    /// Returns true if it was newly added.
    public bool AddTutorial(string title, string body)
    {
        if (seen.Contains(title)) return false;
        seen.Add(title);
        tutorials.Add(new TutorialEntry(title, body));
        return true;
    }

    public List<TutorialEntry> GetAllTutorials() => tutorials;

    // Pre-seed tutorials that are available from the start
    private void SeedDefaultTutorials()
    {
        AddTutorial("Movement",
            "Use WASD to move.\n\nHold Left Shift to sprint — this drains your stamina.\n\nHold Left Ctrl to crouch and move silently.");

        AddTutorial("Interacting",
            "Press E to interact with objects and items near you.\n\nA prompt will appear when something is within range.");

        AddTutorial("Inventory",
            "Press Tab to open and close your inventory.\n\nItems take up space based on their size. Drag items to rearrange them, or hold an item for 1 second to start moving it.\n\nPress R while dragging to rotate an item.");

        AddTutorial("Health & Stamina",
            "Your health does not regenerate on its own. Use healing items from your inventory.\n\nStamina regenerates automatically when you stop sprinting. If it hits zero, Elias will be briefly exhausted.");
    }
}