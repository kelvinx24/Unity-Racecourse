using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// The current status of a racer in a race
public class RacerStatus
{
    public SplinePath currentPath;

    public Vector3 position;
    public Vector3 tangent;
    public Vector3 normal;

    public Quaternion heading;
    public float progress;

    public RacerStatus(SplinePath path, Vector3 position, Vector3 tangent, Vector3 normal, Quaternion heading)
    {
        this.currentPath = path;
        this.position = position;
        this.tangent = tangent;
        this.normal = normal;
        this.heading = heading;

        this.progress = 0;


    }
}

