using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;

public class MLRotation : Agent
{
    public GameObject Goal;
    private float MyScore;
    private float RealScore;
    private float RealTimeScore;
    public GameObject Pillar;


    public override void Initialize()
    {
        //SetResetParameters();
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(gameObject.transform.rotation);
        sensor.AddObservation(RealScore);

        if (Goal != null)
            sensor.AddObservation(Goal.transform.rotation);
    }

    private void Update()
    {
        if (Input.GetKeyDown("space"))
            gameObject.transform.localRotation = Goal.transform.localRotation;
    }

    public override void OnActionReceived(float[] vectorAction)
    {
        //rotation should be same as localRotation when parented to an object?
        float Delta = Quaternion.Angle(gameObject.transform.rotation, Goal.transform.rotation);
        RealScore = Remap(Delta, 0f, 180f, 0f, 1f);
        MyScore = vectorAction[0];

        float MyReward = Mathf.Abs(Mathf.Abs(MyScore - RealScore) - 1f);
        Debug.Log(MyScore + " " + RealScore + " " + MyReward);

        if (MyScore < 0)
        {
            SetReward(-0.5f);
            //EndEpisode();
        }

        if (MyReward > 0.7f)
            SetReward(MyReward);
        else
            SetReward(-0.3f);

        Pillar.transform.localScale = new Vector3(Pillar.transform.localScale.x, MyScore * 2f, Pillar.transform.localScale.x);

        //DebugGraph.MultiLog("Scores " + gameObject.GetInstanceID(), Color.white, RealScore, "RealScore");
        //DebugGraph.MultiLog("Scores " + gameObject.GetInstanceID(), Color.green, MyScore, "MyScore");

        //EndEpisode();
    }

    private float Remap(float value, float from1, float to1, float from2, float to2)
    {
        return Mathf.Clamp((value - from1) / (to1 - from1) * (to2 - from2) + from2, from2, to2);
    }

    public override void OnEpisodeBegin()
    {
        //gameObject.transform.rotation = new Quaternion(0f, 0f, 0f, 0f);
        //gameObject.transform.Rotate(new Vector3(1f, 0f, 0f), Random.Range(-180f, 180f));
        //gameObject.transform.Rotate(new Vector3(0f, 0f, 1f), Random.Range(-180f, 180f));
        //gameObject.transform.Rotate(new Vector3(0f, 1f, 0f), Random.Range(-180f, 180f));

        //Goal.transform.rotation = new Quaternion(0f, 0f, 0f, 0f);
        //Goal.transform.Rotate(new Vector3(1f, 0f, 0f), Random.Range(-180f, 180f));
        //Goal.transform.Rotate(new Vector3(0f, 0f, 1f), Random.Range(-180f, 180f));
        //Goal.transform.Rotate(new Vector3(0f, 1f, 0f), Random.Range(-180f, 180f));
        //Should I be doing something inside of init?
    }
}
