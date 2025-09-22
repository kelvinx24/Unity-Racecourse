using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// The current status of a racer in a race
public struct RacerStatus
{
    public float distanceCovered;
    public Vector3 position;
    public Vector3 tangent;
    public Vector3 normal;

    public Quaternion heading;

    public RacerStatus(float distanceCovered, Vector3 position, Vector3 tangent, Vector3 normal, Quaternion heading)
    {
        this.distanceCovered = distanceCovered;
        this.position = position;
        this.tangent = tangent;
        this.normal = normal;
        this.heading = heading;
    }
}

