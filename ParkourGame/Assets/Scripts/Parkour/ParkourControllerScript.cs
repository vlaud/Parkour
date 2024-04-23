using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParkourControllerScript : MonoBehaviour
{
    public EnvironmentChecker enviromentChecker;
   
    public Animator animator;
    public PlayerScript playerScript;
    [SerializeField] NewParkourAction jumpDownParkourAction;
    [SerializeField] NewParkourAction slideParkourAction;
    [Header("Parkour Action Area")]
    public List<NewParkourAction> newParkourActions;

    private void Update()
    {
        if (Input.GetButton("Jump") && !playerScript.playerInAction && !playerScript.playerHanging)
        {
            var hitData = enviromentChecker.CheckObstacle();
            var slideData = enviromentChecker.CheckSlide();

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

            if (slideData.hitFound)
            {
                slideParkourAction.CheckLookAtObstacle(slideData);
                slideParkourAction.CheckSlidingGapAvailable(slideData, transform);
                playerScript.SlideInfo = slideData;
                StartCoroutine(PerformParkourAction(slideParkourAction));
            }
        }

        if (playerScript.playerOnLedge && !playerScript.playerInAction && Input.GetButtonDown("Jump"))
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
        playerScript.SetControl(false);

        CompareTargetParameter compareTargetParameter = null;
        if(action.AllowTargetMatching)
        {
            compareTargetParameter = new CompareTargetParameter()
            {
                position = action.ComparePosition,
                bodyPart = action.CompareBodyPart,
                positionWeight = action.ComparePositionWeight,
                startTime = action.CompareStartTime,
                endTime = action.CompareEndTime
            };
        }

        yield return playerScript.PerformAction(action.AnimationName, compareTargetParameter, action.RequiredRotation,
            action.LookAtObstacle, action.ParkourActionDelay);

        playerScript.SetControl(true);
    }

    void CompareTarget(NewParkourAction action)
    {
        animator.MatchTarget(action.ComparePosition, transform.rotation,
            action.CompareBodyPart, new MatchTargetWeightMask(action.ComparePositionWeight, 0),
            action.CompareStartTime, action.CompareEndTime);
    }
}
