using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class ProtocolBase
{
    //解码器
    public virtual ProtocolBase Decode(byte[] readbuff, int start, int length)
    {
        return new ProtocolBase();
    }
    //编码器
    public virtual byte[] Encode()
    {
        return new byte[] { };
    }
    //协议名称,用于消息分发
    public virtual string GetName()
    {
        return "";
    }
    //描述
    public virtual string GetDesc()
    {
        return "";
    }
}
