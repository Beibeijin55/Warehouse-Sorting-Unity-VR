using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class TargetBoxHighlight : MonoBehaviour
{
    public Material highlightMaterial;
    public float blinkSpeed = 2f;

    private Renderer boxRenderer;
    private Material originalMaterial;
    private XRGrabInteractable grabInteractable;
    private Rigidbody boxRigidbody;
    private VRNotificationManager notificationManager;
    private bool isHeld;
    private bool isDelivered;

    private void Awake()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();
        boxRigidbody = GetComponent<Rigidbody>();

        // The target must stay on its shelf until the player deliberately
        // grabs it; enabling gravity at scene start makes it fall away.
        if (boxRigidbody != null)
        {
            boxRigidbody.useGravity = false;
        }

        notificationManager = FindObjectOfType<VRNotificationManager>();
    }

    private void OnEnable()
    {
        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.AddListener(OnGrabbed);
            grabInteractable.selectExited.AddListener(OnReleased);
        }
    }

    private void OnDisable()
    {
        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.RemoveListener(OnGrabbed);
            grabInteractable.selectExited.RemoveListener(OnReleased);
        }
    }

    void Start()
    {
        boxRenderer = GetComponent<Renderer>();

        if (boxRenderer == null || highlightMaterial == null)
        {
            Debug.LogWarning("TargetBoxHighlight missing Renderer or Highlight Material on " + gameObject.name);
            return;
        }

        originalMaterial = boxRenderer.material;
    }

    void Update()
    {
        if (boxRenderer == null || highlightMaterial == null || isHeld || isDelivered) return;

        float t = Mathf.PingPong(Time.time * blinkSpeed, 1f);
        boxRenderer.material = t > 0.5f ? highlightMaterial : originalMaterial;
    }

    private void OnGrabbed(SelectEnterEventArgs args)
    {
        isHeld = true;
        if (boxRigidbody != null) boxRigidbody.useGravity = false;
        if (boxRenderer != null) boxRenderer.material = originalMaterial;
    }

    private void OnReleased(SelectExitEventArgs args)
    {
        isHeld = false;
        if (boxRigidbody != null) boxRigidbody.useGravity = true;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("ConveyerBelt"))
        {
            CompleteSorting();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("ConveyerBelt"))
        {
            CompleteSorting();
        }
    }

    private void CompleteSorting()
    {
        if (isDelivered) return;

        isDelivered = true;
        isHeld = false;
        if (boxRenderer != null) boxRenderer.material = originalMaterial;

        if (notificationManager == null)
        {
            notificationManager = FindObjectOfType<VRNotificationManager>();
        }

        if (notificationManager != null)
        {
            notificationManager.ShowNotification(
                "Sorting Successful!",
                VRNotificationManager.NotificationType.Success);
        }
    }
}
