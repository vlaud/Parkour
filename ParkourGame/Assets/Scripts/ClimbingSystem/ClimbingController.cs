using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClimbingController : MonoBehaviour
{
    EnvironmentChecker ec;
    public PlayerScript playerScript;

    ClimbingPoint currentClimbPoint;

    private float InOutValue;
    private float UpDownValue;
    private float LeftRightValue;

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
                    InOutValue = -0.23f;
                    UpDownValue = -0.09f;
                    LeftRightValue = 0.15f;
                    StartCoroutine(ClimbToLedge("IdleToClimb", climbInfo.transform, 0.40f, 54f,
                        playerHandOffset: new Vector3(InOutValue, UpDownValue, LeftRightValue)));
                }
            }
        }
        else
        {
            //leave climb point
            if(Input.GetButton("Leave") && !playerScript.playerInAction)
            {
                StartCoroutine(JumpFromWall());
                return;
            }

            float horizontal = Mathf.Round(Input.GetAxisRaw("Horizontal"));
            float vertical = Mathf.Round(Input.GetAxisRaw("Vertical"));

            var inputDirection = new Vector2(horizontal, vertical);

            if (playerScript.playerInAction || inputDirection == Vector2.zero) return;

            //climb to top
            if(currentClimbPoint.MountPoint && inputDirection.y == 1)
            {
                StartCoroutine(ClimbToTop());
                return;
            }

            //Ledge to Ledge parkour actions
            var neighbour = currentClimbPoint.GetNeighbour(inputDirection);

            if (neighbour == null) return;

            if (neighbour.connetionType == ConnetionType.Jump && Input.GetButton("Jump"))
            {
                currentClimbPoint = neighbour.climbingPoint;

                if (neighbour.pointDirection.y == 1)
                {
                    InOutValue = 0.1f;
                    UpDownValue = 0.05f;
                    LeftRightValue = 0.25f;
                    StartCoroutine(ClimbToLedge("ClimbUp", currentClimbPoint.transform, 0.34f, 0.64f, 
                        playerHandOffset: new Vector3(InOutValue, UpDownValue, LeftRightValue)));
                }
                else if (neighbour.pointDirection.y == -1)
                {
                    InOutValue = 0.2f;
                    UpDownValue = 0.05f;
                    LeftRightValue = 0.25f;
                    StartCoroutine(ClimbToLedge("ClimbDown", currentClimbPoint.transform, 0.31f, 0.68f,
                        playerHandOffset: new Vector3(InOutValue, UpDownValue, LeftRightValue)));
                }
                else if (neighbour.pointDirection.x == 1)
                {
                    StartCoroutine(ClimbToLedge("ClimbRight", currentClimbPoint.transform, 0.20f, 0.51f));
                }
                else if (neighbour.pointDirection.x == -1)
                {
                    InOutValue = 0.1f;
                    UpDownValue = 0.04f;
                    LeftRightValue = 0.25f;
                    StartCoroutine(ClimbToLedge("ClimbLeft", currentClimbPoint.transform, 0.20f, 0.51f,
                        playerHandOffset: new Vector3(InOutValue, UpDownValue, LeftRightValue)));
                }
            }
            else if (neighbour.connetionType == ConnetionType.Move)
            {
                currentClimbPoint = neighbour.climbingPoint;

                if (neighbour.pointDirection.x == 1)
                {
                    InOutValue = 0.2f;
                    UpDownValue = 0.03f;
                    LeftRightValue = 0.25f;
                    StartCoroutine(ClimbToLedge("ShimmyRight", currentClimbPoint.transform, 0f, 0.30f,
                        playerHandOffset: new Vector3(InOutValue, UpDownValue, LeftRightValue)));
                }
                else if (neighbour.pointDirection.x == -1)
                {
                    InOutValue = 0.2f;
                    UpDownValue = 0.03f;
                    LeftRightValue = 0.25f;
                    StartCoroutine(ClimbToLedge("ShimmyLeft", currentClimbPoint.transform, 0f, 0.30f, 
                        AvatarTarget.LeftHand, new Vector3(InOutValue, UpDownValue, LeftRightValue)));
                }
            }
        }
    }

    IEnumerator ClimbToLedge(string animationName, Transform ledgePoint, float compareStartTime, float compareEndTime, 
        AvatarTarget hand = AvatarTarget.RightHand, Vector3? playerHandOffset = null)
    {
        var compareParams = new CompareTargetParameter()
        {
            position = SetHandPosition(ledgePoint, hand, playerHandOffset),
            bodyPart = hand,
            positionWeight = Vector3.one,
            startTime = compareStartTime,
            endTime = compareEndTime
        };

        var requiredRot = Quaternion.LookRotation(-ledgePoint.forward);

        yield return playerScript.PerformAction(animationName, compareParams, requiredRot, true);

        playerScript.playerHanging = true;
    }

    Vector3 SetHandPosition(Transform ledge, AvatarTarget hand, Vector3? playerHandOffset)
    {
        var offsetValue = (playerHandOffset != null) ? playerHandOffset.Value : new Vector3(InOutValue, UpDownValue, LeftRightValue);
        
        var handDirection = (hand == AvatarTarget.RightHand) ? ledge.right : -ledge.right;
        return ledge.position + ledge.forward * InOutValue + Vector3.up * UpDownValue - handDirection * LeftRightValue;
    }

    IEnumerator JumpFromWall()
    {
        playerScript.playerHanging = false;
        yield return playerScript.PerformAction("JumpFromWall");
        playerScript.ResetRequiredRotation();
        playerScript.SetControl(true);
    }

    IEnumerator ClimbToTop()
    {
        playerScript.playerHanging = false;
        yield return playerScript.PerformAction("ClimbToTop");

        playerScript.EnableCC(true);

        yield return new WaitForSeconds(0.5f);

        playerScript.ResetRequiredRotation();
        playerScript.SetControl(true);
    }
}
