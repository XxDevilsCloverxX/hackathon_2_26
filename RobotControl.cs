using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RobotControl : MonoBehaviour
{

    public RobotArm arm;
    public SerialHandler s_handler;

    public enum ControlMode { Manual, Xbox, AnimationPlay, Animator }
    public ControlMode cmode;

    private const float xbox_speed = 0.1f;

    public bool hardware_control; //whether to enable arduino output / input

    float timer;
    float updatetime = 0.5f;

    // Start is called before the first frame update
    void Start()
    {
        //print(Input.GetJoystickNames());
        cmode = ControlMode.Manual;
        hardware_control = true;
        timer = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if(cmode == ControlMode.Xbox)
        {
            float[] rotateChange = new float[] { 0, 0, 0, 0, 0, 0};
            
            rotateChange[0] = Input.GetAxis("Horizontal_Base") * xbox_speed;
            rotateChange[1] = -Input.GetAxis("Vertical_Shoulder") * xbox_speed;
            rotateChange[2] = -Input.GetAxis("Vertical_Elbow") * xbox_speed;
            rotateChange[3] = -Input.GetAxis("Vertical_Forearm") * xbox_speed;
            rotateChange[4] = -Input.GetAxis("Horizontal_Wrist") * xbox_speed;

            //claw implementation
            if (Input.GetAxis("Fire2_Open") > 0.5)
                rotateChange[5] = 1f;
            else if (Input.GetAxis("Fire1_Close") > 0.5)
                rotateChange[5] = -1f;
            else
                rotateChange[5] = 0f;

            arm.addSetPoint(rotateChange);

            //print(rotateChange[0].ToString());
        }

        timer += Time.deltaTime;
        
        //if hardware enabled, set join positions
        if(hardware_control && cmode != ControlMode.Animator && timer > updatetime)
        {
            timer = 0;
            s_handler.SetJointPositions(arm.joints);
        }
        else
        {
            //s_handler.ReturnToHome(); //else set robot in default position
        }
    }

    public void testSerial()
    {
        s_handler.SetJointPositions(arm.joints);
    }

    public void returnToZero()
    {
        arm.returnZero();
        if (hardware_control)
            s_handler.ReturnToHome();
    }

    public void SetMode(ControlMode mode)
    {
        cmode = mode;
    }
}
