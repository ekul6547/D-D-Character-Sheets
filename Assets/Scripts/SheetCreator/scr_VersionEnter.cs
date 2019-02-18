using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace SheetCreator
{
    [ExecuteInEditMode]
    public class scr_VersionEnter : scr_BaseEnter
    {
        protected static int _majorVersion;
        public static int majorVersion
        {
            get
            {
                return _majorVersion;
            }
            set
            {
                _majorVersion = value;
            }
        }

        protected static int _minorVersion;
        public static int minorVersion
        {
            get
            {
                return _minorVersion;
            }
            set
            {
                _minorVersion = value;
            }
        }
        public Text IntDisplay;

        void Start()
        {
            base.Start();
        }
        void Update()
        {
            VariableName = "Version";
            Name.text = VariableName+":";
            IntDisplay.text = ToString();
        }
        public static void Add(int i)
        {
            minorVersion += i;
        }
        public static string ToString()
        {
            return majorVersion.ToString() + "." + minorVersion.ToString();
        }
        public void UpUpAndAway()
        {
            NextUp();
        }
        public static void NextUp()
        {
            majorVersion += 1;
            minorVersion = 0;
        }
        public static void Set(int a, int b)
        {
            majorVersion = Mathf.RoundToInt(a);
            minorVersion = Mathf.RoundToInt(b);
        }
    }
}
