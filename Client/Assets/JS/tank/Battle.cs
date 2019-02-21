using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Battle : MonoBehaviour {
    public static Battle instance;
    public BattleTank[] battleTanks;
    //添加预设
    public GameObject[] tankPerfabs;
	// Use this for initialization
	void Start () {
        instance = this;
        //StartTwoCampBattle(3, 3);
    }
    // Update is called once per frame
    void Update () {
		
	}

    //获取阵营
    public int GetCamp(GameObject tankobj)
    {
        for(int i=0;i<battleTanks.Length;i++)
        {
            BattleTank battleTank = battleTanks[i];
            if (battleTank == null) return 0;
            if (battleTank.tank.gameObject == tankobj) return battleTank.camp;
        }
        return 0;
    }
    //是否同一阵营
    public bool IsSameCamp(GameObject tank1,GameObject tank2)
    {
        Debug.Log(GetCamp(tank1) == GetCamp(tank2));
        return GetCamp(tank1) == GetCamp(tank2);
    }

    //是否获胜
    public bool IsWin(int camp)
    {
        for (int i = 0; i < battleTanks.Length; i++)
        {
            BattleTank Tank = battleTanks[i];
            Debug.Log(Tank + " " + Tank.camp + " " + Tank.tank.Hp);
            if (Tank.camp != camp)
                if (Tank.tank.Hp > 0) return false;

        }
        Debug.Log("阵营" + camp + "获胜");
        PanelMgr.instance.OpenPanel<WinPanel>("", camp);
        return true;
    }
    public bool IsWin(GameObject attTank)
    {
        int camp = GetCamp(attTank);
        return IsWin(camp);
    }
    //清空地图
    public void ClearBattle()
    {
        GameObject[] tanks = GameObject.FindGameObjectsWithTag("Tankk");
        for (int i = 0; i < tanks.Length; i++)
            Destroy(tanks[i]);
    }

    //生成
    public void StartTwoCampBattle(int n1, int n2)
    {
        Transform sp = GameObject.Find("SwopPoints").transform;
        Transform spCamp1 = sp.GetChild(0);
        Transform spCamp2 = sp.GetChild(1);
        if (spCamp1.childCount < n1 || spCamp2.childCount < n2)
        {
            Debug.LogError("出生点数量不够");
            return;
        }
        if(tankPerfabs.Length<2)
        {
            Debug.LogError("坦克预设数量不够");
            return;
        }
        ClearBattle();
        battleTanks = new BattleTank[n1 + n2];
        for(int i=0;i<n1;i++)
        {
            generateTank(1, i, spCamp1, i);
        }
        for(int i=0;i<n2;i++)
        {
            generateTank(2, i, spCamp2, n1+i);
        }
        tankcontrol tankCmp = battleTanks[0].tank;
        tankCmp.ctrlType = tankcontrol.CtrlType.player;
        CameraFollow cf = Camera.main.gameObject.GetComponent<CameraFollow>();
        GameObject target = tankCmp.gameObject;
        cf.SetTarget(target);
    }

    //生成场景
    private void generateTank(int camp, int num, Transform spCamp, int index)
    {
        //throw new NotImplementedException();
        Transform trans = spCamp.GetChild(num);
        Vector3 pos = trans.position;
        Quaternion rot = trans.rotation;
        GameObject perfab = tankPerfabs[camp - 1];
        GameObject tankObj = (GameObject)Instantiate(perfab, pos, rot);
        tankcontrol tankCmp = tankObj.GetComponent<tankcontrol>();
        battleTanks[index] = new BattleTank();
        battleTanks[index].tank = tankCmp;
        battleTanks[index].camp = camp;
    }
}
