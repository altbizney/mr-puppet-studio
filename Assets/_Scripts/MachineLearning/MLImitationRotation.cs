using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;

public class MLImitationRotation : Agent
{
    public GameObject Actor;
    private int Episodes;
    private Quaternion TargetRotation;


    public override void Initialize()
    {
        TargetRotation = Random.rotation;
        Episodes = 0;

        if (!PlayerPrefs.HasKey("ImitationRotationX"))
        {
            PlayerPrefs.SetFloat("ImitationRotationX", TargetRotation.eulerAngles.x);
            PlayerPrefs.SetFloat("ImitationRotationY", TargetRotation.eulerAngles.y);
            PlayerPrefs.SetFloat("ImitationRotationZ", TargetRotation.eulerAngles.z);
        }
    }

    public override void CollectObservations(VectorSensor sensor)
    {

        sensor.AddObservation(Vector3.Dot(gameObject.transform.forward, Actor.transform.forward));
        sensor.AddObservation(Vector3.Dot(gameObject.transform.up, Actor.transform.up));
        sensor.AddObservation(Vector3.Dot(gameObject.transform.right, Actor.transform.right));
        //sensor.AddObservation(gameObject.transform.localRotation);
        //sensor.AddObservation(Actor.transform.localRotation);

        //float QuaternionAngle = Quaternion.Angle(Actor.transform.localRotation, gameObject.transform.localRotation) / 180;
        //sensor.AddObservation(QuaternionAngle);
        //Debug.Log("Observe");
    }

    public override void OnActionReceived(float[] vectorAction)
    {
        if (vectorAction[0] == 1)
            gameObject.transform.GetChild(0).gameObject.transform.localScale = new Vector3(0.08f, 0.24f, 0.08f);
        else
            gameObject.transform.GetChild(0).gameObject.transform.localScale = new Vector3(0.08f, 0.11f, 0.08f);

        //Debug.Log(vectorAction[0]);
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
        //Episodes += 1;
        //Debug.Log("episodes " + Episodes);


        //Academy.Instance.AutomaticSteppingEnabled = false;
        Vector3 SavedRotation = new Vector3(PlayerPrefs.GetFloat("ImitationRotationX"), PlayerPrefs.GetFloat("ImitationRotationY"), PlayerPrefs.GetFloat("ImitationRotationZ"));
        gameObject.transform.rotation = Quaternion.Euler(SavedRotation);

        Actor.transform.localRotation = Random.rotation;
        gameObject.transform.GetChild(0).gameObject.transform.localScale = new Vector3(0.08f, 0.18f, 0.08f);
    }

    public override void Heuristic(float[] actionsOut)
    {
        actionsOut[0] = 0;

        if (Input.GetKey("space"))
            actionsOut[0] = 1;
    }
}
