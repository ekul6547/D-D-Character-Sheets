using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace SheetCreator {
    [ExecuteInEditMode]
    public class scr_TextEnter : scr_BaseEnter {
        
        public InputField Input;
        protected string _Value;
        public string Value
        {
            get
            {
                return _Value;
            }
            set
            {
                _Value = value;
                SetValue();
            }
        }
        void Start()
        {
            base.Start();
        }
        void Update()
        {
            Name.text = VariableName;
        }
        public void SetString(string TO)
        {
            Value = TO;
            Input.text = TO;
        }
        public override void SetValue()
        {
            try
            {
                if (ContentManager.active.activeInstant != null)
                {
                    ContentManager.active.activeInstant.SetString(VariableName, Value);
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning(e);
            }
        }
    }
}
