using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RoomListPanel : PanelBase {

    private Text idText;
    private Text winText;
    private Text lostText;
    private Transform content;
    private GameObject roomPrefab;
    private Button closeBtn;
    private Button newBtn;
    private Button reflashBtn;
    #region 
    public override void Init(params object[] args)
    {
        base.Init(args);
        skinPath = "RoomListPanel";
        layer = PanelMgr.PanelLayer.Panel;
    }
    #endregion
    public override void OnShowing()
    {
        base.OnShowing();
        
        //获取transform
        Transform skinTrans = skin.transform;
        Transform listTrans = skinTrans.Find("ListImage");
        Transform winTrans = skinTrans.Find("WinImage");
        //获取成绩栏部件
        idText = winTrans.Find("IDText").GetComponent<Text>();
        winText = winTrans.Find("WinText").GetComponent<Text>();
        lostText = winTrans.Find("LostText").GetComponent<Text>();
        //获取列表部件
        Transform scroolRect = listTrans.Find("ScrollRect");
        content = scroolRect.Find("Content");
        Debug.Log(content.Find("RoomPrefab").gameObject);
        roomPrefab = content.Find("RoomPrefab").gameObject;
        roomPrefab.SetActive(false);
        closeBtn = listTrans.Find("CloseBtn").GetComponent<Button>();
        newBtn = listTrans.Find("NewBtn").GetComponent<Button>();
        reflashBtn = listTrans.Find("ReflashBtn").GetComponent<Button>();

        //添加按钮监听
        closeBtn.onClick.AddListener(OncloseClick);
        newBtn.onClick.AddListener(OnNewClick);
        reflashBtn.onClick.AddListener(OnReflashClick);

        //添加协议监听

        NetMgr.srvConn.msgDist.AddListener("GetAchieve", RecvGetAchieve);
        NetMgr.srvConn.msgDist.AddListener("GetRoomList", RecvGetRoomList);

        //发送查询
        ProtocolBytes protocol = new ProtocolBytes();
        protocol.AddString("GetRoomList");
        NetMgr.srvConn.Send(protocol);
        

        protocol = new ProtocolBytes();
        protocol.AddString("GetAchieve");
        NetMgr.srvConn.Send(protocol);
    }

    private void RecvGetRoomList(ProtocolBase protocol)
    {
        ClearRoomUnit();
        ProtocolBytes proto = (ProtocolBytes)protocol;
        int start = 0;
        string protoName = proto.GetString(start, ref start);
        int count = proto.GetInt(start, ref start);
        for(int i=0;i<count;i++)
        {
            int num = proto.GetInt(start, ref start);
            int status = proto.GetInt(start, ref start);
            GenerateRoomUnit(i,num,status);
        }
    }
    //创建一个房间单元
    //i :房间号,num :人数 ,status :状态
    private void GenerateRoomUnit(int i, int num, int status)
    {
        //添加房间
        content.GetComponent<RectTransform>().sizeDelta = new Vector2(0, (i + 1) * 110);
        GameObject o = Instantiate(roomPrefab);
        o.transform.SetParent(content);
        o.SetActive(true);

        //房间信息
        Transform trans = o.transform;
        Text nameText = trans.Find("nameText").GetComponent<Text>();
        Text statusText = trans.Find("StatusText").GetComponent<Text>();
        Text countText = trans.Find("CountText").GetComponent<Text>();
        Button Btn = trans.Find("JoinButton").GetComponent<Button>();
        nameText.text = "序号: " + (i+1).ToString();
        if (status == 1)
        {
            countText.color = Color.black;
            countText.text = "状态: 准备中";
        }
        else
        {
            countText.color = Color.red;
            countText.text = "状态: 进行中";
        }
        statusText.text = "人数: " + num;
        
        Btn.name = i.ToString();
        Btn.onClick.AddListener(delegate(){OnJoinClick(Btn.name);}); 
    }

    private void OnJoinClick(string name)
    {
        ProtocolBytes protocol = new ProtocolBytes();
        protocol.AddString("EnterRoom");
        protocol.AddInt(int.Parse(name));
        NetMgr.srvConn.Send(protocol,OnJoinClickBack);
        Debug.Log("请求进入房间:" + name);
    }

    private void OnJoinClickBack(ProtocolBase protocol)
    {
        //参数解析
        ProtocolBytes proto = (ProtocolBytes)protocol;
        int start = 0;
        string protoName = proto.GetString(start, ref start);
        int ret = proto.GetInt(start, ref start);
        //处理
        if(ret==0)
        {
            PanelMgr.instance.OpenPanel<TipPanel>("", "进入房间成功");
            PanelMgr.instance.OpenPanel<RoomPanel>("");
            Close();
        }
        else
        {
            PanelMgr.instance.OpenPanel<TipPanel>("", "进入房间失败");
        }
    }

    private void ClearRoomUnit()
    {
        for (int i = 0; i < content.childCount; i++)
        {
            if (content.GetChild(i).name.Contains("Clone"))
            {
                Destroy(content.GetChild(i).gameObject);
            }
        }
    }

    private void RecvGetAchieve(ProtocolBase protocol)
    {
        //协议解析
        ProtocolBytes proto =(ProtocolBytes) protocol;
        int start = 0;
        string protoName = proto.GetString(start, ref start);
        int win = proto.GetInt(start, ref start);
        int lost = proto.GetInt(start, ref start);

        //处理
        idText.text = "指挥官:" + GameMgr.instance.id;
        winText.text = win.ToString();
        lostText.text = lost.ToString();
    }

    private void OnReflashClick()
    {
        ProtocolBytes protocol = new ProtocolBytes();
        protocol.AddString("GetRoomList");
        NetMgr.srvConn.Send(protocol);
    }

    private void OnNewClick()
    {
        ProtocolBytes protocol = new ProtocolBytes();
        protocol.AddString("CreateRoom");
        NetMgr.srvConn.Send(protocol, OnNewBack);
        
    }

    private void OnNewBack(ProtocolBase protocol)
    {
        ProtocolBytes proto = (ProtocolBytes)protocol;
        int start = 0;
        string protoName = proto.GetString(start, ref start);
        int ret = proto.GetInt(start, ref start);

        if(ret==0)
        {
            PanelMgr.instance.OpenPanel<TipPanel>("","创建成功!");
            PanelMgr.instance.OpenPanel<RoomPanel>("");
            Close();
        }
        else
        {
            PanelMgr.instance.OpenPanel<TipPanel>("","创建失败");
        }
    }

    private void OncloseClick()
    {
        ProtocolBytes protocol = new ProtocolBytes();
        protocol.AddString("Logout");
        NetMgr.srvConn.Send(protocol, OnCloseBack);
    }

    private void OnCloseBack(ProtocolBase proto)
    {
        PanelMgr.instance.OpenPanel<TipPanel>("", "登出成功");
        PanelMgr.instance.OpenPanel<LoginPanel>("", "");
        NetMgr.srvConn.Close();
    }

    public override void OnClosing()
    {
        NetMgr.srvConn.msgDist.DelListener("GetAchieve", RecvGetAchieve);
        NetMgr.srvConn.msgDist.DelListener("GetRoomList", RecvGetRoomList);
    }
}
