using System;
using UnityEngine;
using MLAgents;

public class RobotArmAgent2 : Agent
{
    public GameObject floor;
    public GameObject body;
    public GameObject shoulder;
    public GameObject elbow;
    public GameObject wrist;
    public GameObject target;
    public GameObject end;
    public Collider endOfWrist;

    private RobotArmAcademy2 raAacademy;

    private Rigidbody fbRigid;
    private Rigidbody bsRigid;
    private Rigidbody seRigid;
    private Rigidbody ewRigid;
    private Rigidbody weRigid;

    Vector3 targetVec;
    Vector3 wristpos;
    float distToTarget;
    float distToInit;

    public override void InitializeAgent()
    {
        wristpos = endOfWrist.transform.position;
        MakeRandomTarget();
        fbRigid = body.GetComponent<Rigidbody>();
        bsRigid = shoulder.GetComponent<Rigidbody>();
        seRigid = elbow.GetComponent<Rigidbody>();
        ewRigid = wrist.GetComponent<Rigidbody>();
        weRigid = wrist.GetComponent<Rigidbody>();
        raAacademy = GameObject.Find("Academy").GetComponent<RobotArmAcademy2>();
    }

    public override void CollectObservations()
    {
	AddVectorObs(body.transform.rotation);
	AddVectorObs(fbRigid.angularVelocity);
	AddVectorObs(fbRigid.velocity);
	
	AddVectorObs(shoulder.transform.localPosition);
	AddVectorObs(shoulder.transform.rotation);
	AddVectorObs(bsRigid.angularVelocity);
	AddVectorObs(bsRigid.velocity);
	
	AddVectorObs(elbow.transform.localPosition);
	AddVectorObs(elbow.transform.rotation);
	AddVectorObs(seRigid.angularVelocity);
	AddVectorObs(seRigid.velocity);
	
	AddVectorObs(wrist.transform.localPosition);
	AddVectorObs(wrist.transform.rotation);
	AddVectorObs(ewRigid.angularVelocity);
	AddVectorObs(ewRigid.velocity);
	
	AddVectorObs(target.transform.localPosition);
    }

    public override void AgentAction(float[] vectorAction, string textAction)
    {
    	var torqueX = Mathf.Clamp(vectorAction[0], -1f, 1f) * 50f;
    	fbRigid.AddTorque(new Vector3(0f, torqueX, 0f));
    	torqueX = Mathf.Clamp(vectorAction[1], -1f, 1f) * 50f;
    	bsRigid.AddTorque(new Vector3(torqueX, 0f, 0f));
    	torqueX = Mathf.Clamp(vectorAction[2], -1f, 1f) * 50f;
    	seRigid.AddTorque(new Vector3(torqueX, 0f, 0f));
    	torqueX = Mathf.Clamp(vectorAction[3], -1f, 1f) * 50f;
    	ewRigid.AddTorque(new Vector3(torqueX, 0f, 0f));
        
        distToTarget = Vector3.Distance(target.transform.position, endOfWrist.transform.position);
        distToInit = Vector3.Distance(wristpos, endOfWrist.transform.position);
        if(distToTarget < 2.0f){
            AddReward(0.0001f*distToTarget);
        }
        if(distToTarget > 10f)
        {
            AddReward(-0.0001f*distToTarget);
        }
        if(distToInit < 0.1f){
            AddReward(-0.01f);
        }
        if(endOfWrist.transform.position.y<0){
            AddReward(-1.0f);
            Done();
        }
    }

    public void MakeRandomTarget()
    {
        targetVec.Set(UnityEngine.Random.Range(-4.0f, 3.9f), UnityEngine.Random.Range(4.5f, 8.4f), -7.2f);

        target.transform.position = targetVec;
    }

    public override void AgentReset()
    {
        //body.transform.rotation = Quaternion.Euler(180f, 0f, 0f);
        //shoulder.transform.rotation = Quaternion.Euler(180f, 0f, 0f);
        //elbow.transform.rotation = Quaternion.Euler(180f, 0f, 0f);
        //wrist.transform.rotation = Quaternion.Euler(180f, 0f, 0f);
        
        wristpos = endOfWrist.transform.position;

        endOfWrist.transform.position += Vector3.up;
        endOfWrist.transform.position += Vector3.up;

        fbRigid.velocity = Vector3.zero;
        bsRigid.velocity = Vector3.zero;
        seRigid.velocity = Vector3.zero;
        ewRigid.velocity = Vector3.zero;
        weRigid.velocity = Vector3.zero;

        fbRigid.angularVelocity = Vector3.zero;
        bsRigid.angularVelocity = Vector3.zero;
        seRigid.angularVelocity = Vector3.zero;
        ewRigid.angularVelocity = Vector3.zero;
        weRigid.angularVelocity = Vector3.zero;
    }
    
    public override float[] Heuristic(){
    	var action = new float[4];
    	action[0] = Input.GetAxis("A");
    	action[1] = Input.GetAxis("S");
    	action[2] = Input.GetAxis("D");
    	action[3] = Input.GetAxis("F");
    	return action;
    }

}