using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class SplinePath
{
    public List<Vector3> controlPoints = new List<Vector3>();

    private List<SegmentSample> controlPointSamples = new List<SegmentSample>();

    private List<SegmentSample> samplesTable = new List<SegmentSample>();

    private float cumulativeLength;

    public SplinePath(List<Vector3> controlPoints, List<SegmentSample> controlPointSamples, List<SegmentSample> samplesTable, float cumulativeLength)
    {
        this.controlPoints = controlPoints;
        this.controlPointSamples = controlPointSamples;
        this.samplesTable = samplesTable;
        this.cumulativeLength = cumulativeLength;
    }

    public List<SegmentSample> GetControlPointSamples()
    {
        return controlPointSamples;
    }

    public List<SegmentSample> GetSamplesTable() { return samplesTable; }
}
