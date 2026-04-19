using UnityEngine;
using System.Collections.Generic;

/// Handles throwable item aiming and throwing
public class ThrowSystem : MonoBehaviour
{
    public static ThrowSystem Instance { get; private set; }

    [Header("References")]
    [SerializeField] private LineRenderer arcLine;
    [SerializeField] private SpriteRenderer splashPreview;
    [SerializeField] private Camera mainCamera;
    [Header("Arc Settings")]
    [SerializeField] private int arcSegments = 30;
    [SerializeField] private float arcHeight = 1.5f;
    [Header("Colors")]
    [SerializeField] private Color colorClear = new Color(0.2f, 0.9f, 0.2f, 0.8f);
    [SerializeField] private Color colorBlocked = new Color(0.9f, 0.2f, 0.2f, 0.8f);
    [SerializeField] private Color splashColor = new Color(1f, 0.5f, 0f, 0.25f);
    [Header("Wall Detection")]
    [SerializeField] private LayerMask wallLayer;

    private bool isAiming = false;
    private ThrowableItem currentThrowable;
    private Item currentItem;
    private Vector2 landingPoint;
    private bool pathClear;
    public bool IsAiming => isAiming;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(this); return; }
    }

    void Start()
    {
        if (mainCamera == null) mainCamera = Camera.main;
        SetArcVisible(false);
    }

    void Update()
    {
        if (IsAnyUIOpen())
        {
            if (isAiming) ExitAim();
            return;
        }
        // Check if player has a throwable equipped
        Item equipped = PlayerController.LocalInstance?.EquippedWeapon;
        ThrowableItem throwable = equipped?.GetComponent<ThrowableItem>();

        if (throwable == null)
        {
            if (isAiming) ExitAim();
            return;
        }
        currentThrowable = throwable;
        currentItem = equipped;
        // Right-click to enter/exit aim
        if (Input.GetMouseButtonDown(1)) EnterAim();
        if (Input.GetMouseButtonUp(1)) ExitAim();

        if (isAiming)
        {
            UpdateAim();

            if (Input.GetMouseButtonDown(0) && pathClear)
                Throw();
        }
    }

    private void EnterAim()
    {
        isAiming = true;
        SetArcVisible(true);
        Cursor.visible = false;
    }

    private void ExitAim()
    {
        isAiming = false;
        SetArcVisible(false);
        Cursor.visible = true;
    }

    private void UpdateAim()
    {
        if (mainCamera == null || currentThrowable == null) return;

        Vector2 origin = transform.position;
        Vector2 mouseWorld = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        Vector2 dir = mouseWorld - origin;
        float dist = dir.magnitude;

        // Clamp to max range
        if (dist > currentThrowable.throwRange)
        {
            mouseWorld = origin + dir.normalized * currentThrowable.throwRange;
            dist = currentThrowable.throwRange;
        }

        // Wall detection — raycast along straight line
        RaycastHit2D hit = Physics2D.Raycast(origin, dir.normalized, dist, wallLayer);
        if (hit.collider != null)
        {
            landingPoint = hit.point;
            pathClear = false;
        }
        else
        {
            landingPoint = mouseWorld;
            pathClear = true;
        }

        DrawArc(origin, landingPoint, pathClear);
        UpdateSplashPreview(landingPoint);
    }

    private void DrawArc(Vector2 from, Vector2 to, bool clear)
    {
        if (arcLine == null) return;

        arcLine.positionCount = arcSegments + 1;
        arcLine.startColor = clear ? colorClear : colorBlocked;
        arcLine.endColor = clear ? colorClear : colorBlocked;

        for (int i = 0; i <= arcSegments; i++)
        {
            float t = (float)i / arcSegments;
            Vector2 linear = Vector2.Lerp(from, to, t);
            float arc = arcHeight * Mathf.Sin(t * Mathf.PI);
            arcLine.SetPosition(i, new Vector3(linear.x, linear.y + arc, 0f));
        }
    }

    private void UpdateSplashPreview(Vector2 pos)
    {
        if (splashPreview == null || currentThrowable == null) return;
        float diameter = currentThrowable.splashRadius * 2f;
        splashPreview.transform.position = new Vector3(pos.x, pos.y, 0f);
        splashPreview.transform.localScale = new Vector3(diameter, diameter, 1f);
        splashPreview.color = splashColor;
    }

    private void SetArcVisible(bool visible)
    {
        if (arcLine != null) arcLine.enabled = visible;
        if (splashPreview != null) splashPreview.gameObject.SetActive(visible);
    }

    private void Throw()
    {
        if (currentItem == null || currentThrowable == null) return;
        GameObject go = Instantiate(currentThrowable.projectilePrefab, transform.position, Quaternion.identity);
        ThrowableProjectile proj = go.GetComponent<ThrowableProjectile>();
        if (proj != null) proj.Launch(currentThrowable, transform.position, landingPoint);
        string itemName = currentItem.GetItemName();
        InventoryGrid.Instance.RemoveItem(currentItem);
        InventoryUIManager.Instance?.RefreshInventoryGrid();
        Item nextInstance = InventoryGrid.Instance.GetFirstItemByName(itemName);
        if (nextInstance != null)
        {
            currentItem = nextInstance;
            currentThrowable = nextInstance.GetComponent<ThrowableItem>();
            PlayerController.LocalInstance?.EquipWeapon(nextInstance);
            WeaponHUD.Instance?.Refresh(nextInstance);
        }
        else
        {
            currentItem = null;
            currentThrowable = null;
            PlayerController.LocalInstance?.EquipWeapon(null);
            WeaponHUD.Instance?.Refresh(null);
        }

        ExitAim();
    }

    private static bool IsAnyUIOpen()
    {
        if (InventoryUIManager.Instance != null && InventoryUIManager.Instance.IsOpen) return true;
        if (StoreboxUIManager.Instance != null && StoreboxUIManager.Instance.IsOpen) return true;
        if (CodePadUI.Instance != null && CodePadUI.Instance.IsOpen) return true;
        if (DialogueManager.Instance != null && DialogueManager.Instance.IsOpen) return true;
        return false;
    }
}