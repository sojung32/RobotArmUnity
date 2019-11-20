using System;
using UnityEngine;
using MLAgents;
using System.Net;
using System.Net.Sockets;
using System.Text;

public class RobotArmAgent2 : Agent
{
    public GameObject floor;
    public GameObject body;
    public GameObject shoulder;
    public GameObject elbow;
    public GameObject wrist;
    public GameObject target;
    public GameObject end;
    public GameObject angle;
    public Transform cube;
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
    float targetX, targetY;
    
    public float angleBD, angleBS, angleSE, angleEW;
    
    //Socket sock;
    TcpListener server;
    TcpClient client;
    NetworkStream stream;
    Byte[] bytes = new Byte[20];
    string xy;

    public override void InitializeAgent()
    {
        wristpos = endOfWrist.transform.position;
        fbRigid = body.GetComponent<Rigidbody>();
        bsRigid = shoulder.GetComponent<Rigidbody>();
        seRigid = elbow.GetComponent<Rigidbody>();
        ewRigid = wrist.GetComponent<Rigidbody>();
        weRigid = wrist.GetComponent<Rigidbody>();
        raAacademy = GameObject.Find("Academy").GetComponent<RobotArmAcademy2>();
        
        server = new TcpListener(IPAddress.Parse("192.168.0.12"), 8080);
        server.Start();
        client = server.AcceptTcpClient();
        
        stream = client.GetStream();
        MakeRandomTarget();
    }
    
    public void SendToRos(string msg)
    {
        byte[] sendMsg = Encoding.UTF8.GetBytes(msg);
        stream.Write(sendMsg, 0, sendMsg.Length);
        Debug.Log(msg);
    }

    public override void CollectObservations()
    {
        AddVectorObs(target.transform.localPosition.x);
        AddVectorObs(target.transform.localPosition.y);

        AddVectorObs(body.transform.rotation.y);
	AddVectorObs(shoulder.transform.rotation.x);
	AddVectorObs(elbow.transform.rotation.x);
	AddVectorObs(wrist.transform.rotation.x);
	
	AddVectorObs(angleBD);
	AddVectorObs(angleBS);
	AddVectorObs(angleSE);
	AddVectorObs(angleEW);
    }

    public override void AgentAction(float[] vectorAction, string textAction)
    {
    	var torqueX = Mathf.Clamp(vectorAction[0], -1f, 1f) * 100f;
    	fbRigid.AddTorque(new Vector3(0f, torqueX, 0f));
    	torqueX = Mathf.Clamp(vectorAction[1], -1f, 1f) * 100f;
    	bsRigid.AddTorque(new Vector3(torqueX, 0f, 0f));
    	torqueX = Mathf.Clamp(vectorAction[2], -1f, 1f) * 100f;
    	seRigid.AddTorque(new Vector3(torqueX, 0f, 0f));
    	torqueX = Mathf.Clamp(vectorAction[3], -1f, 1f) * 100f;
    	ewRigid.AddTorque(new Vector3(torqueX, 0f, 0f));
        
        distToTarget = Vector3.Distance(target.transform.position, endOfWrist.transform.position);
        if(distToTarget <= 1.0f){
            AddReward(0.001f/distToTarget);
        }
        if(distToTarget > 1.0f)
        {
            AddReward(+0.0001f/distToTarget);
        }
        if (distToTarget < 9.0f)
        {
            AddReward(-0.0001f * distToTarget);
        }
        if (endOfWrist.transform.position.y < 0){
            AddReward(-1.0f);
            Done();
        }
        angleBD = angle.transform.rotation.y - cube.rotation.y;
        angleBS = shoulder.transform.rotation.x;
        angleSE = Quaternion.Angle(bsRigid.rotation, seRigid.rotation);
        angleEW = Quaternion.Angle(seRigid.rotation, ewRigid.rotation);
    }

    public void MakeRandomTarget()
    {
        
        int i;
        if ((i = stream.Read(bytes, 0, bytes.Length))!=0)
        {
            xy = System.Text.Encoding.UTF8.GetString(bytes, 0, i);
        }
        
        Debug.Log(xy);
        
        targetX = float.Parse(xy.Split('/')[0]);
        targetY = float.Parse(xy.Split('/')[1]);
        targetX = 3.55f-(targetX*7.1f/1280f);
        targetY = 9f-(targetY*4f/720f);
        //targetX = UnityEngine.Random.Range(-6.0f, 5.9f);
        //targetY = UnityEngine.Random.Range(4.5f, 9.0f);
        targetVec.Set(targetX, targetY, -5.7f);

        target.transform.position = targetVec;
        
    }

    public override void AgentReset()
    {
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
