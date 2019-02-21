using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data;
using MySql.Data.MySqlClient;
using System.Text.RegularExpressions;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
namespace serviess
{
    class DataMgr
    {
        MySqlConnection sqlConn;

        public static DataMgr instance;
        public DataMgr()
        {
            instance = this;
            Connect();
        }

        private void Connect()
        {
            //throw new NotImplementedException();
            string connStr = "Database=game;Data Source=127.0.0.1;user id=root; Password=123456;port=3306;";
            sqlConn = new MySqlConnection(connStr);
            try
            {
                sqlConn.Open();

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            return;
        }
        public bool isSafeStr(string str)
        {
            return !Regex.IsMatch(str, @"[-|;|,|\/|\(|\)|\[|\]|\}|\{|%|@|\*|!|\']");
            //return true;
        }
        private bool CanRegister(string id)
        {
            if (!isSafeStr(id)) return false;
            string cmdStr = string.Format("select * from user where ider='{0}';", id);
            MySqlCommand sqlcmd = new MySqlCommand();
            sqlcmd.Connection = sqlConn;
            sqlcmd.CommandText = cmdStr;
            try
            {
                Console.WriteLine(sqlcmd.ExecuteScalar());
                return sqlcmd.ExecuteScalar() == null;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message + "ExecuteScalar失败");
                return false;
            }
        }
        public bool Register(string id, string pw)
        {
            if (!isSafeStr(id) || !isSafeStr(pw))
            {
                Console.WriteLine("[DataMgr]Regoster 使用违法字符");
                return false;
            }
            if (!CanRegister(id))
            {
                Console.WriteLine("用户已存在,或存在非法字符");
                return false;
            }

            string cmdStr = string.Format("insert into user set ider='{0}' ,pw='{1}';", id, pw);
            MySqlCommand cmd = new MySqlCommand();
            cmd.CommandText = cmdStr;
            cmd.Connection = sqlConn;
            try
            {
                cmd.ExecuteNonQuery();
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("insert失败" + e.Message);
                return false;
            }
        }

        public bool CreatePlayer(string id)
        {
            if (!isSafeStr(id)) return false;
            IFormatter formatter = new BinaryFormatter();
            MemoryStream stream = new MemoryStream();
            PlayerData playerData = new PlayerData();
            try
            {
                formatter.Serialize(stream, playerData);
            }
            catch (Exception e)
            {
                Console.WriteLine("[DataMgr]CreatePlayer 序列化 " + e.Message);
                return false;
            }
            byte[] byteArr = stream.ToArray();
            string cmdStr = string.Format("insert into player set id ='{0}', data =@data;", id);
            MySqlCommand cmd = new MySqlCommand(cmdStr, sqlConn);
            cmd.Parameters.Add("@data", MySqlDbType.Blob);
            cmd.Parameters[0].Value = byteArr;
            try
            {
                cmd.ExecuteNonQuery();
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("写入player失败" + e.Message);
                return false;
            }
        }
        public bool CheckPassWord(string id, string pw)
        {
            if (!isSafeStr(id) || !isSafeStr(pw)) return false;
            string cmdStr = string.Format("select * from user where ider='{0}' and pw='{1}';", id, pw);
            MySqlCommand cmd = new MySqlCommand(cmdStr, sqlConn);
            try
            {
                return cmd.ExecuteScalar() != null;
            }
            catch (Exception e)
            {
                Console.WriteLine("账户|密码检查错误"+id +""+pw+"" + e.Message);
                return false;
            }
        }
        public PlayerData GettPlayerData(string id)
        {
            PlayerData playerData = null;
            if (!isSafeStr(id)) return playerData;
            string cmdStr = "select * from player where id ='" + id + "';";
            MySqlCommand cmd = new MySqlCommand(cmdStr, sqlConn);
            byte[] buffer = new byte[1];
            try
            {

                MySqlDataReader rder = cmd.ExecuteReader();
                if (!rder.HasRows)
                {
                    Console.WriteLine("没有获取到角色信息");
                    rder.Close();
                    return playerData;
                }
                rder.Read();
                long len = rder.GetBytes(1, 0, null, 0, 0);
                buffer = new byte[len];
                rder.GetBytes(1, 0, buffer, 0, (int)len);
                rder.Close();

            }

            catch (Exception e)
            {
                Console.WriteLine("*****");
                Console.WriteLine("读取失败" + e.Message);
                return playerData;
            }
            MemoryStream stream = new MemoryStream(buffer);
            try
            {
                BinaryFormatter formatter = new BinaryFormatter();
                playerData = (PlayerData)formatter.Deserialize(stream);
                return playerData;
            }
            catch (SerializationException e)
            {
                Console.WriteLine("[DataMgr]GetPlayerData 反序列化" + e.Message);
                return playerData;
            }
        }
        public bool SavePlayer(Player player)
        {
            string id = player.id;
            PlayerData playerData = player.data;
            IFormatter formatter = new BinaryFormatter();
            MemoryStream steam = new MemoryStream();
            try
            {
                formatter.Serialize(steam, playerData);
            }
            catch (Exception e)
            {
                Console.WriteLine("[dataMgr]SavePlayer 序列化" + e.Message);
                return false;
            }
            byte[] byteArr = steam.ToArray();
            string formatStr = "update player set data= @data where id= '" + id + "';";
            MySqlCommand cmd = new MySqlCommand(formatStr, sqlConn);
            cmd.Parameters.Add("@data", MySqlDbType.Blob);
            cmd.Parameters[0].Value = byteArr;
            try
            {
                Console.WriteLine("保存成功"+ cmd.CommandText);
                cmd.ExecuteNonQuery();
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("[DataMgr]SavePlayer 写入" + e.Message);
                return false;
            }
        }
    }
}
