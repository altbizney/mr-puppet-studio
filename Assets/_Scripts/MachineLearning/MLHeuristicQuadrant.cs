using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;

public class MLHeuristicQuadrant : Agent
{
    public List<GameObject> TouchingQuadrants = new List<GameObject>();

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(gameObject.transform.position.x);
        sensor.AddObservation(gameObject.transform.position.z);
    }

    public override void OnActionReceived(float[] vectorAction)
    {
        if (TouchingQuadrants.Count > 1)
        {
            SetReward(-1f);
        }
        else
        {
            if (vectorAction[0] == 1 && TouchingQuadrants[0].name == "QuadrantRightUp")
                SetReward(1f);
            else if (vectorAction[0] == 2 && TouchingQuadrants[0].name == "QuadrantRightDown")
                SetReward(1f);
            else if (vectorAction[0] == 3 && TouchingQuadrants[0].name == "QuadrantLeftUp")
                SetReward(1f);

            else if (vectorAction[0] == 4 && TouchingQuadrants[0].name == "QuadrantLeftDown")
                SetReward(1f);
            else
                SetReward(-1f);
        }

    }

    private void OnCollisionEnter(Collision Quadrant)
    {
        TouchingQuadrants.Add(Quadrant.gameObject);
    }

    void OnCollisionExit(Collision Quadrant)
    {
        TouchingQuadrants.Remove(Quadrant.gameObject);
    }

    public override void Heuristic(float[] actionsOut)
    {
        //actionsOut[0] = 0;

        if (Input.GetKey(KeyCode.I))
            actionsOut[0] = 1;
        if (Input.GetKey(KeyCode.O))
            actionsOut[0] = 2;
        if (Input.GetKey(KeyCode.K))
            actionsOut[0] = 3;
        if (Input.GetKey(KeyCode.L))
            actionsOut[0] = 4;
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.A))
            transform.Rotate(Vector3.down * 90f * Time.deltaTime);

        if (Input.GetKey(KeyCode.D))
            transform.Rotate(Vector3.up * 90f * Time.deltaTime);

        if (Input.GetKey(KeyCode.S))
            transform.position += transform.forward * Time.deltaTime * 4f;

        if (Input.GetKey(KeyCode.W))
            transform.position += (transform.forward * -1f) * Time.deltaTime * 4f;
    }
}
