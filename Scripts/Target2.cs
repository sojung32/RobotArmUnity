using UnityEngine;
using MLAgents;

public class Target2 : MonoBehaviour
{
    public RobotArmAgent2 robotArm;
    
    
    void OnTriggerEnter(Collider col)
    {
        if(col.gameObject.CompareTag("wristEnd"))
        {
            robotArm.touchTarget = 1;
            robotArm.AddReward(1.0f);
            robotArm.MakeRandomTarget();
            robotArm.Done();
        }
    }
}
