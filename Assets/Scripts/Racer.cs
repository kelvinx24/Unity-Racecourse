using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// A racer in a race
public class Racer : MonoBehaviour
{
    public string racerName;

    public float runningSpeed = 10f;

    public float lateralMoveSpeed = 3f;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(transform.position, transform.forward);

        Gizmos.color = Color.white;
        Gizmos.DrawRay(transform.position, Vector3.Cross(transform.up, transform.forward));
    }
}
