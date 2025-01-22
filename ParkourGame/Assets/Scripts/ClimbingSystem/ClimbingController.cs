using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClimbingController : MonoBehaviour
{
    public enum DIR
    {
        LEFT, RIGHT, UP, DOWN
    }
    
    EnvironmentChecker ec;
    public PlayerScript playerScript;

    [Header("Climbing Action Area")]
    [SerializeField] NewClimbAction currentClimbAction;
    [SerializeField] NewClimbAction IdleToClimb;
    [SerializeField] NewClimbAction DropToFreehang;
    [SerializeField] SerializableDictionary<DIR, NewClimbAction> ClimbActions;
    [SerializeField] SerializableDictionary<DIR, NewClimbAction> ShimmyActions;
    [SerializeField] ClimbingPoint currentClimbPoint;
    [SerializeField] ClimbingPoint tempCheckPoint;

    private void Awake()
    {
        ec = GetComponent<EnvironmentChecker>();
    }

    private void Update()
    {
        ec.RayToClimbPoint();
        if (!playerScript.playerHanging)
        {
            if (Input.GetButton("Jump") && !playerScript.playerInAction)
            {
                if (ec.CheckClimbing(transform.forward, out RaycastHit climbInfo))
                {
                    currentClimbPoint = climbInfo.transform.GetComponent<ClimbingPoint>();

                    playerScript.SetControl(false);
                    SetClimbingAction(IdleToClimb, climbInfo.transform);
                }
            }

            if (Input.GetButton("Leave") && !playerScript.playerInAction)
            {
                if (ec.CheckDropClimbPoint(out RaycastHit DropHit))
                {
                    currentClimbPoint = GetNearestClimbingPoint(DropHit.transform);
                    Debug.Log($"DropHit on Surface: {DropHit.transform}");
                    playerScript.SetControl(false);
                    SetClimbingAction(DropToFreehang, currentClimbPoint.transform);
                }
            }
        }
        else
        {
            //leave climb point
            if (Input.GetButton("Leave") && !playerScript.playerInAction)
            {
                StartCoroutine(JumpFromWall());
                return;
            }

            float horizontal = Mathf.Round(Input.GetAxisRaw("Horizontal"));
            float vertical = Mathf.Round(Input.GetAxisRaw("Vertical"));

            var inputDirection = new Vector2(horizontal, vertical);

            if (playerScript.playerInAction || inputDirection == Vector2.zero) return;

            //climb to top
            if (currentClimbPoint.MountPoint && inputDirection.y == 1)
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
                    SetClimbingAction(ClimbActions[DIR.UP], currentClimbPoint.transform);
                }
                else if (neighbour.pointDirection.y == -1)
                {
                    SetClimbingAction(ClimbActions[DIR.DOWN], currentClimbPoint.transform);
                }
                else if (neighbour.pointDirection.x == 1)
                {
                    ClimbActions[DIR.RIGHT].OffSetValue = currentClimbAction.OffSetValue;
                    SetClimbingAction(ClimbActions[DIR.RIGHT], currentClimbPoint.transform);
                }
                else if (neighbour.pointDirection.x == -1)
                {
                    SetClimbingAction(ClimbActions[DIR.LEFT], currentClimbPoint.transform);
                }
            }
            else if (neighbour.connetionType == ConnetionType.Move)
            {
                currentClimbPoint = neighbour.climbingPoint;

                if (neighbour.pointDirection.x == 1)
                {
                    SetClimbingAction(ShimmyActions[DIR.RIGHT], currentClimbPoint.transform);
                }
                else if (neighbour.pointDirection.x == -1)
                {
                    SetClimbingAction(ShimmyActions[DIR.LEFT], currentClimbPoint.transform);
                }
            }
        }
    }

    void SetClimbingAction(NewClimbAction action, Transform ledge)
    {
        Debug.Log($"action: {action}");
        currentClimbAction = action;
        currentClimbAction.SetComparePostion(ledge);
        StartCoroutine(ClimbToLedge(action));
    }

    IEnumerator ClimbToLedge(NewClimbAction action)
    {
        var requiredRot = Quaternion.LookRotation(-action.LedgePoint.forward);

        yield return playerScript.PerformAction(action.AnimationName, action.CTP, requiredRot, true);

        playerScript.playerHanging = true;
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

    ClimbingPoint GetNearestClimbingPoint(Transform dropClimbPoint)
    {
        var points = dropClimbPoint.GetComponentsInChildren<ClimbingPoint>();

        ClimbingPoint nearestPoint = null;

        float nearestPointDistance = Mathf.Infinity;

        foreach (var point in points)
        {
            float distance = Vector3.Distance(point.transform.position, transform.position);

            if (distance < nearestPointDistance)
            {
                nearestPoint = point;
                nearestPointDistance = distance;
            }
        }

        return nearestPoint;
    }
}
