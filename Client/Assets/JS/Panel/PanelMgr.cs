using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PanelMgr : MonoBehaviour {
    //单例
    public static PanelMgr instance;
    
    //指向场景中的画布
    private GameObject canvas;
    //画板,存放已打开的画板
    public Dictionary<string,PanelBase>dict;
    //层级
    private Dictionary<PanelLayer,Transform>layerDict;
    //分层类型 
    public enum PanelLayer
    {
        Panel, Tips,
    };

    public void Awake()
    {
        instance = this;
        InitLayer();
        dict = new Dictionary<string, PanelBase>();
    }

    private void InitLayer()
    {
        //throw new NotImplementedException();
        canvas = GameObject.Find("Canvas");
        if (canvas == null)
            Debug.LogError("panelMar.InitLayer fail, canvas is null ");
        layerDict = new Dictionary<PanelLayer, Transform>();
        foreach(PanelLayer p1 in Enum.GetValues(typeof(PanelLayer)))
        {
            String name = p1.ToString();
            Transform transform = canvas.transform.Find(name);
            layerDict.Add(p1, transform);
        }
    }

    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
    public void OpenPanel<T>(string skinPath, params object[] args) where T : PanelBase
    {
        string name = typeof(T).ToString();
        if (dict.ContainsKey(name)) return;
        PanelBase panel = canvas.AddComponent<T>();
        panel.Init(args);
        dict.Add(name, panel);

        skinPath = (skinPath != "" ? skinPath : panel.skinPath);
        GameObject skin = Resources.Load<GameObject>(skinPath);
        if (skin == null) Debug.LogError("panelMgr.OpenPanel fail,skin is null, skinpath=" + skinPath);
        panel.skin = (GameObject)Instantiate(skin);
        Transform skinTrans = panel.skin.transform;
        PanelLayer layer = panel.layer;
        Transform parent = layerDict[layer];
        skinTrans.SetParent(parent, false);
        panel.OnShowing();
        panel.OnShowed();
    }
    
    public void ClosePanel(String name)
    {
        PanelBase panel = (PanelBase)dict[name];
        if (panel == null)
            return;
        panel.OnClosing();
        dict.Remove(name);
        panel.OnClosed();
        GameObject.Destroy(panel.skin);
        Component.Destroy(panel);
    }
}
