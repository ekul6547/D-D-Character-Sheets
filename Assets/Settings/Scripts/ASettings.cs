﻿/*******************************************************
 * Copyright (C) 2017 Doron Weiss  - All Rights Reserved
 * You may use, distribute and modify this code under the
 * terms of unity license.
 * 
 * See https://abnormalcreativity.wixsite.com/home for more info
 *******************************************************/
using UnityEngine;
using System.Collections;
using System.IO;

namespace Dweiss
{
    [System.Serializable]
    public abstract class ASettings : MonoBehaviour
    {
        [System.NonSerialized]
        public string FileName = "Settings.cfg";
        [System.NonSerialized]
        public string FolderPath = "Settings/";
        [System.NonSerialized]
        public string SettingFolderPath = "Assets/";
        [System.NonSerialized]
        public string SettingPath;

        private bool _loadSettingInEditorPlay = true;
        public bool LoadSettingInEditorPlay { get { return _loadSettingInEditorPlay; } set { _loadSettingInEditorPlay = value; } }

        private bool _autoSave=true;
        [System.NonSerialized]
        public bool AutoSave = true;
        

        public System.Action onValidateFunc;

        protected void Reset()
        {
            if(GameObject.FindObjectsOfType<ASettings>().Length != 1) {
                Debug.LogError("Too many settings singelton in scene. Destroy one");
            } else {
                name = "SettingsSingleton";
                Debug.Log("Create settings GameObject");
            }

            LoadEditorSetting();
#if UNITY_EDITOR
            SetScriptAwakeOrder<ASettings>(short.MinValue);
#endif
        }

        protected void OnValidate()
        {
#if UNITY_EDITOR
            if (AutoSave && Application.isPlaying == false)
            {
                //Debug.Log("Auto save " + Application.isPlaying + " " );
                SaveToFile(false);
            }
#endif
        }

        protected void FileNames()
        {
            SettingFolderPath = Application.dataPath+"/" + FolderPath;
            SettingPath = SettingFolderPath + FileName;//FilesToCopy/Settings.txt";
        }

        // Use this for initialization
        protected void Awake()
        {
            FileNames();
            try
            {
#if UNITY_EDITOR
                LoadEditorSetting();
#else
			var fileDestination = System.IO.Path.Combine( Application.dataPath, "../");
			fileDestination = System.IO.Path.Combine(fileDestination, FileName);
			fileDestination = System.IO.Path.GetFullPath(fileDestination);
			Debug.Log (name + " Load from " + fileDestination);
			LoadSetting(fileDestination, this);
#endif
                var t = JsonUtility.ToJson(this,true);
                Debug.Log("Setting is: " + t);
            }
            catch (System.Exception e)
            {
                Debug.Log(name + " Error with loading scripts " + e);
            }
        }


        #region Inside Unity

        private void LoadEditorSetting()
        {
            if (LoadSettingInEditorPlay)
            {
                var filePath = SettingPath;
                
                LoadSetting(filePath);

            }
        }
        private string GetJsonFromFile(string path)
        {
            if (File.Exists(path))
            {
                return  File.ReadAllText(path);
            }
            return null;
        }
        private void LoadSetting(string path, bool log = false)
        {
            var json = GetJsonFromFile(path);

            if (json != null)
            {
                if(log) Debug.Log("Load file " + json);
                try
                {
                    JsonUtility.FromJsonOverwrite(json, this);
                } catch(System.Exception e )
                {
                    Debug.LogError("Error with overwriting settings file " + e);
                }
            } else {
                Debug.LogError("Failed loading settings of " + name + " from " + path);
                SaveToFile();
            }
        }
        #endregion
        #region Unity editor

#if UNITY_EDITOR
        public static void SetScriptAwakeOrder<T>(short num)
        {
            string scriptName = typeof(T).Name;
            SetScriptAwakeOrder(scriptName, num);
        }
        public static void SetScriptAwakeOrder(string scriptName, short num)
        {
            foreach (var monoScript in UnityEditor.MonoImporter.GetAllRuntimeMonoScripts())
            {
                if (monoScript.name == scriptName)
                {
                    var exeOrder = UnityEditor.MonoImporter.GetExecutionOrder(monoScript);
                    if (exeOrder != num)
                    {
                        //Debug.Log("Change script " + monoScript.name + " old " + exeOrder + " new " + num);
                        UnityEditor.MonoImporter.SetExecutionOrder(monoScript, num);
                    }
                    break;
                }
            }
        }
#endif
        public void LoadToScript()
        {
            FileNames();
            var filePath = SettingPath;
            LoadSetting(filePath, true);
        }

        public static string lastSave;

        public void SaveToFile(bool forceWrite = true)
        {
            FileNames();
            var filePath = SettingPath;
            Debug.Log("CheckFile from " + filePath);
            var loaded = GetJsonFromFile(filePath);
            string json = null;
            try
            {
                json = JsonUtility.ToJson(this,true);
               
            } catch(System.Exception e)
            {
                Debug.LogError("Error with loading settings file: " + e);
            }
            if (forceWrite || loaded != json || json == null) WriteToFile(json);
        }
        private void WriteToFile(string json)
        {
            UnityEngine.Debug.Log("Save settings file at " + SettingPath + " with data: " + json);
            lastSave = SettingPath;
            if (Directory.Exists(SettingFolderPath) == false)
            {
                Directory.CreateDirectory(SettingFolderPath);
            }
            using (var fs = File.CreateText(SettingPath))
            {
                fs.Write(' ');
            }
            File.WriteAllText(SettingPath, json);
        }
        #endregion
    }
}

