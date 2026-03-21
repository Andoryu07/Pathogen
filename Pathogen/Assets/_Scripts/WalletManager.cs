using UnityEngine;
/// Singleton that tracks the player's Patheos currency total
public class WalletManager : MonoBehaviour
{
    public static WalletManager Instance { get; private set; }
    private int balance = 0;
    public int Balance => balance;

    void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else Destroy(gameObject);
    }

    ///Add Patheos to the wallet (from pickup or enemy drop)
    public void Add(int amount)
    {
        if (amount <= 0) return;
        balance += amount;
        Debug.Log($"[Wallet] +{amount} Patheos — total: {balance}");
        HUDFeedback.Instance?.ShowInfo($"+{amount} Patheos");
        OnBalanceChanged();
    }

    ///Spend Patheos (at Silas merchant). Returns false if insufficient funds
    public bool Spend(int amount)
    {
        if (amount > balance)
        {
            HUDFeedback.Instance?.ShowWarning($"Not enough Patheos (need {amount}, have {balance}).");
            return false;
        }
        balance -= amount;
        Debug.Log($"[Wallet] -{amount} Patheos — total: {balance}");
        OnBalanceChanged();
        return true;
    }

    ///Restore balance from save data
    public void LoadBalance(int amount)
    {
        balance = Mathf.Max(0, amount);
        OnBalanceChanged();
    }

    private void OnBalanceChanged()
    {
        // Notify inventory UI to refresh the balance display
        InventoryUIManager.Instance?.RefreshWalletDisplay();
    }
}