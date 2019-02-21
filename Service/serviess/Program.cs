using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using serviess.Scene;
namespace serviess
{
    class Program
    {
        public static void Main(string[] args)
        {
            DataMgr dataMgr = new DataMgr();
            RoomMgr roomMgr = new RoomMgr();
            ServNet servnet = new ServNet();
            servnet.proto = new ProtocolBytes();
            Scener scene = new Scener();
            //DataMgr.instance.Register("123", "123");
            //DataMgr.instance.CreatePlayer("123");
            servnet.Start("127.0.0.1", 1234);
            while (true)
            {
                string str = Console.ReadLine();
                switch (str)
                {
                    case "quit":
                        servnet.Close();
                        Console.ReadLine();
                        return;
                    case "print":
                        servnet.Print();
                        Console.ReadLine();
                         return;
                }
                Console.ReadLine();
            }
        }
    }
}
