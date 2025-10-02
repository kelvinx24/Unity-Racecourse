using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IdleState : IRacerState
{
    private readonly RacerAI controller;

    public IdleState(RacerAI controller) => this.controller = controller;

    public void Enter() { /* maybe play idle animation */ }
    public void Update() { }
    public void Exit() { }
}
