using UnityEngine;
using System.Collections.Generic;

public class PlayerInteractor : MonoBehaviour
{
    [Header("Interaction Settings")]
    [SerializeField] private float interactionRadius = 1.2f;
    [SerializeField] private LayerMask interactableLayer;
    [Header("UI Prompt")]
    [SerializeField] private GameObject promptPanel;
    [SerializeField] private TMPro.TextMeshProUGUI promptText;
    
    private List<IInteractable> nearbyInteractables = new List<IInteractable>();
    private IInteractable closestInteractable;

    void Awake()
    {
        CircleCollider2D trigger = GetComponent<CircleCollider2D>();
        if (trigger == null)
        {
            trigger = gameObject.AddComponent<CircleCollider2D>();
            trigger.isTrigger = true;
            trigger.radius = interactionRadius;
        }
    }

    void Update()
    {
        FindClosestInteractable();
        UpdateUIPrompt();
        if (Input.GetKeyDown(KeyCode.E) && closestInteractable != null)
        {
            closestInteractable.Interact();
        }
    }

    private void FindClosestInteractable()
    {
        float closestDistance = float.MaxValue;
        closestInteractable = null;

        foreach (var interactable in nearbyInteractables)
        {
            if (interactable.Equals(null))
            {
                continue;
            }
            MonoBehaviour mb = interactable as MonoBehaviour;
            if (mb == null) continue;
            float distance = Vector2.Distance(transform.position, mb.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestInteractable = interactable;
            }
        }
    }

    private void UpdateUIPrompt()
    {
        if (promptPanel == null || promptText == null) return;
        if (closestInteractable != null)
        {
            promptPanel.SetActive(true);
            promptText.text = closestInteractable.GetInteractionPrompt();
        }
        else
        {
            promptPanel.SetActive(false);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        IInteractable interactable = other.GetComponent<IInteractable>();
        if (interactable != null && !nearbyInteractables.Contains(interactable))
        {
            nearbyInteractables.Add(interactable);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        IInteractable interactable = other.GetComponent<IInteractable>();
        if (interactable != null && nearbyInteractables.Contains(interactable))
        {
            nearbyInteractables.Remove(interactable);
        }
    }
}