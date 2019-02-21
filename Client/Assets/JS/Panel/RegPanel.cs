using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RegPanel : PanelBase
{
    private InputField IdInput;
    private InputField pwInput;
    private InputField repInput;
    private Button regBtn;
    private Button closeBtn;
    #region 生命周期
    public override void Init(params object[] args)
    {
        base.Init(args);
        skinPath = "RegPanel";
        layer = PanelMgr.PanelLayer.Panel;

    }
    public override void OnShowing()
    {
        base.OnShowing();
        Transform skinTrans = skin.transform;
        IdInput = skinTrans.Find("IDInput").GetComponent<InputField>();
        pwInput = skinTrans.Find("PWInput").GetComponent<InputField>();
        repInput = skinTrans.Find("RepInput").GetComponent<InputField>();
        regBtn = skinTrans.Find("RegBtn").GetComponent<Button>();
        closeBtn = skinTrans.Find("CloseBtn").GetComponent<Button>();

        regBtn.onClick.AddListener(OnRegClick);
        closeBtn.onClick.AddListener(OnCloseClick);
    }
    #endregion
    private void OnCloseClick()
    {
        //throw new NotImplementedException();
        PanelMgr.instance.OpenPanel<LoginPanel>("");
        Close();
    }

    private void OnRegClick()
    {
        //throw new NotImplementedException();
        if(IdInput.text==""||pwInput.text=="")
        {
            //Debug.Log("账户密码为空");
            PanelMgr.instance.OpenPanel<TipPanel>("", "用户名和密码不能为空");
            return;
        }
        //两次密码不同
        if(pwInput.text!=repInput.text)
        {
            PanelMgr.instance.OpenPanel<TipPanel>("", "两次输入的密码不同");
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
        ProtocolBytes protocol = new ProtocolBytes();
        protocol.AddString("Register");
        protocol.AddString(IdInput.text);
        protocol.AddString(pwInput.text);
        Debug.Log("发送" + protocol.GetDesc());
        NetMgr.srvConn.Send(protocol, OnRegBack);
    }

    private void OnRegBack(ProtocolBase protocol)
    {
        //throw new NotImplementedException();
        ProtocolBytes proto = (ProtocolBytes)protocol;
        int start = 0;
        string protoName = proto.GetString(start, ref start);
        int ret = proto.GetInt(start, ref start);
        if(ret==0)
        {
            PanelMgr.instance.OpenPanel<TipPanel>("", "注册成功!");
            PanelMgr.instance.OpenPanel<LoginPanel>("");
            Close();
        }
        else
        {
            PanelMgr.instance.OpenPanel<TipPanel>("", "注册失败,请更换用户名!");
        }
    }
}
