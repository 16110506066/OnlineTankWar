﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace serviess
{
    class HandlePlayerEvent
    {
        //上线
        public void OnLogin(Player player)
        {
            Scene.Scener.instance.AddPlayer(player.id);
        }
        //下线
        public void OnLogout(Player player)
        {
            if (player.tempData.status == PlayerTempData.Status.Room)
            {
                Room room = player.tempData.room;
                RoomMgr.instance.LeaveRoom(player);
                if (room != null)
                {
                    room.Broadcast(room.GetRoomInfo());
                }
            }
        }
    }
}
