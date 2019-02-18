using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using SheetCreator;

public class ContentInstance : MonoBehaviour {

    public static string filePrefix = "";

    public int ID = -1;
    public string CharacterName = "DefaultName"; //Character name is stored seperately. Makes things easier
    public bool ISNPC = false;

    //public bool IsPlayer = false;

    void Awake()
    {
        /*if (!ContentManager.active.contentList.Contains(this))
        {
            ContentManager.active.contentList.Add(this); //if not in the character list, add to character list
        }*/
    }
    void Update()
    {
        gameObject.name = filePrefix + CharacterName;
        gameObject.transform.Find("Text").GetComponent<Text>().text = CharacterName;
    }

    public void SetAsActive() //Set as the active character. Any edits / saves will be applied to this character
    {
        ContentManager.active.SetAsActive(gameObject.GetComponent<ContentInstance>());
        ReloadFile();
    }

    //Storage formats
    [System.Serializable]
    public class Sheet_string
    {
        public string VariableName;
        public string Value;
        public Sheet_string(string N, string V)
        {
            VariableName = N;
            Value = V;
        }
    }
    [System.Serializable]
    public class Sheet_bool //TODO change to bool? so I can have Value.HasValue
    {
        public string VariableName;
        public bool Value;
        public Sheet_bool(string N, bool V)
        {
            VariableName = N;
            Value = V;
        }
    } 
    [System.Serializable]
    public class Sheet_float
    {
        public string VariableName;
        public float Value;
        public Sheet_float(string N, float V)
        {
            VariableName = N;
            Value = V;
        }
    }
    [System.Serializable]
    public class Sheet_stat
    {
        public string VariableName;
        public Vector2 Value;
        public Sheet_stat(string N, float X, float Y)
        {
            VariableName = N;
            Value = new Vector2(X, Y);
        }
    }

    //Lists because dictionaries don't format to json
    public List<Sheet_string> CustomStrings = new List<Sheet_string>();
    public List<Sheet_bool> CustomBools = new List<Sheet_bool>();
    public List<Sheet_float> CustomFloats = new List<Sheet_float>();
    public List<Sheet_stat> CustomStats = new List<Sheet_stat>();

    //Ease of access to storage
    public string GetStringByName(string Name)
    {
        foreach(Sheet_string S in CustomStrings)
        {
            if (S.VariableName == Name)
                return S.Value;
        }
        return "";
    }
    public bool? GetBoolByName(string Name)
    {
        foreach (Sheet_bool S in CustomBools)
        {
            if (S.VariableName == Name)
                return S.Value;
        }
        return null;
    }
    public float GetFloatByName(string Name)
    {
        foreach (Sheet_float S in CustomFloats)
        {
            if (S.VariableName == Name)
                return S.Value;
        }
        return 0.0f;
    }
    public Vector2 GetStatByName(string Name)
    {
        foreach (Sheet_stat S in CustomStats)
        {
            if (S.VariableName == Name)
                return S.Value;
        }
        return Vector2.zero;
    }

    public void SetString(string Name,string Value)
    {
        foreach(Sheet_string S in CustomStrings)
        {
            if(S.VariableName == Name)
            {
                S.Value = Value;
                return;
            }
        }
        Sheet_string NewString = new Sheet_string(Name,Value);
        CustomStrings.Add(NewString);
    }
    public void SetBool(string Name, bool Value)
    {
        foreach (Sheet_bool S in CustomBools)
        {
            if (S.VariableName == Name)
            {
                S.Value = Value;
                return;
            }
        }
        Sheet_bool NewBool = new Sheet_bool(Name, Value);
        CustomBools.Add(NewBool);
    }
    public void SetFloat(string Name, float Value)
    {
        foreach (Sheet_float S in CustomFloats)
        {
            if (S.VariableName == Name)
            {
                S.Value = Value;
                return;
            }
        }
        Sheet_float NewFloat = new Sheet_float(Name, Value);
        CustomFloats.Add(NewFloat);
    }
    public void SetStat(string Name, float X, float Y)
    {
        foreach (Sheet_stat S in CustomStats)
        {
            if (S.VariableName == Name)
            {
                S.Value = new Vector2(X, Y);
                return;
            }
        }
        Sheet_stat NewFloat = new Sheet_stat(Name, X,Y);
        CustomStats.Add(NewFloat);
    }
    
    [ContextMenu("Save")]
    public void MenuSave()
    {
        Save(true);
    }

