using UnityEngine;

public class PlayerController : MonoBehaviour
{
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

    void Awake()
    {
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

    public void Heal(float amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Min(currentHealth, maxHealth);
    }

    ///Permanently increases max stamina by a fraction (e.g. 0.10 = +10%)
    public void AddStaminaBonus(float fraction)
    {
        float bonus = maxStamina * fraction;
        maxStamina += bonus;
        currentStamina = Mathf.Min(currentStamina + bonus, maxStamina);
    }
    private Item equippedWeapon = null;
    public Item EquippedWeapon => equippedWeapon;

    public void EquipWeapon(Item weapon)
    {
        equippedWeapon = weapon;
        Debug.Log($"[Player] Equipped: {weapon?.GetItemName() ?? "nothing"}");
    }

    private void Die()
    {
        Debug.Log("Player has died!");
    }
}