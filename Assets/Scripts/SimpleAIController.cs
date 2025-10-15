using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleAIController : MonoBehaviour
{
    public RacerStatus status { get; private set; }
    public Race race { get; private set; }


    [SerializeField]
    PIDController controller;
    [SerializeField]
    float power;
    [SerializeField] 
    float currentSpeed;
    [SerializeField]
    float maxSpeed;
    [SerializeField]
    Transform target;

    private Vector3 targetPosition;

    private void Start()
    {
        targetPosition = transform.position;
    }
    // Update is called once per frame
    void Update()
    {
        float dt = Time.deltaTime;
        if (currentSpeed < maxSpeed)
            currentSpeed = Mathf.MoveTowards(currentSpeed, maxSpeed, power * dt);
        else
            currentSpeed = Mathf.MoveTowards(currentSpeed, maxSpeed, power * dt);

        
    }       
}
