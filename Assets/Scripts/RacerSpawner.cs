using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RacerSpawner : MonoBehaviour
{
    public void SpawnRacer(Race race, RacerStatus status)
    {
        Track tarck = race.track;
        Vector3 spawnPos = status.position;
        Quaternion spawnRot = status.heading;

        GameObject racerGO = Instantiate(status.racerData.gameObject, spawnPos, spawnRot);
        var controller = racerGO.GetComponent<RacerAI>();
        controller.Initialize(status, race); // Inject both
    }
}
