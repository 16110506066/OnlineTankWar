using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultiBattle : MonoBehaviour {

    public static MultiBattle instance;
    //预设
    public GameObject[] tankPrefabs;

    public Dictionary<string, BattleTank> list = new Dictionary<string, BattleTank>();

    
	void Start () {
        instance = this;	
	}
	void Update () {
		
	}

    public int GetCamp(GameObject tankObj)
    {
        foreach(BattleTank mt in list.Values)
        {
            if (mt.tank.gameObject == tankObj)
                return mt.camp;
        }
        return 0;
    }
    //相同阵营
    public bool IsSameCamp(GameObject tank1,GameObject tank2)
    {
        return GetCamp(tank1) == GetCamp(tank2);
    }

    //清理
    public void ClearBattle()
    {
        if (list.Count == 0) return;
        list.Clear();
        GameObject[] tanks = GameObject.FindGameObjectsWithTag("Tankk");
        for(int i=0;i<tanks.Length;i++)
        {
            Destroy(tanks[i]);
        }
    }
    public void StartBattle(ProtocolBytes proto)
    {

        int start = 0;
        string protoName = proto.GetString(start, ref start);
        Debug.Log(protoName+"yingyingying");
        if(protoName!= "Fight")
        {
            return;
        }
        int count = proto.GetInt(start, ref start);
        ClearBattle();
        for(int i=0;i<count;i++)
        {
            string id = proto.GetString(start, ref start);
            int team = proto.GetInt(start, ref start);
            int swopID = proto.GetInt(start, ref start);
            GenerateTank(id, team, swopID);
        }
        NetMgr.srvConn.msgDist.AddListener("UpdateUnitInfo", RecvUpdateUnitInfo);
        NetMgr.srvConn.msgDist.AddListener("Shooting",RecvShooting);
        NetMgr.srvConn.msgDist.AddListener("Hit", RecvHit);
        NetMgr.srvConn.msgDist.AddListener("Result", RecvResult);
    }

    private void RecvResult(ProtocolBase protocol)
    {
        int start = 0;
        ProtocolBytes proto = (ProtocolBytes)protocol;
        string protoName = proto.GetString(start, ref start);
        int winTeam = proto.GetInt(start, ref start);

        string id = GameMgr.instance.id;
        BattleTank bt = list[id];
        if (bt.camp == winTeam)
        {
            PanelMgr.instance.OpenPanel<WinPanel>("", 1);
        }
        else
        {
            PanelMgr.instance.OpenPanel<WinPanel>("", 0);
        }
        NetMgr.srvConn.msgDist.DelListener("Shooting", RecvShooting);
        NetMgr.srvConn.msgDist.DelListener("Hit", RecvHit);
        NetMgr.srvConn.msgDist.DelListener("UpdateUnitInfo", RecvUpdateUnitInfo);
        NetMgr.srvConn.msgDist.DelListener("Result", RecvResult);

    }

    private void RecvHit(ProtocolBase protocol)
    {
        int start = 0;
        Debug.LogError("RecvHit");
        ProtocolBytes proto = (ProtocolBytes)protocol;
        string protoName = proto.GetString(start, ref start);
        string attId = proto.GetString(start, ref start);
        string defId = proto.GetString(start, ref start);
        float hurt = proto.GetFloat(start, ref start);
        if(!list.ContainsKey(attId))
        {
            Debug.Log("RecvHit attBt==null+ " + attId);
            return;
        }
        BattleTank attBt = list[attId];
        if(!list.ContainsKey(defId))
        {
            Debug.Log("Recvh defBt ==null" + defId);
            return;
        }
        BattleTank defBt = list[defId];

        defBt.tank.NetBeAttacked(hurt, attBt.tank.gameObject);
    }

    private void RecvShooting(ProtocolBase protocol)
    {
        int start = 0;
        ProtocolBytes proto = (ProtocolBytes)protocol;
        string protoName = proto.GetString(start, ref start);
        string id = proto.GetString(start, ref start);
        Vector3 pos;
        Vector3 rot;
        pos.x = proto.GetFloat(start, ref start);
        pos.y = proto.GetFloat(start, ref start);
        pos.z = proto.GetFloat(start, ref start);
        rot.x = proto.GetFloat(start, ref start);
        rot.y = proto.GetFloat(start, ref start);
        rot.z = proto.GetFloat(start, ref start);

        if(!list.ContainsKey(id))
        {
            Debug.Log("RecvShooting bt==null");
            return;
        }
        BattleTank bt = list[id];
        if(id==GameMgr.instance.id)
        {
            return;
        }
        bt.tank.NetShoot(pos, rot);
    }

    private void RecvUpdateUnitInfo(ProtocolBase protocol)
    {
        //解析
        int start = 0;
        ProtocolBytes proto = (ProtocolBytes)protocol;
        string protoName = proto.GetString(start, ref start);
        string id = proto.GetString(start, ref start);
        Vector3 nPos;
        Vector3 nRot;
        nPos.x = proto.GetFloat(start, ref start);
        nPos.y = proto.GetFloat(start, ref start);
        nPos.z = proto.GetFloat(start, ref start);
        nRot.x = proto.GetFloat(start, ref start);
        nRot.y = proto.GetFloat(start, ref start);
        nRot.z = proto.GetFloat(start, ref start);

        float turretY = proto.GetFloat(start, ref start);
        float gunX = proto.GetFloat(start, ref start);

        //
        Debug.Log("RecvUpdateUnitInfo " + id);
        if(!list.ContainsKey(id))
        {
            Debug.Log("RecvUpdateInfo bt ==NUll");
            return;
        }
        BattleTank bt = list[id];
        if (id == GameMgr.instance.id)
            return;
        bt.tank.NetForecastInfo(nPos, nRot);
        bt.tank.NetTurretTarget(turretY, gunX);

    }

    public void GenerateTank(string id,int team,int swopID)
    {
        Transform sp = GameObject.Find("SwopPoints").transform;
        Transform swopTrans;
        if(team==1)
        {
            Transform teamSwop = sp.GetChild(0);
            swopTrans = teamSwop.GetChild(swopID - 1);

        }
        else
        {
            Transform teamSwop = sp.GetChild(1);
            swopTrans = teamSwop.GetChild(swopID - 1);

        }
        if(swopTrans == null)
        {
            Debug.LogError("GenerateTank 出生点错误!");
            return;
        }
        if(tankPrefabs.Length<2)
        {
            Debug.LogError("预设不够");
        }
        GameObject tankobj = (GameObject)Instantiate(tankPrefabs[team - 1]);
        tankobj.name = id;
        tankobj.transform.position = swopTrans.position;
        tankobj.transform.rotation = swopTrans.rotation;

        BattleTank bt = new BattleTank();
        bt.tank = tankobj.GetComponent<tankcontrol>();
        bt.camp = team;
        list.Add(id,bt);

        if (id == GameMgr.instance.id)
        {
            bt.tank.ctrlType = tankcontrol.CtrlType.player;
            CameraFollow cf = Camera.main.gameObject.GetComponent<CameraFollow>();
            GameObject target = bt.tank.gameObject;
            cf.SetTarget(target);
        }
        else
        {
            bt.tank.ctrlType = tankcontrol.CtrlType.net;
            bt.tank.initNetCtrl();

        }
    }
}
