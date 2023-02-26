using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class to store animation key frame data and frame time
/// </summary>
public class AnimFrame
{
    public int[] joint_data = new int[] { };
    public float time_seconds;

    public AnimFrame(int[] _joint_data, float _time)
    {
        joint_data = _joint_data;
        time_seconds = _time;
    }

    public void set_data(int[] data, float time)
    {
        joint_data = data;
        time_seconds = time;
    }

    public int getJointPose(int index)
    {
        return joint_data[index];
    }

    public void setJointPose(int index, int pose)
    {
        joint_data[index] = pose;
    }
}
