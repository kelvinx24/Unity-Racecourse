using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowPathState : IRacerState
{
    private readonly RacerAI controller;

    public FollowPathState(RacerAI controller) => this.controller = controller;

    public void Enter() { /* maybe play idle animation */ }
    public void Update() { }
    public void Exit() { }
}
