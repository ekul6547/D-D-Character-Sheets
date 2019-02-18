using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SheetCreator
{
    public class SheetSaveHooks : MonoBehaviour
    {
        public InputField SheetNameInput;
        public GameObject MainPage;
        public GameObject[] Fields;
        public scr_EditBox EditBefore;

        void Awake()
        {
            scr_SaveSheetCustom SaveInst = gameObject.GetComponent<scr_SaveSheetCustom>();
            SaveInst.PAGES_DO_NOT_CHANGE = MainPage;
            SaveInst.OPTIONS_DO_NOT_CHANGE = Fields;
            SaveInst.InputName = SheetNameInput;
            SaveInst.EditBefore = EditBefore;
        }
    }
}