using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TipPanel : PanelBase {
    private Text textt;
    private Button btn;
    string str = "";
    #region 
    public override void Init(params object[] args)
    {
        base.Init(args);
        skinPath = "TipPanel";
        layer = PanelMgr.PanelLayer.Tips;
        if(args.Length==1)
        {
            str = (string)args[0];
        }
    }
    //显示之前
    public override void OnShowing()
    {
        base.OnShowing();
        Transform skinTrans = skin.transform;
        //文字
        textt = skinTrans.Find("Text").GetComponent<Text>();
        textt.text = str;
        //关闭按钮
        btn = skinTrans.Find("Btn").GetComponent<Button>();
        btn.onClick.AddListener(OnbBtnClick);
    }
    #endregion

    private void OnbBtnClick()
    {
        Close();
    }
}
