using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;

public class MLHeuristicTraining : Agent
{
    public GameObject Goal;
    private float MyScore;
    private float RealScore;

    public override void Initialize()
    {
        //SetResetParameters();
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(gameObject.transform.position);
        sensor.AddObservation(Goal.transform.position);

        sensor.AddObservation(RealScore);
    }


    public override void OnActionReceived(float[] vectorAction)
    {
        Vector3 GoalVector = new Vector3(gameObject.transform.position.x, 0f, gameObject.transform.position.z);
        Vector3 MyVector = new Vector3(Goal.transform.position.x, 0f, Goal.transform.position.z);
        float MyReward;

        RealScore = Vector3.Distance(GoalVector, MyVector);
        RealScore = Remap(RealScore, 0f, 8.485281f, 0f, 1f);

        MyScore = vectorAction[0];

        if (MyScore < 0)
        {
            SetReward(-0.3f);
            EndEpisode();
        }

        if (RealScore == MyScore)
            MyReward = 1f;
        else if (Mathf.Abs(RealScore - MyScore) < 0.01f)
            MyReward = 0.5f;
        else if (Mathf.Abs(RealScore - MyScore) < 0.08f)
            MyReward = 0.3f;
        else if (Mathf.Abs(RealScore - MyScore) < 0.1f)
            MyReward = 0.1f;
        else
            MyReward = -0.2f;

        //DebugGraph.MultiLog("Scores ", Color.white, RealScore, "RealScore");
        //DebugGraph.MultiLog("Scores ", Color.green, MyScore, "MyScore");
        //DebugGraph.MultiLog("Scores ", Color.blue, MyReward, "Reward");
        //Debug.Log(RealScore + " " + MyScore);// + " " + MyReward);

        SetReward(MyReward);
        EndEpisode();
    }

    private float Remap(float value, float from1, float to1, float from2, float to2)
    {
        return Mathf.Clamp((value - from1) / (to1 - from1) * (to2 - from2) + from2, from2, to2);
    }

    /*
        public override void Heuristic(float[] actionsOut)
        {
            //put movement in here

            if (Input.GetKey(KeyCode.D))
            {
                transform.position += new Vector3(3f * Time.deltaTime, 0, 0);
                Debug.Log("Pressed D");
            }

            if (Input.GetKey(KeyCode.A))
                transform.position += new Vector3(-3f * Time.deltaTime, 0, 0);

        }
    */

    public override void OnEpisodeBegin()
    {
        //Should I be doing something inside of init?
        transform.localPosition = new Vector3(Random.Range(-3f, 3f), gameObject.transform.localPosition.y, Random.Range(-3f, 3f));
        Goal.transform.localPosition = new Vector3(Random.Range(-3f, 3f), Goal.transform.localPosition.y, Random.Range(-3f, 3f));
    }
}
