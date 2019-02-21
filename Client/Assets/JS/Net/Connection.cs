using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using UnityEngine;

public class Connection{
    const int BUFFER_SIZE = 1024;
    private Socket socket;

    
    private byte[] readbuff = new byte[BUFFER_SIZE];
    private int buffCount = 0;

    //分包
    public Int32 msgLength = 0;
    public byte[] lenBytes = new byte[sizeof(Int32)];
    //协议
    public ProtocolBase proto;
    //心跳时间
    public float lastTickTime = 0;
    public float heartBeatTime = 30;
    //消息分发
    public MsgDistribution msgDist = new MsgDistribution();

    public enum Status
    {
        None,Connected,
    };
    public Status status = Status.None;
    // Use this for initialization
    //连接服务器
    public void Update()
    {
        msgDist.Update();
        if(status==Status.Connected)
        {
            if(Time.time-lastTickTime >heartBeatTime)
            {
                ProtocolBase protocol = NetMgr.GetHeatBeatProtocol();
                Send(protocol);
                lastTickTime = Time.time;
            }
        }
    }
    public bool Connect(string host ,int port)
    {
        try
        {
            //socket
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Connect(host, port);

            socket.BeginReceive(readbuff, buffCount, BUFFER_SIZE - buffCount, SocketFlags.None, ReceiveCb, readbuff);

            Debug.Log("success");
            status = Status.Connected;
            return true;
        }
        catch(Exception e)
        {
            Debug.Log("失败" +e.Message);
            return false;
        }
    }
    public bool Close()
    {
        try
        {
            socket.Close();
            return true;
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
            return false;
        }
    }
    private void ReceiveCb(IAsyncResult ar)
    {
        //throw new NotImplementedException();
        try
        {

            int count = socket.EndReceive(ar);
            buffCount = buffCount + count;
            ProcessData();
            socket.BeginReceive(readbuff, buffCount, BUFFER_SIZE - buffCount, SocketFlags.None, ReceiveCb, buffCount);
        }
        catch(Exception e)
        {
            Debug.Log(e.Message+"接收数据出错");
            return;
        }
    }


    private void ProcessData()
    {
        //throw new NotImplementedException();
        //黏包分包处理
        if (buffCount < sizeof(Int32)) return;
        //包长
        Array.Copy(readbuff, lenBytes, sizeof(Int32));
        msgLength = BitConverter.ToInt32(lenBytes, 0);
        if (buffCount < msgLength + sizeof(Int32))
            return;
        ProtocolBase protocol = proto.Decode(readbuff, sizeof(Int32), msgLength);
        Debug.Log(protocol.GetDesc()+"解码收到");
        lock(msgDist.msgList)
        {
            msgDist.msgList.Add(protocol);
        }
        //清除已处理的消息
        int count = buffCount - msgLength - sizeof(Int32);
        Array.Copy(readbuff,sizeof(Int32)+msgLength,readbuff,0,count);
        buffCount = count;
        if(buffCount>0)
        {
            ProcessData();

        }
    }
    public bool Send(ProtocolBase protocol )
    {
        if(status!=Status.Connected)
        {
            Debug.Log("[Connection]还没有连接");
            return true;
        }
        
        byte[] b = protocol.Encode();
        byte[] length = BitConverter.GetBytes(b.Length);

        byte[] sendbuff = length.Concat(b).ToArray();
        socket.Send(sendbuff);
        Debug.Log("发发送消息" + protocol.GetDesc());
        return true;
    }
    public bool Send(ProtocolBase protocol,string cbName,MsgDistribution.Delegate cb)
    {
        if (status != Status.Connected) return false;
        msgDist.AddOnceListener(cbName, cb);
        return Send(protocol);
    }
    public bool Send(ProtocolBase protocol,MsgDistribution.Delegate cb)
    {
        string cbName = protocol.GetName();
        return Send(protocol, cbName, cb);
    }
}
