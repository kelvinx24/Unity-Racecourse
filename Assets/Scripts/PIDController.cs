using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PIDController : MonoBehaviour
{
    public enum DerivativeMeasurement
    {
        Velocity,
        ErrorRateOfChange
    }

    //PID coefficients
    public float proportionalGain;
    public float integralGain;
    public float derivativeGain;

    public float outputMin = -1;
    public float outputMax = 1;
    public float integralSaturation;
    public DerivativeMeasurement derivativeMeasurement;

    public float valueLast;
    public Vector3 posLast;
    public float errorLast;
    public float integrationStored;
    public float velocity;  //only used for the info display
    public bool derivativeInitialized;


    public float UpdatePID(float dt, float currentValue, float targetValue)
    {
        if (dt <= 0) throw new ArgumentOutOfRangeException(nameof(dt));

        float error = targetValue - currentValue;

        //calculate P term
        float P = proportionalGain * error;

        //calculate I term
        integrationStored = Mathf.Clamp(integrationStored + (error * dt), -integralSaturation, integralSaturation);
        float I = integralGain * integrationStored;

        //calculate both D terms
        float errorRateOfChange = (error - errorLast) / dt;
        errorLast = error;

        float valueRateOfChange = (currentValue - valueLast) / dt;
        valueLast = currentValue;
        velocity = valueRateOfChange;

        //choose D term to use
        float deriveMeasure = 0;

        if (derivativeInitialized)
        {
            if (derivativeMeasurement == DerivativeMeasurement.Velocity)
            {
                deriveMeasure = -valueRateOfChange;
            }
            else
            {
                deriveMeasure = errorRateOfChange;
            }
        }
        else
        {
            derivativeInitialized = true;
        }

        float D = derivativeGain * deriveMeasure;

        float result = P + I + D;

        return Mathf.Clamp(result, outputMin, outputMax);
    }

    public float UpdatePosition(float dt, Vector3 currentPos, Vector3 targetPos)
    {
        float error = Vector3.Distance(currentPos, targetPos);

        //calculate P term
        float P = proportionalGain * error;

        //calculate I term
        integrationStored = Mathf.Clamp(integrationStored + (error * dt), -integralSaturation, integralSaturation);
        float I = integralGain * integrationStored;

        //calculate both D terms
        float errorRateOfChange = (error - errorLast) / dt;
        errorLast = error;

        float valueRateOfChange = Vector3.Distance(currentPos, posLast) / dt;
        posLast = currentPos;
        velocity = valueRateOfChange;

        //choose D term to use
        float deriveMeasure = 0;

        if (derivativeInitialized)
        {
            if (derivativeMeasurement == DerivativeMeasurement.Velocity)
            {
                deriveMeasure = -valueRateOfChange;
            }
            else
            {
                deriveMeasure = errorRateOfChange;
            }
        }
        else
        {
            derivativeInitialized = true;
        }

        float D = derivativeGain * deriveMeasure;

        float result = P + I + D;

        return Mathf.Clamp(result, outputMin, outputMax);
    }

    public void Reset()
    {
        derivativeInitialized = false;
    }
}