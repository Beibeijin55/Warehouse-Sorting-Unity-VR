using UnityEngine;

// Detects whether a box has been placed on a conveyor sorting zone.
// Correct zone + highlighted target box => success.
// Wrong zone + highlighted target box => error.
// Any non-target physical box => error.
public class ConveyorSortingZone : MonoBehaviour
{
    public VRNotificationManager notificationManager;

    [Header("Sorting Rule")]
    public bool isCorrectDeliveryZone = true;

    [TextArea]
    public string wrongZoneMessage = "Wrong conveyor! Please place the highlighted box in the green drop zone.";

    [TextArea]
    public string wrongBoxMessage = "Incorrect box! Please select the highlighted box.";

    private void OnCollisionEnter(Collision collision)
    {
        Evaluate(collision.collider);
    }

    private void OnTriggerEnter(Collider other)
    {
        Evaluate(other);
    }

    private void Evaluate(Collider itemCollider)
    {
        TargetBoxHighlight targetBox = itemCollider.GetComponentInParent<TargetBoxHighlight>();
        if (targetBox != null)
        {
            if (isCorrectDeliveryZone)
            {
                targetBox.TryCompleteSorting();
            }
            else
            {
                ShowError(wrongZoneMessage);
            }
            return;
        }

        // Only physical objects reaching the belt are treated as sorting attempts.
        if (itemCollider.attachedRigidbody == null) return;

        ShowError(wrongBoxMessage);
    }

    private void ShowError(string message)
    {
        if (notificationManager == null)
        {
            notificationManager = FindObjectOfType<VRNotificationManager>();
        }

        if (notificationManager != null)
        {
            notificationManager.ShowNotification(message, VRNotificationManager.NotificationType.Error);
        }
    }
}
