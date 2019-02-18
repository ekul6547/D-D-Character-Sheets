using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace SheetCreator
{
    [ExecuteInEditMode]
    public class scr_BoolEnter : scr_BaseEnter
    {
        protected bool _Value = false;
        public bool Value
        {
            get
            {
                return _Value;
            }
            set
            {
                _Value = value;
                SetValue();
                UpdateSprite();
            }
        }

        public Image ButtonImage;
        public Sprite TrueSprite;
        public Sprite FalseSprite;

        void Start()
        {
            base.Start();
            UpdateSprite();
        }
        void Update()
        {
            Name.text = VariableName;
        }
        void UpdateSprite()
        {
            if (Value == true)
            {
                ButtonImage.sprite = TrueSprite;
            }
            else
            {
                ButtonImage.sprite = FalseSprite;
            }
        }
        public void toggle()
        {
            Value = !Value;
        }
        public void SetBool(bool TO)
        {
            Value = TO;
        }
        public void SetValue()
        {
            try
            {
                if (ContentManager.active.activeInstant != null)
                {
                    ContentManager.active.activeInstant.SetBool(VariableName, Value);
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning(e);
            }
        }
    }
}