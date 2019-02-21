using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class tankcontrol : MonoBehaviour {

    // Use this for initialization
    public Transform turret;
    private float turretRotSpeed = 0.5f;
    private float turretRotTarget = 0;
    public Transform gun;
    private float maxRoll = 7f;
    private float minRoll = -4f;
    private float turretRollTarget = 0;
    public List<AxleInfo> axleInfos;
    private float motor = 0;
    public float maxMotorTorque;
    private float steering = 0;
    public float maxSteeringAngle;
    private float brakeTorque = 0;
    public float maxBrakeTorque = 100;
    void Start() {
        turret = transform.Find("turret");
        gun = turret.Find("gun");
    }

    // Update is called once per frame
    public void TurretRotation()
    {
        //Debug.Log(turretRotTarget);
        if (Camera.main == null) return;
        if (turret == null) return;
        float angle = turret.eulerAngles.y - turretRotTarget;
        if (angle < 0) angle += 360;
        //Debug.Log(turretRotSpeed);
        //Debug.Log(angle);
        if (angle > turretRotSpeed && angle < 180)
            turret.Rotate(0f, -turretRotSpeed, 0f);
        else if (angle > 180 && angle < 360 - turretRotSpeed)
            turret.Rotate(0f, turretRotSpeed, 0f);

    }
    public void TurretRoll()
    {
        if (Camera.main == null) return;
        if (gun == null) return;
        //float angle = gun.eulerAngles.x - turretRollTarget;
        Vector3 worldEuler = gun.eulerAngles;
        Vector3 localEuler = gun.localEulerAngles;
        worldEuler.x = turretRollTarget;
        gun.eulerAngles = worldEuler;
        Vector3 euler = gun.localEulerAngles;
        if (euler.x > 180) euler.x -= 360;
        if (euler.x > maxRoll) euler.x = maxRoll;
        if (euler.x < minRoll) euler.x = minRoll;
        Debug.Log(euler.x);
        gun.localEulerAngles = new Vector3(euler.x, localEuler.y, localEuler.z);
    }
    public void PlayerCtrl()
    {
        motor = maxMotorTorque * Input.GetAxis("Vertical");
        steering = -1 * maxSteeringAngle * Input.GetAxis("Horizontal");
        turretRotTarget = Camera.main.transform.eulerAngles.y;
        turretRollTarget = Camera.main.transform.eulerAngles.x;
        brakeTorque = 0;
        foreach(AxleInfo axlenInfo in axleInfos)
        {
            if (axlenInfo.leftWheel.rpm > 5 && motor < 0)
                brakeTorque = maxBrakeTorque;
            else if (axlenInfo.leftWheel.rpm < -5 && motor > 0)
                brakeTorque = maxBrakeTorque;
            continue;
        }
    }
    void Update () {
        PlayerCtrl();
        foreach(AxleInfo axleInfo in axleInfos)
        {
            if(axleInfo.steering)
            {
                axleInfo.leftWheel.steerAngle = steering;
                axleInfo.rightWheel.steerAngle = steering;
            }
            if(axleInfo.motor)
            {
                axleInfo.leftWheel.motorTorque = motor;
                axleInfo.rightWheel.motorTorque = motor;
            }
            if(true)
            {
                axleInfo.leftWheel.brakeTorque = brakeTorque;
                axleInfo.rightWheel.brakeTorque = brakeTorque;
            }
        }
        TurretRotation();
        TurretRoll();
    }
}
