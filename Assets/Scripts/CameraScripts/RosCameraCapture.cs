using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Sensor;
using Unity.Robotics.ROSTCPConnector.MessageGeneration;
using RosMessageTypes.Std;

public class RosCameraCapture : MonoBehaviour
{
    //TODO: Test to make sure it works and make it tidy;
    private Camera thisCamera;
    private ROSConnection ros;
    public string topicName;

    public float publishMessageFrequency;
    private float timeElapsed;

    public int ImageWidth;
    public int ImageHeight;

    private RenderTexture renderTexture;
    private Texture2D texture2D;
    private Rect rect;

    private OperationMode operationMode;

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
        //Set operation mode to execute
        operationMode = OperationMode.EXEC;
    }

    private void SetupCamera()
    {
        renderTexture = new RenderTexture(ImageWidth, ImageHeight, 24);
        texture2D = new Texture2D(ImageWidth, ImageHeight);
        rect = new Rect(0, 0, ImageWidth, ImageHeight);
        thisCamera.targetTexture = renderTexture;
    }

    private Texture2D CaptureScreenshot()
    {
        RenderTexture oldRT = RenderTexture.active;
        RenderTexture.active = thisCamera.targetTexture;
        thisCamera.Render();
        texture2D.ReadPixels(rect, 0, 0);
        texture2D.Apply();
        RenderTexture.active = oldRT;
        return texture2D;
    }

    public void PublishImage(Texture2D image)
    {
        HeaderMsg headerMsg = new HeaderMsg();
        ImageMsg imageMsg = image.ToImageMsg(headerMsg);
        imageMsg.width = (uint)ImageWidth;
        imageMsg.height = (uint)ImageHeight;
        ros.Publish(topicName, imageMsg);
    }

    public void Exec()
    {
        timeElapsed += Time.deltaTime;
        if (timeElapsed > publishMessageFrequency)
        {
            Texture2D cameraImage = CaptureScreenshot();
            PublishImage(cameraImage);
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
