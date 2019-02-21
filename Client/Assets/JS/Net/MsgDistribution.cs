using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System;
public class MsgDistribution
{

    //帧处理消息数
    public int num = 15;
    //列表
    public List<ProtocolBase> msgList = new List<ProtocolBase>();
    //委托
    public delegate void Delegate(ProtocolBase proto);
    //事件监听
    private Dictionary<string, Delegate> eventDict = new Dictionary<string, Delegate>();
    private Dictionary<string, Delegate> onceDict = new Dictionary<string, Delegate>();

    public void Update()
    {
        for(int i=0;i<num;i++)
        {
            if (msgList.Count > 0)
            {
                DispatchMsgEvent(msgList[0]);
                lock (msgList)
                {
                    msgList.RemoveAt(0);
                }
            }
            else break;             
        }
    }

    private void DispatchMsgEvent(ProtocolBase protocol)
    {
        //throw new NotImplementedException();
        string name = protocol.GetName();
        Debug.Log("消息分发处理" + name);
        if (eventDict.ContainsKey(name))
        {
            eventDict[name](protocol);
        }
        if(onceDict.ContainsKey(name))
        {
            onceDict[name](protocol);
            onceDict[name] = null;
            onceDict.Remove(name);
        }
    }
    public void AddListener(string name, Delegate cb)
    {
        if (eventDict.ContainsKey(name))
            eventDict[name] += cb;
        else eventDict[name] = cb;
    }
    public void AddOnceListener(string name,Delegate cb)
    {
        if (onceDict.ContainsKey(name))
        {
            onceDict[name] += cb;
        }
        else onceDict[name] = cb;
    }
    public void DelListener(string name,Delegate cb)
    {
        if(eventDict.ContainsKey(name))
        {
            eventDict[name] -= cb;
            if(eventDict[name]==null)
            {
                eventDict.Remove(name);
            }
        }
    }
    public void DelOnceLister(string name,Delegate cb)
    {
        if(onceDict.ContainsKey(name))
        {
            onceDict[name] -= cb;
            if (onceDict[name] == null)
            {
                onceDict.Remove(name);
            }
        }
    }
}
