using UnityEngine;

/// Central audio manager. Handles SFX and music
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Movement")]
    [SerializeField] private AudioClip walkClip;
    [SerializeField] private AudioClip crouchClip;
    [SerializeField] private AudioClip sprintClip;
    [SerializeField] private float walkVolume = 0.5f;
    [SerializeField] private float crouchVolume = 0.25f;
    [SerializeField] private float sprintVolume = 0.7f;
    [Header("Pistol")]
    [SerializeField] private AudioClip pistolFireClip;
    [SerializeField] private AudioClip pistolReloadClip;
    [SerializeField] private float pistolFireVolume = 0.8f;
    [SerializeField] private float pistolReloadVolume = 0.6f;
    [Header("Crowbar")]
    [SerializeField] private AudioClip crowbarSwingClip;
    [SerializeField] private float crowbarSwingVolume = 0.7f;
    [Header("Inventory")]
    [SerializeField] private AudioClip inventoryOpenClip;
    [SerializeField] private AudioClip inventoryCloseClip;
    [SerializeField] private AudioClip inventoryTabClip;
    [SerializeField] private float inventoryVolume = 0.4f;
    [Header("Items")]
    [SerializeField] private AudioClip itemEquipClip;    // equipping a weapon
    [SerializeField] private AudioClip itemUseClip;      // using/consuming an item
    [SerializeField] private AudioClip itemPickupClip;   // picking up world item
    [SerializeField] private float itemVolume = 0.5f;
    [Header("Audio Sources")]
    [SerializeField] private AudioSource movementSource;
    [SerializeField] private AudioSource nonStackSource;   // for non-stackable one-shots
    [SerializeField] private AudioSource[] sfxPool;          // stackable pool

    private enum MovementState { None, Walk, Crouch, Sprint }
    private MovementState currentMovement = MovementState.None;

    void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); return; }
    }

    public void PlayWalk() => SetMovement(MovementState.Walk);
    public void PlayCrouch() => SetMovement(MovementState.Crouch);
    public void PlaySprint() => SetMovement(MovementState.Sprint);
    public void StopMovement() => SetMovement(MovementState.None);

    private void SetMovement(MovementState state)
    {
        if (currentMovement == state) return;
        currentMovement = state;
        if (movementSource == null) return;

        if (state == MovementState.None) { movementSource.Stop(); return; }

        AudioClip clip = state == MovementState.Walk ? walkClip :
                           state == MovementState.Crouch ? crouchClip : sprintClip;
        float volume = state == MovementState.Walk ? walkVolume :
                           state == MovementState.Crouch ? crouchVolume : sprintVolume;

        if (clip == null) { movementSource.Stop(); return; }

        movementSource.clip = clip;
        movementSource.volume = volume;
        movementSource.loop = true;
        movementSource.Play();
    }

    public void PlayPistolFire() => PlayStackable(pistolFireClip, pistolFireVolume);
    public void PlayPistolReload() => PlayNonStack(pistolReloadClip, pistolReloadVolume);
    public void PlayCrowbarSwing() => PlayStackable(crowbarSwingClip, crowbarSwingVolume);
    public void PlayInventoryOpen() => PlayNonStack(inventoryOpenClip, inventoryVolume);
    public void PlayInventoryClose() => PlayNonStack(inventoryCloseClip, inventoryVolume);
    public void PlayInventoryTab() => PlayNonStack(inventoryTabClip, inventoryVolume);
    public void PlayItemEquip() => PlayNonStack(itemEquipClip, itemVolume);
    public void PlayItemUse() => PlayNonStack(itemUseClip, itemVolume);
    public void PlayItemPickup() => PlayStackable(itemPickupClip, itemVolume);

    [Header("Enemy - Infected")]
    [SerializeField] private AudioClip infectedPassiveClip;
    [SerializeField] private AudioClip infectedSpotClip;
    [SerializeField] private AudioClip infectedWindupClip;
    [SerializeField] private AudioClip infectedAttackClip;
    [SerializeField] private AudioClip infectedDeathClip;
    [SerializeField] private float infectedVolume = 0.6f;
    [Header("Enemy - Stalker")]
    [SerializeField] private AudioClip stalkerPassiveClip;
    [SerializeField] private AudioClip stalkerSpotClip;
    [SerializeField] private AudioClip stalkerWindupClip;
    [SerializeField] private AudioClip stalkerAttackClip;
    [SerializeField] private AudioClip stalkerDeathClip;
    [SerializeField] private float stalkerVolume = 0.6f;
    [Header("Enemy - Leaper")]
    [SerializeField] private AudioClip leaperPassiveClip;
    [SerializeField] private AudioClip leaperSpotClip;
    [SerializeField] private AudioClip leaperWindupClip;
    [SerializeField] private AudioClip leaperAttackClip;
    [SerializeField] private AudioClip leaperDeathClip;
    [SerializeField] private float leaperVolume = 0.6f;
    [Header("Enemy - Brute")]
    [SerializeField] private AudioClip brutePassiveClip;
    [SerializeField] private AudioClip bruteSpotClip;
    [SerializeField] private AudioClip bruteWindupClip;
    [SerializeField] private AudioClip bruteAttackClip;
    [SerializeField] private AudioClip bruteDeathClip;
    [SerializeField] private float bruteVolume = 0.7f;

    public void PlayInfectedPassive() => PlayStackable(infectedPassiveClip, infectedVolume);
    public void PlayInfectedSpot() => PlayStackable(infectedSpotClip, infectedVolume);
    public void PlayInfectedWindup() => PlayStackable(infectedWindupClip, infectedVolume);
    public void PlayInfectedAttack() => PlayStackable(infectedAttackClip, infectedVolume);
    public void PlayInfectedDeath() => PlayStackable(infectedDeathClip, infectedVolume);
    public void PlayStalkerPassive() => PlayStackable(stalkerPassiveClip, stalkerVolume);
    public void PlayStalkerSpot() => PlayStackable(stalkerSpotClip, stalkerVolume);
    public void PlayStalkerWindup() => PlayStackable(stalkerWindupClip, stalkerVolume);
    public void PlayStalkerAttack() => PlayStackable(stalkerAttackClip, stalkerVolume);
    public void PlayStalkerDeath() => PlayStackable(stalkerDeathClip, stalkerVolume);
    public void PlayLeaperPassive() => PlayStackable(leaperPassiveClip, leaperVolume);
    public void PlayLeaperSpot() => PlayStackable(leaperSpotClip, leaperVolume);
    public void PlayLeaperWindup() => PlayStackable(leaperWindupClip, leaperVolume);
    public void PlayLeaperAttack() => PlayStackable(leaperAttackClip, leaperVolume);
    public void PlayLeaperDeath() => PlayStackable(leaperDeathClip, leaperVolume);
    public void PlayBrutePassive() => PlayStackable(brutePassiveClip, bruteVolume);
    public void PlayBruteSpot() => PlayStackable(bruteSpotClip, bruteVolume);
    public void PlayBruteWindup() => PlayStackable(bruteWindupClip, bruteVolume);
    public void PlayBruteAttack() => PlayStackable(bruteAttackClip, bruteVolume);
    public void PlayBruteDeath() => PlayStackable(bruteDeathClip, bruteVolume);

    public void PlaySFX(AudioClip clip, float volume = 1f, bool stackable = true)
    {
        if (clip == null) return;
        if (stackable) PlayStackable(clip, volume);
        else PlayNonStack(clip, volume);
    }

    private float musicVolumeMultiplier = 1f;
    private float sfxVolumeMultiplier = 1f;

    public void SetMusicVolume(float value)
    {
        musicVolumeMultiplier = Mathf.Clamp01(value);
        // Apply to music source if you add one later
        // For now stores the value for use when music is implemented
    }

    public void SetSFXVolume(float value)
    {
        sfxVolumeMultiplier = Mathf.Clamp01(value);
        // Apply to all sfx sources
        if (nonStackSource != null) nonStackSource.volume = sfxVolumeMultiplier;
        foreach (var src in sfxPool)
            if (src != null) src.volume = sfxVolumeMultiplier;
        if (movementSource != null) movementSource.volume = sfxVolumeMultiplier;
    }

    /// Apply saved volume settings from PlayerPrefs on startup
    public void ApplySavedVolumes()
    {
        SetMusicVolume(PlayerPrefs.GetFloat("Settings_MusicVolume", 1f));
        SetSFXVolume(PlayerPrefs.GetFloat("Settings_SFXVolume", 1f));
    }

    private void PlayStackable(AudioClip clip, float volume)
    {
        if (clip == null) return;
        foreach (var src in sfxPool)
        {
            if (src != null && !src.isPlaying)
            {
                src.PlayOneShot(clip, volume);
                return;
            }
        }
        // Pool exhausted — use first slot
        if (sfxPool.Length > 0 && sfxPool[0] != null)
            sfxPool[0].PlayOneShot(clip, volume);
    }

    private void PlayNonStack(AudioClip clip, float volume)
    {
        if (clip == null || nonStackSource == null) return;
        nonStackSource.Stop();
        nonStackSource.clip = clip;
        nonStackSource.volume = volume;
        nonStackSource.loop = false;
        nonStackSource.Play();
    }
}