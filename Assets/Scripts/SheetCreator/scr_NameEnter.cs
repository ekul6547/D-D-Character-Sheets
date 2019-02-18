using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace SheetCreator {
    [ExecuteInEditMode]
    public class scr_NameEnter : scr_BaseEnter {

        public string defTo = "Character Name";
        public InputField Input;
        protected string _stringValue;
        public string stringValue
        {
            get
            {
                return _stringValue;
            }
            set
            {
                _stringValue = value;
                SetValue();
            }
        }
        public void SetValues(string stringTO, bool boolTO)
        {
            _stringValue = stringTO;
            Input.text = stringTO;
            _boolValue = boolTO;
            SetValue();
            UpdateSprite();
        }
        void Awake()
        {
            VariableName = defTo;
        }
        new void Start()
        {
            VariableName = defTo;
            base.Start();
        }
        void Update()
        {
            VariableName = defTo;
            Name.text = VariableName;
        }

        public override void SetValue()
        {
            try
            {
                if (ContentManager.active.activeInstant != null)
                {
                    ContentManager.active.activeInstant.CharacterName = stringValue;
                    ContentManager.active.activeInstant.ISNPC = boolValue;
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning(e);
            }
        }

        public Image ButtonImage;
        public Sprite TrueSprite;
        public Sprite FalseSprite;
        protected bool _boolValue = false;
        public bool boolValue
        {
            get
            {
                return _boolValue;
            }
            set
            {
                _boolValue = value;
                SetValue();
                UpdateSprite();
            }
        }
        void UpdateSprite()
        {
            if (boolValue == true)
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
            boolValue = !boolValue;
        }
    }
}
