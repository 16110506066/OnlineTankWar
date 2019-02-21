using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace serviess
{
    public class Room
    {
        public enum Status
        {
            Prepare = 1, Fight = 2
        }
        public Status status = Status.Prepare;
        //玩家
        public int maxPlayers = 6;
        public Dictionary<string, Player> list = new Dictionary<string, Player>();


        //添加玩家
        public bool AddPlayer(Player player)
        {
            lock (list)
            {
                if (list.Count > maxPlayers) return false;
                PlayerTempData tempData = player.tempData;
                tempData.room = this;
                tempData.team = SwichTeam();
                tempData.status = PlayerTempData.Status.Room;
                tempData.isOwner = false;
                if (list.Count == 0)
                    tempData.isOwner = true;
                string id = player.id;
                list.Add(id, player);
            }
            return true;
        }
        private int SwichTeam()
        {
            int count1 = 0;
            int count2 = 0;
            foreach (Player py in list.Values)
            {

                if (py.tempData.team == 1) count1++;
                if (py.tempData.team == 2) count2++;
            }
            if (count1 <= count2) return 1;
            return 2;
        }
        public void DelPlayer(string id)
        {
            lock (list)
            {
                if (!list.ContainsKey(id)) return;
                bool isOwner = list[id].tempData.isOwner;
                list[id].tempData.status = PlayerTempData.Status.None;
                list.Remove(id);
                if (isOwner) UpdateOwner();
            }

        }
        private void UpdateOwner()
        {
            lock (list)
            {
                if (list.Count <= 0) return;
                foreach (Player player in list.Values)
                {
                    player.tempData.isOwner = false;
                }
                list.Values.First().tempData.isOwner = true;
            }
        }
        public void Broadcast(ProtocolBase protocol)
        {
            foreach (Player player in list.Values)
            {
                player.Send(protocol);
            }
        }
        public ProtocolBytes GetRoomInfo()
        {
            ProtocolBytes protocol = new ProtocolBytes();
            protocol.AddString("GetRoomInfo");
            //房间信息
            protocol.AddInt(list.Count);
            foreach (Player py in list.Values)
            {
                protocol.AddString(py.id);
                protocol.AddInt(py.tempData.team);
                protocol.AddInt(py.data.win);
                protocol.AddInt(py.data.fail);
                int isOwner = py.tempData.isOwner ? 1 : 0;
                protocol.AddInt(isOwner);
            }
            return protocol;
        }

        public bool CanStart()
        {
            if (status != Status.Prepare)
            {
                return false;
            }
            int count1 = 0;
            int count2 = 0;

            foreach (Player player in list.Values)
            {
                if (player.tempData.team == 1) count1++;
                if (player.tempData.team == 2) count2++;
            }
            if (count1 < 1 || count2 < 1) return false;
            return true;
        }
        public void StartFight()
        {
            ProtocolBytes protocol = new ProtocolBytes();
            protocol.AddString("Fight");
            status = Status.Fight;
            int teampos1 = 1;
            int teampos2 = 1;
            lock (list)
            {
                protocol.AddInt(list.Count);
                foreach (Player p in list.Values)
                {
                    p.tempData.hp = 200;
                    protocol.AddString(p.id);
                    protocol.AddInt(p.tempData.team);
                    if (p.tempData.team == 1)
                        protocol.AddInt(teampos1++);
                    else protocol.AddInt(teampos2++);
                    p.tempData.status = PlayerTempData.Status.Fight;
                }
                Broadcast(protocol);
            }
        }
        private int IsWin()
        {
            if (status != Status.Fight)
                return 0;
            int count1 = 0;
            int count2 = 0;
            foreach (Player player in list.Values)
            {
                PlayerTempData pt = player.tempData;
                if (pt.team == 1 && pt.hp > 0) count1++;
                if (pt.team == 2 && pt.hp > 0) count2++;
            }
            if (count1 <= 0) return 2;
            if (count2 <= 0) return 1;
            return 0;
        }
        public void UpdateWin()
        {
            int isWin = IsWin();
            if (isWin == 0)
                return;

            lock (list)
            {
                status = Status.Prepare;
                foreach (Player player in list.Values)
                {
                    player.tempData.status = PlayerTempData.Status.Room;
                    if (player.tempData.team == isWin)
                        player.data.win++;
                    else player.data.fail++;

                }
            }
            ProtocolBytes protocol = new ProtocolBytes();
            protocol.AddString("Result");
            protocol.AddInt(isWin);
            Broadcast(protocol);
        }
    }
}


