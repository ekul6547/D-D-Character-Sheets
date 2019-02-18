using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SheetCreator
{
    public class scr_SaveSheetCustom : MonoBehaviour
    {

        [System.Serializable]
        public class DataEnter //Basic entry method as to how to store data
        {
            public string VariableName; //Name of variable
            public DataTypes type; //Type of variable
            public Vector2 Position; //Position
            public string[] Options; //And extra options. For example, the dropdown list contents
            public DataEnter(string s, DataTypes t, Vector2 v)
            {
                VariableName = s;
                type = t;
                Position = v;
            }
            public DataEnter(string s, DataTypes t, Vector2 v, string[] o)
            {
                VariableName = s;
                type = t;
                Position = v;
                Options = o;
            }
            public void SetOptions(string[] s)
            {
                Options = s;
            }
        }

        //DO NOT CHANGE. Non-serialized so they don't get saved
        [System.NonSerialized]
        public GameObject PAGES_DO_NOT_CHANGE;
        [System.NonSerialized]
        public GameObject[] OPTIONS_DO_NOT_CHANGE;
        [System.NonSerialized]
        public InputField InputName;
        [System.NonSerialized]
        public scr_EditBox EditBefore;


        public string Version; //Version. Yes, it is stored awkwardly

        //Each data entry for each page
        public List<DataEnter> MAIN = new List<DataEnter>();
        public List<DataEnter> STATUS = new List<DataEnter>();
        public List<DataEnter> APPEARENCE = new List<DataEnter>();
        public List<DataEnter> EQUIPMENT = new List<DataEnter>();
        public List<DataEnter> INVENTORY = new List<DataEnter>();
        public List<DataEnter> SKILLS = new List<DataEnter>();
        public List<DataEnter> MAGIC = new List<DataEnter>();

        public void Save() //Main saving method. Saves to Client side, not server
        {
            if(EditBefore != null)
            {
                EditBefore.setEdit(null);
            }
            if (InputName.text != "" && InputName.text != " ")
            {
                //Clear all current data
                MAIN.Clear();
                STATUS.Clear();
                APPEARENCE.Clear();
                EQUIPMENT.Clear();
                INVENTORY.Clear();
                SKILLS.Clear();
                MAGIC.Clear();

                scr_VersionEnter.Add(1); //Increase minor version number

                Version = scr_VersionEnter.ToString();

                scr_BaseEnter[] objs = PAGES_DO_NOT_CHANGE.GetComponentsInChildren<scr_BaseEnter>(true);
                foreach (scr_BaseEnter o in objs) //Locate and store each input field into the lists above
                {
                    bool t = o.gameObject.activeSelf;
                    o.gameObject.SetActive(true);
                    DataEnter data = new DataEnter(o.VariableName, o.type, new Vector2(o.transform.localPosition.x, o.transform.localPosition.y), o.options.ToArray()); //Create Data Entry

                    //Store in appropriate list
                    if (o.tab == 1)
                        STATUS.Add(data);
                    else if (o.tab == 2)
                        APPEARENCE.Add(data);
                    else if (o.tab == 3)
                        EQUIPMENT.Add(data);
                    else if (o.tab == 4)
                        INVENTORY.Add(data);
                    else if (o.tab == 5)
                        SKILLS.Add(data);
                    else if (o.tab == 6)
                        MAGIC.Add(data);
                    else
                        MAIN.Add(data);
                    o.gameObject.SetActive(t);
                }
                ContentManager.active.SaveSheetInstance("sheet_" + InputName.text, "CustomSheets", this, true); //Send off to save
            }
        }
        public void Load() //Main Loading method. Loads Client side, not from server
        {
            string json = SaveInstance.active.GetJsonFromFile("sheet_" + InputName.text, "CustomSheets"); //Load from client
            LoadJson(json);
        }

        public string GetJson() //Returns a Json format of this class. Used to send from the host to a client
        {
            return SaveInstance.active.TurnToJson(this, false);
        }

        public void LoadJson(string json) //Loads a given json into the class
        {
            //Clear any data
            MAIN.Clear();
            STATUS.Clear();
            APPEARENCE.Clear();
            EQUIPMENT.Clear();
            INVENTORY.Clear();
            SKILLS.Clear();
            MAGIC.Clear();
            if (json != null)
            {
                try
                {
                    JsonUtility.FromJsonOverwrite(json, this);
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning(e);
                }
            }

            GameObject[] objs = GameObject.FindGameObjectsWithTag("InputArea"); //Get all input areas, then destroy them all. This means even for invalid json, it will clear the sheet.
            if (objs.Length > 0)
            {
                foreach (GameObject o in objs)
                {
                    Destroy(o);
                }
            }

            if (json != null)
            {
                //Loading the version number
                int c = 0;
                int d = 0;
                string[] s = Version.Split(".".ToCharArray());
                if (s != null && s.Length > 1)
                {
                    int.TryParse(s[0], out c);
                    int.TryParse(s[1], out d);
                }
                else
                {
                    c = 0;
                    d = 0;
                }
                scr_VersionEnter.Set(c, d);

                //Loading MAIN
                GameObject a;
                foreach (DataEnter D in MAIN)
                {
                    a = Instantiate(OPTIONS_DO_NOT_CHANGE[(int)D.type], Vector3.zero, new Quaternion(0, 0, 0, 0), PAGES_DO_NOT_CHANGE.transform.Find("0Main")); //Create the input areas
                    a.transform.localPosition = new Vector3(D.Position.x, D.Position.y); //Set position
                    scr_BaseEnter be = a.GetComponent<scr_BaseEnter>();
                    be.VariableName = D.VariableName; //Set the Variable Name
                    be.tab = 0; //Set which tab it belongs to
                    be.options.AddRange(D.Options); //Add options
                    be.UpdateColour();
                }

                //Loading STATUS
                foreach (DataEnter D in STATUS)
                {
                    a = Instantiate(OPTIONS_DO_NOT_CHANGE[(int)D.type], Vector3.zero, new Quaternion(0, 0, 0, 0), PAGES_DO_NOT_CHANGE.transform.Find("1Current"));
                    a.transform.localPosition = new Vector3(D.Position.x, D.Position.y);
                    scr_BaseEnter be = a.GetComponent<scr_BaseEnter>();
                    be.VariableName = D.VariableName;
                    be.tab = 1;
                    be.options.AddRange(D.Options);
                    be.UpdateColour();
                }

                //Loading APPEARENCE
                foreach (DataEnter D in APPEARENCE)
                {
                    a = Instantiate(OPTIONS_DO_NOT_CHANGE[(int)D.type], Vector3.zero, new Quaternion(0, 0, 0, 0), PAGES_DO_NOT_CHANGE.transform.Find("2Appearance"));
                    a.transform.localPosition = new Vector3(D.Position.x, D.Position.y);
                    scr_BaseEnter be = a.GetComponent<scr_BaseEnter>();
                    be.VariableName = D.VariableName;
                    be.tab = 2;
                    be.options.AddRange(D.Options);
                    be.UpdateColour();
                }

                //Loading EQUIPMENT
                foreach (DataEnter D in EQUIPMENT)
                {
                    a = Instantiate(OPTIONS_DO_NOT_CHANGE[(int)D.type], Vector3.zero, new Quaternion(0, 0, 0, 0), PAGES_DO_NOT_CHANGE.transform.Find("3Equipment"));
                    a.transform.localPosition = new Vector3(D.Position.x, D.Position.y);
                    scr_BaseEnter be = a.GetComponent<scr_BaseEnter>();
                    be.VariableName = D.VariableName;
                    be.tab = 3;
                    be.options.AddRange(D.Options);
                    be.UpdateColour();
                }

                //Loading INVENTORY
                foreach (DataEnter D in INVENTORY)
                {
                    a = Instantiate(OPTIONS_DO_NOT_CHANGE[(int)D.type], Vector3.zero, new Quaternion(0, 0, 0, 0), PAGES_DO_NOT_CHANGE.transform.Find("4Inventory"));
                    a.transform.localPosition = new Vector3(D.Position.x, D.Position.y);
                    scr_BaseEnter be = a.GetComponent<scr_BaseEnter>();
                    be.VariableName = D.VariableName;
                    be.tab = 4;
                    be.options.AddRange(D.Options);
                    be.UpdateColour();
                }

                //Loading SKILLS
                foreach (DataEnter D in SKILLS)
                {
                    a = Instantiate(OPTIONS_DO_NOT_CHANGE[(int)D.type], Vector3.zero, new Quaternion(0, 0, 0, 0), PAGES_DO_NOT_CHANGE.transform.Find("5Skills"));
                    a.transform.localPosition = new Vector3(D.Position.x, D.Position.y);
                    scr_BaseEnter be = a.GetComponent<scr_BaseEnter>();
                    be.VariableName = D.VariableName;
                    be.tab = 5;
                    be.options.AddRange(D.Options);
                    be.UpdateColour();
                }

                //Loading MAGIC
                foreach (DataEnter D in MAGIC)
                {
                    a = Instantiate(OPTIONS_DO_NOT_CHANGE[(int)D.type], Vector3.zero, new Quaternion(0, 0, 0, 0), PAGES_DO_NOT_CHANGE.transform.Find("6Magic"));
                    a.transform.localPosition = new Vector3(D.Position.x, D.Position.y);
                    scr_BaseEnter be = a.GetComponent<scr_BaseEnter>();
                    be.VariableName = D.VariableName;
                    be.tab = 6;
                    be.options.AddRange(D.Options);
                    be.UpdateColour();
                }
            }
        }
    }
    //Type Enum for the Input Areas
    public enum DataTypes
    {
        NAME = 0,
        VERSION = 1,
        TEXT = 2,
        LARGETEXT = 3,
        FLOAT = 4,
        BOXFLOAT = 5,
        BOOL = 6,
        DICE = 7,
        STAT = 8,
        DROPDOWN = 9
    }
}