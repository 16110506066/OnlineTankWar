using System;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
public class Net : MonoBehaviour
{

    public InputField hostInput;
    public InputField portInput;
    public InputField idInput;
    public InputField pwInput;

    public Text recvText;
    private string recvstr;
    public Text clientText;
    public Button bt;
    public Button lgb;
    public Button adb;
    public Button gtb;
    //public InputField textinput;
    public Button sdbt;



    int buffCount = 0;
    byte[] lenBytes = new byte[sizeof(UInt32)];
    Int32 msgLength = 0;
    Socket socket;
    const int BUFFER_SIZE = 1024;
    byte[] readBuff = new byte[BUFFER_SIZE];

    //协议
    ProtocolBase proto = new ProtocolBytes();
    private void Awake()
    {
        bt.onClick.AddListener(Connettion);
        sdbt.onClick.AddListener(OnSendClick);
        lgb.onClick.AddListener(OnlogClick);
        adb.onClick.AddListener(OnaddClick);
        gtb.onClick.AddListener(OngetClick);
    }
    private void Update()
    {
        recvText.text = recvstr;
    }
    public void Connettion()
    {
        recvText.text = "";

        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        string host = hostInput.text;
        int port = int.Parse(portInput.text);
        socket.Connect(host, port);
        clientText.text = "客户端地址" + socket.LocalEndPoint.ToString();
        socket.BeginReceive(readBuff, 0, BUFFER_SIZE, SocketFlags.None, ReceiveCb, null);

    }
    private void ReceiveCb(IAsyncResult ar)
    {
        try
        {
            int count = socket.EndReceive(ar);
            string str = System.Text.Encoding.UTF8.GetString(readBuff, 0, count);
            buffCount += count;
            ProcessData();
            if (recvstr.Length >= 300) recvstr = "";
            recvstr += str + "\n";

            socket.BeginReceive(readBuff, 0, BUFFER_SIZE, SocketFlags.None, ReceiveCb, null);

        }
        catch (Exception e)
        {
            recvstr += "连接断开  " + e.Message;
            socket.Close();
        }
    }

    private void ProcessData()
    {
        //throw new NotImplementedException();
        if (buffCount < sizeof(Int32)) return;
        Array.Copy(readBuff, lenBytes, sizeof(Int32));
        msgLength = BitConverter.ToInt32(lenBytes, 0);
        if (buffCount < msgLength + sizeof(Int32)) return;
        //string str = System.Text.Encoding.UTF8.GetString(readBuff, sizeof(Int32), (int)msgLength);
        //recvstr = str;
        ProtocolBase protocol = proto.Decode(readBuff, sizeof(Int32), msgLength);
        HandleMsg(protocol);
        int count = buffCount - msgLength - sizeof(Int32);
        Array.Copy(readBuff, msgLength, readBuff, 0, count);
        buffCount = count;
        if (buffCount > 0)
        {
            ProcessData();
        }
    }

    private void HandleMsg(ProtocolBase protocol)
    {

        ProtocolBytes proto = (ProtocolBytes)protocol;
        int start = 0;
        string protoName = proto.GetString(start, ref start);       
        int ret = proto.GetInt(start, ref start);
        Debug.Log("接受" + proto.GetDesc());
        recvstr = "接受" + proto.GetName() + "" + ret.ToString();
    }

    public void OnSendClick()
    {
        ProtocolBytes protocol = new ProtocolBytes();
        protocol.AddString("HeatBeat");
        Debug.Log("发送" + protocol.GetDesc());
        Send(protocol);
    }
    private void Send(ProtocolBase protocol)
    {
        //string str = textinput.text;
        byte[] bytes = protocol.Encode();
        byte[] length = BitConverter.GetBytes(bytes.Length);
        byte[] sendbuff = length.Concat(bytes).ToArray();
        try
        {
            socket.Send(sendbuff);
        }
        catch (Exception e)
        {
            Debug.Log("失败" + e.Message);
        }
    }
    public void OnlogClick()
    {
        ProtocolBytes protocol = new ProtocolBytes();
        protocol.AddString("Login");
        protocol.AddString(idInput.text);
        protocol.AddString(pwInput.text);
        Debug.Log("发送" + protocol.GetDesc());
        Send(protocol);
    }
    public void OnaddClick()
    {
        ProtocolBytes protocol = new ProtocolBytes();
        protocol.AddString("AddScore");
        Debug.Log("发送" + protocol.GetDesc());
        Send(protocol);
    }
    public void OngetClick()
    {
        ProtocolBytes protocol = new ProtocolBytes();
        protocol.AddString("GetScore");
        Debug.Log("发送" + protocol.GetDesc());
        Send(protocol);
    }
}
