using System;
using System.Collections.Generic;
using UnityEngine;
public class tankcontrol : MonoBehaviour {

    #region 变量
    //炮台
    public Transform turret;
    //旋转角度
    private float turretRotSpeed = 0.5f;
    private float turretRotTarget = 0;
    public Transform gun;
    private float maxRoll = 7f;
    private float minRoll = -8f;
    private float turretRollTarget = 0;
    //wheelscollider 
    public List<AxleInfo> axleInfos;
    private float motor = 0;
    public float maxMotorTorque;
    private float steering = 0;
    public float maxSteeringAngle;
    private float brakeTorque = 0;
    public float maxBrakeTorque = 100;
    private Transform wheels;
    private Transform tracks;
    //履带声音
    public AudioSource motorAudioSource;
    public AudioClip motorCilp;
    //子弹,发射间隔
    public GameObject bullet;
    public float lastShotTime;
    private float shootInterval = 0.5f;
    //生命
    public float maxHp = 100;
    public float Hp = 100;
    //摧毁特效
    public GameObject destoryEffect;
    //准星
    public Texture2D centerSingt;
    public Texture2D tankSight;
    //生命条
    public Texture2D hpBarBg;
    public Texture2D hpBar;
    //击杀图标
    public Texture2D killUI;
    private float KillUIStartTime = float.MinValue;
    //炮弹音源,音效
    public AudioSource shootAudioSource;
    public AudioClip shootClip;
    //AI
    private AI ai;
    //枚举坦克状态

    private float lastSendInfoTme = float.MinValue;
    public enum CtrlType
    {
        none, player, computer, net
    }
    public CtrlType ctrlType = CtrlType.player;

    //上一次的位置
    Vector3 lPos;
    Vector3 lRot;
    //预测的位置
    Vector3 fPos;
    Vector3 fRot;
    //时间间隔
    float delta = 1;
    //接收时间
    float lastRecvInfoTime = float.MinValue;

    #endregion


    void Start() {
        turret = transform.Find("turret");
        gun = turret.Find("gun");
        wheels = transform.Find("wheels");
        tracks = transform.Find("tracks");
        motorAudioSource = gameObject.AddComponent<AudioSource>();
        motorAudioSource.spatialBlend = 1;
        shootAudioSource = gameObject.AddComponent<AudioSource>();
        shootAudioSource.spatialBlend = 1;
        if (ctrlType == CtrlType.computer)
        {
            ai = gameObject.AddComponent<AI>();
            ai.tank = this;
        }

    }

    void Update()
    {
        if (ctrlType == CtrlType.net)
        {
            NetUpdate();
            return;
        }
        PlayerCtrl();
        CombuterCtrl();
        NoneCtrl();
        foreach (AxleInfo axleInfo in axleInfos)
        {
            if (axleInfo.steering)//旋转角
            {
                axleInfo.leftWheel.steerAngle = steering;
                axleInfo.rightWheel.steerAngle = steering;
            }
            if (axleInfo.motor)//马力
            {
                axleInfo.leftWheel.motorTorque = motor;
                axleInfo.rightWheel.motorTorque = motor;
            }
            if (true)//刹车
            {
                axleInfo.leftWheel.brakeTorque = brakeTorque;
                axleInfo.rightWheel.brakeTorque = brakeTorque;
            }
            if (axleInfos[1] != null) WheelsRotation(axleInfos[1].leftWheel);

            if (axleInfos[1] != null && axleInfo == axleInfos[1])
            {
                WheelsRotation(axleInfos[1].leftWheel);
                TrackMove();
            }
        }
        //CalExplodePoint();
        MotorSound();
        TurretRotation();
        TurretRoll();
    }

    

    #region 控制操作
    //横方向转向角
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

