using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetMgr {
    public static Connection srvConn = new Connection();
    //public static Connection platformConn=new Connection();
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	public static void Update () {
        srvConn.Update();
        //platformConn.Update();
	}

    public static ProtocolBase GetHeatBeatProtocol()
    {
        //throw new NotImplementedException();
        ProtocolBytes protocol = new ProtocolBytes();
        protocol.AddString("HeatBeat");
        return protocol;
    }
}
