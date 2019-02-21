using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Root : MonoBehaviour {

    private void Start()
    {
        PanelMgr.instance.OpenPanel<LoginPanel>("");
    }
    private void Update()
    {
        NetMgr.Update();
    }
}
