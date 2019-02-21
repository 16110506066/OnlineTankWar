using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace serviess
{
    public partial class HandleConnMsg
    {
        public void MsgHeatBeat(Conn conn, ProtocolBase protoBase)
        {
            conn.lastTickTime = Sys.Getimetamp();
            Console.WriteLine("[更新心跳时间]" + conn.GetAdress());
        }
        public void MsgRegister(Conn conn, ProtocolBase protoBase)
        {
            int start = 0;
            ProtocolBytes protocol = (ProtocolBytes)protoBase;
            string protoName = protocol.GetString(start, ref start);
            string id = protocol.GetString(start, ref start);
            string pw = protocol.GetString(start, ref start);
            string strFormat = "[收到注册协议]" + conn.GetAdress();
            Console.WriteLine(strFormat + " 用户名:" + id + " 密码:" + pw);
            //构建返回协议
            protocol =new ProtocolBytes();
            protocol.AddString("Register");
            //注册
            if(DataMgr.instance.Register(id,pw))
            {
                protocol.AddInt(0);
                //创建角色
                
            }
            else protocol.AddInt(-1);
            DataMgr.instance.CreatePlayer(id);
            //返回给客户端
            conn.Send(protocol);
        }
        //登录
        //Login str 用户名 str 密码
        //-1失败,0成功
        public void MsgLogin(Conn conn, ProtocolBase protobase)
        {
            //获取数值
            int start = 0;
            ProtocolBytes protocol = (ProtocolBytes)protobase;
            string protoname = protocol.GetString(start, ref start);
            string id = protocol.GetString(start, ref start);
            string pw = protocol.GetString(start, ref start);
            string strFormat = "[收到登录协议]" + conn.GetAdress();
            Console.WriteLine(strFormat + " 用户名:" + id + " 密码:" + pw);
            //返回协议
            ProtocolBytes protocolRet = new ProtocolBytes();
            protocolRet.AddString("Login");
            //验证
            if (!DataMgr.instance.CheckPassWord(id, pw))
            {
                protocolRet.AddInt(-1);
                conn.Send(protocolRet);
                return;
            }
            //是否已经登录
            ProtocolBytes protocolLogout = new ProtocolBytes();
            protocolLogout.AddString("Logout");
            if (!Player.KickOff(id, protocolLogout))
            {
                protocolRet.AddInt(-1);
                conn.Send(protocolRet);
                return;
            }

            PlayerData playerData = DataMgr.instance.GettPlayerData(id);
            if (playerData == null)
            {
                protocolRet.AddInt(-1);
                conn.Send(protocolRet);
                return;
            }
            
            conn.player = new Player(id, conn);
            conn.player.data = playerData;
            //事件触发
            ServNet.instance.handlePlayerEvent.OnLogin(conn.player);
            //返回
            protocolRet.AddInt(0);
            conn.Send(protocolRet);
            return;
        }
        //返回协议 : 0-正常下线
        public void MsgLogout(Conn conn, ProtocolBase protoBase)
        {
            ProtocolBytes protocol = new ProtocolBytes();
            protocol.AddString("Logout");
            protocol.AddInt(0);
            if (conn.player == null)
            {
                conn.Send(protocol);
                conn.Close();
            }
            else
            {
                conn.Send(protocol);
                conn.player.Logout();
            }
        }
        
    }
}
