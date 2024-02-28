using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClimbingController : MonoBehaviour
{
    EnvironmentChecker ec;

    private void Awake()
    {
        ec = GetComponent<EnvironmentChecker>();
    }

    private void Update()
    {
        if(Input.GetButton("Jump"))
        {
            if(ec.CheckClimbing(transform.forward, out RaycastHit climbInfo))
            {
                Debug.Log("Climb Point Found");
            }
        }
    }
}
