using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.Mathematics;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;
using static UnityEngine.PlayerLoop.PreLateUpdate;


public class AnnotatorCamera : MonoBehaviour
{
    public List<float[]> activeDetections = new List<float[]>(); //class, x_min, y_min, x_max, y_max
    public Dictionary<string, int> classes = new Dictionary<string, int>();
    public Dictionary<int, string> classes_mir = new Dictionary<int, string>(); //mirrored dictionary classes to get class name from its int id
    public string datasetFolder = @"C:\Users\MTile\Pictures\Dataset";

    int datasetRecordingInterval = 1;

    public bool displayFPS;
    public bool displayOnGUI;
    public bool recordDetections;

    void TakeSnapShot()
    {
        if (!Directory.Exists(datasetFolder)) 
        { 
            Directory.CreateDirectory(datasetFolder); // if this path does not exist yet it will get created
        } 

        int fileNo = 0;
        while (File.Exists(Path.Combine(datasetFolder, ("Snapshot_" + fileNo + ".png"))) || File.Exists(Path.Combine(datasetFolder, ("Snapshot_" + fileNo + ".txt")))) 
        { 
           ++fileNo; 
        }

        string snapShotName = "Snapshot_" + fileNo; // puts the current time right into the screenshot name
        string pngFileName = Path.Combine(datasetFolder, (snapShotName + ".png"));
        ScreenCapture.CaptureScreenshot(pngFileName, 2); // takes the sceenshot, the "2" is for the scaled resolution, you can put this to 600 but it will take really long to scale the image up
        string txtFileName = Path.Combine(datasetFolder, (snapShotName + ".txt"));
        if (!File.Exists(txtFileName)) 
        {
            using (StreamWriter writer = File.CreateText(txtFileName))
            {
                foreach (float[] detection in activeDetections)
                {
                    writer.WriteLine(detection[0].ToString() + " " + detection[1].ToString() + " " + detection[2].ToString() + " " + detection[3].ToString() + " " + detection[4].ToString());
                }
            }
        }

    }

    private void Start()
    {
        activeDetections = new List<float[]>();

    }

    private void Update()
    {
        if (activeDetections.Count > 0)
        {
            if (recordDetections)
            {
                if (Time.time >= datasetRecordingInterval)
                {
                    // Change the next update (current second+1)
                    datasetRecordingInterval = Mathf.FloorToInt(Time.time) + 1;
                    // Call your fonction
                    TakeSnapShot();
                }
            }
        }
        activeDetections.Clear();
    }

    private void OnGUI()
    {
        if (displayFPS)
        {
            GUI.Label(new Rect(0, 0, 100, 100), (1.0f / Time.smoothDeltaTime).ToString());
        }
        if (activeDetections.Count > 0 && displayOnGUI) 
        {
            foreach (float[] detection in activeDetections) 
            {
                Rect r = Rect.MinMaxRect(detection[1], detection[2], detection[3], detection[4]);
                GUI.Box(r, classes_mir[(int)detection[0]]);
            }
        }
            
    }
}
