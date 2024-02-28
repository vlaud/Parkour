using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParkourControllerScript : MonoBehaviour
{
    public EnvironmentChecker enviromentChecker;
    bool playerInAction;
    public Animator animator;
    public PlayerScript playerScript;
    [SerializeField] NewParkourAction jumpDownParkourAction;

    [Header("Parkour Action Area")]
    public List<NewParkourAction> newParkourActions;

    private void Update()
    {
        if (Input.GetButton("Jump") && !playerInAction)
        {
            var hitData = enviromentChecker.CheckObstacle();

            if (hitData.hitFound)
            {
                foreach (var action in newParkourActions)
                {
                    if (action.CheckIfAvailable(hitData, transform))
                    {
                        //perform parkour action
                        StartCoroutine(PerformParkourAction(action));
                        break;
                    }
                }
            }
        }

        if(playerScript.playerOnLedge && !playerInAction && Input.GetButtonDown("Jump"))
        {
            if(playerScript.LedgeInfo.angle <= 50)
            {
                playerScript.playerOnLedge = false;
                StartCoroutine(PerformParkourAction(jumpDownParkourAction));
            }
        }
    }

    IEnumerator PerformParkourAction(NewParkourAction action)
    {
        playerInAction = true;
        playerScript.SetControl(false);

        animator.CrossFade(action.AnimationName, 0.2f);
        yield return null;

        var animationState = animator.GetNextAnimatorStateInfo(0);
        if (!animationState.IsName(action.AnimationName))
            Debug.Log("Animation Name is Incorrect");

        float timerCounter = 0f;

        while (timerCounter <= animationState.length)
        {
            timerCounter += Time.deltaTime;

            //Make player to look towards the obstacle
            if(action.LookAtObstacle)
            {
                transform.rotation = Quaternion.RotateTowards(transform.rotation, action.RequiredRotation, playerScript.rotSpeed);
            }

            if(action.AllowTargetMatching && !animator.IsInTransition(0))
            {
                CompareTarget(action);
            }

            if(animator.IsInTransition(0) && timerCounter > 0.5f)
            {
                break;
            }

            yield return null;
        }

        yield return new WaitForSeconds(action.ParkourActionDelay);

        playerScript.SetControl(true);
        playerInAction = false;
    }

    void CompareTarget(NewParkourAction action)
    {
        animator.MatchTarget(action.ComparePosition, transform.rotation,
            action.CompareBodyPart, new MatchTargetWeightMask(action.ComparePositionWeight, 0),
            action.CompareStartTime, action.CompareEndTime, true);
    }
}
