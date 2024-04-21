using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Sensor;
using Unity.Robotics.ROSTCPConnector.MessageGeneration;
using RosMessageTypes.Std;
using UnityEditor;
using RosMessageTypes.BuiltinInterfaces;
using System;

public class RosCameraCapture : MonoBehaviour
{
    //TODO: Test to make sure it works and make it tidy;

    private Camera thisCamera;
    private RenderTexture renderTexture;

    private ROSConnection ros;

    public string topicName;
    public string frameName;

    public bool isStereo;

    public float publishMessageFrequency;
    private float timeElapsed;

    //Messages
    HeaderMsg headerMsg;

    public int imageWidth;
    public int imageHeight;

    private OperationMode operationMode;


    private Texture2D destinationTexture;

    enum OperationMode
    {
        SETUP,
        EXEC,
        WAIT
    }

    public void Setup()
    {
        //Set The Tag and the name if it is not MainCamera
        if (this.gameObject.tag != "MainCamera")
        {
            //Secondary camera different than the MainCamera.
        }

        //Get Component's Camera
        thisCamera = GetComponent<Camera>();

        //Setup Camera
        SetupCamera();

        //Initialize ROS
        ros = ROSConnection.GetOrCreateInstance();

        //Initialize Publisher
        ros.RegisterPublisher<ImageMsg>(topicName);
        ros.RegisterPublisher<CameraInfoMsg>(topicName + "_camera_info");

        //Initialize Messages
        headerMsg = new HeaderMsg((uint)0, new TimeMsg(), frameName);

        //Set operation mode to execute
        operationMode = OperationMode.EXEC;
    
    }

    private void SetupCamera()
    {
        renderTexture = new RenderTexture(imageWidth, imageHeight, 24);
        thisCamera.targetTexture = renderTexture;
    }

    private Texture2D CaptureScreenshot()
    {
        if (!EditorApplication.isUpdating)
            Unsupported.RestoreOverrideLightingSettings();

        var tmp = RenderTexture.GetTemporary((int)imageWidth, (int)imageHeight);

        Graphics.Blit(renderTexture, tmp);

        RenderTexture.active = tmp;
        Texture2D image = new Texture2D((int)imageWidth, (int)imageHeight, TextureFormat.RGB24, false, false);
        image.ReadPixels(new Rect(0, 0, imageWidth, imageHeight), 0, 0);
        image.Apply();
        RenderTexture.ReleaseTemporary(tmp);

        return image;
    }

    public void PublishImage(Texture2D image)
    {
        headerMsg.seq++;
        headerMsg.stamp.sec = (uint)DateTimeOffset.Now.ToUnixTimeSeconds();
        headerMsg.stamp.nanosec = (uint)DateTimeOffset.Now.ToUnixTimeMilliseconds() * 1000000;
        ImageMsg imageMsg = image.ToImageMsg(headerMsg);
        imageMsg.width = (uint)imageWidth;
        imageMsg.height = (uint)imageHeight;
        ros.Publish(topicName, imageMsg);
    }

    public void PublishInfoTopic()
    {
        CameraInfoMsg cameraInfoMsg = CameraInfoGenerator.ConstructCameraInfoMessage(thisCamera, headerMsg);
        ros.Publish(topicName + "_camera_info", cameraInfoMsg);
    }


    public void Exec()
    {
        timeElapsed += Time.deltaTime;
        if (timeElapsed > (1.0f / publishMessageFrequency))
        {
            if (!isStereo) 
            { 
            }
            Texture2D cameraImage = CaptureScreenshot();
            PublishImage(cameraImage);
            PublishInfoTopic();
            timeElapsed = 0;
        }
    }

    private void Start()
    {
        operationMode = OperationMode.SETUP;
    }

    private void OnEnable()
    {
        operationMode = OperationMode.SETUP;
    }

    private void OnDisable()
    {
        operationMode = OperationMode.SETUP;
    }

    private void Update()
    {
        switch(operationMode)
        {
            case OperationMode.SETUP:
                Setup();
                break;
            case OperationMode.EXEC:
                Exec(); 
                break;
            case OperationMode.WAIT:
            default:
                break;
        }
    }
}
