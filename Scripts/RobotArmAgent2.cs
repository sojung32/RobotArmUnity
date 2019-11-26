using System;
using UnityEngine;
using MLAgents;
using System.Text;

public class RobotArmAgent2 : Agent
{
    public GameObject floor;
    public GameObject body;
    public GameObject shoulder;
    public Transform shoulderJoint;
    public GameObject elbow;
    public GameObject wrist;
    public GameObject target;
    public GameObject angle;
    public Transform cube;
    public GameObject end;

    private RobotArmAcademy2 raAacademy;

    private Rigidbody fbRigid;
    private Rigidbody bsRigid;
    private Rigidbody seRigid;
    private Rigidbody ewRigid;

    Vector3 targetVec;
    float distToTarget;
    int underground = 0;
    public int touchTarget = 0;
    
    Quaternion bodyq, quat;

    public float angleBD, angleBS, angleSE, angleEW;

    public override void InitializeAgent()
    {
        fbRigid = body.GetComponent<Rigidbody>();
        bsRigid = shoulder.GetComponent<Rigidbody>();
        seRigid = elbow.GetComponent<Rigidbody>();
        ewRigid = wrist.GetComponent<Rigidbody>();
        raAacademy = GameObject.Find("Academy").GetComponent<RobotArmAcademy2>();

        MakeRandomTarget();
    }

    public override void CollectObservations()
    {
        AddVectorObs(body.transform.localPosition);
        AddVectorObs(body.transform.rotation.y);
        AddVectorObs(body.transform.eulerAngles.y);
	AddVectorObs(fbRigid.angularVelocity);
	AddVectorObs(fbRigid.velocity);
        
        AddVectorObs(shoulder.transform.localPosition);
        AddVectorObs(shoulder.transform.rotation.x);
        AddVectorObs(shoulder.transform.eulerAngles.x);
	AddVectorObs(bsRigid.angularVelocity);
	AddVectorObs(bsRigid.velocity);
        
        AddVectorObs(elbow.transform.localPosition);
        AddVectorObs(elbow.transform.rotation.x);
        AddVectorObs(elbow.transform.eulerAngles.x);
	AddVectorObs(seRigid.angularVelocity);
	AddVectorObs(seRigid.velocity);
        
        AddVectorObs(wrist.transform.localPosition);
        AddVectorObs(wrist.transform.rotation.x);
        AddVectorObs(wrist.transform.eulerAngles.x);
	AddVectorObs(ewRigid.angularVelocity);
	AddVectorObs(ewRigid.velocity);
        
        AddVectorObs(target.transform.localPosition);
        AddVectorObs(end.transform.localPosition);

	AddVectorObs(touchTarget);
	AddVectorObs(Vector3.Distance(target.transform.position, end.transform.position));


        //AddVectorObs(angleBD);
        //AddVectorObs(angleBS);
        //AddVectorObs(angleSE);
        //AddVectorObs(angleEW);
    }

    public override void AgentAction(float[] vectorAction, string textAction)
    {
        var torqueX = Mathf.Clamp(vectorAction[0], -1f, 1f) * 150f;
        fbRigid.AddTorque(new Vector3(0f, torqueX, 0f));
        torqueX = Mathf.Clamp(vectorAction[1], -1f, 1f) * 150f;
        bsRigid.AddTorque(new Vector3(torqueX, 0f, 0f));
        torqueX = Mathf.Clamp(vectorAction[2], -1f, 1f) * 150f;
        seRigid.AddTorque(new Vector3(torqueX, 0f, 0f));
        torqueX = Mathf.Clamp(vectorAction[3], -1f, 1f) * 150f;
        ewRigid.AddTorque(new Vector3(torqueX, 0f, 0f));

        distToTarget = Vector3.Distance(target.transform.position, end.transform.position);
        if (distToTarget <= 1.0f)
        {
            AddReward(0.001f / distToTarget);
        }
        if (distToTarget > 5.0f)
        {
            AddReward(-0.00001f * distToTarget * distToTarget);
        }
        if (end.transform.position.y < 0)
        {
            AddReward(-1.0f);
            underground = 1;
            Done();
        }
        //angleBD = angle.transform.rotation.y - cube.rotation.y;
        //angleBS = shoulder.transform.rotation.x;
        //angleSE = Quaternion.Angle(bsRigid.rotation, seRigid.rotation);
        //angleEW = Quaternion.Angle(seRigid.rotation, ewRigid.rotation);
        //Debug.Log(shoulder.transform.rotation.x);
    }

    public void MakeRandomTarget()
    {
        targetVec.Set(UnityEngine.Random.Range(-10.5f, 10.5f), UnityEngine.Random.Range(2.0f, 12.5f), UnityEngine.Random.Range(-10.5f, -3.0f));
        target.transform.position = targetVec;

        while (Vector3.Distance(target.transform.position, shoulderJoint.position) > 10.5f)
        {
            targetVec.Set(UnityEngine.Random.Range(-10.5f, 10.5f), UnityEngine.Random.Range(2.0f, 12.5f), UnityEngine.Random.Range(-10.5f, -3.0f));
            target.transform.position = targetVec;
        }
    }

    public override void AgentReset()
    {
        if (underground == 1)
        {
            wrist.transform.position += Vector3.up;
            wrist.transform.position += Vector3.up;
            underground = 0;
        }
        if (touchTarget == 1)
        {
            touchTarget = 0;
        }
        
        fbRigid.velocity = Vector3.zero;
        bsRigid.velocity = Vector3.zero;
        seRigid.velocity = Vector3.zero;
        ewRigid.velocity = Vector3.zero;

        fbRigid.angularVelocity = Vector3.zero;
        bsRigid.angularVelocity = Vector3.zero;
        seRigid.angularVelocity = Vector3.zero;
        ewRigid.angularVelocity = Vector3.zero;
        
        /*bodyq.Set(0f,1f,0f,1);
        body.transform.rotation = bodyq;
        
        quat.Set(0f,0f,0f,1);
        shoulder.transform.rotation = quat;
        elbow.transform.rotation = quat;
        wrist.transform.rotation = quat;*/
    }

    public override float[] Heuristic()
    {
        var action = new float[4];
        action[0] = Input.GetAxis("A");
        action[1] = Input.GetAxis("S");
        action[2] = Input.GetAxis("D");
        action[3] = Input.GetAxis("F");
        return action;
    }

}
