using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class AI : MonoBehaviour {
    //坦克
    public tankcontrol tank;
    //锁定坦克
    private GameObject target;
    //视野
    private float sightDistance = 30;
    //上次搜寻时间
    private float lastSearchTargetTime = 0;
    //搜寻间隔
    private float searchTargetInterval = 3;

    private int indexe=0;
    //路径
    private Path path = new Path();

    //动画状态
    public enum Status
    {
        Patrol, Attack
    }
    private Status status = Status.Patrol;

    //上次更新路径信息
    private float lastUpdatewaypointTime = float.MinValue;

    private float updateWaypointtInterval = 10;
    private void Start()
    {
        InitWaypoint();
    }
    void Update()
    {

        //Debug.Log(tank.ctrlType);
        if (tank.ctrlType != tankcontrol.CtrlType.computer)
        {
            return;
        }
        if (path.IsReach(this.transform))
        {
            path.NextWaypoint();
        }
        if(path.isFinish)InitWaypoint();
        TargetUpdate();
        if (status == Status.Patrol)
            PatrolUPdate();
        else if (status == Status.Attack)
            AttackUpdate();
    }

    private void OnDrawGizmos()
    {
        path.DrawWaypoints();
    }
    //改变状态
    public void ChangeStatus(Status status)
    {
        if (status == Status.Patrol)
            PatrolStart();
        else if (status == Status.Attack)
            AddackStart();
    }
    //初始化路径
    void InitWaypoint()
    {
        GameObject obj = GameObject.Find("WayPtcon");
        //if (obj) path.InitByobj(obj);
        if (indexe >= obj.transform.childCount) indexe = 0;
        Debug.Log(obj.transform.GetChild(indexe));
        if (obj && obj.transform.GetChild(indexe) != null)
        {
            path.InitByNavMeshPath(transform.position, obj.transform.GetChild(indexe++).position);
        }
    }

    //搜寻目标
    void TargetUpdate()
    {
        float interval = Time.time - lastSearchTargetTime;
        if (interval < searchTargetInterval)
            return;
        lastSearchTargetTime = Time.time;
        if (target != null)
            HasTarget();
        else Notarget();
    }
    //没有目标的情况,搜索视野中的坦克
    private void Notarget()
    {
        //throw new NotImplementedException();
        float minHp = float.MaxValue;
        GameObject[] targets = GameObject.FindGameObjectsWithTag("Tankk");
        for (int i = 0; i < targets.Length; i++)
        {
            tankcontrol tank = targets[i].GetComponent<tankcontrol>();
            if (tank == null) continue;

            if (targets[i] == gameObject)
                continue;
            if (Battle.instance.IsSameCamp(targets[i], gameObject)) continue;
            if (tank.ctrlType == tankcontrol.CtrlType.none)
                continue;
            Vector3 pos = transform.position;
            Vector3 targetpos = targets[i].transform.position;
            if (Vector3.Distance(pos, targetpos) > sightDistance)
                continue;
            if (minHp > tank.Hp)
            {
                target = tank.gameObject;
                minHp = tank.Hp;
            }
        }
        if (target != null) Debug.Log("获取目标" + target.name);
    }

    //被攻击
    public void OnAttecked(GameObject attackTank)
    {
        if (Battle.instance.IsSameCamp(attackTank, gameObject)) return;
        target = attackTank;
        Debug.Log(target);
    }
    //有目标,判断目标丢失
    void HasTarget()
    {
        tankcontrol targetTank = target.GetComponent<tankcontrol>();
        Vector3 pos = transform.position;
        Vector3 targetPos = target.transform.position;
        if (targetTank.ctrlType == tankcontrol.CtrlType.none)
        {
            Debug.Log("目标死亡,丢失目标");
            target = null;
        }
        else if (Vector3.Distance(pos, targetPos) > sightDistance)
        {
            Debug.Log("距离过远,丢失目标");
            target = null;
        }

    }


    //攻击中
    private void AttackUpdate()
    {
        //throw new NotImplementedException();

        if (target == null)
            ChangeStatus(Status.Patrol);
        float interval = Time.time - lastUpdatewaypointTime;
        if (interval < updateWaypointtInterval) return;
        lastUpdatewaypointTime = Time.time;
        Vector3  targetPos = target.transform.position;
        path.InitByNavMeshPath(transform.position, targetPos);
    }
    //巡逻中
    private void PatrolUPdate()
    {
        //throw new NotImplementedException();

        if (target != null)
            ChangeStatus(Status.Attack);
        float interval = Time.time - lastUpdatewaypointTime;
        if (interval < updateWaypointtInterval) return;
        lastUpdatewaypointTime = Time.time;
        if (path.waypoints == null||path.isFinish)
        {
            GameObject obj = GameObject.Find("WayPton");
            if(obj!=null)
            {
                int count = obj.transform.childCount;
                if (count == 0) return;
                int index = Random.Range(0, count);
                Vector3 targetPos = obj.transform.GetChild(index).position;
                path.InitByNavMeshPath(transform.position, targetPos);
            }
        }
    }
    //攻击开始
    private void AddackStart()
    {
        //throw new NotImplementedException();
        Vector3 targetPos = target.transform.position;
        path.InitByNavMeshPath(transform.position, targetPos);
    }
    //巡逻开始
    private void PatrolStart()
    {
        //throw new NotImplementedException();
    }
    //获取炮管和炮塔的目标角度
    public  Vector3 GetTurrgetTarget()
    {
        if (target == null)
        {
            float y = transform.eulerAngles.y;
            Vector3 rot = new Vector3(0, y, 0);
            return rot;
        }
        else
        {
            Vector3 pos = transform.position;
            Vector3 targetpos = target.transform.position;
            Vector3 vec = targetpos - pos;
            return Quaternion.LookRotation(vec).eulerAngles;
        }
        //throw new NotImplementedException();
    }

    //判断是否开炮
    public bool Isshoot()
    {
        if (target == null)
            return false;
        float turretRoll = tank.turret.eulerAngles.y;
        float angle = turretRoll - GetTurrgetTarget().y;
        if (angle < 0) angle += 360;
        if (angle < 30 || angle > 330) return true;
        return false;
    }

    //获取转向角
    public float GetSteering()
    {
        if (tank == null)
            return 0;
        Vector3 itp = transform.InverseTransformPoint(path.waypoint);
        if (itp.x > path.deviation / 5)
            return tank.maxSteeringAngle;
        else if (itp.x < -path.deviation / 5)
            return -tank.maxSteeringAngle;
        else return 0;
    }
    //获取马力
    public float GetMotor()
    {
        if (tank == null) return 0;
        Vector3 itp = transform.InverseTransformPoint(path.waypoint);
        float x = itp.x;
        float z = itp.z;
        float r = 6;
        if (z < 0 && Mathf.Abs(x) < -z && Mathf.Abs(x) < r)
            return -tank.maxMotorTorque;
        else return tank.maxMotorTorque;

    }
    //获取阻力
    public float GetBrakeTorque()
    {
        if (path.isFinish) return tank.maxMotorTorque;
        else return 0;
    }
}
