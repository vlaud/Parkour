using UnityEngine;

[CreateAssetMenu(fileName = "NewClimbAction", menuName = "Parkour Menu/Create New Climb Action")]
public class NewClimbAction : ScriptableObject
{
    [Header("Checking Climbing")]
    [SerializeField] string animationName;
    [SerializeField] private Vector3 offSetValue;

    [Header("Target Matching")]
    [SerializeField] CompareTargetParameter ctp;

    public Transform LedgePoint { get; set; }

    public void SetComparePostion(Transform ledge)
    {
        LedgePoint = ledge;
        ctp.position = SetHandPosition();
    }

    Vector3 SetHandPosition()
    {
        var handDirection = (ctp.bodyPart == AvatarTarget.RightHand) ? LedgePoint.right : -LedgePoint.right;
        return LedgePoint.position + LedgePoint.forward * offSetValue.x + Vector3.up * offSetValue.y - handDirection * offSetValue.z;
    }

    public string AnimationName => animationName;

    public Vector3 OffSetValue
    {
        get => offSetValue;
        set => offSetValue = value;
    }

    public CompareTargetParameter CTP => ctp;
}
