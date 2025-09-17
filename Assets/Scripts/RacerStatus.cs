using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct RacerStatus
{
    public float distanceCovered;
    public Vector3 position;
    public Vector3 heading;

    public RacerStatus(float distanceCovered, Vector3 position, Vector3 heading)
    {
        this.distanceCovered = distanceCovered;
        this.position = position;
        this.heading = heading;
    }
}

