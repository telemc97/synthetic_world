using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class AnnotatorObject : MonoBehaviour
{
    Camera mainCamera;
    Bounds b;
    OperationMode operationMode;
    Renderer thisRenderer;
    public string objectClass;
    public string datasetFolder;
    int clsID;

    enum OperationMode
    {
        INIT,
        VISIBLE,
        INVISIBLE
    }

    // Start is called before the first frame update
    void Start()
    {
        Setup();
    }

    void Setup()
    {
        mainCamera = Camera.main;
        b = this.GetComponent<Collider>().bounds;
        thisRenderer = GetComponent<Renderer>();    
        operationMode = OperationMode.INIT;
        objectClass = this.transform.tag;
        if (!mainCamera.GetComponent<AnnotatorCamera>().classes.ContainsKey(this.transform.tag))
        {
            int val = mainCamera.GetComponent<AnnotatorCamera>().classes.Count;
            mainCamera.GetComponent<AnnotatorCamera>().classes.Add(this.transform.tag, val);
            mainCamera.GetComponent<AnnotatorCamera>().classes_mir.Add(val, this.transform.tag);
        }
        clsID = mainCamera.GetComponent<AnnotatorCamera>().classes[this.transform.tag];
    }

    void GetAnnotation()
    {
        Vector3[] pts = new Vector3[8];
        // All 8 vertices of the bounds 
        pts[0] = mainCamera.WorldToScreenPoint(new Vector3(b.center.x + b.extents.x, b.center.y + b.extents.y, b.center.z + b.extents.z));
        pts[1] = mainCamera.WorldToScreenPoint(new Vector3(b.center.x + b.extents.x, b.center.y + b.extents.y, b.center.z - b.extents.z));
        pts[2] = mainCamera.WorldToScreenPoint(new Vector3(b.center.x + b.extents.x, b.center.y - b.extents.y, b.center.z + b.extents.z));
        pts[3] = mainCamera.WorldToScreenPoint(new Vector3(b.center.x + b.extents.x, b.center.y - b.extents.y, b.center.z - b.extents.z));
        pts[4] = mainCamera.WorldToScreenPoint(new Vector3(b.center.x - b.extents.x, b.center.y + b.extents.y, b.center.z + b.extents.z));
        pts[5] = mainCamera.WorldToScreenPoint(new Vector3(b.center.x - b.extents.x, b.center.y + b.extents.y, b.center.z - b.extents.z));
        pts[6] = mainCamera.WorldToScreenPoint(new Vector3(b.center.x - b.extents.x, b.center.y - b.extents.y, b.center.z + b.extents.z));
        pts[7] = mainCamera.WorldToScreenPoint(new Vector3(b.center.x - b.extents.x, b.center.y - b.extents.y, b.center.z - b.extents.z));
        // Get them in GUI space 
        for (int i = 0; i < pts.Length; i++) pts[i].y = Screen.height - pts[i].y;
        // Calculate the min and max positions 
        Vector3 min = pts[0];
        Vector3 max = pts[0];
        for (int i = 1; i < pts.Length; i++)
        {
            min = Vector3.Min(min, pts[i]);
            max = Vector3.Max(max, pts[i]);
        }
        // Construct a rect of the min and max positions
        float[] det_point = new float[5];
        det_point[0] = (float)clsID;
        det_point[1] = min.x;
        det_point[2] = min.y;
        det_point[3] = max.x;
        det_point[4] = max.y;
        mainCamera.GetComponent<AnnotatorCamera>().activeDetections.Add(det_point);
    }

    void Update()
    {
        switch (operationMode)
        {
            case OperationMode.INIT:
                Setup();
                if (thisRenderer.isVisible)
                {
                    operationMode = OperationMode.VISIBLE;
                }
                break;
            case OperationMode.VISIBLE:
                GetAnnotation();
                if (thisRenderer.isVisible)
                {
                    operationMode = OperationMode.INVISIBLE;
                }
                break;
            case OperationMode.INVISIBLE:
                operationMode = OperationMode.INIT;
                break;
            default:
                operationMode = OperationMode.INIT;
                break;
        }

    }
}
