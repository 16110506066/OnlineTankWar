using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour {

    public float speed = 100f;
    public GameObject explode;
    public float maxLiftTime;
    public float instantiateTime;
    public GameObject attactTank;
    public AudioClip explodeClip;
    // Use this for initialization
    void Start () {
        instantiateTime = Time.time;	
	}

    // Update is called once per frame
    void Update()
    {
        transform.position += transform.forward * speed * Time.deltaTime;
        if (Time.time - instantiateTime > maxLiftTime)
            Destroy(gameObject);
    }

    //获取攻击力
    private float GetAttc()
    {
        float att = 100 - (Time.time - instantiateTime) * 40;
        if (att < 1) att = 1;
        return att;
    }
    private void OnCollisionEnter(Collision collision)
    {
        tankcontrol tank = collision.gameObject.GetComponent<tankcontrol>();
        if (collision.gameObject == tank)
        {
            return;
        }
        GameObject explodeObj = (GameObject)Instantiate(explode, transform.position, transform.rotation);
        AudioSource audio = explodeObj.AddComponent<AudioSource>();
        audio.spatialBlend = 1;
        audio.PlayOneShot(explodeClip);
        Destroy(gameObject);
        if (tank != null)
        {
            float att = GetAttc();
            tank.BeAttacked(att, attactTank);
        }
        tankcontrol tankcmp = collision.gameObject.GetComponent<tankcontrol>();
        if (tankcmp != null && attactTank.name == GameMgr.instance.id)
        {
            float att = GetAttc();
            tankcmp.SendHit(tankcmp.name, att);
        }
    }
}
