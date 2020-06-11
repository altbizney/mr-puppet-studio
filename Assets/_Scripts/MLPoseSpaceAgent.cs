using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;

public class MLPoseSpaceAgent : Agent
{
    public GameObject Pose;
    //Rigidbody ;

    public override void Initialize()
    {
        //SetResetParameters();
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(gameObject.transform.rotation);
        sensor.AddObservation(Pose.transform.rotation);
    }

    public override void OnActionReceived(float[] vectorAction)
    {
        Vector3 RotateAmount;
        RotateAmount.x = Mathf.Clamp(vectorAction[0], -1f, 1f) * 150f;
        RotateAmount.y = Mathf.Clamp(vectorAction[1], -1f, 1f) * 150f;
        RotateAmount.z = Mathf.Clamp(vectorAction[2], -1f, 1f) * 150f;

        transform.Rotate(RotateAmount);

        if (Quaternion.Angle(transform.rotation, Pose.transform.rotation) <= 10f)
        {
            SetReward(1f);
            EndEpisode();
        }
        else
            SetReward(-0.0001f);
    }

    public override void OnEpisodeBegin()
    {
        gameObject.transform.rotation = new Quaternion(0f, 0f, 0f, 0f);
        gameObject.transform.Rotate(new Vector3(1f, 0f, 0f), Random.Range(-180f, 180f));
        gameObject.transform.Rotate(new Vector3(0f, 0f, 1f), Random.Range(-180f, 180f));
        gameObject.transform.Rotate(new Vector3(0f, 1f, 0f), Random.Range(-180f, 180f));

        Pose.transform.rotation = new Quaternion(0f, 0f, 0f, 0f);
        Pose.transform.Rotate(new Vector3(1f, 0f, 0f), Random.Range(-180f, 180f));
        Pose.transform.Rotate(new Vector3(0f, 0f, 1f), Random.Range(-180f, 180f));
        Pose.transform.Rotate(new Vector3(0f, 1f, 0f), Random.Range(-180f, 180f));
    }
}
