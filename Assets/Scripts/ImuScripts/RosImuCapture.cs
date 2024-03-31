using System.Collections;
using System.Collections.Generic;
using RosMessageTypes.Sensor;
using RosMessageTypes.Geometry;
using RosMessageTypes.Std;
using UnityEngine;
using System;
using Unity.Robotics.ROSTCPConnector;

public class RosImuCapture : MonoBehaviour
{
    private OperationMode operationMode;

    enum OperationMode
    {
        SETUP,
        EXEC,
        WAIT
    }

    public float publishMessageFrequency;
    private float timeElapsed;

    private ROSConnection ros;
    public string topicName;

    public void Setup()
    {
        //Initialize ROS
        ros = ROSConnection.GetOrCreateInstance();
        //Initialize Publisher
        ros.RegisterPublisher<ImuMsg>(topicName);
        //Set operation mode to execute
        operationMode = OperationMode.EXEC;
    }

    public void Exec()
    {
        timeElapsed += Time.deltaTime;
        if (timeElapsed > publishMessageFrequency)
        {
            CalculateAndPublish();
            timeElapsed = 0;
        }

    }

    //Reference to:
    Vector3 oldRotation;
    Vector3 oldVelocity;
    Vector3 oldPosition;
    long oldTime;
    public void CalculateAndPublish()
    {
        //Get current timestamp
        long timeInterval = DateTimeOffset.Now.ToUnixTimeSeconds() - oldTime;

        //Header
        HeaderMsg headerMsg = new HeaderMsg();

        //Orientation
        QuaternionMsg quaternionMsg = new QuaternionMsg();
        quaternionMsg.w = this.gameObject.transform.rotation.w;
        quaternionMsg.y = this.gameObject.transform.rotation.z;
        quaternionMsg.x = this.gameObject.transform.rotation.x;
        quaternionMsg.z = this.gameObject.transform.rotation.y;

        //Angular Velocity
        Vector3Msg angularVelocityMsg = new Vector3Msg();
        angularVelocityMsg.y = Math.Abs(oldRotation.z - this.gameObject.transform.rotation.eulerAngles.z) / timeInterval;
        angularVelocityMsg.x = Math.Abs(oldRotation.x - this.gameObject.transform.rotation.eulerAngles.x) / timeInterval;
        angularVelocityMsg.z = Math.Abs(oldRotation.y - this.gameObject.transform.rotation.eulerAngles.y) / timeInterval;

        oldRotation.y = this.gameObject.transform.rotation.eulerAngles.y;
        oldRotation.x = this.gameObject.transform.rotation.eulerAngles.x;
        oldRotation.z = this.gameObject.transform.rotation.eulerAngles.z;

        //Linear Accelaration
        Vector3Msg linearAccelarationMsg = new Vector3Msg();
        linearAccelarationMsg.y = Math.Abs(oldVelocity.z - (Math.Abs(oldPosition.z - this.gameObject.transform.position.z) / timeInterval));
        linearAccelarationMsg.x = Math.Abs(oldVelocity.x - (Math.Abs(oldPosition.x - this.gameObject.transform.position.x) / timeInterval));
        linearAccelarationMsg.z = Math.Abs(oldVelocity.y - (Math.Abs(oldPosition.y - this.gameObject.transform.position.y) / timeInterval));

        oldPosition.y = this.gameObject.transform.position.y;
        oldPosition.x = this.gameObject.transform.position.x;
        oldPosition.z = this.gameObject.transform.position.z;

        oldVelocity.y = Math.Abs(oldPosition.y - this.gameObject.transform.position.y) / timeInterval;
        oldVelocity.x = Math.Abs(oldPosition.x - this.gameObject.transform.position.x) / timeInterval;
        oldVelocity.z = Math.Abs(oldPosition.y - this.gameObject.transform.position.y) / timeInterval;
        
        ImuMsg imuMsg = new ImuMsg();
        imuMsg.header = headerMsg;
        imuMsg.orientation = quaternionMsg;
        imuMsg.angular_velocity = angularVelocityMsg;
        imuMsg.linear_acceleration = linearAccelarationMsg;

        ros.Publish(topicName, imuMsg);
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
