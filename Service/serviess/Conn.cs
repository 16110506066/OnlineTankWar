using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace serviess
{
    public class Conn
    {
        public int BUFFER_SIZE = 1024;

        public Socket socket;

        public bool isUse = false;

        public byte[] readBuff;
        public int buffCount = 0;
        //黏包分包
        public byte[] lenBytes = new byte[sizeof(UInt32)];
        public Int32 msgLength = 0;
        public long lastTickTime = long.MinValue;
        public Player player;

        public Conn()
        {
            readBuff = new byte[BUFFER_SIZE];
        }
        public void Init(Socket socket)
        {
            lastTickTime = Sys.Getimetamp();
            this.socket = socket;
            isUse = true;
            buffCount = 0;
        }
        //剩余buff
        public int BuffRemain()
        {
            return BUFFER_SIZE - buffCount;
        }

        //地址
        public string GetAdress()
        {
            if (!isUse) return "无法获取地址";
            return socket.RemoteEndPoint.ToString();
        }

        //关闭
        public void Close()
        {
            if (!isUse) return;
            if (player != null)
            {
                player.Logout();
                return;
            }
            Console.WriteLine("[断开连接]" + GetAdress());
            socket.Shutdown(SocketShutdown.Both);
            socket.Close();
            isUse = false;
        }
        //发送协议
        public void Send(ProtocolBase protocol)
        {
            ServNet.instance.Send(this, protocol);
        }
    }
}
