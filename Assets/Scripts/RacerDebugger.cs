using UnityEngine;

public class RacerDebugger : MonoBehaviour
{
    public Racer racer;                // assign in inspector

    private Vector3 predicted;
    private Vector3 closestPoint;
    private Vector3 closestTan;

    // Call this each update from your race loop
    public void UpdateDebug(Vector3 predictedMove, Vector3 closest, Vector3 tangent)
    {
        predicted = predictedMove;
        closestPoint = closest;
        closestTan = tangent;
    }

    void OnDrawGizmos()
    {
        //Debug.LogWarning(closestPoint == predicted);
        // Draw current position
        Gizmos.color = Color.white;
        Gizmos.DrawSphere(racer.transform.position, 0.1f);

        // Draw predicted move
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(predicted, 0.5f);

        // Draw closest point on spline
        Gizmos.color = Color.cyan;
        Gizmos.DrawSphere(closestPoint, 0.5f);

        // Connect current → predicted → closest
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(racer.transform.position, predicted);

        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(predicted, closestPoint);

        // Draw tangent at closest point
        Gizmos.color = Color.green;
        Gizmos.DrawLine(closestPoint, closestPoint + closestTan * 2f);
    }
}
