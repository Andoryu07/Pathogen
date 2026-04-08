using UnityEngine;

public class EndGameTrigger : MonoBehaviour
{
    [SerializeField] private GameEndPanel gameEndPanel;
    [SerializeField] private bool triggerOnEnable = true;

    void OnEnable()
    {
        if (triggerOnEnable)
        {
            StartTriggerSequence();
        }
    }

    public void StartTriggerSequence()
    {
        // Small delay so the unlock animation/sound can play first
        Invoke(nameof(TriggerEnd), 0.5f);
    }

    private void TriggerEnd()
    {
        if (gameEndPanel != null)
            gameEndPanel.Show();
        else if (GameEndPanel.Instance != null)
            GameEndPanel.Instance.Show();
        else
            Debug.LogWarning("[EndGameTrigger] No GameEndPanel found!");
    }
}