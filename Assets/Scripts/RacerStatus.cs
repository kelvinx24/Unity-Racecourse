using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// The current status of a racer in a race
public struct RacerStatus
{
    public float distanceCovered;
    public Vector3 position;
    public Quaternion heading;

    public RacerStatus(float distanceCovered, Vector3 position, Quaternion heading)
    {
        this.distanceCovered = distanceCovered;
        this.position = position;
        this.heading = heading;
    }
}

