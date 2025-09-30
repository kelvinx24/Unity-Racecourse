using System.Collections;
using System.Collections.Generic;
using Unity.Jobs;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

// A race with racers that are running on a track
public class Race : MonoBehaviour
{
    public List<Racer> racerList = new List<Racer>();

    public Track track;

    private Dictionary<Racer, RacerStatus> racersStatus = new Dictionary<Racer, RacerStatus>();

    // Start is called before the first frame update
    void Start()
    {
       InitiateRacers();
    }

    // Update is called once per frame
    void Update()
    {
        foreach (KeyValuePair<Racer, RacerStatus> entry in racersStatus)
        {
            Racer racer = entry.Key;
            RacerStatus status = entry.Value;

            // Step 1: predict forward motion
            Vector3 forwardMove = status.tangent * racer.runningSpeed * Time.deltaTime;
            Vector3 predicted = status.position + forwardMove;

            // Step 2: snap predicted point back onto spline
            Vector3 closestPoint = status.currentPath.ClosestPointPosition(predicted, status.tangent);
            Vector3 closestTan = status.currentPath.ClosestPointTangent(predicted, status.tangent).normalized;

            // Optional: if you have a spline normal method
            Vector3 closestNormal = Vector3.Cross(Vector3.up, closestTan).normalized;

            racer.GetComponent<RacerDebugger>().UpdateDebug(predicted, closestPoint, closestTan);


            // Step 3: move racer to spline centerline
            racer.transform.position += (closestPoint - racer.transform.position).normalized * racer.runningSpeed * Time.deltaTime;

            // Step 4: rotate racer to face along tangent
            Quaternion targetRot = Quaternion.LookRotation(closestTan, Vector3.up);
            racer.transform.rotation = targetRot;

            // Step 5: update racer status
            status.position = racer.transform.position;
            status.heading = targetRot;
            status.tangent = closestTan;
            status.normal = closestNormal;
        }

    }




    // Step 4: store new state

    private void UpdateRacerStatus(ref RacerStatus racerStatus)
    {

    }

    private void InitiateRacers()
    {
        List<SegmentSample> innerControlSamples = track.GetInnerSpline().GetControlPointSamples();
        SegmentSample startPoint = innerControlSamples[innerControlSamples.Count - 1];

        float trackWidth = track.trackSize;
        int numRacers = racerList.Count;
        float laneWidth = trackWidth / (float) numRacers;

        for (int i = 0; i < numRacers + 0; i++)
        {
            // Adjust racer spline path according to starting position
            List<Vector3> adjustedControlPoints = new List<Vector3>();
            for (int j = 0; j < innerControlSamples.Count; j++)
            {
                float offset = (i + 0.5f) * laneWidth;
                SegmentSample controlPoint = innerControlSamples[j];
                Vector3 adjustedPoint = controlPoint.Position + controlPoint.Normal * offset;
                adjustedControlPoints.Add(adjustedPoint);
            }

            // Initiate RacerStatus
            SplinePath racerPath = SplineCreator.CreateSplinePath(adjustedControlPoints, track.samplesPerSegment);
            List<SegmentSample> racerPathControlSamples = racerPath.GetControlPointSamples();
            SegmentSample startSample = racerPathControlSamples[racerPathControlSamples.Count - 1];

            Quaternion heading = Quaternion.LookRotation(startSample.Tangent, Vector3.up);
            RacerStatus racerStatus = new RacerStatus(racerPath, startSample.Position, startSample.Tangent, startSample.Normal, heading);
            racersStatus.Add(racerList[i], racerStatus);

            racerList[i].transform.position = startSample.Position;
            racerList[i].transform.rotation = heading;
        }
        

    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        foreach (RacerStatus racerStatus in racersStatus.Values)
        {
            //Gizmos.DrawRay(racerStatus.position, racerStatus.tangent);
            foreach (SegmentSample s in racerStatus.currentPath.GetSamplesTable())
            {
                Gizmos.DrawWireSphere(s.Position, 0.5f);
            }
        }
    }
}