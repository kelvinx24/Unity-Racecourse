using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class RacingState : IRacerState
{
    private readonly RacerAI controller;
    private readonly float racingSpeed = 5f;

    public RacingState(RacerAI controller) => this.controller = controller;


    public void Enter() { }

    public void Update()
    {
        if (controller.raceMode == RaceMode.Outside)
        {
            MoveTowardsClosesetPoint(controller.race.track.GetOuterSpline());
            RecalculatePath();
        }
        else if (controller.raceMode == RaceMode.Inside)
        {
            MoveTowardsClosesetPoint(controller.race.track.GetInnerSpline());
            RecalculatePath();
        }
        else
        {
            MoveTowardsClosesetPoint(controller.status.currentPath);
        }
    }

    private void MoveTowardsClosesetPoint(SplinePath path)
    {
        RacerStatus status = controller.status;

        // Step 1: predict forward motion
        Vector3 forwardMove = status.tangent * racingSpeed * Time.deltaTime;
        Vector3 predicted = status.position + forwardMove;

        // Step 2: snap predicted point back onto spline
        Vector3 closestPoint = path.ClosestPointPosition(predicted, status.tangent);
        Vector3 closestTan = path.ClosestPointTangent(predicted, status.tangent).normalized;

        // Optional: if you have a spline normal method
        Vector3 closestNormal = Vector3.Cross(Vector3.up, closestTan).normalized;

        // Step 3: move racer to spline centerline
        controller.transform.position += (closestPoint - controller.transform.position).normalized * racingSpeed * Time.deltaTime;

        // Step 4: rotate racer to face along tangent
        Quaternion targetRot = Quaternion.LookRotation(closestTan, Vector3.up);
        controller.transform.rotation = targetRot;

        // Step 5: update racer status
        status.position = controller.transform.position;
        status.heading = targetRot;
        status.tangent = closestTan;
        status.normal = closestNormal;
    }

    private void RecalculatePath()
    {
        SplinePath innerSpline = controller.race.track.GetInnerSpline();
        Vector3 closestPoint = innerSpline.ClosestPointPosition(controller.status.position, controller.status.tangent);

        float distance = Vector3.Distance(controller.status.position, closestPoint);

        SplinePath newPath = SplineCreator.CreateNewOffsetSplineByNormal(innerSpline, distance, 100);
        controller.status.currentPath = newPath;
    }

    public void Exit() { }
}
