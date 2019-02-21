using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Path{

    public Vector3[] waypoints;//路径
    public int Index = -1;//当前路径索引
    public Vector3 waypoint;//当前寻路点
    bool isLoop = false;//是否循环
    public float deviation = 5;//误差
    public bool isFinish = false;//是否完成

    //是否到达当前寻路点
    public bool IsReach(Transform trans)
    {
        Vector3 pos = trans.position;
        float distance = Vector3.Distance(waypoint, pos);
        return distance < deviation;
    }

    //变成下一个寻路点
    public void NextWaypoint()
    {
        if (Index < 0)
            return;
        if (Index < waypoints.Length - 1)
            Index++;
        else
        {
            if (isLoop) Index = 0;
            else isFinish = true;
        }
        waypoint = waypoints[Index];
    }
    //生成路点
    public void InitByobj(GameObject obj, bool isLoop = false)
    {
        int length = obj.transform.childCount;
        if (length == 0)
        {
            waypoints = null;
            Index = -1;
            //Debug.LogWarning("Path.initByobjlength==0");
            return;
        }
        waypoints = new Vector3[length];
        for (int i = 0; i < length; i++)
        {
            Transform trans = obj.transform.GetChild(i);
            waypoints[i] = trans.position;
        }
        Index = 0;
        waypoint = waypoints[Index];
        this.isLoop = isLoop;
        isFinish = false;
        //throw new NotImplementedException();
    }

    //基于NavMesh路径初始化
    public void InitByNavMeshPath(Vector3 pos,Vector3 targetPos)
    {
        //Debug.Log(targetPos);
        waypoints = null;
        Index = -1;
        NavMeshPath navPath = new NavMeshPath();
        bool hasFoundPath = NavMesh.CalculatePath(pos, targetPos, NavMesh.AllAreas, navPath);
        if (!hasFoundPath) return;
        int length = navPath.corners.Length;
        waypoints = new Vector3[length];
        for (int i = 0; i < length; i++)
            waypoints[i] = navPath.corners[i];
        Index = 0;
        waypoint = waypoints[Index];
        isFinish = false;
    }
    //辅助调试路径
    public void DrawWaypoints()
    {
        if (waypoints == null) return;
        int length = waypoints.Length;
        for(int i=0;i<length;i++)
        {
            if (Index == i)
                Gizmos.DrawSphere(waypoints[i], 1);
            else Gizmos.DrawCube(waypoints[i], Vector3.one);
        }
    }
}
