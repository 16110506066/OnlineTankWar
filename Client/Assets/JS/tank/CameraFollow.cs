using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour {

    public float distance = 15;
    public float rot = 270f * Mathf.PI * 2 / 360;
    private float roll = 30f * Mathf.PI * 2 / 360;
    private GameObject target;
    public float rotspeed = 0.2f;
    public float rollspeed = 0.2f;
    private float maxRoll = 30f * Mathf.PI * 2 / 360;
    private float minRoll = 0f * Mathf.PI * 2 / 360;
    public float maxDistance = 22f;
    public float minDistance = 5f;
    public float zoomspeed = 0.2f;
	// Use this for initialization
	void Start () {
        //target = GameObject.Find("tank");
        //SetTarget(target);
    }
	public void SetTarget(GameObject target)
    {
        if (target.transform.Find("cameraPoint") != null)
            this.target = target.transform.Find("cameraPoint").gameObject;
        else this.target = target;
    }
	// Update is called once per frame
    void Rotate()
    {
        float w = Input.GetAxis("Mouse X");
        rot -= w * rotspeed;

    }
    void Roll()
    {
        float w = Input.GetAxis("Mouse Y") * rollspeed * 0.5f;
        roll -= w;
        if (roll > maxRoll) roll = maxRoll;
        if (roll < minRoll) roll = minRoll;
    }
    void Zoom()
    {
        if(Input.GetAxis("Mouse ScrollWheel")>0)
        {
            if(distance>minDistance) distance -= zoomspeed;
        }
        else if(Input.GetAxis("Mouse ScrollWheel")<0)
        {
            if (distance < maxDistance) distance += zoomspeed;
        }
    }
	void LateUpdate () {
        //Debug.Log(target);
        
        if (target==null)
        {
            return;
        }
        if(Camera.main==null)
        {
            return;
        }
        Rotate();
        Roll();
        Zoom();
        Vector3 targetPos = target.transform.position;
        Vector3 cameraPos;
        float d = distance * Mathf.Cos(roll);
        float height = distance * Mathf.Sin(roll);
        cameraPos.x = targetPos.x + d * Mathf.Cos(rot);
        cameraPos.z = targetPos.z + d * Mathf.Sin(rot);
        cameraPos.y = targetPos.y + height;
        Camera.main.transform.position = cameraPos;
        Camera.main.transform.LookAt(target.transform);
	}
}
