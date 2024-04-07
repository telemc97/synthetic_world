using RosMessageTypes.Geometry;
using RosMessageTypes.Std;
using RosMessageTypes.Tf2;
using System.Collections;
using System.Collections.Generic;
using System.Security.Policy;
using Unity.Robotics.ROSTCPConnector;
using Unity.Robotics.ROSTCPConnector.ROSGeometry;
using UnityEngine;

public class StereoTransformationPublisher : MonoBehaviour
{
    public Transform baseLink;

    public string baseLinkFrame;
    public string childLinkFrame;

    string topicName;

    private ROSConnection ros;
    private TFSystem tf;

    TFStream tfStream;

    //Used Messages
    Vector3Msg translationMsg;
    QuaternionMsg quaternionMsg;
    TransformMsg transformMsg;
    TransformStampedMsg transformStampedMsg;
    HeaderMsg headerMsg;
    
    private float timeElapsed;
    public float publishMessageFrequency;


    private OperationMode operationMode;

    enum OperationMode
    {
        SETUP,
        EXEC,
        WAIT
    }

    private void Setup()
    {
        //Initialize ROS
        ros = ROSConnection.GetOrCreateInstance();
        tf = TFSystem.GetOrCreateInstance();
        //Get child frame id
        childLinkFrame = this.GetComponentInChildren<RosCameraCapture>().frameName;
        topicName = baseLinkFrame + "_to_" + childLinkFrame;
        //Initialize Publisher
        ros.RegisterPublisher<TransformStampedMsg>(topicName);
        //Initialize Messages
        headerMsg = new HeaderMsg();
        headerMsg.frame_id = baseLinkFrame;
        translationMsg = new Vector3Msg();
        quaternionMsg = new QuaternionMsg();
        transformMsg = new TransformMsg();
        transformStampedMsg = new TransformStampedMsg();
        transformStampedMsg.child_frame_id = childLinkFrame;
        //Set operation mode to execute
        operationMode = OperationMode.EXEC;
    }

    public void PublishCameraTransform()
    {
        //TODO transformation is the other way around
        //Local positions and rotation based to Transform baseLink
        Vector3 localTransform = baseLink.transform.TransformPoint(this.transform.position);
        Quaternion localRotation = Quaternion.Euler(baseLink.transform.TransformDirection(this.transform.rotation.eulerAngles));

        //Populate Translation Msg
        translationMsg = localTransform.To<FLU>();

        //Populate Rotation Msg
        quaternionMsg.x = localRotation.z;
        quaternionMsg.y = -localRotation.x;
        quaternionMsg.z = localRotation.y;
        quaternionMsg.w = localRotation.w;

        //Populate Transform Msg
        transformMsg.translation = translationMsg;
        transformMsg.rotation = quaternionMsg;

        //Populate Transform Stamped Msg
        transformStampedMsg.header = headerMsg;
        transformStampedMsg.transform = transformMsg;
        ros.Publish(topicName, transformStampedMsg);
    }

    public void PublishTF()
    {
        tfStream = tf.GetOrCreateFrame(childLinkFrame);
    }

    private void Exec()
    {
        timeElapsed += Time.deltaTime;
        if (timeElapsed > (1.0f / publishMessageFrequency))
        {
            PublishTF();
            PublishCameraTransform();
            timeElapsed = 0;
        }
    }

    private void Update()
    {
        switch (operationMode)
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
