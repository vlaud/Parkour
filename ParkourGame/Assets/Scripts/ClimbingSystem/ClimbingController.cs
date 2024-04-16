using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClimbingController : MonoBehaviour
{
    EnvironmentChecker ec;
    public PlayerScript playerScript;

    ClimbingPoint currentClimbPoint;

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
                    currentClimbPoint = climbInfo.transform.GetComponent<ClimbingPoint>();

                    playerScript.SetControl(false);
                    StartCoroutine(ClimbToLedge("IdleToClimb", climbInfo.transform, 0.40f, 0.54f));
                }
            }
        }
        else
        {
            //Ledge to Ledge parkour actions

            float horizontal = Mathf.Round(Input.GetAxisRaw("Horizontal"));
            float vertical = Mathf.Round(Input.GetAxisRaw("Vertical"));

            var inputDirection = new Vector2(horizontal, vertical);

            if (playerScript.playerInAction || inputDirection == Vector2.zero) return;

            var neighbour = currentClimbPoint.GetNeighbour(inputDirection);

            if (neighbour == null) return;

            if (neighbour.connetionType == ConnetionType.Jump && Input.GetButton("Jump"))
            {
                currentClimbPoint = neighbour.climbingPoint;

                if (neighbour.pointDirection.y == 1)
                {
                    StartCoroutine(ClimbToLedge("ClimbUp", currentClimbPoint.transform, 0.34f, 0.64f));
                }
                else if (neighbour.pointDirection.y == -1)
                {
                    StartCoroutine(ClimbToLedge("ClimbDown", currentClimbPoint.transform, 0.31f, 0.68f));
                }
                else if (neighbour.pointDirection.x == 1)
                {
                    StartCoroutine(ClimbToLedge("ClimbRight", currentClimbPoint.transform, 0.20f, 0.51f));
                }
                else if (neighbour.pointDirection.x == -1)
                {
                    StartCoroutine(ClimbToLedge("ClimbLeft", currentClimbPoint.transform, 0.20f, 0.51f));
                }
            }
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
