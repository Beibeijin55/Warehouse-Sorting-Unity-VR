using UnityEngine;
using UnityEngine.UI;
using TMPro; 
using System.Collections;

public class VRNotificationManager : MonoBehaviour
{
    public enum NotificationType { Success, Warning, Error, Info }

    [Header("UI References")]
    public TextMeshProUGUI notificationText;
    public Image backgroundImage;
    public CanvasGroup canvasGroup;

    [Header("VR Spatial Settings")]
    public Transform vrCamera; 
    public float displayTime = 3f;
    public float distanceFromCamera = 1.5f;
    public float heightOffset = 0.1f;

    [Header("UI Color Theme")]
    public Color successColor = new Color(0.2f, 0.6f, 0.2f, 0.9f); 
    public Color warningColor = new Color(0.8f, 0.6f, 0f, 0.9f);   
    public Color errorColor = new Color(0.8f, 0.2f, 0.2f, 0.9f);   
    public Color infoColor = new Color(0.2f, 0.2f, 0.2f, 0.9f);    

    private Coroutine currentCoroutine;

    // Safety check to prevent null reference errors if components are not assigned
    void Awake()
    {
        if (notificationText == null) Debug.LogError("[VRNotification] Text missing! Please assign in Inspector.");
        if (backgroundImage == null) Debug.LogError("[VRNotification] Background missing! Please assign in Inspector.");
        if (canvasGroup == null) Debug.LogError("[VRNotification] CanvasGroup missing! Please assign in Inspector.");
    }

    void Start()
    {
        // Auto-assign main camera if not manually set in Inspector
        if (vrCamera == null && Camera.main != null)
        {
            vrCamera = Camera.main.transform;
        }

        // Initialize UI state: hidden and non-interactable to avoid invisible raycast blockers
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
    }

    public void ShowNotification(string message, NotificationType type)
    {
        if (notificationText != null) notificationText.text = message;

        if (backgroundImage != null)
        {
            switch (type)
            {
                case NotificationType.Success: backgroundImage.color = successColor; break;
                case NotificationType.Warning: backgroundImage.color = warningColor; break;
                case NotificationType.Error: backgroundImage.color = errorColor; break;
                default: backgroundImage.color = infoColor; break;
            }
        }

        if (vrCamera != null)
        {
            Vector3 targetPosition = vrCamera.position + (vrCamera.forward * distanceFromCamera);
            targetPosition.y += heightOffset;
            transform.position = targetPosition;
            
            // WARNING: Do NOT use LookAt()! 
            // Unity Canvas text will render mirrored if facing the camera directly.
            // This formula forces the Canvas to face away, rendering text correctly.
            transform.forward = transform.position - vrCamera.position;
        }

        // Interrupt any ongoing animation and reset alpha before starting a new one
        if (currentCoroutine != null) StopCoroutine(currentCoroutine);
        if (canvasGroup != null) canvasGroup.alpha = 0; 
        
        currentCoroutine = StartCoroutine(NotificationSequence());
    }

    private IEnumerator NotificationSequence()
    {
        // Fade In (Clamp01 prevents alpha from exceeding 1.0)
        while (canvasGroup.alpha < 1)
        {
            canvasGroup.alpha = Mathf.Clamp01(canvasGroup.alpha + Time.deltaTime * 3f);
            yield return null;
        }

        yield return new WaitForSeconds(displayTime);

        // Fade Out (Clamp01 prevents alpha from dropping below 0.0)
        while (canvasGroup.alpha > 0)
        {
            canvasGroup.alpha = Mathf.Clamp01(canvasGroup.alpha - Time.deltaTime * 2f);
            yield return null;
        }
    }

    // --- Editor Test Buttons ---
    [ContextMenu("Test: Success")]
    public void TestSuccess() => ShowNotification("Sorting Successful!", NotificationType.Success);

    [ContextMenu("Test: Warning")]
    public void TestWarning() => ShowNotification("Wrong Shelf!", NotificationType.Warning);

    [ContextMenu("Test: Error")]
    public void TestError() => ShowNotification("Incorrect SKU!", NotificationType.Error);
}