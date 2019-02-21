using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace serviess
{
    public partial class HandlePlayerMsg
    {
        //获取积分
        //返回 : int分数
        public void MsgGetScore(Player player ,ProtocolBase protoBase)
        {
            ProtocolBytes protocolRet=new ProtocolBytes();
            protocolRet.AddString("GetScore");
            protocolRet.AddInt(player.data.score);
            player.Send(protocolRet);
            Console.WriteLine("MsgGetScore " + player.id + player.data.score);
        }
        //加分
        //无返回
        public void MsgAddScore(Player player, ProtocolBase protoBase)
        {
            int start = 0;  
            ProtocolBytes protocol = (ProtocolBytes)protoBase;
            string protoName = protocol.GetString(start, ref start);
            player.data.score += 1;
            Console.WriteLine("MsgAddScore " + player.id + " " + player.data.score.ToString());
        }
        //获取列表
        public void MsgGetList(Player player, ProtocolBase protocol)
        {
            Scene.Scener.instance.SendPlayerList(player);
        }
        //更新信息
        public void MsgUpdateInfo(Player player, ProtocolBase protoBase)
        {
            //获取分数
            int start = 0;
            ProtocolBytes protocol = (ProtocolBytes)protoBase;
            string protoname = protocol.GetString(start, ref start);
            float x = protocol.GetFloat(start, ref start);
            float y = protocol.GetFloat(start, ref start);
            float z = protocol.GetFloat(start, ref start);
            int score = protocol.GetInt(start, ref start);
            Scene.Scener.instance.UpdateInfo(protoname, x, y, z, score);

            //广播
            ProtocolBytes protocolret = new ProtocolBytes();
            protocolret.AddString("UpdateInfo");
            protocolret.AddString(player.id);
            protocolret.AddFloat(x);
            protocolret.AddFloat(y);
            protocolret.AddFloat(z);
            protocolret.AddInt(score);
            ServNet.instance.Broadcast(protocolret);
        }
        //获取玩家信息
        public void MsgGetAchieve(Player player, ProtocolBase protoBase)
        {
            ProtocolBytes protocolRet = new ProtocolBytes();
            protocolRet.AddString("GetAchieve");
            protocolRet.AddInt(player.data.win);
            protocolRet.AddInt(player.data.fail);
            player.Send(protocolRet);
            Console.WriteLine("MsgGetScore " + player.id + player.data.win);
        }

    }
}
