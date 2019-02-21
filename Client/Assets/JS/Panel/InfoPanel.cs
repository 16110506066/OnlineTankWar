using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InfoPanel : PanelBase {
    private Button CloseBtn;

    #region
    public override void Init(params object[] args)
    {
        base.Init(args);
        skinPath = "InfoPanel";
        layer = PanelMgr.PanelLayer.Panel;
    }
    public override void OnShowing()
    {
        base.OnShowing();
        Transform SkinTrans = skin.transform;
        CloseBtn = SkinTrans.Find("CloseBtn").GetComponent<Button>();
        CloseBtn.onClick.AddListener(OnCloseClick);
    }

    public void OnCloseClick()
    {
        Close();
    }
    #endregion

}
