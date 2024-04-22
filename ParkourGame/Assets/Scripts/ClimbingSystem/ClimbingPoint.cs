using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ClimbingPoint : MonoBehaviour
{
    public bool MountPoint;
    public List<Neighbour> neighbours;

    private void Awake()
    {
        var twoWayClimbNeighbour = neighbours.Where(n => n.isPointTwoWay);
        foreach (var neighbour in twoWayClimbNeighbour)
        {
            neighbour.climbingPoint?.CreatePointConnection(this, -neighbour.pointDirection, neighbour.connetionType, neighbour.isPointTwoWay);
        }
    }

    public void CreatePointConnection(ClimbingPoint climbingPoint, Vector2 pointDirection,
        ConnetionType connetionType, bool isPointTwoWay)
    {
        var neighbour = new Neighbour()
        {
            climbingPoint = climbingPoint,
            pointDirection = pointDirection,
            connetionType = connetionType,
            isPointTwoWay = isPointTwoWay
        };

        neighbours.Add(neighbour);
    }

    public Neighbour GetNeighbour(Vector2 climbDirection)
    {
        Neighbour neighbour = null;

        if (climbDirection.y != 0)
        {
            neighbour = neighbours.FirstOrDefault(n => n.pointDirection.y == climbDirection.y);
        }

        if (neighbour == null && climbDirection.x != 0)
        {
            neighbour = neighbours.FirstOrDefault(n => n.pointDirection.x == climbDirection.x);
        }

        return neighbour;
    }

    private void OnDrawGizmos()
    {
        Debug.DrawRay(transform.position, transform.forward, Color.blue);
        foreach (var neighbour in neighbours)
        {
            if(neighbour.climbingPoint != null)
                Debug.DrawLine(transform.position, neighbour.climbingPoint.transform.position, (neighbour.isPointTwoWay) ? Color.green : Color.black);
        }
    }
}

[System.Serializable]
public class Neighbour
{
    public ClimbingPoint climbingPoint;
    public Vector2 pointDirection;
    public ConnetionType connetionType;
    public bool isPointTwoWay = true;
}

public enum ConnetionType { Jump, Move }
