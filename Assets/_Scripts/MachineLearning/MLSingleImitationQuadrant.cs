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
        sensor.AddObservation(Actor.transform.position.x);
    }

    public override void OnActionReceived(float[] vectorAction)
    {
        if (vectorAction[0] == 1)
            gameObject.transform.localScale = new Vector3(0.5f, 2.5f, 0.5f);
        else
            gameObject.transform.localScale = new Vector3(1f, 1f, 1f);

        //EndEpisode();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.O))
            EndEpisode();
    }

    public override void OnEpisodeBegin()
    {
        gameObject.transform.localScale = new Vector3(1f, 1f, 1f);
        Actor.transform.position = new Vector3(Random.Range(-2f, 2f), 0f, Random.Range(-2f, 2f));
    }

    public override void Heuristic(float[] actionsOut)
    {
        actionsOut[0] = 0;

        if (Input.GetKey("space"))
            actionsOut[0] = 1;
    }
}
