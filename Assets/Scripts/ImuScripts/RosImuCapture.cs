using RosMessageTypes.Sensor;
using RosMessageTypes.Geometry;
using RosMessageTypes.Std;
using UnityEngine;
using System;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.BuiltinInterfaces;

public class RosImuCapture : MonoBehaviour
{
    private OperationMode operationMode;

    enum OperationMode
    {
        SETUP,
        EXEC,
        WAIT
    }

    Vector3 oldRotation;
    Vector3 oldVelocity;
    Vector3 oldPosition;
    long oldTime;

    //Messages
    QuaternionMsg quaternionMsg;
    Vector3Msg angularVelocityMsg;
    Vector3Msg linearAccelarationMsg;
    HeaderMsg headerMsg;
    ImuMsg imuMsg;

    public float publishMessageFrequency;
    private float timeElapsed;

    public string frameName;

    private ROSConnection ros;
    private TFSystem tf;

    public string topicName;

    public void Setup()
    {
        //Initialize ROS
        ros = ROSConnection.GetOrCreateInstance();
        tf = TFSystem.GetOrCreateInstance();
        //Initialize Publisher
        ros.RegisterPublisher<ImuMsg>(topicName);
        //Initialize Messages
        headerMsg = new HeaderMsg((uint)0, new TimeMsg(), frameName);
        quaternionMsg = new QuaternionMsg();
        angularVelocityMsg = new Vector3Msg();
        linearAccelarationMsg = new Vector3Msg();
        imuMsg = new ImuMsg();
        //Set operation mode to execute
        operationMode = OperationMode.EXEC;
    }

    public void Exec()
    {
        timeElapsed += Time.deltaTime;
        if (timeElapsed > (1.0f / publishMessageFrequency))
        {
            CalculateAndPublish();
            timeElapsed = 0;
        }

    }

    //Reference to:
    //Vector3 oldRotation;
    //Vector3 oldVelocity;
    //Vector3 oldPosition;
    //long oldTime;
    public void CalculateAndPublish()
    {
        //Get current timestamp
        long timeInterval = DateTimeOffset.Now.ToUnixTimeSeconds() - oldTime;

        //Header

        //Orientation
        quaternionMsg.w = this.gameObject.transform.rotation.w;
        quaternionMsg.y = this.gameObject.transform.rotation.z;
        quaternionMsg.x = this.gameObject.transform.rotation.x;
        quaternionMsg.z = this.gameObject.transform.rotation.y;

        //Angular Velocity
        angularVelocityMsg.y = -oldRotation.x + this.gameObject.transform.rotation.eulerAngles.x / timeInterval;
        angularVelocityMsg.x = oldRotation.z - this.gameObject.transform.rotation.eulerAngles.z / timeInterval;
        angularVelocityMsg.z = oldRotation.y - this.gameObject.transform.rotation.eulerAngles.y / timeInterval;

        oldRotation.y = this.gameObject.transform.rotation.eulerAngles.y;
        oldRotation.x = this.gameObject.transform.rotation.eulerAngles.x;
        oldRotation.z = this.gameObject.transform.rotation.eulerAngles.z;

        //Linear Accelaration
        linearAccelarationMsg.y = (-oldVelocity.x + (-oldPosition.x + this.gameObject.transform.position.x)) / timeInterval;
        linearAccelarationMsg.x = (oldVelocity.z - (oldPosition.z - this.gameObject.transform.position.z)) / timeInterval;
        linearAccelarationMsg.z = (oldVelocity.y - (oldPosition.y - this.gameObject.transform.position.y)) / timeInterval;

        oldPosition.y = this.gameObject.transform.position.y;
        oldPosition.x = this.gameObject.transform.position.x;
        oldPosition.z = this.gameObject.transform.position.z;

        oldVelocity.y = (oldPosition.y - this.gameObject.transform.position.y) / timeInterval;
        oldVelocity.x = (oldPosition.x - this.gameObject.transform.position.x) / timeInterval;
        oldVelocity.z = (oldPosition.y - this.gameObject.transform.position.y) / timeInterval;

        headerMsg.seq++;
        headerMsg.stamp.sec = (uint)DateTimeOffset.Now.ToUnixTimeSeconds();
        headerMsg.stamp.nanosec = (uint)DateTimeOffset.Now.ToUnixTimeMilliseconds() * 1000000;
        imuMsg.header = headerMsg;
        imuMsg.orientation = quaternionMsg;
        imuMsg.angular_velocity = angularVelocityMsg;
        imuMsg.linear_acceleration = linearAccelarationMsg;

        ros.Publish(topicName, imuMsg);
    }

    public void PublishTF()
    {
        tf.GetOrCreateFrame(frameName);
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
