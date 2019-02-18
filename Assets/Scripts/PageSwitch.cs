using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SheetCreator;

public class PageSwitch : MonoBehaviour {

    public scr_EditBox Edit;
    public GameObject[] Pages;

    public void SetPage(int i)
    {
        if(i < Pages.Length)
        {
            foreach(GameObject O in Pages)
            {
                O.SetActive(false);
            }
            Pages[i].SetActive(true);
            if (Edit != null)
            {
                Edit.main = (RectTransform)Pages[i].transform;
            }
        }
    }
}
