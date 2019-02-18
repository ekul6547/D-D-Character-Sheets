using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace SheetCreator
{
    [ExecuteInEditMode]
    public class scr_FloatEnter : scr_BaseEnter
    {
        protected float _Value;
        public float Value
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
        public InputField IntDisplay;

        void Start()
        {
            base.Start();
        }
        void Update()
        {
            Name.text = VariableName;
            if (!IntDisplay.isFocused)
            {
                IntDisplay.text = Value.ToString();
            }
        }
        public void Add(float i)
        {
            Value += i;
        }
        public override void Set(string i)
        {
            float a = 0;
            try
            {
                a = float.Parse(i);
            }
            catch
            {
                a = 0;
            }
            Value = a;
        }
        public void SetFloat(float TO)
        {
            Set(TO.ToString());
        }
        public override void SetValue()
        {
            try
            {
                if (ContentManager.active.activeInstant != null)
                {
                    ContentManager.active.activeInstant.SetFloat(VariableName, Value);
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning(e);
            }
        }
    }
}
