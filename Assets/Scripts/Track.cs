using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class Track : MonoBehaviour
{
    public List<Transform> controlPoints = new List<Transform>();

    public float trackSize;

    public GameObject splinePrefab;

    private List<Vector3> controlPointsPositions = new List<Vector3>();

    [Range(1, 200)]
    public int samplesPerSegment;

    private SplinePath innerSpline;

    private SplinePath outerSpline;

    private List<GameObject> innerSplineObjects = new List<GameObject>();

    private List<GameObject> outerSplineObjects = new List<GameObject>();


    private void Awake()
    {
        for (int i = 0; i < controlPoints.Count; i++)
        {
            Transform t = controlPoints[i];
            controlPointsPositions.Add(t.position);
        }

        CreateSplines();

        CreateSplineSampleObjects();
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    private void CreateSplines()
    {
        innerSpline = SplineCreator.CreateSplinePath(controlPointsPositions, samplesPerSegment);


        List<Vector3> outerControlPoints = new List<Vector3>();
        List<SegmentSample> controlPointSamples = innerSpline.GetControlPointSamples();

        for (int i = 0; i < controlPointSamples.Count;i++)
        {
            Vector3 pointNormal = controlPointSamples[i].Normal;
            Vector3 offsetPosition = controlPointSamples[i].Position + pointNormal * trackSize;
            outerControlPoints.Add(offsetPosition);
        }

        outerSpline = SplineCreator.CreateSplinePath(outerControlPoints, samplesPerSegment);

    }

    private void CreateSplineSampleObjects()
    {
        List<SegmentSample> innerSamples = innerSpline.GetSamplesTable();
        GameObject parent = new GameObject("Track Objects");

        foreach (SegmentSample sample in innerSamples)
        {
            GameObject gameObject = Instantiate(splinePrefab, sample.Position, Quaternion.identity);
            innerSplineObjects.Add(gameObject);
            gameObject.transform.SetParent(parent.transform, true);
        }

        List<SegmentSample> outerSamples = outerSpline.GetSamplesTable();
        foreach (SegmentSample sample in outerSamples)
        {
            GameObject gameObject = Instantiate(splinePrefab, sample.Position, Quaternion.identity);
            innerSplineObjects.Add(gameObject);
            gameObject.transform.SetParent(parent.transform, true);

        }
    }

    public SplinePath GetInnerSpline() { return innerSpline; }

    public SplinePath GetOuterSpline() { return outerSpline; }
}
