using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

// A race with racers that are running on a track
public class Race : MonoBehaviour
{
    public List<Racer> racerList = new List<Racer>();

    public float[] targetRacerOffset;

    private float[] racerOffset;

    public float laneSize = 0.0f;

    private float[] racerDistance;

    // Start is called before the first frame update
    void Start()
    {
        int racers = racerList.Count;


        racerDistance = new float[racers];
        racerOffset = new float[racers];
        targetRacerOffset = new float[racers];
        //SplineCreator.RacerStatus startingStatus = splineCreator.AdvanceRacer(0f, 0f, 0f); 

        for (int i = 0; i < racers; i++)
        {
            racerOffset[i] = i;
            targetRacerOffset[i] = i;
        }
    }

    // Update is called once per frame
    void Update()
    {
        int racers = racerList.Count;
        for (int i = 0; i < racers; i++)
        {
            Racer r = racerList[i];
            float oldOffset = racerOffset[i];

            racerOffset[i] = Mathf.MoveTowards(racerOffset[i], targetRacerOffset[i], r.lateralMoveSpeed * Time.deltaTime);
            float lateralVel = (racerOffset[i] - oldOffset) / Time.deltaTime;
            float forwardSpeed = Mathf.Sqrt(Mathf.Max(0f, r.runningSpeed * r.runningSpeed - lateralVel * lateralVel));

        }
    }

    private void UpdateRacerStatus(ref RacerStatus racerStatus)
    {

    }
}