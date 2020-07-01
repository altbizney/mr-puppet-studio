using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;

public class MLImitationRotation : Agent
{
    public GameObject Actor;

    /*
        public override void Initialize()
        {
            //Academy.Instance.AutomaticSteppingEnabled = false;
        }
        */

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(gameObject.transform.localEulerAngles);
        sensor.AddObservation(Actor.transform.localEulerAngles);
        sensor.AddObservation(Quaternion.Angle(gameObject.transform.localRotation, Actor.transform.localRotation));
        Debug.Log(gameObject.transform.localEulerAngles + " " + Actor.transform.localEulerAngles + " " + Quaternion.Angle(gameObject.transform.localRotation, Actor.transform.localRotation));
    }

    public override void OnActionReceived(float[] vectorAction)
    {
        if (vectorAction[0] == 1)
            gameObject.transform.GetChild(0).gameObject.transform.localScale = new Vector3(0.08f, 0.24f, 0.08f);
        else
            gameObject.transform.GetChild(0).gameObject.transform.localScale = new Vector3(0.08f, 0.11f, 0.08f);

        Debug.Log(vectorAction[0]);
    }


    void FixedUpdate()
    {
        if (Input.GetKeyDown("space"))
        {
            Academy.Instance.EnvironmentStep();
            RequestDecision();
            //EndEpisode();
        }

        if (Input.GetKeyDown(KeyCode.V))
        {
            Academy.Instance.EnvironmentStep();
            RequestDecision();
            //EndEpisode();
        }

        if (Input.GetKeyDown(KeyCode.M))
        {
            EndEpisode();
        }
    }


    public override void OnEpisodeBegin()
    {
        Academy.Instance.AutomaticSteppingEnabled = false;
        gameObject.transform.GetChild(0).gameObject.transform.localScale = new Vector3(0.08f, 0.18f, 0.08f);
        gameObject.transform.localRotation = Random.rotation;
        //Actor.transform.localRotation = Random.rotation;
    }

    public override void Heuristic(float[] actionsOut)
    {
        actionsOut[0] = 0;

        if (Input.GetKey("space"))
            actionsOut[0] = 1;
    }
}
