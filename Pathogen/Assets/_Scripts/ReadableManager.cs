using UnityEngine;
using System.Collections.Generic;

public class ReadableManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject readablePanel;
    [SerializeField] private Transform readableListParent;

    private List<Item> readables = new List<Item>();
    private HashSet<string> collectedNames = new HashSet<string>();
    public static ReadableManager Instance { get; private set; }

    void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else Destroy(gameObject);
    }

    public bool AddReadable(Item readable)
    {
        if (readable.GetItemType() != ItemType.Readable)
        {
            Debug.LogError("Not a readable");
            return false;
        }
        if (!readables.Contains(readable))
        {
            readables.Add(readable);
            collectedNames.Add(readable.GetItemName());
            Debug.Log($"Document '{readable.GetItemName()}' added to diary");
            return true;
        }
        return false;
    }

    public bool HasReadable(string readableName)
        => readables.Exists(r => r.GetItemName() == readableName);

    public bool HasCollectedDocument(string name)
        => collectedNames.Contains(name);

    public List<Item> GetAllReadables() => readables;

    ///Restore collected document names from save data
    public void LoadDocuments(List<string> names)
    {
        if (names == null) return;
        foreach (var name in names)
            collectedNames.Add(name);
    }
}