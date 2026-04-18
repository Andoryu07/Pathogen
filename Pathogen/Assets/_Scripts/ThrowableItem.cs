using UnityEngine;

public enum ThrowableType { PipeBomb, Molotov }

/// Defines all throw and damage properties read by ThrowSystem
public class ThrowableItem : MonoBehaviour
{
    [Header("Type")]
    public ThrowableType throwableType = ThrowableType.PipeBomb;

    [Header("Throw Settings")]
    public float throwSpeed = 6f;    // travel speed
    public float throwRange = 8f;    // max range before landing
    public float fuseDelay = 0.3f;  // seconds after landing before damage triggers

    [Header("Splash")]
    public float splashRadius = 2.5f;

    [Header("Pipe Bomb")]
    public float explosionDamage = 120f;

    [Header("Molotov")]
    public float fireDamagePerSec = 20f;
    public float fireDuration = 5f;

    [Header("Prefabs")]
    [Tooltip("The in-flight projectile prefab.")]
    public GameObject projectilePrefab;
    [Tooltip("The fire zone prefab (Molotov only).")]
    public GameObject fireZonePrefab;
    [Tooltip("The explosion effect prefab (Pipe Bomb only).")]
    public GameObject explosionEffectPrefab;
}