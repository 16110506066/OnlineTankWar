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
    public List<axleinfo>axleinfos;
    private float motor = 0;
    public float maxMotorTorque;
    private flaot brakeTorque = 0;
    public float maxBrakeTorque = 100;
    private float steering = 0;
    public float maxsteeringAngle;
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
    public void playerCtrl()
    {
        motor = maxMotorTorque * Input.GetAxis("Vertical");
        steering = maxSteeringAngle * Input.getAxis("Horizontal");
    }
    void f()
    {
        float steer = 30;
        float x = Input.GetAxis("Horizontal");
        transform.Rotate(0, x * steer * Time.deltaTime,0);
        float speed = 5f;
        float y = Input.GetAxis("Vertical");
        Vector3 s = y * transform.forward * speed * Time.deltaTime;
        transform.position += s;
    }
	void Update () {
        
        turretRotTarget = Camera.main.transform.eulerAngles.y;
        turretRollTarget = Camera.main.transform.eulerAngles.x;
        TurretRotation();
        TurretRoll();
    }
}
