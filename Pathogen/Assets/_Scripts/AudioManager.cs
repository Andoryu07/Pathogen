using UnityEngine;
/// Central audio manager. Handles SFX and music
/// Stackable = multiple instances overlap (e.g. UI clicks)
/// Non-stackable = stops previous instance before playing (e.g. footsteps, reload)
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
    [SerializeField] private AudioClip itemEquipClip;    
    [SerializeField] private AudioClip itemUseClip;     
    [SerializeField] private AudioClip itemPickupClip;   
    [SerializeField] private float itemVolume = 0.5f;
    [Header("Audio Sources")]
    [SerializeField] private AudioSource movementSource;
    [SerializeField] private AudioSource nonStackSource;  
    [SerializeField] private AudioSource[] sfxPool;        
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
    public void PlaySFX(AudioClip clip, float volume = 1f, bool stackable = true)
    {
        if (clip == null) return;
        if (stackable) PlayStackable(clip, volume);
        else PlayNonStack(clip, volume);
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