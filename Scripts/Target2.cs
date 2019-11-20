using UnityEngine;
using MLAgents;

public class Target2 : MonoBehaviour
{
    public RobotArmAgent2 robotArm;
    
    string state;
    
    void OnTriggerEnter(Collider col)
    {
        if(col.gameObject.CompareTag("wristEnd"))
        {
            state = robotArm.angleBD + "/"+ robotArm.angleBS;
            state = state + "/" + robotArm.angleSE + "/" + robotArm.angleEW;
            
            robotArm.SendToRos(state);
        
            robotArm.AddReward(1.0f);
            robotArm.MakeRandomTarget();
	    robotArm.Done();
        }
    }
}
