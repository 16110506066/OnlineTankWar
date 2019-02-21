using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace serviess
{
    class HandleRoomMsg
    {
    }
    public partial class HandlePlayerMsg
    {
        //获得列表
        public void MsgGetRoomList(Player player, ProtocolBase protoBase)
        {
            player.Send(RoomMgr.instance.GetRoomList());
        }
        //创建
        public void MsgCreateRoom(Player player, ProtocolBase protoBase)
        {
            ProtocolBytes protocol = new ProtocolBytes();
            protocol.AddString("CreateRoom");
            //条件检测
            if (player.tempData.status != PlayerTempData.Status.None)
            {
                protocol.AddInt(-1);
                player.Send(protocol);
                Console.WriteLine("MsgCreateRoom Fail" + player.id);
                return;
            }
            RoomMgr.instance.CreateRoom(player);
            protocol.AddInt(0);
            player.Send(protocol);
            Console.WriteLine("MsgCreateRoom ok" + player.id);
        }

        //加入
        public void MsgEnterRoom(Player player, ProtocolBase proto)
        {
            int start = 0;
            ProtocolBytes protocol = (ProtocolBytes)proto;
            string protoName = protocol.GetString(start, ref start);
            int index = protocol.GetInt(start, ref start);
            Console.WriteLine("[收到 MsetenterRoom] " + player.id + "" + index);

            protocol = new ProtocolBytes();
            protocol.AddString("EnterRoom");

            if (index < 0 || index >= RoomMgr.instance.list.Count)
            {
                Console.WriteLine("MsgEnterRoom Index error" + player.id);
                protocol.AddInt(-1);
                player.Send(protocol);
                return;
            }
            Room room = RoomMgr.instance.list[index];

            //判断房间状态
            if (room.status != Room.Status.Prepare)
            {
                Console.WriteLine("MsgEnterRoom statuserr" + player.id);
                protocol.AddInt(-1);
                player.Send(protocol);
                return;
            }
            if (room.AddPlayer(player))
            {
                room.Broadcast(room.GetRoomInfo());
                protocol.AddInt(0);
                player.Send(protocol);
            }
            else
            {
                Console.WriteLine("MsgEnterRoom maxPlayer erro " + player.id);
                protocol.AddInt(-1);
                player.Send(protocol);
            }
        }
        //获得房间信息
        public void MsgGetRoomInfo(Player player,ProtocolBase protoBase)
        {
            if(player.tempData.status!=PlayerTempData.Status.Room)
            {
                Console.WriteLine("MsgGetRoomInfo status err"+player.id);
                return ;
            }
            Room room=player.tempData.room;
            player.Send(room.GetRoomInfo());
        }
        //离开
        public void MsgLeaveRoom(Player player, ProtocolBase protoBase)
        {
            ProtocolBytes protocol = new ProtocolBytes();
            protocol.AddString("LeaveRoom");
            //条件检测
            if (player.tempData.status != PlayerTempData.Status.Room)
            {
                protocol.AddInt(-1);
                player.Send(protocol);
                Console.WriteLine("MsgLeaveRoom status err" + player.id);
                return;
            }
            protocol.AddInt(0);
            player.Send(protocol);
            Room room = player.tempData.room;
            RoomMgr.instance.LeaveRoom(player);

            if (room != null)
            {
                room.Broadcast(room.GetRoomInfo());
            }
        }
    }
}
