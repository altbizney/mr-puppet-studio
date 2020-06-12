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
        MyScore = vectorAction[0];

        RealScore = Remap(RealScore, 0f, 6f, 0f, 1f);
        SetReward(Mathf.Abs((RealScore - MyScore) - 1));

        Debug.Log((Mathf.Abs((RealScore - MyScore) - 1)));

        // How correct was your score? if 0==0 give 1, if 0.5==0.5 give 1. if 1==0 give 0;

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

    /*
        you can move the dude
        left right up down

        once you are at a place you can say, this is good

                //use getkey or getaxis so not inputs get lost
        //whatever we put into actions out is recieved by the onacctions recieved method

        //do x/y do 2d positions
        //we have one buttons for not very good, -1 reward
        //we have another button for very good, +1 reward

        //what is the question and the answer

        --------------

        instead, we guess a score from action array???????
        we input pos. we output score. this is the goal. 
        find score by doing distance. if its right, give reward. if its wrong, dont give reward. 
        actually, player just outputs score. 
    */

    public override void OnEpisodeBegin()
    {
        //random local of agent. or would that be in init?
        transform.localPosition = new Vector3(Random.Range(-3f, 3f), gameObject.transform.localPosition.y, 0f);
    }
}
