using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RAnimation
{
    string name;
    public List<AnimFrame> frames;
    float timer = 0;

    public AnimFrame currentFrame;
    public bool isFinished;

    public RAnimation(string name)
    {
        frames = new List<AnimFrame>();
    }

    public RAnimation(List<AnimFrame> _frames, string _name)
    {
        name = _name;
        frames = _frames;
        frames = sortFrames();
    }

    public void setFrame(int index, int[] data, float time)
    {
        frames[index].set_data(data, time);
        frames = sortFrames();
    }

    public void addFrame(float time, int[] data)
    {
        AnimFrame newFrame = new AnimFrame(data, time);
        frames.Add(newFrame);
        frames = sortFrames();
    }

    public void reset()
    {
        timer = 0;
        isFinished = false;
    }

    public List<AnimFrame> sortFrames()
    {
        List<AnimFrame> new_list = new List<AnimFrame>();
        foreach(AnimFrame frame in frames)
        {
            bool found_place = false;
            foreach (AnimFrame compare_frame in new_list)
            {
                
                if(frame.time_seconds < compare_frame.time_seconds)
                {
                    found_place = true;
                    new_list.Insert(new_list.IndexOf(compare_frame), frame);
                }
                if (found_place)
                    break;
            }
            if (!found_place)
                new_list.Add(frame);
        }

        return new_list;
    }


    //add time to the timer asnd return the current frame
    public float[] execute(float time)
    {
        timer += time;
        foreach(AnimFrame frame in frames)
        {
            int index = frames.IndexOf(frame);
            if(timer > frame.time_seconds && timer < frames[index+1].time_seconds)
            {
                currentFrame = frame;
                return getCurrentSetPoint(frame, frames[index+1]);
            }
        }


        isFinished = true;
        //else we are at end of animation
        int[] data = frames[frames.Count - 1].joint_data;
        float[] newData = new float[data.Length];
        foreach(int i in data)
        {
            newData[i] = (float)data[i];
        }

        return newData; //else return last frame
    }

    public float[] getCurrentSetPoint(AnimFrame current, AnimFrame next)
    {
        float[] setpoints = new float[] { 0, 0, 0, 0, 0, 0 };

        for (int i = 0; i < current.joint_data.Length; i++) {

            //smoothly transition to next frame
            setpoints[i] = current.joint_data[i] + (next.joint_data[i] - current.joint_data[i]) * (timer - current.time_seconds) / (next.time_seconds - current.time_seconds);
        }

        return setpoints;
    }
}
