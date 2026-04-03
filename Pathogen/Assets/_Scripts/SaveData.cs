using System;
using System.Collections.Generic;
/// Fully serializable snapshot of all game state
/// Serialized to JSON by SaveManager using JsonUtility
[Serializable]
public class SaveData
{
    // Meta
    public string saveName = "";
    public int difficulty = 1;   // Difficulty enum as int (0=Casual,1=Normal,2=Hardcore)
    public string sceneName = "";
    public string timestamp = "";
    public float totalPlaytime = 0f;
    public int saveCount = 0;
    // Player
    public float playerHP = 100f;
    public float playerMaxHP = 100f;
    public float playerStamina = 100f;
    public float playerMaxStamina = 100f;
    public float playerPosX = 0f;
    public float playerPosY = 0f;
    // Infection
    public int infectionStage = 0;
    public int infectionHits = 0;
    // Inventory
    public List<SavedItem> inventoryItems = new List<SavedItem>();
    // Storebox
    public List<SavedItem> storeboxItems = new List<SavedItem>();
    // Special Items
    public bool hasLighter = true;
    public bool hasHazardMask = false;
    public int hipPouchCount = 0;
    public int gridWidth = 7;
    // Wallet
    public int patheosBalance = 0;
    // Weapon Upgrades
    public List<SavedUpgrade> upgradeLevels = new List<SavedUpgrade>();
    // Quests
    public List<SavedQuest> questStates = new List<SavedQuest>();
    // Crafting
    public List<string> unlockedRecipeNames = new List<string>();
    // Documents
    public List<string> collectedDocumentNames = new List<string>();
    // Tutorials
    public List<SavedTutorial> tutorials = new List<SavedTutorial>();
    // Collectibles
    public int talismanCount = 0;
    // World state
    public List<string> deadEnemyIDs = new List<string>();
    public List<string> collectedPickupIDs = new List<string>();
    public List<string> unlockedLockIDs = new List<string>();
    public List<SavedSearchSpot> searchSpotStates = new List<SavedSearchSpot>();
    public bool volkovDefeated = false;

}

[Serializable]
public class SavedItem
{
    public string itemName = "";
    public int gridX = 0;
    public int gridY = 0;
    public bool rotated = false;
    public int stackCount = 1;
}

[Serializable]
public class SavedUpgrade
{
    public string key = "";
    public int level = 0;
}

[Serializable]
public class SavedQuest
{
    public string questName = "";
    public int state = 0;
    public int progress = 0;
}

[Serializable]
public class SavedTutorial
{
    public string title = "";
    public string body = "";
}

[Serializable]
public class SavedSearchSpot
{
    public string spotID = "";
    public bool isRevealed = false;
    public List<string> remainingItems = new List<string>();
}