using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Jobs;
using Unity.VisualScripting;
using UnityEngine;

public enum RaceState { Waiting, Countdown, Racing, Finished }

// A race with racers that are running on a track
public class Race : MonoBehaviour
{
    public static event Action OnRaceStarted;
    public static event Action OnRaceEnded;

    public List<Racer> participatingRacers = new List<Racer>();

    public List<RacerStatus> racerStatuses = new List<RacerStatus>();

    public Track track;

    public RacerSpawner spawner;

    private RaceState state = RaceState.Waiting;


    //private List<RacerStatus> racersStatus = new List<RacerStatus>();

    // Start is called before the first frame update
    void Start()
    {
       InitiateRacers();
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void BeginRace()
    {
        StartCoroutine(StartRaceRoutine());
    }

    private IEnumerator StartRaceRoutine()
    {
        state = RaceState.Countdown;

        for (int i = 3; i > 0; i--)
        {
            Debug.Log(i);
            yield return new WaitForSeconds(1f);
        }

        Debug.Log("GO!");
        state = RaceState.Racing;
        OnRaceStarted?.Invoke();
    }

    public void CheckFinish(RacerStatus status)
    {
        if (state != RaceState.Racing)
            return;

        if (status.Finished)
        {
            state = RaceState.Finished;
            Debug.Log($"{status.racerData.Name} wins!");
            OnRaceEnded?.Invoke();
        }
    }

    public void RegisterRacer(Racer data)
    {
        participatingRacers.Add(data);
    }

    private void InitiateRacers()
    {
        List<SegmentSample> innerControlSamples = track.GetInnerSpline().GetControlPointSamples();
        SegmentSample startPoint = innerControlSamples[innerControlSamples.Count - 1];

        float trackWidth = track.trackSize;
        int numRacers = participatingRacers.Count;
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
            racerStatus.racerData = participatingRacers[i].racerData;
            racerStatuses.Add(racerStatus);

            spawner.SpawnRacer(this, racerStatus);

        }
        

    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        foreach (RacerStatus racerStatus in racerStatuses)
        {
            Gizmos.DrawRay(racerStatus.position, racerStatus.tangent);
            foreach (SegmentSample s in racerStatus.currentPath.GetSamplesTable())
            {
                Gizmos.DrawWireSphere(s.Position, 0.5f);
            }
        }
    }
}