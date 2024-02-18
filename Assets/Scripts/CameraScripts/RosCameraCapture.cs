using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Sensor;

public class RosCameraCapture : MonoBehaviour
{
    //TODO: Test to make sure it works and make it tidy;
    Camera thisCamera;
    private ImageMsg imageMsg;
    private ROSConnection ros;
    private string topicName;
    private RenderTexture renderTexture;

    public float publishMessageFrequency;
    private float timeElapsed;

    private OperationMode operationMode;

    enum OperationMode
    {
        SETUP,
        EXEC,
        WAIT
    }

    public void Setup()
    {
        //Set The Tag
        this.gameObject.tag = "RosCamera";
        //Get Component's Camera
        thisCamera = GetComponent<Camera>();
        //Initialize ROS
        ros = ROSConnection.GetOrCreateInstance();
        //Initialize Publisher
        ros.RegisterPublisher<ImageMsg>(topicName);
        //Set operation mode to execute
        operationMode = OperationMode.EXEC;
        //Try and get unique name ID
        topicName = GetObjectName();
    }

    private string GetObjectName()
    {
        string gameObjectName;
        System.Random r = new System.Random();
        gameObjectName = this.gameObject.name + r.Next(0, 999).ToString();
        //Ceck if ID is already in the Scene if yes do not update the operationMode to try get new id on the next iteration
        GameObject[] gameObjects = GameObject.FindGameObjectsWithTag("RosCamera");
        foreach (GameObject gameObject in gameObjects)
        {
            if (gameObject.name == gameObjectName)
            {
                operationMode = OperationMode.SETUP;
            }
        }

        return gameObjectName;
    }

    private byte[] CaptureScreenshot()
    {
        Camera.main.targetTexture = renderTexture;
        RenderTexture currentRT = RenderTexture.active;
        RenderTexture.active = renderTexture;
        Camera.main.Render();
        Texture2D mainCameraTexture = new Texture2D(renderTexture.width, renderTexture.height);
        mainCameraTexture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        mainCameraTexture.Apply();
        RenderTexture.active = currentRT;
        // Get the raw byte info from the screenshot
        byte[] imageBytes = mainCameraTexture.GetRawTextureData();
        Camera.main.targetTexture = null;
        return imageBytes;
    }

    public void PublishImage(byte[] image)
    {
        //check if images resolution changes ?
        imageMsg.width = (uint)Camera.main.targetTexture.width;
        imageMsg.height = (uint)Camera.main.targetTexture.height;
        imageMsg.data = image;
        ros.Publish(topicName, imageMsg);
    }

    public void Exec()
    {
        timeElapsed += Time.deltaTime;
        if (timeElapsed > publishMessageFrequency)
        {
            byte[] cameraImage = CaptureScreenshot();
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
