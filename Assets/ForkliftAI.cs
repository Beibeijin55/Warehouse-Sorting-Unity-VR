using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ForkliftAI : MonoBehaviour
{
    [Header("Waypoints")]
    public Transform waypointA;
    public Transform waypointB;

    [Header("Settings")]
    public float moveSpeed = 1f;
    public float rotationSpeed = 80f;  
    public float arrivalThreshold = 1f;
    public float waitTimeAtWaypoint = 3f;

    [Header("Delivery Route")]
    [Tooltip("Drive to the nearest black conveyor belt at the start of the scene.")]
    public bool driveToConveyor = true;
    public string conveyorTag = "ConveyerBelt";

    private Transform currentTarget;
    private bool isWaiting = false;
    private bool parkedAtConveyor = false;
    private bool drivingToConveyor = false;
    private float originalX;
    private float originalZ;

    private enum State
    {
        RotateToTarget,
        MoveToTarget,
        WaitAtTarget
    }

    private State currentState;

    void Start()
    {
        originalX = transform.eulerAngles.x;
        originalZ = transform.eulerAngles.z;

        currentTarget = driveToConveyor ? CreateNearestConveyorTarget() : null;
        drivingToConveyor = currentTarget != null;
        if (currentTarget == null)
        {
            currentTarget = waypointA;
        }
        currentState = State.RotateToTarget;
    }

    private Transform CreateNearestConveyorTarget()
    {
        GameObject[] conveyorObjects;
        try
        {
            conveyorObjects = GameObject.FindGameObjectsWithTag(conveyorTag);
        }
        catch (UnityException)
        {
            Debug.LogWarning("ForkliftAI: Conveyor tag is missing, using waypoints instead.");
            return null;
        }

        Collider nearestCollider = null;
        Vector3 nearestPoint = Vector3.zero;
        float nearestDistance = float.MaxValue;

        foreach (GameObject conveyorObject in conveyorObjects)
        {
            Collider conveyorCollider = conveyorObject.GetComponent<Collider>();
            if (conveyorCollider == null) continue;

            Vector3 point = conveyorCollider.ClosestPoint(transform.position);
            float distance = (point - transform.position).sqrMagnitude;
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestPoint = point;
                nearestCollider = conveyorCollider;
            }
        }

        if (nearestCollider == null) return null;

        // Use a temporary target at the closest point on the black conveyor.
        GameObject target = new GameObject("Forklift Conveyor Delivery Target");
        nearestPoint.y = transform.position.y;
        target.transform.position = nearestPoint;
        return target.transform;
    }

    void Update()
    {
        if (isWaiting) return;

        switch (currentState)
        {
            case State.RotateToTarget:
                RotateTowardsTarget();
                break;
            case State.MoveToTarget:
                MoveTowardsTarget();
                break;
               
        }
    }

    /*Responsible for rotating the forklift so it can start moving towards its waypoint.
     * 
     */
    void RotateTowardsTarget()
    {
        Vector3 directionToTarget = (currentTarget.position - transform.position);
        directionToTarget.y = 0;
        directionToTarget.Normalize();

        Quaternion targetRotation = Quaternion.LookRotation(directionToTarget, Vector3.up);

        float currentY = transform.eulerAngles.y;
        float targetY = targetRotation.eulerAngles.y;

        float newY = Mathf.MoveTowardsAngle(currentY, targetY, rotationSpeed * Time.deltaTime);
        transform.eulerAngles = new Vector3(originalX, newY, originalZ);

        if (Mathf.Abs(Mathf.DeltaAngle(newY, targetY)) < 1f)
        {
            currentState = State.MoveToTarget;
        }
    }

    /*Moves the forklift towards the next waypoint.
     * 
     */
    void MoveTowardsTarget()
    {
        // Calculate the direction to the current target
        Vector3 directionToTarget = (currentTarget.position - transform.position);
        directionToTarget.y = 0; // Ensure movement remains on the horizontal plane
        directionToTarget.Normalize();

        // Move toward the target dynamically
        transform.position += directionToTarget * moveSpeed * Time.deltaTime;

        // Check if close enough to the target
        float distance = Vector3.Distance(transform.position, currentTarget.position);
        if (distance <= arrivalThreshold)
        {
            if (drivingToConveyor && !parkedAtConveyor)
            {
                parkedAtConveyor = true;
                isWaiting = true;
                currentState = State.WaitAtTarget;
                Debug.Log("ForkliftAI: Arrived at the conveyor belt.");
                return;
            }

            // Arrived at a normal waypoint
            StartCoroutine(WaitAtWaypoint());
        }
    }



    /*Waits a little bit at each waypoint to simulate "working"
     */
    IEnumerator WaitAtWaypoint()
    {
        currentState = State.WaitAtTarget;
        isWaiting = true;

        // Wait for the specified time
        yield return new WaitForSeconds(waitTimeAtWaypoint);

        // Switch target
        currentTarget = (currentTarget == waypointA) ? waypointB : waypointA;

        // After waiting, go back to rotation phase
        currentState = State.RotateToTarget;
        isWaiting = false;
    }
}
