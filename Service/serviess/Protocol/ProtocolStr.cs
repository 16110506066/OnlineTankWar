using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace serviess
{
    public class ProtocolStr : ProtocolBase
    {
        public string str;
        public override ProtocolBase Decode(byte[] readbuff, int start, int length)
        {
            ProtocolStr protocol = new ProtocolStr();
            protocol.str = System.Text.Encoding.UTF8.GetString(readbuff, start, length);
            return (ProtocolBase)protocol;
        }
        public override byte[] Encode()
        {
            byte[] b = System.Text.Encoding.UTF8.GetBytes(str);
            return b;
            //return base.Encode();
        }
        public override string GetName()
        {
            if (str.Length == 0) return "";
            return str.Split(',')[0];
            //return base.GetName();
        }
        public override string GetDesc()
        {
            return str;
            //return base.GetDesc();
        }
    }
}
