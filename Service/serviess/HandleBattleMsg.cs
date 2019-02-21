using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace serviess
{
    public partial  class HandlePlayerMsg
    {
        //start
        public void MsgStartFight(Player player, ProtocolBase protoBase)
        {
            ProtocolBytes protocol = new ProtocolBytes();
            protocol.AddString("StartFight");

            if (player.tempData.status != PlayerTempData.Status.Room)
            {
                Console.WriteLine("MsgStartFight status err " + player.id);
                protocol.AddInt(-1);
                player.Send(protocol);
                return;
            }

            if (!player.tempData.isOwner)
            {
                Console.WriteLine("MsgStartFight owner err " + player.id);
                protocol.AddInt(-1);
                player.Send(protocol);
                return;
            }

            Room room = player.tempData.room;
            if (!room.CanStart())
            {
                Console.WriteLine("MsgStartFight Canstart err " + player.id);
                protocol.AddInt(-1);
                player.Send(protocol);
                return;
            }

            protocol.AddInt(0);
            player.Send(protocol);
            room.StartFight();
        }

        public void MsgUpdateUnitInfo(Player player, ProtocolBase protocolBase)
        {
            int start = 0;
            ProtocolBytes protocol = (ProtocolBytes)protocolBase;
            string protoName = protocol.GetString(start, ref start);
            float posx = protocol.GetFloat(start, ref start);
            float posy = protocol.GetFloat(start, ref start);
            float posz = protocol.GetFloat(start, ref start);
            float rotx = protocol.GetFloat(start, ref start);
            float roty = protocol.GetFloat(start, ref start);
            float rotz = protocol.GetFloat(start, ref start);

            float gunRot = protocol.GetFloat(start, ref start);
            float gunRoll = protocol.GetFloat(start, ref start);
            //获取房间
            if (player.tempData.status != PlayerTempData.Status.Fight)
                return;
            Room room = player.tempData.room;

            player.tempData.posX = posx;
            player.tempData.posY = posy;
            player.tempData.posZ = posz;
            player.tempData.lastUpdateTime = Sys.Getimetamp();

            ProtocolBytes protocolRet = new ProtocolBytes();
            protocolRet.AddString("UpdateUnitInfo");
            protocolRet.AddString(player.id);
            protocolRet.AddFloat(posx);
            protocolRet.AddFloat(posy);
            protocolRet.AddFloat(posz);
            protocolRet.AddFloat(rotx);
            protocolRet.AddFloat(roty);
            protocolRet.AddFloat(rotz);

            protocolRet.AddFloat(gunRot);
            protocolRet.AddFloat(gunRoll);
            room.Broadcast(protocolRet);
        }
        public void MsgShooting(Player player, ProtocolBase protoBase)
        {
            //获取数值
            ProtocolBytes protocol = (ProtocolBytes)protoBase;
            int start = 0;
            string protoName = protocol.GetString(start, ref start);
            float posx = protocol.GetFloat(start, ref start);
            float posy = protocol.GetFloat(start, ref start);
            float posz = protocol.GetFloat(start, ref start);
            float rotx = protocol.GetFloat(start, ref start);
            float roty = protocol.GetFloat(start, ref start);
            float rotz = protocol.GetFloat(start, ref start);
            if (player.tempData.status != PlayerTempData.Status.Fight)
                return;
            Room room = player.tempData.room;

            ProtocolBytes protocolRet = new ProtocolBytes();
            protocolRet.AddString("Shooting");
            protocolRet.AddString(player.id);
            protocolRet.AddFloat(posx);
            protocolRet.AddFloat(posy);
            protocolRet.AddFloat(posz);
            protocolRet.AddFloat(rotx);
            protocolRet.AddFloat(roty);
            protocolRet.AddFloat(rotz);
            room.Broadcast(protocolRet);
        }
        public void MsgHit(Player player, ProtocolBase protoBase)
        {
            int start = 0;
            ProtocolBytes protocol = (ProtocolBytes)protoBase;
            string protoName = protocol.GetString(start, ref start);
            string enemyName = protocol.GetString(start, ref start);
            float damage = protocol.GetFloat(start, ref start);
            long lastShootTime = player.tempData.lastShootTime;
            if (Sys.Getimetamp() - lastShootTime < 1)
            {
                Console.WriteLine("有点问题");
                return;
            }
            player.tempData.lastShootTime = Sys.Getimetamp();
            //Get Room
            if (player.tempData.status != PlayerTempData.Status.Fight)
                return;
            Room room = player.tempData.room;
            //life
            if (!room.list.ContainsKey(enemyName))
            {
                Console.WriteLine("MsgHit net Contains enemy " + enemyName);
                return;
            }
            Player enemy = room.list[enemyName];
            if (enemy == null)
                return;
            if (enemy.tempData.hp <= 0)
                return;
            enemy.tempData.hp -= damage;
            Console.WriteLine("MsgHit enemyname" + enemyName);
            ProtocolBytes protocolRet = new ProtocolBytes();
            protocolRet.AddString("Hit");
            protocolRet.AddString(player.id);
            protocolRet.AddString(enemy.id);
            protocolRet.AddFloat(damage);
            room.Broadcast(protocolRet);
            room.UpdateWin();

        }
    }
}
