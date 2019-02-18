using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Dweiss;

public class DebugSaveText : MonoBehaviour {

    Text T;
    void Start()
    {
        T = GetComponent<Text>();
    }

    void Update () {
        T.text = ASettings.lastSave;
	}
}
