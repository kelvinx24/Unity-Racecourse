using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// A race with racers that are running on a track
public class Race : MonoBehaviour
{
    public List<Racer> racerList = new List<Racer>();

    public SplineCreator splineCreator;

    private float[] racerDistance;

    // Start is called before the first frame update
    void Start()
    {
        racerDistance = new float[racerList.Count];
        //SplineCreator.RacerStatus startingStatus = splineCreator.AdvanceRacer(0f, 0f, 0f); 
        foreach (Racer r in racerList)
        {

        }
    }

    // Update is called once per frame
    void Update()
    {
        int racers = racerList.Count;
        for (int i = 0; i < racers; i++)
        {
            Racer r = racerList[i];
         
            RacerStatus newRacerStatus = splineCreator.AdvanceRacer(racerDistance[i], r.runningSpeed, Time.deltaTime);

            r.transform.position = newRacerStatus.position;
            r.transform.rotation = newRacerStatus.heading;

            racerDistance[i] = newRacerStatus.distanceCovered;
        }
    }

    private void UpdateRacerStatus(ref RacerStatus racerStatus)
    {

    }
}