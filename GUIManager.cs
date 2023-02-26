using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GUIManager : MonoBehaviour
{

    const int ManualID = 0;
    const int AnimationsID= 1;
    const int GamepadID = 2;

    public RobotControl controller;
    public SerialHandler Serial_Handler;


    public enum MenuMode { Control, Settings, Animate}

    public MenuMode menuMode;


    //game elements for menus
    public List<GameObject> ControlMenuUI;
    public List<GameObject> SettingsMenuUI;
    public List<GameObject> AnimateMenuUI;

    //to manage what elements should display when mode changes
    public List<GameObject> ManualUI;
    public List<GameObject> AnimationUI;
    public List<GameObject> GamepadUI;


    #region ControlMenuUI

    public TMP_Dropdown controlModeDrop;

    public Toggle hardware_control;

    public Transform JointMenuTrans;
    public GameObject jointItemPrefab;
    public int activeJoint = 0;
    #endregion

    // Start is called before the first frame update
    void Start()
    {
        menuMode = MenuMode.Control;
        updateMode();
    }

    // Update is called once per frame
    void Update()
    {
        if (menuMode == MenuMode.Control)
        {
            disableElements(AnimateMenuUI);
            enableElements(ControlMenuUI);
            switch (controller.cmode)
            {
                case RobotControl.ControlMode.Manual:
                    disableElements(GamepadUI);
                    disableElements(AnimationUI);
                    enableElements(ManualUI);
                    populateSelectedJoint(activeJoint);
                    break;
                case RobotControl.ControlMode.AnimationPlay:
                    disableElements(GamepadUI);
                    disableElements(ManualUI);
                    enableElements(AnimationUI);
                    break;
                case RobotControl.ControlMode.Xbox:
                    disableElements(ManualUI);
                    disableElements(AnimationUI);
                    enableElements(GamepadUI);
                    break;
            }
        }
        else if (menuMode == MenuMode.Animate)
        {
            disableElements(ControlMenuUI);
            enableElements(AnimateMenuUI);
        }
    }

    void enableElements(List<GameObject> elements)
    {
        if (elements == null) return;
        foreach (GameObject e in elements)
        {
            e.SetActive(true);
        }
    }

    void disableElements(List<GameObject> elements)
    {
        if (elements == null) return;
        foreach(GameObject e in elements)
        {
            e.SetActive(false);
        }
    }

    #region ControlMenuButtons

    public void SetControlView()
    {
        menuMode = MenuMode.Control;
    }

    public void updateMode() 
    { 
        switch (controlModeDrop.value)
        {
            case ManualID:
                controller.SetMode(RobotControl.ControlMode.Manual);
                populateJoints(JointMenuTrans);
                populateSelectedJoint(0); //default populate first joint
                break;
            case AnimationsID:
                controller.SetMode(RobotControl.ControlMode.AnimationPlay);
                break;
            case GamepadID:
                controller.SetMode(RobotControl.ControlMode.Xbox);
                break;
        }
    }

    public void toggleHardwareControl()
    {
        controller.hardware_control = hardware_control.isOn;
    }

    #endregion

    #region ManualControl

    private void populateJoints(Transform parentMenu) {

        for (int i = 0; i < JointMenuTrans.childCount; i++)
        {
            Destroy(JointMenuTrans.GetChild(i).gameObject);
        }

        int id = 0;
        float ver_pos = JointMenuTrans.position.y - 10;
        foreach(RJoint j in controller.arm.joints)
        {
            GameObject o = Instantiate(jointItemPrefab, parentMenu);
            o.transform.position = new Vector3(0, ver_pos, 0);
            MenuEntry entry = o.GetComponent<MenuEntry>();

            entry.setMenuID(id);
            entry.setName(j.gameObject.name);
            entry.setButtonAction(populateSelectedJoint); //use this command when joint item is pressed

            id++;
            ver_pos -= 50;
        }
    }

    /// <summary>
    /// called by joint item menu entry
    /// </summary>
    public void populateSelectedJoint(int id)
    {
        activeJoint = id;

        RJoint joint = controller.arm.joints[id];

        if (menuMode == MenuMode.Control)
        {
            GameObject.Find("MotorIDField").GetComponent<TMP_InputField>().placeholder.GetComponent<TMP_Text>().text = joint.pwmID.ToString();
            GameObject.Find("SetPointField").GetComponent<TMP_InputField>().placeholder.GetComponent<TMP_Text>().text = joint.getSetPoint().ToString();
            GameObject.Find("AngleField").GetComponent<TMP_InputField>().placeholder.GetComponent<TMP_Text>().text = joint.position.ToString();
            GameObject.Find("UpperLimField").GetComponent<TMP_InputField>().placeholder.GetComponent<TMP_Text>().text = joint.limit_upper.ToString();
            GameObject.Find("LowerLimField").GetComponent<TMP_InputField>().placeholder.GetComponent<TMP_Text>().text = joint.limit_lower.ToString();
            GameObject.Find("ZeroPosField").GetComponent<TMP_InputField>().placeholder.GetComponent<TMP_Text>().text = joint.zero_pos.ToString();
            GameObject.Find("JNameText").GetComponent<TMP_Text>().text = joint.gameObject.name;
            GameObject.Find("OnTargetTxt").GetComponent<TMP_Text>().text = joint.onTarget.ToString();
            GameObject.Find("ServoTypeDrop").GetComponent<TMP_Dropdown>().value = (int)joint.motor_type;
            GameObject.Find("SpeedField").GetComponent<TMP_InputField>().placeholder.GetComponent<TMP_Text>().text = joint.speed.ToString();

            //change color based on if the joint is at its set position
            if (joint.onTarget)
                GameObject.Find("OnTargetTxt").GetComponent<TMP_Text>().color = Color.green;
            else
                GameObject.Find("OnTargetTxt").GetComponent<TMP_Text>().color = Color.red;
        }
        else if(menuMode == MenuMode.Animate)
        {

        }
    }

    public void updateJointValues()
    {
        RJoint joint = controller.arm.joints[activeJoint];

        try
        {
            int j_setpoint = joint.getSetPoint();

            //update joints based on fields
            string id = GameObject.Find("MotorIDField").GetComponent<TMP_InputField>().text;
            if (id != "")
                int.TryParse(id, out joint.pwmID);


            string setpoint = GameObject.Find("SetPointField").GetComponent<TMP_InputField>().text;
            int.TryParse(setpoint, out j_setpoint);
            if (setpoint != "")
                joint.setSetPoint(j_setpoint);

            string angle = GameObject.Find("AngleField").GetComponent<TMP_InputField>().text;
            if (angle != "")
                float.TryParse(angle, out joint.position);

            string upper_lim = GameObject.Find("UpperLimField").GetComponent<TMP_InputField>().text;
            if (upper_lim != "")
                int.TryParse(upper_lim, out joint.limit_upper);
            
            string lower_lim = GameObject.Find("LowerLimField").GetComponent<TMP_InputField>().text;
            if (lower_lim != "")
                int.TryParse(lower_lim, out joint.limit_lower);

            string zero_pos = GameObject.Find("ZeroPosField").GetComponent<TMP_InputField>().text;
            if(zero_pos != "")
                int.TryParse(zero_pos, out joint.zero_pos);

            string speed = GameObject.Find("SpeedField").GetComponent<TMP_InputField>().text;
            if (speed != "")
                float.TryParse(speed, out joint.speed);

            joint.motor_type = (RJoint.ServoType)GameObject.Find("ServoTypeDrop").GetComponent<TMP_Dropdown>().value;
            

            joint.gameObject.name = GameObject.Find("JNameText").GetComponent<TMP_Text>().text;

            //clear fields
            GameObject.Find("MotorIDField").GetComponent<TMP_InputField>().text = "";
            GameObject.Find("SetPointField").GetComponent<TMP_InputField>().text  = "";
            GameObject.Find("AngleField").GetComponent<TMP_InputField>().text = "";
            GameObject.Find("UpperLimField").GetComponent<TMP_InputField>().text = "";
            GameObject.Find("LowerLimField").GetComponent<TMP_InputField>().text = "";
            GameObject.Find("ZeroPosField").GetComponent<TMP_InputField>().text = "";
            GameObject.Find("JNameText").GetComponent<TMP_Text>().text  = "";
            GameObject.Find("SpeedField").GetComponent<TMP_InputField>().text = "";
        }
        catch { print("error something is not right yo.");  }

    }
    #endregion

    #region Serial
    public void ConnectPort()
    {
        Serial_Handler.Connect();
    }
    #endregion

    #region SettingsMenuButtons
    #endregion

    #region AnimateMenuButtons
    public void SetAnimationView()
    {
        menuMode = MenuMode.Animate;
    }

    #endregion
}
