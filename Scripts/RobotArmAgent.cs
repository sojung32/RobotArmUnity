using UnityEngine;
using MLAgents;

public class RobotArmAgent : Agent
{
    public GameObject floor;
    public GameObject body;
    public GameObject shoulder;
    public GameObject elbow;
    public GameObject wrist;
    public GameObject target;
    public Collider endOfWrist;

    private RobotArmAcademy raAacademy;

    private Rigidbody fbRigid;
    private Rigidbody bsRigid;
    private Rigidbody seRigid;
    private Rigidbody ewRigid;

    Vector3 targetVec;
    float distToTarget;

    public override void InitializeAgent()
    {
        fbRigid = body.GetComponent<Rigidbody>();
        bsRigid = shoulder.GetComponent<Rigidbody>();
        seRigid = elbow.GetComponent<Rigidbody>();
        ewRigid = wrist.GetComponent<Rigidbody>();
        raAacademy = GameObject.Find("Academy").GetComponent<RobotArmAcademy>();
    }

    public override void CollectObservations()
    {
        AddVectorObs(body.transform.rotation.y);
        AddVectorObs(shoulder.transform.rotation.x);
        AddVectorObs(elbow.transform.rotation.x);
        AddVectorObs(wrist.transform.rotation.x);
        AddVectorObs(target.transform.localPosition);

        AddVectorObs(fbRigid.angularVelocity);
        AddVectorObs(fbRigid.velocity);
        AddVectorObs(bsRigid.angularVelocity);
        AddVectorObs(bsRigid.velocity);
        AddVectorObs(seRigid.angularVelocity);
        AddVectorObs(seRigid.velocity);
        AddVectorObs(ewRigid.angularVelocity);
        AddVectorObs(ewRigid.velocity);
    }

    public override void AgentAction(float[] vectorAction, string textAction)
    {
        if (wrist.transform.position.y < 0.5 || elbow.transform.position.y < 0)
        {
            AddReward(-0.05f);
        }

        var torqueX = Mathf.Clamp(vectorAction[0], -1f, 1f) * 100f;
        fbRigid.AddTorque(new Vector3(0f, torqueX, 0f));
        torqueX = Mathf.Clamp(vectorAction[1], -1f, 1f) * 100f;
        bsRigid.AddTorque(new Vector3(torqueX, 0f, 0f));
        torqueX = Mathf.Clamp(vectorAction[2], -1f, 1f) * 100f;
        seRigid.AddTorque(new Vector3(torqueX, 0f, 0f));
        torqueX = Mathf.Clamp(vectorAction[3], -1f, 1f) * 100f;
        ewRigid.AddTorque(new Vector3(torqueX, 0f, 0f));

        distToTarget = Vector3.Distance(target.transform.position, endOfWrist.transform.position);
        if(distToTarget < 1.0f)
        {
            AddReward(0.005f);
        }
        if(distToTarget > 6.0f)
        {
            AddReward(-0.005f);
        }
    }

    private void OnTriggerEnter(Collider coll)
    {
        if (coll.gameObject == wrist)
        {
            AddReward(1.0f);
            MakeRandomTarget();
        }
    }

    void MakeRandomTarget()
    {
        targetVec.Set(Random.Range(-4f, 4f), Random.Range(1f, 7f), -6);

        target.transform.position = targetVec;
    }

    public override void AgentReset()
    {
        //body.transform.rotation = Quaternion.Euler(180f, 0f, 0f);
        //shoulder.transform.rotation = Quaternion.Euler(180f, 0f, 0f);
        //elbow.transform.rotation = Quaternion.Euler(180f, 0f, 0f);
        //wrist.transform.rotation = Quaternion.Euler(180f, 0f, 0f);

        fbRigid.velocity = Vector3.zero;
        bsRigid.velocity = Vector3.zero;
        seRigid.velocity = Vector3.zero;
        ewRigid.velocity = Vector3.zero;

        fbRigid.angularVelocity = Vector3.zero;
        bsRigid.angularVelocity = Vector3.zero;
        seRigid.angularVelocity = Vector3.zero;
        ewRigid.angularVelocity = Vector3.zero;

        MakeRandomTarget();
    }
}
