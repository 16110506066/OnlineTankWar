using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoginPanel : PanelBase {

    private InputField idInput;
    private InputField pwInput;
    private Button loginBtn;
    private Button regBtn;
    #region 生命周期
    // Use this for initialization
    public override void Init(params object[] args)
    {
        base.Init(args);
        skinPath = "LoginPanel";
        layer = PanelMgr.PanelLayer.Panel;
    }
    public override void OnShowing()
    {
        base.OnShowing();
        Transform skinTrans = skin.transform;
        idInput = skinTrans.Find("IDInput").GetComponent<InputField>();
        pwInput = skinTrans.Find("PWInput").GetComponent<InputField>();
        loginBtn = skinTrans.Find("LoginBtn").GetComponent<Button>();
        regBtn = skinTrans.Find("RegBtn").GetComponent<Button>();

        loginBtn.onClick.AddListener(OnLoginClick);
        regBtn.onClick.AddListener(OnRegClick);
    }
    #endregion
    private void OnRegClick()
    {
        //throw new NotImplementedException();
        PanelMgr.instance.OpenPanel<RegPanel>("");
        Close();
    }

    private void OnLoginClick()
    {
        //throw new NotImplementedException();
        if(idInput.text==""||pwInput.text=="")
        {
            Debug.Log("用户名或密码不能为空!");
            PanelMgr.instance.OpenPanel<TipPanel>("", "用户或密码不能为空!");
            return;
        }
        if(NetMgr.srvConn.status!=Connection.Status.Connected)
        {
            string host = "127.0.0.1";
            int port = 1234;
            NetMgr.srvConn.proto = new ProtocolBytes();
            if(!NetMgr.srvConn.Connect(host, port))
            {
                PanelMgr.instance.OpenPanel<TipPanel>("", "连接网络失败,请检查网络!");
            }

        }
        //发送
        ProtocolBytes protocol = new ProtocolBytes();
        protocol.AddString("Login");
        GameMgr.instance.name = idInput.text;
        protocol.AddString(idInput.text);
        protocol.AddString(pwInput.text);
        Debug.Log("发送" + protocol.GetDesc());
        NetMgr.srvConn.Send(protocol, OnLoginBack);
    }

    private void OnLoginBack(ProtocolBase protocol)
    {
        //throw new NotImplementedException();
        ProtocolBytes proto = (ProtocolBytes)protocol;
        int start = 0;
        string protoName = proto.GetString(start,ref start);
        int ret = proto.GetInt(start, ref start);
        if (ret == 0)
        {
            Debug.Log("登陆成功!");
            //开始游戏
            PanelMgr.instance.OpenPanel<TipPanel>("", "登录成功!");
            
            PanelMgr.instance.OpenPanel<RoomListPanel>("");
            GameMgr.instance.id = idInput.text;
            //Walk.instance.StartGame(idInput.text);
            Close();
        }
        else
        {
            //Debug.Log("登录失败");
            PanelMgr.instance.OpenPanel<TipPanel>("", "登录失败,请检查用户名密码!");
        }
    }
}