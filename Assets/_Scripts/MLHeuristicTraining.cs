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
        //send score
        //send my pos and goal pose
        //does goal pos just stay the same?
        sensor.AddObservation(gameObject.transform.position);
        sensor.AddObservation(Goal.transform.position);

        //observe score somehow
        sensor.AddObservation(RealScore);
    }


    public override void OnActionReceived(float[] vectorAction)
    {
        RealScore = Mathf.Abs(gameObject.transform.position.x - Goal.transform.position.x);
        RealScore = Remap(RealScore, 0f, 6f, 0f, 1f);

        MyScore = vectorAction[0];
        float MyReward;

        if (MyScore < 0)
        {
            SetReward(-0.3f);
            EndEpisode();
        }

        if (Mathf.Abs(RealScore - MyScore) < 0.2f)
        {
            MyReward = 0.1f;//SetReward(0.3f);
        }
        else if (Mathf.Abs(RealScore - MyScore) < 0.1f)
            MyReward = 0.3f; //SetReward(0.7f);
        else if (Mathf.Abs(RealScore - MyScore) < 0.05f)
            MyReward = 0.5f; //SetReward(0.7f);
        else if (RealScore == MyScore)
            MyReward = 1f; //SetReward(1f);
        else
            MyReward = -0.2f; //SetReward(-0.2f);


        //DebugGraph.MultiLog("Scores ", Color.white, RealScore, "RealScore");
        //DebugGraph.MultiLog("Scores ", Color.green, MyScore, "MyScore");
        //DebugGraph.MultiLog("Scores ", Color.blue, MyReward, "Reward");
        Debug.Log(RealScore + " " + MyScore);// + " " + MyReward);

        SetReward(MyReward);
        EndEpisode();

        // How correct was your score? if 0==0 give 1, if 0.5==0.5 give 1. if 1==0 give 0;
    }

    private float Remap(float value, float from1, float to1, float from2, float to2)
    {
        return Mathf.Clamp((value - from1) / (to1 - from1) * (to2 - from2) + from2, from2, to2);
    }

    /*
        public override void Heuristic(float[] actionsOut)
        {
            //put movement in here
            //left right

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
        //random local of agent. or would that be in init?
        transform.localPosition = new Vector3(Random.Range(-3f, 3f), gameObject.transform.localPosition.y, 0f);
    }
}
