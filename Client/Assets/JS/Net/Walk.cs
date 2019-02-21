using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Walk : MonoBehaviour {

    //预设
    public GameObject prefab;
    //player
    Dictionary<string, GameObject> players = new Dictionary<string, GameObject>();
    //self
    string playerId = "";
    //上一次移动的时间
    public float lastMoveTime;
    public static Walk instance;
	void Start () {
        instance = this;
	}
	
	// Update is called once per frame
	void Update () {
        Move();
	}
    void AddPlayer(string id,Vector3 pos,int score)
    {
        GameObject player = (GameObject)Instantiate(prefab, pos, Quaternion.identity);
        TextMesh textmesh = player.GetComponentInChildren<TextMesh>();
        textmesh.text = id + ":" + score.ToString();
        players.Add(id, player);
    }
    void DelPlayer(string id)
    {
        //已经初始化该玩家
        if (players.ContainsKey(id))
        {
            Destroy(players[id]);
            players.Remove(id);
        }
    }
    //更新分数
    public void UpdateScore(string id,int score)
    {
        GameObject player = players[id];
        if (player == null)
        {
            return;
        }
        TextMesh textMesh = player.GetComponentInChildren<TextMesh>();
        textMesh.text = id + ":" + score;
    }
    //更新信息
    public void UpdateInfo(string id,Vector3 pos,int score)
    {
        //只更新自己的分数
        if(id==playerId)
        {
            UpdateScore(id, score);
            return;
        }
        //其他
        //已初始化过该玩家
        if (players.ContainsKey(id))
        {
            players[id].transform.position = pos;
            UpdateScore(id, score);
        }
        else
        {
            AddPlayer(id, pos, score);
        }
    }
    public void StartGame(string id)
    {
        playerId = id;
#pragma warning disable CS0618 // 类型或成员已过时
        UnityEngine.Random.seed = (int)DateTime.Now.Ticks;
#pragma warning restore CS0618 // 类型或成员已过时
        float x = -900 + UnityEngine.Random.Range(-50, 50);
        float y = 0;
        float z = -900 + UnityEngine.Random.Range(-50, 50);
        Vector3 pos = new Vector3(x, y, z);
        AddPlayer(playerId, pos, 100);
        //同步
        SendPos();
        ProtocolBytes proto = new ProtocolBytes();
        proto.AddString("GetList");
        NetMgr.srvConn.Send(proto, GetList);
        NetMgr.srvConn.msgDist.AddListener("UpdateInfo", UpdateInfo);
        NetMgr.srvConn.msgDist.AddListener("PlayerLeave", PlayerLeave);
    }
    private void SendPos()
    {
        GameObject player = players[playerId];
        Vector3 pos = player.transform.position;
        //消息
        ProtocolBytes proto = new ProtocolBytes();
        proto.AddString("UpdateInfo");
        proto.AddFloat(pos.x);
        proto.AddFloat(pos.y);
        proto.AddFloat(pos.z);
        NetMgr.srvConn.Send(proto);
    }
    private void GetList(ProtocolBase protocol)
    {
        ProtocolBytes proto = (ProtocolBytes)protocol;
        //获取头部数值
        int start = 0;
        string protoName = proto.GetString(start, ref start);
        int count = proto.GetInt(start, ref start);
        //遍历
        for(int i=0;i<count;i++)
        {
            string id = proto.GetString(start, ref start);
            float x = proto.GetFloat(start, ref start);
            float y = proto.GetFloat(start, ref start);
            float z = proto.GetFloat(start, ref start);
            int score = proto.GetInt(start, ref start);
            Vector3 pos = new Vector3(x, y, z);
            UpdateInfo(id, pos, score);
        }
    }
    private void UpdateInfo(ProtocolBase protocol)
    {
        ProtocolBytes proto = (ProtocolBytes)protocol;
        int start = 0;
        string protoName = proto.GetString(start, ref start);
        string id = proto.GetString(start, ref start);
        float x = proto.GetFloat(start, ref start);
        float y = proto.GetFloat(start, ref start);
        float z = proto.GetFloat(start, ref start);
        int score = proto.GetInt(start, ref start);
        Vector3 pos = new Vector3(x, y, z);
        UpdateInfo(id, pos, score);

    }
    private void PlayerLeave(ProtocolBase protocol)
    {
        ProtocolBytes proto = (ProtocolBytes)protocol;
        int start = 0;
        string protoName = proto.GetString(start, ref start);
        string id = proto.GetString(start, ref start);
        DelPlayer(id);
    }

    
    public void Move()
    {
        if (playerId == "") return;
        if (players[playerId] == null) return;
        if (Time.time - lastMoveTime < 0.1) return;
        lastMoveTime = Time.time;
        GameObject player = players[playerId];
        //上
        if(Input.GetKey(KeyCode.UpArrow))
        {
            player.transform.position += new Vector3(0, 0, 1);
            SendPos();
        }
        if (Input.GetKey(KeyCode.DownArrow))
        {
            player.transform.position += new Vector3(0, 0, -1);
            SendPos();
        }
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            player.transform.position += new Vector3(1, 0, 0);
            SendPos();
        }
        if (Input.GetKey(KeyCode.RightArrow))
        {
            player.transform.position += new Vector3(-1, 0, 0);
            SendPos();
        }
        if (Input.GetKey(KeyCode.Space))
        {
            ProtocolBytes proto = new ProtocolBytes();
            proto.AddString("AddScore");
            NetMgr.srvConn.Send(proto);
        }
    }
}
