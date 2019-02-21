using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RoomPanel :PanelBase{

    private List<Transform> prefabs = new List<Transform>();
    private Button closeBtn;
    private Button startBtn;

    public override void Init(params object[] args)
    {
        base.Init(args);
        skinPath = "RoomPanel";
        layer = PanelMgr.PanelLayer.Panel;
    }
    public override void OnShowing()
    {
        base.OnShowing();
        Transform skinTrans = skin.transform;
        //获取组件
        for (int i = 0; i < 6; i++)
        {
            string name = "PlayerPrefab" + i;
            Transform prefab=skinTrans.Find(name);
            prefabs.Add(prefab);
        }
        closeBtn = skinTrans.Find("CloseBtn").GetComponent<Button>();
        startBtn = skinTrans.Find("StartBtn").GetComponent<Button>();
        closeBtn.onClick.AddListener(OnCloseClick);
        startBtn.onClick.AddListener(OnStartClick);

        NetMgr.srvConn.msgDist.AddListener("GetRoomInfo", GetRoomInfo);
        NetMgr.srvConn.msgDist.AddListener("Fight", Fight);

        ProtocolBytes protocol = new ProtocolBytes();
        protocol.AddString("GetRoomInfo");
        NetMgr.srvConn.Send(protocol);
    }
    public override void OnClosing()
    {
        //base.OnClosing();
        NetMgr.srvConn.msgDist.DelListener("GetRoomInfo",GetRoomInfo);
        NetMgr.srvConn.msgDist.DelListener("Fight",Fight);

    }
    public void Fight(ProtocolBase proto)
    {
        ProtocolBytes protocol = (ProtocolBytes)proto;
        Debug.Log(protocol);
        MultiBattle.instance.StartBattle(protocol);
        Close();

    }

    private void GetRoomInfo(ProtocolBase proto)
    {
        ProtocolBytes protocol = (ProtocolBytes)proto;
        int start = 0;
        string protoName = protocol.GetString(start, ref start);
        int count = protocol.GetInt(start, ref start);
        Debug.Log(count);
        int i = 0;
        for(i=0;i<count;i++)
        {
            string id = protocol.GetString(start, ref start);
            int team = protocol.GetInt(start, ref start);
            int win = protocol.GetInt(start, ref start);
            int lost = protocol.GetInt(start, ref start);
            int isOnwer = protocol.GetInt(start, ref start);
            //处理
            Transform trans = prefabs[i];
            Text text = trans.Find("Text").GetComponent<Text>();
            string str = string.Format("名字: {0}\n阵营: {1}\n胜利: {2}\n失败: {3}\n",
                id, team == 1 ? "红" : "蓝", win.ToString(), lost.ToString());
            if(id==GameMgr.instance.id)
            {
                str += "【我自己】";
            }
            if(isOnwer==1)
            {
                str += "房主♔♚♕♛";
            }
            text.text = str;
            if (team == 1)
            {
                text.color = Color.red;
                trans.GetComponent<Image>().color = new Color(176,15,0);
            }
            else
            {
                text.color = Color.blue;
                trans.GetComponent<Image>().color = new Color(20, 176, 0);
            }

        }
        for(;i<6;i++)
        {
            Transform trans = prefabs[i];
            Text text = trans.Find("Text").GetComponent<Text>();
            text.text = "[等待玩家]";
            text.color = Color.gray;
            trans.GetComponent<Image>().color = new Color(30, 30, 30);
        }
    }


    private void OnStartClick()
    {
        ProtocolBytes protocol = new ProtocolBytes();
        protocol.AddString("StartFight");
        NetMgr.srvConn.Send(protocol,OnStartBack);
    }

    private void OnStartBack(ProtocolBase proto)
    {
        ProtocolBytes protocol = (ProtocolBytes)proto;
        int start = 0;
        string proName = protocol.GetString(start, ref start);
        int ret = protocol.GetInt(start, ref start);
        if(ret!=0)
        {
            PanelMgr.instance.OpenPanel<TipPanel>("", "开启失败!至少每一队以一个人才能开始,只有房主才能开始游戏");

        }
    }

    private void OnCloseClick()
    {
        ProtocolBytes protocol = new ProtocolBytes();
        protocol.AddString("LeaveRoom");
        NetMgr.srvConn.Send(protocol, OnCloseBack);
    }

    private void OnCloseBack(ProtocolBase proto)
    {
        ProtocolBytes protocol = (ProtocolBytes)proto;
        int start = 0;
        String proName = protocol.GetString(start, ref start);
        int ret = protocol.GetInt(start, ref start);
        //处理
        if (ret == 0)
        {
            PanelMgr.instance.OpenPanel<TipPanel>("", "退出成功");
            PanelMgr.instance.OpenPanel<RoomListPanel>("");
            Close();
        }
        else
        {
            PanelMgr.instance.OpenPanel<TipPanel>("", "退出失败!");
        }
    }
}
