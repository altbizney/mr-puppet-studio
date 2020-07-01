using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;

public class MLSingleImitationQuadrant : Agent
{
    Vector3 Target;
    public GameObject Actor;

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(gameObject.transform.position.x);
        sensor.AddObservation(gameObject.transform.position.z);
        sensor.AddObservation(Actor.transform.position.x);
        sensor.AddObservation(Actor.transform.position.z);
    }

    public override void OnActionReceived(float[] vectorAction)
    {
        if (vectorAction[0] == 1)
            gameObject.transform.localScale = new Vector3(0.5f, 4.5f, 0.5f);
        else
            gameObject.transform.localScale = new Vector3(1f, 1f, 1f);

        //if (vectorAction[0] == 1)
        //    AddReward
        //Debug.Log(vectorAction[0]);

        //EndEpisode();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.O))
            EndEpisode();
    }

    public override void OnEpisodeBegin()
    {
        Academy.Instance.AutomaticSteppingEnabled = false;
        gameObject.transform.localScale = new Vector3(1f, 1f, 1f);
        //gameObject.transform.position = new Vector3(Random.Range(-10f, 10f), 0f, Random.Range(-10f, 10f));
        //Actor.transform.position = new Vector3(Random.Range(-10f, 10f), 0f, Random.Range(-10f, 10f));
        //Actor.transform.position = new Vector3(-2f, 0f, -2f);

    }

    public override void Heuristic(float[] actionsOut)
    {
        actionsOut[0] = 0;

        if (Input.GetKey("space"))
            actionsOut[0] = 1;
    }
}
