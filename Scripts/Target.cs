using UnityEngine;
using MLAgents;

public class Target : MonoBehaviour
{
    public RobotArmAgent robotArm;
    
    string state;
    
    void OnTriggerEnter(Collider col)
    {
        if(col.gameObject.CompareTag("wristEnd"))
        {
            robotArm.touchTarget = 1;
            robotArm.AddReward(1.0f);
            if (robotArm.socketCon==1)
	    {
	        state = robotArm.body.transform.eulerAngles.y+"/"+robotArm.shoulder.transform.eulerAngles.x;
                state = state + "/"+(robotArm.elbow.transform.eulerAngles.x-robotArm.shoulder.transform.eulerAngles.x);
		state = state +"/"+(robotArm.wrist.transform.eulerAngles.x-robotArm.elbow.transform.eulerAngles.x);
            
                robotArm.SendToRos(state);
            
            //robotArm.MakeRandomTarget();
	        robotArm.SocketClosing();
	        robotArm.socketCon = 0;
	    }
	    
            robotArm.Done();
        }
    }
}
