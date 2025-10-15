using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[Serializable]
public class RacingState : IRacerState
{
    private readonly RacerAI controller;
    public float racingSpeed = 0f;
    float speedFactor = 1f; // tune

    [SerializeField]
    public SegmentSample lookAheadSample;


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
            MoveForwards(controller.status.currentPath);
        }
    }

    private void MoveForwards(SplinePath path)
    {
        RacerStatus status = controller.status;
        RacerData rd = status.racerData;

        // Find out lookahead sample
        ClosestSample cs = path.ClosestSample(controller.transform.position, controller.transform.forward);
        SegmentSample sample = path.GetSamplesTable()[cs.ClosestIndex];

        int maxLookAheadDistance = 100;
        int minLookAheadDistance = 10;
        int lookAheadSampleIndex = minLookAheadDistance;

        if (racingSpeed > 0f)
        {
            float ratio = racingSpeed / rd.BaseSpeed;
            int lookAheadSampleDistance = (int) Mathf.Lerp(minLookAheadDistance, maxLookAheadDistance, ratio);
            lookAheadSampleIndex = (cs.ClosestIndex + lookAheadSampleDistance) % path.GetSamplesTable().Count;
        }

        lookAheadSample = path.GetSamplesTable()[lookAheadSampleIndex];

        // Adjust speed for lookahead curvature
        Debug.Log("Curvature: " + lookAheadSample.Curvature);
        Debug.Log("Curved Speed: " + rd.Cornering / Mathf.Sqrt(lookAheadSample.Curvature + 0.0001f));
        float targetSpeed = Mathf.Min(rd.BaseSpeed, rd.Cornering / Mathf.Sqrt(lookAheadSample.Curvature + 0.01f));
        if (racingSpeed < targetSpeed)
        {
            racingSpeed = Mathf.MoveTowards(racingSpeed, targetSpeed, rd.Acceleration * Time.deltaTime);
        }
        else
        {
            racingSpeed = Mathf.MoveTowards(racingSpeed, targetSpeed, rd.Deceleration * Time.deltaTime);
        }

        // Predict forward motion
        Vector3 forwardMove = status.tangent * racingSpeed * Time.deltaTime;
        Vector3 predicted = status.position + forwardMove;

        ClosestSample predictedClosestSample = path.ClosestSample(predicted, status.tangent);
        SegmentSample predictedSample = path.GetSamplesTable()[predictedClosestSample.ClosestIndex];

        // Step 2: snap predicted point back onto spline
        Vector3 closestPoint = predictedSample.Position;
        Vector3 closestTan = predictedSample.Tangent.normalized;

        Vector3 closestNormal = Vector3.Cross(Vector3.up, closestTan).normalized;

        // Step 3: move racer to closest point on spline
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

    private void MoveTowardsClosesetPoint(SplinePath path)
    {
        RacerStatus status = controller.status;
        RacerData rd = status.racerData;


        // Step 1: predict forward motion
        Vector3 forwardMove = status.tangent * racingSpeed * Time.deltaTime;
        Vector3 predicted = status.position + forwardMove;

        // Step 2: snap predicted point back onto spline
        ClosestSample cs = path.ClosestSample(predicted, status.tangent);
        SegmentSample sample = path.GetSamplesTable()[cs.ClosestIndex];

        Vector3 closestPoint = sample.Position;
        Vector3 closestTan = sample.Tangent.normalized;

        // Optional: if you have a spline normal method
        Vector3 closestNormal = Vector3.Cross(Vector3.up, closestTan).normalized;

        // Step 3: move racer to closest point on spline
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

	// Theoretically no need to calculate a new path. 
	// Can continously calculate next position with tangent of closest point of spline.
    private void RecalculatePath()
    {
        SplinePath innerSpline = controller.race.track.GetInnerSpline();
        Vector3 closestPoint = innerSpline.ClosestPointPosition(controller.status.position, controller.status.tangent);

        float distance = Vector3.Distance(controller.status.position, closestPoint);

        SplinePath newPath = SplineCreator.CreateNewOffsetSplineByNormal(innerSpline, distance, 100);
        controller.status.currentPath = newPath;
    }

    private void LookaheadRefinedSample(float lookDistance)
    {
        SplinePath path = controller.status.currentPath;
        var cs = path.FindRefinedClosestSample(controller.status.position, controller.status.tangent);
        
        path.FindClosestSampleByDistance(lookDistance + cs.distance);
    }

    private void LookaheadSample(int samplesAmount)
    {
        SplinePath splinePath = controller.status.currentPath;
        ClosestSample cs = splinePath.ClosestSample(controller.status.position, controller.status.tangent);

        int lookaheadSampleIdx = samplesAmount + cs.ClosestIndex;
        SegmentSample s = splinePath.GetSamplesTable()[lookaheadSampleIdx];
    }

    public void Exit() { }
}
