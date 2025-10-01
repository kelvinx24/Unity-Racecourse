using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RacerAI : MonoBehaviour
{
    protected RacerStatus status;
    private bool canMove = false;
    private Race race;

    public void Initialize(RacerStatus racerStatus, Race race)
    {
        status = racerStatus;
        this.race = race;
    }

    private void OnEnable()
    {
        Race.OnRaceStarted += EnableMovement;
        Race.OnRaceEnded += DisableMovement;
    }

    private void OnDisable()
    {
        Race.OnRaceStarted -= EnableMovement;
        Race.OnRaceEnded -= DisableMovement;
    }

    private void EnableMovement()
    {
        canMove = true;
    }

    private void DisableMovement()
    {
        canMove = false;
    }

    private void Update()
    {
        if (canMove)
            Advance();
    }

    private void Advance()
    {

    }
}
