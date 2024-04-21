using RosMessageTypes.Geometry;
using RosMessageTypes.Std;
using RosMessageTypes.Tf2;
using System.Collections.Generic;
using Unity.Robotics.ROSTCPConnector;
using Unity.Robotics.ROSTCPConnector.ROSGeometry;
using UnityEngine;
using RosMessageTypes.BuiltinInterfaces;
using System;


public class StereoTransformationPublisher : MonoBehaviour
{
    const string TfTopic = "/tf";

    public Transform baseLink;

    public string baseLinkFrame;
    public string childLinkFrame;

    private ROSConnection ros;

    //Used Messages
    Vector3Msg translationMsg;
    QuaternionMsg quaternionMsg;
    TransformMsg transformMsg;
    TransformStampedMsg transformStampedMsg;
    HeaderMsg headerMsg;
    TFMessageMsg tFMessageMsg;

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

        //Get Child Frame ID
        childLinkFrame = this.GetComponentInChildren<RosCameraCapture>().frameName;

        //Initialize Publisher
        ros.RegisterPublisher<TFMessageMsg>(TfTopic);

        //Initialize Messages
        headerMsg = new HeaderMsg((uint)0, new TimeMsg(), baseLinkFrame);
        translationMsg = new Vector3Msg();
        quaternionMsg = new QuaternionMsg();
        transformMsg = new TransformMsg();
        transformStampedMsg = new TransformStampedMsg();
        tFMessageMsg = new TFMessageMsg();

        //Finally Set Operation Mode To Wait
        operationMode = OperationMode.EXEC;
    }

    private void Wait()
    {

    }

    public void PublishCameraTransform()
    {
        List<TransformStampedMsg> tfMessageList = new List<TransformStampedMsg>();

        //TODO transformation is the other way around
        //Local positions and rotation based to Transform baseLink
        Vector3 localTransform = baseLink.InverseTransformPoint(this.transform.position);
        Quaternion localRotation = Quaternion.Euler(baseLink.InverseTransformDirection(this.transform.rotation.eulerAngles));

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
        headerMsg.seq++;
        headerMsg.stamp.sec = (uint)DateTimeOffset.Now.ToUnixTimeSeconds();
        headerMsg.stamp.nanosec = (uint)DateTimeOffset.Now.ToUnixTimeMilliseconds() * 1000000;
        transformStampedMsg.header = headerMsg;
        transformStampedMsg.transform = transformMsg;
        transformStampedMsg.child_frame_id = childLinkFrame;

        tfMessageList.Add(transformStampedMsg);
        tFMessageMsg.transforms = tfMessageList.ToArray();
        ros.Publish(TfTopic, tFMessageMsg);
    }

    private void Exec()
    {
        timeElapsed += Time.deltaTime;
        if (timeElapsed > (1.0f / publishMessageFrequency))
        {
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
                Wait(); 
                break;
            default:
                break;
        }
    }
}
