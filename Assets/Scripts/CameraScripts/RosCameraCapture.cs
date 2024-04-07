using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Sensor;
using RosMessageTypes.Geometry;
using Unity.Robotics.ROSTCPConnector.MessageGeneration;
using RosMessageTypes.Std;

public class RosCameraCapture : MonoBehaviour
{
    //TODO: Test to make sure it works and make it tidy;


    private Camera thisCamera;
    private ROSConnection ros;
    private TFSystem tf;

    public string topicName;
    public string frameName;

    public bool isStereo;

    public float publishMessageFrequency;
    private float timeElapsed;

    //Messages
    HeaderMsg headerMsg;

    public int imageWidth;
    public int imageHeight;

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
        tf = TFSystem.GetOrCreateInstance();
        //Initialize Publisher
        ros.RegisterPublisher<ImageMsg>(topicName);
        ros.RegisterPublisher<CameraInfoMsg>(topicName + "_camera_info");
        //Initialize Messages
        headerMsg = new HeaderMsg();
        headerMsg.frame_id = frameName;
        //Set operation mode to execute
        operationMode = OperationMode.EXEC;
    
    }

    private void SetupCamera()
    {
        renderTexture = new RenderTexture(imageWidth, imageHeight, 24);
        texture2D = new Texture2D(imageWidth, imageHeight);
        rect = new Rect(0, 0, imageWidth, imageHeight);
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
