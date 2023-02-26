using ArduinoSerialAPI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SerialHandler : MonoBehaviour
{

    SerialHelper helper;


    public TMP_Text Status_Text;
	public Image Status_Image;
	public TMP_Text Console_Text;

	public TMP_Dropdown port_select;

	bool connected;

	private string[] ports = new string[]
		{
			"COM1", "COM2", "COM3", "COM4", "COM5",
			"COM6", "COM7", "COM8", "COM9", "COM10",
			"COM11", "COM12", "COM13", "COM14", "COM15",
			"COM16", "COM17", "COM18", "COM19", "COM20",
		};

    // Start is called before the first frame update
    void Start()
    {
		Connect();
	}

	/// <summary>
	/// attempts connection to arduino
	/// </summary>
    public void Connect()
    {
		if (helper != null)
			helper.Disconnect();

		print("attempting connection!");
		Status_Text.text = "Status: Disconnected";
		Status_Image.color = Color.red;

		try
		{
			print(ports[port_select.value]);
			helper = SerialHelper.CreateInstance(ports[port_select.value]);
			helper.setTerminatorBasedStream("\n"); //delimits received messages based on '\n' char


			helper.OnConnected += () => {
				Debug.Log("Connected");
				Status_Text.text = "Status: Connected";
				Status_Image.color = Color.green;
				connected = true;
			};

			helper.OnConnectionFailed += () => {
				Debug.Log("Failed");
				Status_Text.text = "Status: Failed";
				Status_Image.color = Color.red;
			};

			helper.OnDataReceived += () => {
				Console_Text.text = helper.Read();
			};

			helper.OnPermissionNotGranted += () => {
				//....
			};

			helper.Connect();
		}
		catch (Exception ex)
		{
			Console_Text.text = ex.Message;
		}
	}

	/// <summary>
	/// Sets joint positions in hardware
	/// </summary>
	/// <param name="joints"></param>
	public void SetJointPositions(RJoint[] joints)
		{
		StartCoroutine(sendData(joints));
    }

	IEnumerator sendData(RJoint[] joints)
    {
		for (int j = 0; j < joints.Length; j++)
		{
				RJoint joint = joints[j];
				int cmd_type = SetAngle;
				string[] key = new string[] { "pin", "deg" };
				int[] data = new int[] { joint.pwmID, joint.getSetPoint() };
				SendCommand(cmd_type, key, data);

			yield return new WaitForSeconds(0.05f);
		}
	}

	public void ReturnToHome()
    {
		helper.SendData("{\"cmd\":3}");
		//if(connected)
			//SendCommand(ReturnHome, new string[] { }, new int[] { });
    }

	public void SetJointData(RJoint[] joints)
    {
		//not implemented
    }

	public void GetJointPositions()
    {
		//not implemented
    }

	public void GetJointData()
    {
		//not implemented
	}

	const int Initialize = 0;
	const int SetAngle = 1;
	const int GetStatus = 2;
	const int ReturnHome = 3;

	public void SendCommand(int cmd_type, string[] keys, int[] data)
    {
		if (connected) {
			string finalData = "{";
			finalData += "\"cmd\":" + cmd_type.ToString();
			finalData += ",\"data\":{";

			finalData += "\"" + keys[0].ToString() + "\": \"";
			finalData += data[0].ToString() + "\",";

			finalData += "\"" + keys[1].ToString() + "\":";
			finalData += data[1].ToString();

			/*for (int i=0; i<keys.Length; i++)
			{
				finalData += "\"" + keys[i].ToString() + "\": \"";
				finalData += data[i].ToString() + "\",";
			}
			finalData = finalData.Remove(finalData.Length - 1);*/
			finalData += "}}\n";

			print(finalData);
			helper.SendData(finalData);
		}
    }

    // Update is called once per frame
    void Update()
    {
		
	}
}
