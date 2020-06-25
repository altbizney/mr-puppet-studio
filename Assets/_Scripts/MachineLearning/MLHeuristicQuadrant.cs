using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;

public class MLHeuristicQuadrant : Agent
{
    private GameObject[] Quadrants;
    private float IdealQuadrant;
    private bool ThinkMode;
    //int KeyPress;

    public override void CollectObservations(VectorSensor sensor)
    {
        //Debug.Log("observing" + gameObject.transform.position.x);

        sensor.AddObservation(gameObject.transform.position.x);
        sensor.AddObservation(gameObject.transform.position.z);
    }

    public override void Initialize()
    {
        Quadrants = new GameObject[4];
        Quadrants[0] = GameObject.Find("QuadrantRightDown");
        Quadrants[1] = GameObject.Find("QuadrantRightUp");
        Quadrants[2] = GameObject.Find("QuadrantLeftDown");
        Quadrants[3] = GameObject.Find("QuadrantLeftUp");
        Academy.Instance.AutomaticSteppingEnabled = false;
    }

    public override void OnActionReceived(float[] vectorAction)
    {
        Debug.Log(vectorAction[0]);
        if (IdealQuadrant == vectorAction[0])
        {
            AddReward(1f);
            //EndEpisode();
        }
        else
            AddReward(-0.1f);

        for (int i = 0; i < 4; i++)
        {
            if ((int)vectorAction[0] == i)
                Quadrants[i].transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);//(Vector3.down * 180f * Time.deltaTime);
            else
                Quadrants[i].transform.localScale = new Vector3(1f, 1f, 1f);
        }
    }

    public override void OnEpisodeBegin()
    {
        ThinkMode = false;
        Academy.Instance.AutomaticSteppingEnabled = false;
        transform.localPosition = new Vector3(0f, gameObject.transform.localPosition.y, 0f);
        foreach (GameObject q in Quadrants)
        {
            q.GetComponent<Renderer>().transform.localScale = new Vector3(1f, 1f, 1f);
        }
        IdealQuadrant = Random.Range(0, 4);
    }

    void FixedUpdate()
    {
        //if (ThinkMode)
        //{
        Academy.Instance.EnvironmentStep();
        RequestDecision();
        //EndEpisode();
        //}
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.A))
            transform.position += (transform.right * 1f) * Time.deltaTime / 2f;

        if (Input.GetKey(KeyCode.D))
            transform.position += (transform.right * -1f) * Time.deltaTime / 2f;

        if (Input.GetKey(KeyCode.S))
            transform.position += transform.forward * Time.deltaTime / 2f;

        if (Input.GetKey(KeyCode.W))
            transform.position += (transform.forward * -1f) * Time.deltaTime / 2f;

        if (Input.GetKey("space"))
        {
            if (!ThinkMode)
                ThinkMode = true;
        }
    }

    /*
public override void CollectDiscreteActionMasks(DiscreteActionMasker actionMasker)
{
    if (IdealQuadrant == 0)
        actionMasker.SetMask(0, new int[3] { 1, 2, 3 });
    else if (IdealQuadrant == 1)
        actionMasker.SetMask(0, new int[3] { 0, 2, 3 });
    else if (IdealQuadrant == 2)
        actionMasker.SetMask(0, new int[3] { 1, 0, 3 });
    else if (IdealQuadrant == 3)
        actionMasker.SetMask(0, new int[3] { 1, 2, 0 });

    IdealQuadrant = 999999999;
}
*/

    /*
        public override void Heuristic(float[] actionsOut)
        {
            actionsOut[0] = 0;

            //if (Input.GetKey(KeyCode.I)) l
            //actionsOut[0] = 1;
            //if (Input.GetKey(KeyCode.O))
            //actionsOut[0] = 2;
            // if (Input.GetKey(KeyCode.K))
            //actionsOut[0] = 3;
            if (Input.GetKey(KeyCode.L))
            {
                //RequestDecision();
            }
        }
    */

}
