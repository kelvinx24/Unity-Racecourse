using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class SimpleMove : MonoBehaviour
{
    public PIDController controller;
    public float accel;
    public Transform target;


    /*
    private void Update()
    {
        float result = controller.UpdatePID(Time.deltaTime, transform.position.x, target.x);
        transform.position = new Vector3(transform.position.x + accel * result, 0, 0);
        Debug.Log("Force applied: " + (accel * result));

    }
    */

    private void FixedUpdate()
    {
        Rigidbody rb = GetComponent<Rigidbody>();

        float result = controller.UpdatePID(Time.fixedDeltaTime, rb.position.x, target.position.x);
        Debug.Log(result);
        rb.AddForce(new Vector3(result * accel, 0, 0));
    }
}
