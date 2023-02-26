//using Microsoft.VisualBasic.FileIO;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class AnimationHandler : MonoBehaviour
{

    public List<RAnimation> animations;
    public string animFilepath;

    // Start is called before the first frame update
    void Start()
    {
        animationsFromPath();
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    /// <summary>
    /// Returns a set of animations stored in a file location
    /// </summary>
    public void animationsFromPath()
    {
        if (animations == null)
            animations = new List<RAnimation>();

        string filepath = Application.persistentDataPath + animFilepath;

        //get all files
        string[] animFiles = Directory.GetFiles(filepath);

        //create animation for each file
        foreach (string file in animFiles)
        {
            RAnimation newAnim = new RAnimation(file);

            string filedata = System.IO.File.ReadAllText(file);
            string[] lines = filedata.Split("\n"[0]);
            

            foreach (string line in lines) {

                string[] fields = (line.Trim()).Split(","[0]);

                float time;
                float.TryParse(fields[0], out time);
                int[] data = new int[] { 0, 0, 0, 0, 0, 0 };
                int.TryParse(fields[1], out data[0]);
                int.TryParse(fields[2], out data[1]);
                int.TryParse(fields[3], out data[2]);
                int.TryParse(fields[4], out data[3]);
                int.TryParse(fields[5], out data[4]);
                int.TryParse(fields[6], out data[5]);

                //add new frame to animation
                newAnim.addFrame(time, data);
            }

        }
    }
}
