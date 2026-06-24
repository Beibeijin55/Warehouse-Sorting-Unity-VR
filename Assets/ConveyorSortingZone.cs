using UnityEngine;

// Added automatically to every black conveyor belt by VRNotificationManager.
// It accepts highlighted target boxes and reports a clear error for another box.
public class ConveyorSortingZone : MonoBehaviour
{
    public VRNotificationManager notificationManager;

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
            targetBox.TryCompleteSorting();
            return;
        }

        // Only physical objects reaching the belt are treated as sorting attempts.
        if (itemCollider.attachedRigidbody == null) return;

        if (notificationManager == null)
        {
            notificationManager = FindObjectOfType<VRNotificationManager>();
        }

        if (notificationManager != null)
        {
            notificationManager.ShowNotification(
                "Incorrect box! Please select the highlighted box.",
                VRNotificationManager.NotificationType.Error);
        }
    }
}
