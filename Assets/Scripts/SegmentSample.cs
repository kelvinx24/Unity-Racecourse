using UnityEngine;

[System.Serializable]
public class SegmentSample
{
    public float Cumulative;
    public int OverallIndex;
    public int SegmentIndex;
    public float SegmentT;
    public Vector3 Position;
    public Vector3 Tangent;
    public Vector3 Normal;
    public float Curvature;

    public SegmentSample(
        float cumulative,
        int overallIndex,
        int segmentIndex,
        float segmentT,
        Vector3 position,
        Vector3 tangent,
        Vector3 normal,
        float curvature
    )
    {
        Cumulative = cumulative;
        OverallIndex = overallIndex;
        SegmentIndex = segmentIndex;
        SegmentT = segmentT;
        Position = position;
        Tangent = tangent;
        Normal = normal;
        Curvature = curvature;
    }
}
