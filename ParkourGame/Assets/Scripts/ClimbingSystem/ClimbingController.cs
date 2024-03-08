using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClimbingController : MonoBehaviour
{
    EnvironmentChecker ec;
    public PlayerScript playerScript;

    public float InOutValue;
    public float UpDownValue;
    public float LeftRightValue;

    private void Awake()
    {
        ec = GetComponent<EnvironmentChecker>();
    }

    private void Update()
    {
        if (!playerScript.playerHanging)
        {
            if (Input.GetButton("Jump") && !playerScript.playerInAction)
            {
                if (ec.CheckClimbing(transform.forward, out RaycastHit climbInfo))
                {
                    playerScript.SetControl(false);
                    StartCoroutine(ClimbToLedge("IdleToClimb", climbInfo.transform, 0.40f, 54f));
                }
            }
        }
        else
        {
            //Ledge to Ledge parkour actions
        }
    }

    IEnumerator ClimbToLedge(string animationName, Transform ledgePoint, float compareStartTime, float compareEndTime)
    {
        var compareParams = new CompareTargetParameter()
        {
            position = SetHandPosition(ledgePoint),
            bodyPart = AvatarTarget.RightHand,
            positionWeight = Vector3.one,
            startTime = compareStartTime,
            endTime = compareEndTime
        };

        var requiredRot = Quaternion.LookRotation(-ledgePoint.forward);

        yield return playerScript.PerformAction(animationName, compareParams, requiredRot, true);

        playerScript.playerHanging = true;
    }

    Vector3 SetHandPosition(Transform ledge)
    {
        //InOutValue = -0.23f;
        //UpDownValue = -0.09f;
        //LeftRightValue = 0.20f;

        return ledge.position + ledge.forward * InOutValue + Vector3.up * UpDownValue - ledge.right * LeftRightValue;
    }
}
