using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;

public class MLRotationDotProduct : Agent
{
    public GameObject Goal;
    //public GameObject Pillar;
    //public GameObject RealPillar;
    private float[] RealDelta = new float[3];
    private float[] BrainDelta = new float[3];

    /*
    public override void Initialize()
    {
        //SetResetParameters();
    }
    */

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(gameObject.transform.rotation);
        sensor.AddObservation(RealDelta[0]);
        sensor.AddObservation(RealDelta[1]);
        sensor.AddObservation(RealDelta[2]);

        if (Goal != null)
            sensor.AddObservation(Goal.transform.rotation);
    }

    public override void OnActionReceived(float[] vectorAction)
    {
        //The same rotation is 180, completely different rotation is 0
        //RealDelta[0] = Mathf.Abs(Vector3.Angle(Goal.transform.up, gameObject.transform.up) - 180f);
        //RealDelta[1] = Mathf.Abs(Vector3.Angle(gameObject.transform.forward, Goal.transform.forward) - 180f);
        //RealDelta[2] = Mathf.Abs(Vector3.Angle(gameObject.transform.right, Goal.transform.right) - 180f);

        RealDelta[0] = Vector3.Dot(transform.forward, Goal.transform.forward);
        RealDelta[1] = Vector3.Dot(transform.up, Goal.transform.up);
        RealDelta[2] = Vector3.Dot(transform.right, Goal.transform.right);

        RealDelta[0] = Remap(RealDelta[0], -1f, 1f, 0f, 1f);
        RealDelta[1] = Remap(RealDelta[1], -1f, 1f, 0f, 1f);
        RealDelta[2] = Remap(RealDelta[2], -1f, 1f, 0f, 1f);

        BrainDelta[0] = Mathf.Clamp(vectorAction[0], 0f, 1f);
        BrainDelta[1] = Mathf.Clamp(vectorAction[1], 0f, 1f);
        BrainDelta[2] = Mathf.Clamp(vectorAction[2], 0f, 1f);

        //This might be weird. You can potentially get a reward if one access is correct, but the rest arent. 
        //No way to tell them if one is right, just how close to total is. 

        float MyReward;
        for (int i = 0; i < 2; i++)
        {
            MyReward = Mathf.Abs(Mathf.Abs(RealDelta[i] - BrainDelta[i]) - 1f) / 3; // Divide by reward system by 3 to stay within the [-1,1] range
            if (MyReward > 0.25f)
                AddReward(MyReward);
            else
                AddReward(-0.15f);
        }

        EndEpisode();

        //float AverageRealScore = (RealDelta[0] + RealDelta[1] + RealDelta[2]) / 3;
        //float AverageScore = (BrainDelta[0] + BrainDelta[1] + BrainDelta[2]) / 3;

        //DebugGraph.Log(AverageScore);
        //Pillar.transform.localScale = new Vector3(Pillar.transform.localScale.x, AverageScore, Pillar.transform.localScale.z);
        //RealPillar.transform.localScale = new Vector3(Pillar.transform.localScale.x, AverageRealScore, Pillar.transform.localScale.z);

        //float my = Random.Range(0f, 1f);
        //Debug.Log(RealDelta[0] + " " + my + " " + Mathf.Abs(Mathf.Abs(RealDelta[0] - my) - 1f));
        //DebugGraph.MultiLog("Scores " + gameObject.GetInstanceID(), Color.white, RealDelta[0], "RealDelta");
        //DebugGraph.MultiLog("Scores " + gameObject.GetInstanceID(), Color.green, BrainDelta[0], "BrainDelta");
        //DebugGraph.MultiLog("Scores " + gameObject.GetInstanceID(), Color.green, (Mathf.Abs(RealDelta[0] - BrainDelta[0]) - 1f) / 3, "Reward");

    }

    private float Remap(float value, float from1, float to1, float from2, float to2)
    {
        return Mathf.Clamp((value - from1) / (to1 - from1) * (to2 - from2) + from2, from2, to2);
    }

    public override void OnEpisodeBegin()
    {
        gameObject.transform.rotation = new Quaternion(0f, 0f, 0f, 0f);
        gameObject.transform.Rotate(new Vector3(1f, 0f, 0f), Random.Range(-180f, 180f));
        gameObject.transform.Rotate(new Vector3(0f, 0f, 1f), Random.Range(-180f, 180f));
        gameObject.transform.Rotate(new Vector3(0f, 1f, 0f), Random.Range(-180f, 180f));

        Goal.transform.rotation = new Quaternion(0f, 0f, 0f, 0f);
        Goal.transform.Rotate(new Vector3(1f, 0f, 0f), Random.Range(-180f, 180f));
        Goal.transform.Rotate(new Vector3(0f, 0f, 1f), Random.Range(-180f, 180f));
        Goal.transform.Rotate(new Vector3(0f, 1f, 0f), Random.Range(-180f, 180f));
    }
}
