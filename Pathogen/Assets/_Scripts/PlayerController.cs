using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // Single-player convenience reference — set in Awake
    public static PlayerController LocalInstance { get; private set; }

    [Header("Movement Settings")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float sprintSpeed = 8f;
    [SerializeField] private float crouchSpeed = 2.5f;
    [SerializeField] private float acceleration = 10f;
    [Header("Health Settings")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float currentHealth;
    [Header("Stamina Settings")]
    [SerializeField] private float maxStamina = 100f;
    [SerializeField] private float currentStamina;
    [SerializeField] private float staminaDrainRate = 20f;
    [SerializeField] private float staminaRegenRate = 15f;
    [SerializeField] private float staminaRegenDelay = 1f;
    [SerializeField] private float exhaustedThreshold = 50f;
    [Header("Exhaustion")]
    [SerializeField] private bool isExhausted = false;
    [SerializeField] private float exhaustedTimer = 0f;
    [Header("Input")]
    [SerializeField] private Vector2 moveInput;
    [SerializeField] private bool isSprinting;
    [SerializeField] private bool isCrouching;

    private Rigidbody2D rb;
    private Vector2 currentVelocity;
    private float regenDelayTimer = 0f;
    public float CurrentStamina { get { return currentStamina; } }
    public float MaxStamina { get { return maxStamina; } }
    public bool IsExhausted { get { return isExhausted; } }
    public float CurrentHealth { get { return currentHealth; } }
    public float MaxHealth { get { return maxHealth; } }
    public bool IsSprinting => isSprinting;
    public bool IsCrouching => isCrouching;
    public bool IsMoving => moveInput.magnitude > 0.1f;

    void Awake()
    {
        LocalInstance = this;
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogError("Rigidbody2D is missing on the player", this);
        }
        currentStamina = maxStamina;
        currentHealth = maxHealth;
    }

    void Update()
    {
        ReadInput();
        HandleStamina();
    }

    void FixedUpdate()
    {
        Move();
    }

    private void ReadInput()
    {
        if (!movementEnabled) { moveInput = Vector2.zero; isSprinting = false; return; }
        moveInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        if (moveInput.magnitude > 1f)
        {
            moveInput = moveInput.normalized;
        }
        bool wantsToSprint = Input.GetKey(KeyCode.LeftShift);
        if (wantsToSprint && !isExhausted && currentStamina > 0 && !isCrouching)
        {
            isSprinting = true;
        }
        else
        {
            isSprinting = false;
        }
        isCrouching = Input.GetKey(KeyCode.LeftControl);
    }

    private void HandleStamina()
    {
        if (isSprinting && moveInput.magnitude > 0.1f)
        {
            currentStamina -= staminaDrainRate * Time.deltaTime;
            currentStamina = Mathf.Max(0, currentStamina);
            regenDelayTimer = 0f;
            if (currentStamina <= 0)
            {
                isExhausted = true;
                isSprinting = false;
            }
        }
        else
        {
            regenDelayTimer += Time.deltaTime;
            if (regenDelayTimer >= staminaRegenDelay)
            {
                currentStamina += staminaRegenRate * Time.deltaTime;
                currentStamina = Mathf.Min(currentStamina, maxStamina);

                if (isExhausted)
                {
                    float exhaustedThresholdValue = maxStamina * (exhaustedThreshold / 100f);

                    if (currentStamina >= exhaustedThresholdValue)
                    {
                        isExhausted = false;
                    }
                }
            }
        }
    }

    private void Move()
    {
        float targetSpeed = walkSpeed;
        if (isSprinting)
        {
            targetSpeed = sprintSpeed;
        }
        else if (isCrouching)
        {
            targetSpeed = crouchSpeed;
        }
        // Aiming caps movement to crouch speed (cannot sprint while aiming)
        if (AimSystem.Instance != null && AimSystem.Instance.IsAiming)
            targetSpeed = Mathf.Min(targetSpeed, crouchSpeed);
        Vector2 targetVelocity = moveInput * targetSpeed;
        rb.linearVelocity = Vector2.SmoothDamp(rb.linearVelocity, targetVelocity, ref currentVelocity, acceleration * Time.fixedDeltaTime);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawRay(transform.position, rb.linearVelocity);
    }

    public void DrainStamina(float amount)
    {
        currentStamina -= amount;
        currentStamina = Mathf.Max(0, currentStamina);
        if (currentStamina <= 0)
        {
            isExhausted = true;
        }
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Max(0, currentHealth);
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    ///Directly set health values — used by save/load
    public void SetHealth(float current, float max)
    {
        maxHealth = max;
        currentHealth = Mathf.Clamp(current, 0f, maxHealth);
    }

    ///Directly set stamina values — used by save/load
    public void SetStamina(float current, float max)
    {
        maxStamina = max;
        currentStamina = Mathf.Clamp(current, 0f, maxStamina);
    }

    public void Heal(float amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Min(currentHealth, maxHealth);
        if (Mathf.Approximately(currentHealth, maxHealth))
            InfectionManager.Instance?.OnPlayerFullyHealed();
    }

    ///Permanently increases max stamina by a fraction (e.g. 0.10 = +10%)
    public void AddStaminaBonus(float fraction)
    {
        float bonus = maxStamina * fraction;
        maxStamina += bonus;
        currentStamina = Mathf.Min(currentStamina + bonus, maxStamina);
    }
    /// Reduces maxHealth and maxStamina by a fraction (called by InfectionManager)
    /// Also clamps current values so they don't exceed the new reduced max
    public void ApplyInfectionPenalty(float hpFraction, float stamFraction)
    {
        maxHealth -= maxHealth * hpFraction;
        maxStamina -= maxStamina * stamFraction;
        currentHealth = Mathf.Min(currentHealth, maxHealth);
        currentStamina = Mathf.Min(currentStamina, maxStamina);
    }
    /// Restores the stat reduction previously applied by ApplyInfectionPenalty
    /// Uses the same fractions to reverse exactly what was taken
    public void RemoveInfectionPenalty(float hpFraction, float stamFraction)
    {
        // Reverse: if maxHealth was multiplied by (1 - f), divide by (1 - f) to restore
        float hpDivisor = 1f - hpFraction;
        float stamDivisor = 1f - stamFraction;
        if (hpDivisor > 0f) maxHealth /= hpDivisor;
        if (stamDivisor > 0f) maxStamina /= stamDivisor;
    }
    private Item equippedWeapon = null;
    public Item EquippedWeapon => equippedWeapon;

    public void EquipWeapon(Item weapon)
    {
        equippedWeapon = weapon;
        // Apply any accumulated talisman bonuses to the newly equipped weapon
        if (weapon != null)
        {
            WeaponItem wi = weapon.GetComponent<WeaponItem>();
            if (wi != null) TalismanManager.Instance?.ApplyBonusesToWeapon(wi);
        }
        Debug.Log($"[Player] Equipped: {weapon?.GetItemName() ?? "nothing"}");
    }

    private bool movementEnabled = true;

    ///Enables or disables all player movement — used by DialogueManager
    public void SetMovementEnabled(bool enabled)
    {
        movementEnabled = enabled;
        if (!enabled && rb != null) rb.linearVelocity = Vector2.zero;
    }

    private void Die()
    {
        Debug.Log("Player has died!");
    }
}