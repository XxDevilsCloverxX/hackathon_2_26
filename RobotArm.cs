using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RobotArm : MonoBehaviour
{
    public RJoint[] joints;
    public static RobotArm instance = null;

    public enum RobotMode { Setpoint, Free};
    public RobotMode mode;

    public float[] setPoint;

    void Awake()
    {
        
    }

    // Start is called before the first frame update
    void Start()
    {
        setPoint = new float[] { 0f, 0f, 0f, 0f, 0f, 0f };
        //int[] test_point = new int[] { 90, 0, 50, -20, -10 };
        //Rotate(test_point);

        for(int i = 0; i < joints.Length; i++)
        {
            setPoint[i] = joints[i].getSetPoint();
        }
    }

    // Update is called once per frame
    void Update()
    {
    }

    /// <summary>
    /// Rotate to reach a setpoint
    /// </summary>
    /// <param name="_setpoint"></param>
    public void Rotate(float[] _setpoint)
    {

        setPoint = _setpoint;
        mode = RobotMode.Setpoint;

        //go through each joint info and rotate that specified ammount
        for (int i = 0; i < setPoint.Length; i++)
        {
            joints[i].rotate((int)setPoint[i]); //rotate all joints by current joint to rotate on
        }
    }

    /// <summary>
    /// Rotate arm freely
    /// </summary>
    /// <param name="joint_args"></param>
    public void Rotate(bool[] joint_args)
    {

        mode = RobotMode.Free;
        //go through each joint info and rotate that specified ammount
        for (int i = 0; i < joint_args.Length; i++)
        {
             joints[i].rotate(joint_args[i]); //rotate all joints by current joint to rotate on
        }
    }

    //add values to all joint setpoints on arm
    public void addSetPoint(float[] difference)
    {

        for(int j = 0; j < difference.Length; j++)
        {
            if (setPoint[j] + difference[j] < joints[j].limit_upper && setPoint[j] + difference[j] > joints[j].limit_lower)
            {
                setPoint[j] += difference[j];
                //print("difference: " + difference[j]);
            }
        }

        Rotate(setPoint);
    }

    /// <summary>
    /// return all joints to zero position
    /// </summary>
    public void returnZero()
    {
        for (int i = 0; i < joints.Length; i++)
        {
            setPoint[i] = joints[i].zero_pos;
        }

        Rotate(setPoint);
    }

    private void OnDrawGizmos()
    {
        
        foreach(RJoint j in joints)
        {
            if (j.draw)
            {
                Vector3 start = j.origin_set_obj.transform.position;
                Vector3 direction = j.origin_set_obj.transform.up;
                
                Gizmos.color = Color.yellow;
                //print("draw! " + start + "  in direction:" + direction);
                Gizmos.DrawLine(start, start + direction * 10);
            }
        }
    }
}
