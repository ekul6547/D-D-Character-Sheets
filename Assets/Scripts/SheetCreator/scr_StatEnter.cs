using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace SheetCreator
{
    [ExecuteInEditMode]
    public class scr_StatEnter : scr_BaseEnter
    {
        protected Vector2 _Value;
        public Vector2 Value
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
        public InputField IntDisplayA;
        public InputField IntDisplayB;

        void Start()
        {
            base.Start();
        }
        void Update()
        {
            Name.text = VariableName;
            if (!IntDisplayA.isFocused)
            {
                IntDisplayA.text = Value.x.ToString(); ;
            }

            if (!IntDisplayB.isFocused)
            {
                IntDisplayB.text = Value.y.ToString();
            }
        }
        public void AddA(float i)
        {
            Value = new Vector2(Value.x + i, Value.y);
        }
        public void AddB(float i)
        {
            Value = new Vector2(Value.x, Value.y + i);
        }
        public void SetA(string i)
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
            Value = new Vector2(a, Value.y);
        }
        public void SetB(string i)
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
            Value = new Vector2(Value.x, a);
        }
        public void SetStats(Vector2 stats)
        {
            SetA(stats.x.ToString());
            SetB(stats.y.ToString());
        }
        public override void SetValue()
        {
            try
            {
                if (ContentManager.active.activeInstant != null)
                {
                    ContentManager.active.activeInstant.SetStat(VariableName, Value.x, Value.y);
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning(e);
            }
        }
    }
}
