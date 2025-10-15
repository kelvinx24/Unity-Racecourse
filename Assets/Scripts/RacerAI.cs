using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum RaceMode { Outside, Inside, Follow }

public class RacerAI : MonoBehaviour
{
    public RacerStatus status { get; private set; }
    public Race race { get; private set; }

    public RaceMode raceMode = RaceMode.Follow;

    public RacingState currentState;

    private bool canMove = false;

    public void Initialize(RacerStatus racerStatus, Race race)
    {
        status = racerStatus;
        this.race = race;

        ChangeState(new RacingState(this));
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

    public void ChangeState(RacingState newState)
    {
        currentState?.Exit();
        currentState = newState;
        currentState?.Enter();
    }

    private void Update()
    {
        currentState?.Update();
        Debug.Log("Current Speed: " + currentState.racingSpeed);
    }

    private void Advance()
    {

    }

    private void OnDrawGizmos()
    {
        SegmentSample s = currentState.lookAheadSample;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(s.Position, new Vector3(1, 1, 1));
    }
}