    public void Save(bool overridePassword = false) //Send save data
    {
        if (!overridePassword)
        {
            ContentManager.active.LocalClient.CheckForPassword(ID, this, "save");
            return;
        }
        //string fp = filePrefix;
        //string FileName = fp + CharacterName;
        //ContentManager.active.Save(true, FileName, "CharacterData", this, true);
        ContentManager.active.SaveCharacter(ID, CharacterName, ISNPC);
        
        SaveInstance.data_struct send_data;

        send_data.name = CharacterName;
        send_data.isNPC = ISNPC;

        List<string> tempList = new List<string>();
        foreach (Sheet_string v in CustomStrings)
        {
            tempList.Add(SaveInstance.Format(new object[]{ v.VariableName,v.Value}));
        }
        send_data.strings = tempList.ToArray();
        tempList.Clear();
        foreach (Sheet_float v in CustomFloats)
        {
            tempList.Add(SaveInstance.Format(new object[] { v.VariableName, v.Value }));
        }
        send_data.floats = tempList.ToArray();
        tempList.Clear();
        foreach (Sheet_bool v in CustomBools)
        {
            tempList.Add(SaveInstance.Format(new object[] { v.VariableName, v.Value }));
        }
        send_data.bools = tempList.ToArray();
        tempList.Clear();
        foreach (Sheet_stat v in CustomStats)
        {
            tempList.Add(SaveInstance.Format(new object[] { v.VariableName, v.Value.x, v.Value.y }));
        }
        send_data.stats = tempList.ToArray();
        ContentManager.active.SaveVariables(ID,SaveInstance.active.TurnToJson(send_data, false));
    }


    [ContextMenu("Load")]
    public void MenuLoad() //Load for no character name. Purely for context menu
    {
        Load();
    }
    public void ReloadFile() //Reload data from server
    {
        Load();
    }
    public void Load(bool overridePassword = false) //Load data of LoadName
    {
        if (overridePassword)
        {
            LoadServer(ID);
        }
        else
        {
            ContentManager.active.LocalClient.CheckForPassword(ID, this, "load");
        }
    }

    int CheckFor = -1;
    void LoadServer(int LoadID) //Load from server
    {
        ContentManager.active.LocalClient.GetData(LoadID);
        CheckFor = LoadID;
    }
    public bool isLookingFor(int n)
    {
        return n == CheckFor;
    }
    public void OnClientRecieveData(int LoadID,string data) //Called when client receives the json data
    {
        if(LoadID == CheckFor && CheckFor != -1)
        {
            LoadData(data);
        }
        CheckFor = -1;
    }
    void LoadData(string data)
    {
        SaveInstance.data_struct full_data = JsonUtility.FromJson<SaveInstance.data_struct>(data);
        CustomStrings.Clear();

        CharacterName = full_data.name;
        ISNPC = full_data.isNPC;

        foreach(string s in full_data.strings)
        {
            string[] unpacked = s.Split(SaveInstance.separator, System.StringSplitOptions.None);
            CustomStrings.Add(new Sheet_string(unpacked[0], unpacked[1]));
        }
        CustomFloats.Clear();
        foreach (string s in full_data.floats)
        {
            string[] unpacked = s.Split(SaveInstance.separator, System.StringSplitOptions.None);
            CustomFloats.Add(new Sheet_float(unpacked[0], float.Parse(unpacked[1])));
        }
        CustomBools.Clear();
        foreach (string s in full_data.bools)
        {
            string[] unpacked = s.Split(SaveInstance.separator, System.StringSplitOptions.None);
            CustomBools.Add(new Sheet_bool(unpacked[0], unpacked[0] == "1"));
        }
        CustomStats.Clear();
        foreach (string s in full_data.stats)
        {
            string[] unpacked = s.Split(SaveInstance.separator, System.StringSplitOptions.None);
            CustomStats.Add(new Sheet_stat(unpacked[0], int.Parse(unpacked[1]),int.Parse(unpacked[2])));
        }
        ReloadData();
    }
    public void ReloadData() //Load from lists into the input areas
    {
        if (ContentManager.active.activeInstant == gameObject.GetComponent<ContentInstance>())
        {
            GameObject[] fields = GameObject.FindGameObjectsWithTag("InputArea");
            foreach(GameObject O in fields)
            {
                scr_NameEnter N = O.GetComponent<scr_NameEnter>(); //Setting Name
                if (N != null)
                {
                    N.SetValues(CharacterName,ISNPC);
                    continue;
                }

                scr_TextEnter T = O.GetComponent<scr_TextEnter>(); //Settings Text entries
                if(T != null)
                {
                    T.SetString(GetStringByName(T.VariableName));
                    continue;
                }

                scr_BoolEnter B = O.GetComponent<scr_BoolEnter>(); //Setting bools
                if (B != null)
                {
                    bool? s = GetBoolByName(B.VariableName);
                    if(s.HasValue) //Bool might not exist
                        B.SetBool(s.Value);
                    continue;
                }

                scr_FloatEnter F = O.GetComponent<scr_FloatEnter>(); //Setting float and dropdown entries
                if (F != null)
                {
                    F.SetFloat(GetFloatByName(F.VariableName));
                    continue;
                }

                scr_StatEnter S = O.GetComponent<scr_StatEnter>(); //Setting Stat entries
                if (S != null)
                {
                    S.SetStats(GetStatByName(S.VariableName));
                    continue;
                }
            }
        }
    }
}