using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// A racer in a race
public class Racer : MonoBehaviour
{
    public RacerData racerData;
    public RacerStatus currentStatus;
    public RacerAI racerController;

    public void Advance()
    {
        racerController.Advance();
    }
}
