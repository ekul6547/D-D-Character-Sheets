using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace SheetCreator
{
    public class Networking_Host : NetworkBehaviour
    {

        public static Networking_Host active;

        [SyncVar]
        public string Host = ""; //Saves Name of Host Client
        static GameObject[] clients; //List of currently connected clients. Regularly updated / refreshed

        void Awake()
        {
            active = this;
        }

        [Command]
        public void CmdSetHost() //Saves Host's name
        {
            ReloadClients();
            Host = clients[0].GetComponent<Networking_Client>().Name;
        }

        static void ReloadClients() //Refreshes list of client connections
        {
            clients = GameObject.FindGameObjectsWithTag("Player");
        }

        [Command]
        public void CmdAddNewOption(string ClientName, int  CharID) //Add new character option to specific or ALL clients
        {
            foreach (GameObject obj in clients)
            {
                if (obj.GetComponent<Networking_Client>().Name == ClientName || ClientName == "" || ClientName == "ALL")
                {
                    Debug.Log("Sent Character: " + CharID + " to " + ClientName);
                    string CharName = SaveInstance.active.getNameFromID(CharID);
                    obj.GetComponent<Networking_Client>().AddToList(ClientName, CharID, CharName); //Send character Name NOT data
                }
            }
        }

        public void LoadList(string ClientName) //Request for list of characters
        {
            Debug.Log("Sending Request for list from " + ClientName);
            CmdLoadList(ClientName);
        }

        [Command]
        public void CmdLoadList(string ClientName) //Received request for list
        {
            Debug.Log("Detecting Files for " + ClientName);
            string FolderPath = Application.dataPath + "/Settings/CharacterData/";
            int[] data = SaveInstance.active.FindAllIds(FolderPath); //Find all files that are characters
            ReloadClients();
            for(int i = 0; i < data.Length; i ++)
            {
                //string CharName = Path.GetFileName(s); //Get the character name
                //CharName = CharName.Substring(ContentInstance.filePrefix.Length, CharName.Length - 4); //remove .cfg off the end
                CmdAddNewOption(ClientName, data[i]); //Send Character Option
            }
        }

        [Command]
        public void CmdGetData(string ClientName, int CharID)
        {
            string strings = SaveInstance.active.getAllData(CharID);
            foreach (GameObject obj in clients)
            {
                Networking_Client C = obj.GetComponent<Networking_Client>();
                if (C.Name == ClientName)
                {
                    Debug.Log("Sending data for " + CharID + " to " + ClientName + ":    " + strings);
                    C.RpcSetData(CharID, strings);
                }
            }
        }
        
        [Command]
        public void CmdSaveCharacter(string ClientName, int CharacterID, string CharacterName, bool npc)
        {
            SaveInstance.active.NewCharacter(CharacterID, CharacterName, npc);
        }
        public void SaveCharacter(string ClientName, int CharacterID, string CharacterName, bool npc)
        {
            CmdSaveCharacter(ClientName, CharacterID, CharacterName, npc);
        }
        public void SaveVariable(string ClientName, string CharacterName, string VariableName, object value)
        {

            if (value.GetType() == typeof(string))
                CmdSaveString(ClientName, CharacterName, VariableName, (string)value);
            if (value.GetType() == typeof(float))
                CmdSaveFloat(ClientName, CharacterName, VariableName, (float)value);
            if (value.GetType() == typeof(bool))
                CmdSaveBool(ClientName, CharacterName, VariableName, (bool)value);
            if (value.GetType() == typeof(Vector2))
            {
                Vector2 V = (Vector2)value;
                CmdSaveStat(ClientName, CharacterName, VariableName, Mathf.RoundToInt(V.x), Mathf.RoundToInt(V.y));
            }
        }
        public void SaveVariables(string ClientName,int CharID, string data)
        {
            CmdSaveVariables(ClientName,CharID, data);
        }

        [Command]
        public void CmdSaveVariables(string ClientName,int CharID, string data)
        {
            Debug.Log("Received: " + data + " For " + CharID + " From " + ClientName);
            SaveInstance.data_struct unpacked_data = JsonUtility.FromJson <SaveInstance.data_struct>(data);
            int id = CharID;
            foreach (string s in unpacked_data.strings)
            {
                string[] variable_array = s.Split(SaveInstance.separator, System.StringSplitOptions.None);
                SaveInstance.active.SaveString(id, variable_array[0], variable_array[1]);
            }
            foreach (string s in unpacked_data.floats)
            {
                string[] variable_array = s.Split(SaveInstance.separator, System.StringSplitOptions.None);
                try
                {
                    float f = float.Parse(variable_array[1]);
                    SaveInstance.active.SaveFloat(id, variable_array[0], f);
                }
                catch
                {
                    Debug.LogWarning("Failed conversion of float from " + ClientName);
                }
            }
            foreach (string s in unpacked_data.bools)
            {
                string[] variable_array = s.Split(SaveInstance.separator, System.StringSplitOptions.None);
                try
                {
                    bool b = bool.Parse(variable_array[1]);
                    SaveInstance.active.SaveBool(id, variable_array[0], b);
                }
                catch
                {
                    Debug.LogWarning("Failed conversion of bool from " + ClientName);
                }
            }
            foreach (string s in unpacked_data.stats)
            {
                string[] variable_array = s.Split(SaveInstance.separator, System.StringSplitOptions.None);
                try
                {
                    int i_a = int.Parse(variable_array[1]);
                    int i_b = int.Parse(variable_array[2]);
                    SaveInstance.active.SaveStat(id, variable_array[0], i_a, i_b);
                }
                catch
                {
                    Debug.LogWarning("Failed conversion of stat from " + ClientName);
                }
            }
        }
        [Command]
        public void CmdSaveString(string ClientName, string CharacterName, string VariableName, string Value)
        {
            int id = SaveInstance.active.getIdFromName(CharacterName);
            SaveInstance.active.SaveString(id, VariableName, Value);
        }
        [Command]
        public void CmdSaveFloat(string ClientName, string CharacterName, string VariableName, float Value)
        {
            int id = SaveInstance.active.getIdFromName(CharacterName);
            SaveInstance.active.SaveFloat(id, VariableName, Value);
        }
        [Command]
        public void CmdSaveBool(string ClientName, string CharacterName, string VariableName, bool Value)
        {
            int id = SaveInstance.active.getIdFromName(CharacterName);
            SaveInstance.active.SaveBool(id, VariableName, Value);
        }
        [Command]
        public void CmdSaveStat(string ClientName, string CharacterName, string VariableName, int Value_a, int Value_b)
        {
            int id = SaveInstance.active.getIdFromName(CharacterName);
            SaveInstance.active.SaveStat(id, VariableName, Value_a,Value_b);
        }

        [Command]
        public void CmdGetJsonFromFile(string ClientName, string name, string folder) //Request for character data
        {
            string json = SaveInstance.active.GetJsonFromFile(name, folder); //Load Json data
            ReloadClients();
            foreach (GameObject client in clients)
            {
                Networking_Client clientData = client.GetComponent<Networking_Client>();
                if (clientData.Name == ClientName)
                {
                    clientData.RpcSetJsonData(int.Parse(name), json); //Send character option data
                }
            }
        }

        public void SetUpdate(string exclude) //Set update available for everyone except exclude
        {
            ReloadClients();
            foreach (GameObject obj in clients)
            {
                Networking_Client C = obj.GetComponent<Networking_Client>();
                if (C.Name != exclude)
                {
                    C.DoUpdates();
                }
            }
        }

        [Command]
        public void CmdRequiresLoadPassword(string ClientName, int CharID)
        {
            bool hasPassword = SaveInstance.active.NeedsPasswordForLoad(CharID);
            Debug.Log("Checking NeedsPasswordForLoad for " + CharID + " result: " + hasPassword.ToString());
            foreach (GameObject obj in clients)
            {
                Networking_Client C = obj.GetComponent<Networking_Client>();
                if (C.Name == ClientName)
                {
                    C.ReceiveCheckForPassword(CharID, hasPassword);
                }
            }
        }
        [Command]
        public void CmdRequiresSavePassword(string ClientName, int CharID)
        {
            bool hasPassword = SaveInstance.active.NeedsPasswordForSave(CharID);
            Debug.Log("Checking NeedsPasswordForSave for " + CharID + " result: " + hasPassword.ToString());
            foreach (GameObject obj in clients)
            {
                Networking_Client C = obj.GetComponent<Networking_Client>();
                if (C.Name == ClientName)
                {
                    C.ReceiveCheckForPassword(CharID, hasPassword);
                }
            }
        }
        [Command]
        public void CmdTestCanView(string ClientName, int CharID, string Check)
        {
            bool passwordResult = SaveInstance.active.CanView(CharID, Check);
            Debug.Log("Checking CanView for " + CharID + " result: " + passwordResult.ToString());
            foreach (GameObject obj in clients)
            {
                Networking_Client C = obj.GetComponent<Networking_Client>();
                if (C.Name == ClientName)
                {
                    C.ReceivePasswordTest(CharID, passwordResult);
                }
            }
        }
        [Command]
        public void CmdTestCanSave(string ClientName, int CharID, string Check)
        {
            bool passwordResult = SaveInstance.active.CanSave(CharID, Check);
            Debug.Log("Checking CanSave for " + CharID + " result: " + passwordResult.ToString());
            foreach (GameObject obj in clients)
            {
                Networking_Client C = obj.GetComponent<Networking_Client>();
                if (C.Name == ClientName)
                {
                    C.ReceivePasswordTest(CharID, passwordResult);
                }
            }
        }

        [Command]
        public void CmdDownloadSheetFromHost(string ClientName) //Request for sheet host is using
        {
            Debug.Log("Identifying Host: " + Host);
            ReloadClients();
            foreach (GameObject obj in clients)
            {
                Networking_Client C = obj.GetComponent<Networking_Client>();
                if (C.Name == Host)
                {
                    C.RpcRequestSheetJson(ClientName); //Request Sheet data from host
                }
            }
        }
        [Command]
        public void CmdUploadSheetJson(string ClientName, string json) //Received sheet data from host
        {
            foreach (GameObject obj in clients)
            {
                Networking_Client C = obj.GetComponent<Networking_Client>();
                if (C.Name == ClientName)
                {
                    Debug.Log("Sending sheet to " + ClientName);
                    C.UploadSheetJson(json); //Send sheet data to client
                }
            }
        }
    }
}