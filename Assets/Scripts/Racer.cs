using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// A racer in a race
public class Racer : MonoBehaviour
{
    public string racerName;

    public float runningSpeed = 10f;

    public float lateralMoveSpeed = 3f;

    private Vector3 previousPosition;


    // Start is called before the first frame update
    void Start()
    {
        previousPosition = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        //Debug.Log("Delta Speed: " + (previousPosition - transform.position).magnitude);
        previousPosition = transform.position;
    }

    private void OnDrawGizmos()
    {
        //Gizmos.color = Color.yellow;
        //Gizmos.DrawRay(transform.position, transform.forward);

        //Gizmos.color = Color.white;
        //Gizmos.DrawRay(transform.position, Vector3.Cross(transform.up, transform.forward));
    }
}
