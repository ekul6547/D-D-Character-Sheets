using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Linq;

namespace SheetCreator
{
    [ExecuteInEditMode]
    public class scr_DropDownEnter : scr_FloatEnter
    {
        public Dropdown dropDown;

        void Start()
        {
            base.Start();
            UpdateOptions();
        }
        void Update()
        {
            Name.text = VariableName;
            dropDown.value = Mathf.RoundToInt(Value);
        }
        public void SetFloat(int TO)
        {
            Set(TO.ToString());
        }

        public void UpdateOptions()
        {
            dropDown.ClearOptions();
            dropDown.AddOptions(GetNonOptions());
        }
    }
}