    //纵方向转向角
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
        gun.localEulerAngles = new Vector3(euler.x, localEuler.y, localEuler.z);
    }
    //wheel旋转角度
    public void WheelsRotation(WheelCollider collider)
    {
        if (wheels == null)
            return;
        Vector3 position;
        Quaternion rotation;
        collider.GetWorldPose(out position, out rotation);

        foreach (Transform wheel in wheels)
        {
            wheel.rotation = rotation;
        }
    }

    //移动
    public void TrackMove()
    {
        if (tracks == null) return;
        float offset = 0;
        if (wheels.GetChild(0) != null)
            offset = wheels.GetChild(0).localEulerAngles.x / 90f;
        foreach (Transform track in tracks)
        {
            MeshRenderer mr = track.gameObject.GetComponent<MeshRenderer>();
            if (mr == null) continue;
            Material mtl = mr.material;
            mtl.mainTextureOffset = new Vector2(0, offset);
        }
    }

    //马达声音
    void MotorSound()
    {
        if (motor != 0 && !motorAudioSource.isPlaying)
        {
            motorAudioSource.loop = true;
            motorAudioSource.clip = motorCilp;
            motorAudioSource.Play();
        }
        else if (motor == 0)
        {
            motorAudioSource.Pause();
        }
    }

    //生成炮弹
    public void Shoot()
    {
        if (Time.time - lastShotTime < shootInterval)
        {
            return;
        }
        if (bullet == null) return;
        //BeAttacked(40);
        Vector3 pos = gun.position + gun.forward * 5;
        GameObject bulletObj = (GameObject)Instantiate(bullet, pos, gun.rotation);
        Bullet bulletCmp = bulletObj.GetComponent<Bullet>();
        if (bulletCmp != null)
        {
            bulletCmp.attactTank = this.gameObject;
        }
        if (ctrlType == CtrlType.player) SendShootInfo(bulletObj.transform);
        lastShotTime = Time.time;
        shootAudioSource.PlayOneShot(shootClip);
    }
    #endregion
    #region 控制类型
    //角色控制
    public void PlayerCtrl()
    {
        if (ctrlType != CtrlType.player) return;
        motor = maxMotorTorque * Input.GetAxis("Vertical");
        steering = maxSteeringAngle * Input.GetAxis("Horizontal");
        TargetSignPos();
        brakeTorque = 0;
        foreach (AxleInfo axlenInfo in axleInfos)
        {
            if (axlenInfo.leftWheel.rpm > 5 && motor < 0)
                brakeTorque = maxBrakeTorque;
            else if (axlenInfo.leftWheel.rpm < -5 && motor > 0)
                brakeTorque = maxBrakeTorque;
            continue;
        }
        if (Input.GetMouseButton(0))
        {
            Shoot();
        }
        if (Time.time - lastSendInfoTme > 0.2f)
        {
            SendUnitInfo();
            lastSendInfoTme = Time.time;
        }
    }
    //电脑控制
    public void CombuterCtrl()
    {
        if (ctrlType != CtrlType.computer)
            return;
        Vector3 rot = ai.GetTurrgetTarget();
        turretRotTarget = rot.y;
        turretRollTarget = rot.x;
        if (ai.Isshoot())
        {
            Shoot();
        }
        steering = ai.GetSteering();
        motor = ai.GetMotor();
        brakeTorque = ai.GetBrakeTorque();
    }

    //无人控制
    public void NoneCtrl()
    {
        if (ctrlType != CtrlType.none)
            return;
        motor = 0;
        steering = 0;
        brakeTorque = maxBrakeTorque / 2;
    }
    #endregion
    #region 图标判定

    //击中目标
    public void BeAttacked(float att, GameObject attackTank)
    {
        if (Hp <= 0)
        {
            return;
        }
        Hp -= att;
        if (ai != null)
        {
            ai.OnAttecked(attackTank);
        }
        if (Hp <= 0)
        {
            Battle.instance.IsWin(attackTank);
            Debug.Log(attackTank);
            GameObject destoryObj = (GameObject)Instantiate(destoryEffect);
            destoryObj.transform.SetParent(transform, false);
            destoryObj.transform.localPosition = Vector3.zero;
            ctrlType = CtrlType.none;
            if (attackTank != null)
            {
                tankcontrol Tankcmp = attackTank.GetComponent<tankcontrol>();
                if (Tankcmp != null && Tankcmp.ctrlType == CtrlType.player)
                    Tankcmp.startDrawKill();
            }
        }
    }
    private void startDrawKill()
    {
        KillUIStartTime = Time.time;
    }
    private void DrawKillUI()
    {
        if (Time.time - KillUIStartTime < 1f)
        {
            Rect rect = new Rect(Screen.width / 2 - killUI.width / 2,
                30, killUI.width, killUI.height);
            GUI.DrawTexture(rect, killUI);
        }
    }
    //计算目标角度
    public void TargetSignPos()
    {
        Vector3 hitPoint = Vector3.zero;
        RaycastHit raycastHit;
        Vector3 centerVec = new Vector3(Screen.width / 2, Screen.height / 2, 0);
        Ray ray = Camera.main.ScreenPointToRay(centerVec);
        if (Physics.Raycast(ray, out raycastHit, 400.0f))
        {
            hitPoint = raycastHit.point;
        }
        else
        {
            hitPoint = ray.GetPoint(400);
        }
        Vector3 dir = hitPoint - turret.position;
        Quaternion angle = Quaternion.LookRotation(dir);

        turretRotTarget = angle.eulerAngles.y;
        turretRollTarget = angle.eulerAngles.x;
        //Transform targetcube = GameObject.Find("TargetCube").transform;
        //targetcube.position = hitPoint;
    }

    //实际射击位置

    public Vector3 CalExplodePoint()
    {
        Vector3 hitPoint = Vector3.zero;
        RaycastHit hit;
        Vector3 pos = gun.position + gun.forward * 5;
        Ray ray = new Ray(pos, gun.forward);
        if (Physics.Raycast(ray, out hit, 400.0f))
        {
            hitPoint = hit.point;
        }
        else
        {
            hitPoint = ray.GetPoint(400);
        }
        //Transform explodeCube = GameObject.Find("ExplodeCube").transform;
        //explodeCube.position = hitPoint;
        //Debug.Log(hitPoint);
        return hitPoint;
    }

    //绘制准星
    public void DrawSight()
    {
        Vector3 explodePoint = CalExplodePoint();
        Vector3 screenPoint = Camera.main.WorldToScreenPoint(explodePoint);
        Rect tankRect = new Rect(screenPoint.x - tankSight.width / 2,
            Screen.height - screenPoint.y - tankSight.height / 2, tankSight.width, tankSight.height);
        GUI.DrawTexture(tankRect, tankSight);
        Rect centerRect = new Rect(Screen.width / 2 - centerSingt.width / 2,
            Screen.height / 2 - centerSingt.height / 2, centerSingt.width, centerSingt.height);
        GUI.DrawTexture(centerRect, centerSingt);

    }

    //生命条绘制
    public void DrawHp()
    {
        Rect bgRect = new Rect(30, Screen.height - hpBarBg.height - 15, hpBarBg.width, hpBarBg.height);
        GUI.DrawTexture(bgRect, hpBar);
        string text = Mathf.Ceil(Hp).ToString() + "/" + Mathf.Ceil(maxHp).ToString();
        Rect textRect = new Rect(bgRect.x + 80, bgRect.y - 10, 50, 50);
        GUI.Label(textRect, text);
    }
    private void OnGUI()
    {
        if (ctrlType != CtrlType.player) return;
        DrawSight();
        DrawHp();
        DrawKillUI();
    }
    #endregion
    #region 联网交互
    public void SendUnitInfo()
    {
        ProtocolBytes proto = new ProtocolBytes();
        proto.AddString("UpdateUnitInfo");

        Vector3 pos = transform.position;
        Vector3 rot = transform.eulerAngles;
        proto.AddFloat(pos.x);
        proto.AddFloat(pos.y);
        proto.AddFloat(pos.z);
        proto.AddFloat(rot.x);
        proto.AddFloat(rot.y);
        proto.AddFloat(rot.z);

        float angleY = turretRotTarget;
        proto.AddFloat(angleY);

        float angleX = turretRollTarget;
        proto.AddFloat(angleX);
        NetMgr.srvConn.Send(proto);
    }

    //预测
    public void NetForecastInfo(Vector3 nPos, Vector3 nRot)
    {
        //预测位置
        fPos = lPos + (nPos - lPos) * 2;
        fRot = lRot + (nRot - lRot) * 2;
        if (Time.time - lastRecvInfoTime > 0.3f)
        {
            fPos = nPos;
            fRot = nRot;
        }
        //时间
        delta = Time.time - lastRecvInfoTime;
        //更新
        lPos = nPos;
        lRot = nRot;
        lastRecvInfoTime = Time.time;
    }
    //初始位置
    public void initNetCtrl()
    {
        lPos = transform.position;
        lRot = transform.eulerAngles;
        fPos = lPos;
        fRot = lRot;
        Rigidbody r = GetComponent<Rigidbody>();
        r.constraints = RigidbodyConstraints.FreezeAll;
    }
    //网络同步
    private void NetUpdate()
    {
        Vector3 pos = transform.position;
        Vector3 rot = transform.eulerAngles;
        //更新位置
        if (delta > 0)
        {
            transform.position = Vector3.Lerp(pos, fPos, delta);
            transform.rotation = Quaternion.Lerp(Quaternion.Euler(rot), Quaternion.Euler(fRot), delta);
        }
        //炮塔旋转
        TurretRotation();
        TurretRoll();
        //音效
        NetWheelsRotation();
    }

    private void NetWheelsRotation()
    {
        float z = transform.InverseTransformPoint(fPos).z;
        if (Math.Abs(z) < 0.1f||delta<=0.05f)
        {
            motorAudioSource.Pause();
            return;
        }
        //轮子转动
        foreach (Transform wheel in wheels)
        {
            wheel.localEulerAngles += new Vector3(360 * z / delta, 0, 0);
        }
        float offset = -wheels.GetChild(0).localEulerAngles.x / 90f;
        //履带转动
        foreach (Transform track in tracks)
        {
            MeshRenderer mr = track.gameObject.GetComponent<MeshRenderer>();
            if (mr == null) continue;
            Material mtl = mr.material;
            mtl.mainTextureOffset = new Vector2(0, offset);
        }
        //声音
        if(!motorAudioSource.isPlaying)
        {
            motorAudioSource.loop = true;
            motorAudioSource.clip = motorCilp;
            motorAudioSource.Play();
        }
    }

    public void NetTurretTarget(float y,float x)
    {
        turretRotTarget = y;
        turretRollTarget = x;
    }

    public void SendShootInfo(Transform bulletTrans)
    {
        ProtocolBytes proto = new ProtocolBytes();
        proto.AddString("Shooting");
        Vector3 pos = bulletTrans.position;
        Vector3 rot = bulletTrans.eulerAngles;
        proto.AddFloat(pos.x);
        proto.AddFloat(pos.y);
        proto.AddFloat(pos.z);
        proto.AddFloat(rot.x);
        proto.AddFloat(rot.y);
        proto.AddFloat(rot.z);
        NetMgr.srvConn.Send(proto);
    }

    public void NetShoot(Vector3 pos, Vector3 rot)
    {
        GameObject buttleObj = (GameObject)Instantiate(bullet, pos, Quaternion.Euler(rot));
        Bullet bulletCmp = buttleObj.GetComponent<Bullet>();
        if (bulletCmp != null) bulletCmp.attactTank = gameObject;

        //音效
        shootAudioSource.PlayOneShot(shootClip);
    }

    public void SendHit(string id,float damage)
    {
        ProtocolBytes proto = new ProtocolBytes();
        proto.AddString("Hit");
        proto.AddString(id);
        proto.AddFloat(damage);
        NetMgr.srvConn.Send(proto);
    }
    public void NetBeAttacked(float att, GameObject attackTank)
    {
        Debug.LogError("123213");
        if (Hp <= 0)
            return;
        if (Hp > 0)
            Hp -= att;
        if(Hp<=0)
        {
            ctrlType = CtrlType.none;
            GameObject destoryObj = (GameObject)Instantiate(destoryEffect);
            destoryObj.transform.SetParent(transform, false);
            destoryObj.transform.localPosition = Vector3.zero;

            //播放击杀提示
            if (attackTank != null)
            {
                tankcontrol tankcmp = attackTank.GetComponent<tankcontrol>();
                if (tankcmp != null && tankcmp.ctrlType == CtrlType.player)
                    tankcmp.startDrawKill();
            }
        }
    }
    #endregion
}
