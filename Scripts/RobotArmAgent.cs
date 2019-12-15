using System;
using UnityEngine;
using MLAgents;
using System.Text;
using System.Net;
using System.Net.Sockets;

public class RobotArmAgent : Agent
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

    private RobotArmAcademy raAacademy;

    private Rigidbody fbRigid;
    private Rigidbody bsRigid;
    private Rigidbody seRigid;
    private Rigidbody ewRigid;

    Vector3 targetVec;
    Vector3 vectorobs;
    float cubeDist;
    float distToTarget;
    int underground = 0;
    public int touchTarget = 0;

    public float angleBD, angleBS, angleSE, angleEW;
    float targetX, targetY, targetZ;
    TcpListener server = new TcpListener(IPAddress.Any/*IPAddress.Parse("225.0.0.222")*/, 55565);
    TcpClient client;
    //public Socket server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
    //Socket client;
    //IPEndPoint ep = new IPEndPoint(IPAddress.Any, 5555);
    NetworkStream stream;
    Byte[] bytes = new Byte[20];
    string xyz, data;
    int recv, length;
    public int socketCon = 0;

    public override void InitializeAgent()
    {
        fbRigid = body.GetComponent<Rigidbody>();
        bsRigid = shoulder.GetComponent<Rigidbody>();
        seRigid = elbow.GetComponent<Rigidbody>();
        ewRigid = wrist.GetComponent<Rigidbody>();
        raAacademy = GameObject.Find("Academy").GetComponent<RobotArmAcademy>();
        
        server.Start();
        
	client = server.AcceptTcpClient();
        stream = client.GetStream();
	socketCon = 1;
	
        MakeRandomTarget();
    }
    
    public void SendToRos(string msg)
    {
        byte[] sendMsg = Encoding.UTF8.GetBytes(msg);
        stream.Write(sendMsg, 0, sendMsg.Length);
	//client.Send(sendMsg, 0, sendMsg.Length, SocketFlags.None);
    }

    public void SocketClosing()
    {
	//client.Close();
	//server.Close();
	stream.Close();
	client.Close();
	server.Stop();
    }

    public override void CollectObservations()
    {
        AddVectorObs(body.transform.rotation.y);
        AddVectorObs(body.transform.eulerAngles.y);
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
        AddVectorObs(end.transform.localPosition);
        AddVectorObs(cube.localPosition);
	
	AddVectorObs(Vector3.Distance(target.transform.position, end.transform.position));
        
        //AddVectorObs(angleBD);
        //AddVectorObs(angleBS);
        //AddVectorObs(angleSE);
        //AddVectorObs(angleEW);
    }

    public override void AgentAction(float[] vectorAction, string textAction)
    {
        var torqueX = Mathf.Clamp(vectorAction[0], -1f, 1f) * 100f;
        fbRigid.AddTorque(new Vector3(0f, torqueX, 0f));
        torqueX = Mathf.Clamp(vectorAction[1], -1f, 1f) * 100f;
        bsRigid.AddRelativeTorque(new Vector3(torqueX, 0f, 0f));
        torqueX = Mathf.Clamp(vectorAction[2], -1f, 1f) * 100f;
        seRigid.AddRelativeTorque(new Vector3(torqueX, 0f, 0f));
        torqueX = Mathf.Clamp(vectorAction[3], -1f, 1f) * 100f;
        ewRigid.AddRelativeTorque(new Vector3(torqueX, 0f, 0f));

        distToTarget = Vector3.Distance(target.transform.position, end.transform.position);
        if (distToTarget <= 2.0f)
        {
            AddReward(0.001f / distToTarget / distToTarget);
        }
        if (distToTarget > 6.0f)
        {
            AddReward(-0.00001f * distToTarget * distToTarget);
        }
        if (end.transform.position.y < 0)
        {
            AddReward(-1.0f);
            underground = 1;
            Done();
        }
        angleBD = body.transform.eulerAngles.y;
        angleBS = shoulder.transform.eulerAngles.x;
        //angleSE = Quaternion.Angle(bsRigid.rotation, seRigid.rotation);
        angleSE = elbow.transform.eulerAngles.x-shoulder.transform.eulerAngles.x;
        angleEW = wrist.transform.eulerAngles.x-elbow.transform.eulerAngles.x;
        //Debug.Log(angleBD);
    }

    public void MakeRandomTarget()
    {
	try
	{
            //recv = client.Receive(bytes);
	
	    //string data = Encoding.UTF8.GetString(bytes, 0, recv);
            if ((length = stream.Read(bytes, 0, bytes.Length))!=0)
            {
                //xyz = System.Text.Encoding.UTF8.GetString(bytes, 0, i);
		data = Encoding.Default.GetString(bytes,0,length);
            }
        
            targetX = float.Parse(data.Split('/')[0]);
            targetY = float.Parse(data.Split('/')[1]);
            targetZ = float.Parse(data.Split('/')[2]);
            targetVec.Set(targetX, targetY, targetZ);
            target.transform.position = targetVec;
	}
	catch (Exception e)
	{
            targetVec.Set(UnityEngine.Random.Range(-10.5f, 10.5f), UnityEngine.Random.Range(2.0f, 12.5f), UnityEngine.Random.Range(-10.5f, -3.0f));
            target.transform.position = targetVec;

            while (Vector3.Distance(target.transform.position, shoulderJoint.position) > 10.5f)
            {
                targetVec.Set(UnityEngine.Random.Range(-10.5f, 10.5f), UnityEngine.Random.Range(2.0f, 12.5f), UnityEngine.Random.Range(-10.5f, -5.0f));
                target.transform.position = targetVec;
            }
	    Debug.Log(e);
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
