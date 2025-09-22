using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaneRaceDemo : MonoBehaviour
{
    public float innerRadius = 36f;
    public float outerRadius = 38f;
    public float speed = 8f; // meters/second
    public Transform innerRunner;
    public Transform outerRunner;

    private float thetaIn;   // radians
    private float thetaOut;  // radians

    void Update()
    {
        float dt = Time.deltaTime;

        // Angular velocities
        float omegaIn = speed / innerRadius;
        float omegaOut = speed / outerRadius;

        // Advance angles
        thetaIn += omegaIn * dt;
        thetaOut += omegaOut * dt;

        // Position runners on circular tracks (XZ plane)
        innerRunner.position = new Vector3(
            innerRadius * Mathf.Cos(thetaIn),
            0f,
            innerRadius * Mathf.Sin(thetaIn)
        );

        outerRunner.position = new Vector3(
            outerRadius * Mathf.Cos(thetaOut),
            0f,
            outerRadius * Mathf.Sin(thetaOut)
        );

        // Debug info
        float lapIn = (thetaIn * innerRadius) / (2f * Mathf.PI * innerRadius);
        float lapOut = (thetaOut * outerRadius) / (2f * Mathf.PI * outerRadius);
        Debug.Log($"Inner progress: {lapIn:F2} laps | Outer progress: {lapOut:F2} laps");
    }
}
