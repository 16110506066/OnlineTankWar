using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace serviess
{
    class RoomMgr
    {
        public static RoomMgr instance;
        public RoomMgr(){ instance = this; }
        
        //列表
        public List<Room> list=new List<Room>();

        public void CreateRoom(Player player)
        {
            Room room = new Room();
            lock (list)
            {
                list.Add(room);
                room.AddPlayer(player);
            }
        }

        public void LeaveRoom(Player player)
        {
            PlayerTempData tempData = player.tempData;
            if (tempData.status == PlayerTempData.Status.None) return;
            player.tempData.isOwner = false;
            Room room = tempData.room;
            lock (list)
            {
                room.DelPlayer(player.id);
                if (room.list.Count == 0) list.Remove(room);
            }
        }

        public ProtocolBytes GetRoomList()
        {
            ProtocolBytes protocol = new ProtocolBytes();
            protocol.AddString("GetRoomList");

            //数量
            int count = list.Count;
            protocol.AddInt(count);
            for (int i = 0; i < count; i++)
            {
                Room room = list[i];
                protocol.AddInt(room.list.Count);
                protocol.AddInt((int)room.status);
            }
            return protocol;
        }
    }
}
