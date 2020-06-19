using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DotProduct : MonoBehaviour
{
    public GameObject GoalNorth;
    public GameObject GoalSouth;
    public GameObject GoalEast;
    public GameObject GoalWest;

    public GameObject PillarNorth;
    public GameObject PillarSouth;
    public GameObject PillarEast;
    public GameObject PillarWest;

    public GameObject PillarNorthForward;
    public GameObject PillarSouthForward;
    public GameObject PillarEastForward;
    public GameObject PillarWestForward;

    public GameObject PillarNorthRight;
    public GameObject PillarSouthRight;
    public GameObject PillarEastRight;
    public GameObject PillarWestRight;

    void Update()
    {
        float dotNorthForward = Vector3.Dot(transform.forward, GoalNorth.transform.forward);
        float dotSouthForward = Vector3.Dot(transform.forward, GoalSouth.transform.forward);
        float dotEastForward = Vector3.Dot(transform.forward, GoalEast.transform.forward);
        float dotWestForward = Vector3.Dot(transform.forward, GoalWest.transform.forward);

        dotNorthForward = Remap(dotNorthForward, -1f, 1f, 0f, 1f);
        dotSouthForward = Remap(dotSouthForward, -1f, 1f, 0f, 1f);
        dotEastForward = Remap(dotEastForward, -1f, 1f, 0f, 1f);
        dotWestForward = Remap(dotWestForward, -1f, 1f, 0f, 1f);

        float dotNorthRight = Vector3.Dot(transform.right, GoalNorth.transform.right);
        float dotSouthRight = Vector3.Dot(transform.right, GoalSouth.transform.right);
        float dotEastRight = Vector3.Dot(transform.right, GoalEast.transform.right);
        float dotWestRight = Vector3.Dot(transform.right, GoalWest.transform.right);

        dotNorthRight = Remap(dotNorthRight, -1f, 1f, 0f, 1f);
        dotSouthRight = Remap(dotSouthRight, -1f, 1f, 0f, 1f);
        dotEastRight = Remap(dotEastRight, -1f, 1f, 0f, 1f);
        dotWestRight = Remap(dotWestRight, -1f, 1f, 0f, 1f);

        float dotNorthUp = Vector3.Dot(transform.up, GoalNorth.transform.up);
        float dotSouthUp = Vector3.Dot(transform.up, GoalSouth.transform.up);
        float dotEastUp = Vector3.Dot(transform.up, GoalEast.transform.up);
        float dotWestUp = Vector3.Dot(transform.up, GoalWest.transform.up);

        dotNorthUp = Remap(dotNorthUp, -1f, 1f, 0f, 1f);
        dotSouthUp = Remap(dotSouthUp, -1f, 1f, 0f, 1f);
        dotEastUp = Remap(dotEastUp, -1f, 1f, 0f, 1f);
        dotWestUp = Remap(dotWestUp, -1f, 1f, 0f, 1f);

        PillarNorth.transform.localScale = new Vector3(PillarNorth.transform.localScale.x, dotNorthUp * 2f, PillarNorth.transform.localScale.x);
        PillarSouth.transform.localScale = new Vector3(PillarNorth.transform.localScale.x, dotSouthUp * 2f, PillarNorth.transform.localScale.x);
        PillarEast.transform.localScale = new Vector3(PillarNorth.transform.localScale.x, dotEastUp * 2f, PillarNorth.transform.localScale.x);
        PillarWest.transform.localScale = new Vector3(PillarNorth.transform.localScale.x, dotWestUp * 2f, PillarNorth.transform.localScale.x);

        PillarNorthRight.transform.localScale = new Vector3(PillarNorth.transform.localScale.x, dotNorthRight * 2f, PillarNorth.transform.localScale.x);
        PillarSouthRight.transform.localScale = new Vector3(PillarNorth.transform.localScale.x, dotSouthRight * 2f, PillarNorth.transform.localScale.x);
        PillarEastRight.transform.localScale = new Vector3(PillarNorth.transform.localScale.x, dotEastRight * 2f, PillarNorth.transform.localScale.x);
        PillarWestRight.transform.localScale = new Vector3(PillarNorth.transform.localScale.x, dotWestRight * 2f, PillarNorth.transform.localScale.x);

        PillarNorthForward.transform.localScale = new Vector3(PillarNorth.transform.localScale.x, dotNorthForward * 2f, PillarNorth.transform.localScale.x);
        PillarSouthForward.transform.localScale = new Vector3(PillarNorth.transform.localScale.x, dotSouthForward * 2f, PillarNorth.transform.localScale.x);
        PillarEastForward.transform.localScale = new Vector3(PillarNorth.transform.localScale.x, dotEastForward * 2f, PillarNorth.transform.localScale.x);
        PillarWestForward.transform.localScale = new Vector3(PillarNorth.transform.localScale.x, dotWestForward * 2f, PillarNorth.transform.localScale.x);

        //DebugGraph.MultiLog("Scores " + gameObject.GetInstanceID(), Color.white, dotNorth, "North");
        //DebugGraph.MultiLog("Scores " + gameObject.GetInstanceID(), Color.green, dotSouth, "South");
        //DebugGraph.MultiLog("Scores " + gameObject.GetInstanceID(), Color.blue, dotEast, "East");
        //DebugGraph.MultiLog("Scores " + gameObject.GetInstanceID(), Color.red, dotWest, "West");
    }


    private float Remap(float value, float from1, float to1, float from2, float to2)
    {
        return Mathf.Clamp((value - from1) / (to1 - from1) * (to2 - from2) + from2, from2, to2);
    }
}
