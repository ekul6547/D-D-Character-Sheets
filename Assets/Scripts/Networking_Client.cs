using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace SheetCreator
{

    public class Networking_Client : NetworkBehaviour
    {

        [SyncVar]
        public string Name; //Client Name - Randomised
        static string selection = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";

        public bool newUpdates = false; //If anyone else has saved any other character

        [SyncVar]
        public string JsonData;

        public override void OnStartLocalPlayer() //Start after object is loaded
        {
            if (isServer)
            {
                SaveInstance.active.LoadDB();
            }
            if (!isLocalPlayer)
            {
                return;
            }
            string S = "";
            for (int i = 16; i > 0; i--)
            {
                S += RandomChar();
            }
            Name = S;
            CmdSetName(S); //New name for all clients and server
            Debug.Log("Local Player set to " + Name);

            ContentManager.active.LocalClient = GetComponent<Networking_Client>(); //Set the local client for ContentManager
            if (Networking_Host.active.Host == "")
                Networking_Host.active.CmdSetHost(); //If the first player, set as host
            ContentManager.active.CheckLoad(); //Load the character list from server. THIS MUST BE DONE AFTER HOST HAS BEEN SET

            base.OnStartLocalPlayer();
        }

        public void DoUpdates() //Activate Updates Available
        {
            newUpdates = true;
            RpcDoUpdates();
        }
        [ClientRpc]
        public void RpcDoUpdates()
        {
            newUpdates = true;
        }

        public void StopUpdates() //Stop Updates Available
        {
            newUpdates = false;
            RpcStopUpdates();
        }
        [ClientRpc]
        public void RpcStopUpdates()
        {
            newUpdates = false;
        }

        [Command]
        public void CmdSetName(string N) //Set Name on server
        {
            Name = N;
        }
        string RandomChar() //Random Char from string
        {
            return selection.Substring(Random.Range(0, selection.Length), 1);
        }

        public void AddToList(string ClientName, int charID, string LoadName) //Add option to list (Client side
        {
            RpcAddToList(charID, LoadName);
        }
        [ClientRpc]
        public void RpcAddToList(int ID, string LoadName)
        {
            if (!isLocalPlayer)
                return;
            Debug.Log("Received Character " + ID + ": " + LoadName + " from Server");
            ContentManager.active.AddCharacterOption(ID, LoadName);
        }

        public void GetJsonData(string LoadName, string folder) //Request the json data for a character
        {
            if (!isLocalPlayer)
            {
                return;
            }
            Debug.Log("Requesting data for: " + LoadName);
            Networking_Host.active.CmdGetJsonFromFile(Name, LoadName, folder);
        }
        public void GetData(int LoadID)
        {
            if (!isLocalPlayer)
            {
                return;
            }
            Debug.Log("Requesting data for: " + LoadID);
            Networking_Host.active.CmdGetData(Name, LoadID);
        }

        ContentInstance password_inst; //The instance you're checking for
        string passwordCheckFunction = ""; //"load" or "save"
        int passwordCheckID = -1; //who you're checking for
        //Load Save When password is true
        void PasswordLoadSave(int CharID)
        {
            if (password_inst.ID == CharID)
            {
                if (passwordCheckFunction == "save")
                {
                    password_inst.Save(true);
                    password_inst = null;
                    passwordCheckFunction = "";
                    passwordCheckID = -1;
                }
                else if (passwordCheckFunction == "load")
                {
                    password_inst.Load(true);
                    password_inst = null;
                    passwordCheckFunction = "";
                    passwordCheckID = -1;
                }
                else
                {
                    ForceCancelPassword();
                }
            }
            else
            {
                ForceCancelPassword();
            }
        }
        void ForceCancelPassword()
        {
            ContentManager.active.AddMessage("Error when checking for password");
            password_inst = null;
            passwordCheckFunction = "";
            passwordCheckID = -1;
        }

        //Call a check for a password
        public void CheckForPassword(int CharacterID,ContentInstance instance, string checkFunction)
        {
            passwordCheckFunction = checkFunction;
            password_inst = instance;
            if (passwordCheckFunction == "save")
                Networking_Host.active.CmdRequiresSavePassword(Name, CharacterID);
            else if (passwordCheckFunction == "load")
                Networking_Host.active.CmdRequiresLoadPassword(Name, CharacterID);
            else
                ForceCancelPassword();
        }
        //Received if character has a password
        public void ReceiveCheckForPassword(int CharacterID, bool hasPassword)
        {
            RpcReceiveCheckForPassword(CharacterID, hasPassword);
        }
        [ClientRpc]
        public void RpcReceiveCheckForPassword(int CharacterID, bool hasPassword)
        {
            if (hasPassword)
            {
                ContentManager.active.StartPasswordCheck(CharacterID);
                passwordCheckID = CharacterID;
            }
            else
            {
                PasswordLoadSave(CharacterID);
            }
        }

        //Call a check for a password - fokr canview/ cansave
        public void TestPassword(int CharID, string Check)
        {
            if (passwordCheckFunction == "load")
                Networking_Host.active.CmdTestCanView(Name, CharID, Check);
            else if (passwordCheckFunction == "save")
                Networking_Host.active.CmdTestCanSave(Name, CharID, Check);
            else
                ForceCancelPassword();
        }
        //Received if the password is correct
        public void ReceivePasswordTest(int CharID, bool result)
        {
            RpcReceivePasswordTest(CharID, result);
        }
        [ClientRpc]
        public void RpcReceivePasswordTest(int CharID, bool result)
        {
            if (result)
            {
                PasswordLoadSave(CharID);
            }
            else
            {
                ContentManager.active.AddMessage("Password is incorrect");
            }
        }

        [ClientRpc]
        public void RpcSetJsonData(int LoadID, string json) //Receive the json data for a character
        {
            if (!isLocalPlayer)
                return;
            Debug.Log("Retrieved data from server for: " + LoadID + "     " + json);
            JsonData = json;
            callDataReceived(LoadID,json);
        }

        [ClientRpc]
        public void RpcSetData(int LoadID, string data)
        {
            if (!isLocalPlayer)
                return;
            Debug.Log("Retrieved data from server for: "+ LoadID +"     "+data);
            callDataReceived(LoadID, data);
        }

        void callDataReceived(int LoadID,string data) //Called when json data is received
        {
            if (!isLocalPlayer)
            {
                return;
            }
            GameObject[] contents = GameObject.FindGameObjectsWithTag("ContentInstance");
            foreach (GameObject obj in contents)
            {
                ContentInstance inst = obj.GetComponent<ContentInstance>();
                if (inst.isLookingFor(LoadID))
                {
                    inst.OnClientRecieveData(LoadID, data); //Sends the json data to instances
                }
            }
        }

        public void Download() //Request sheet host is using
        {
            Debug.Log("Requesting Host's sheet");
            Networking_Host.active.CmdDownloadSheetFromHost(Name);
        }

        public void UploadSheetJson(string json) //Receive sheet from host
        {
            RpcSheetUpload(json);
        }
        [ClientRpc]
        public void RpcSheetUpload(string json) //Save sheet data received
        {
            if (!isLocalPlayer)
                return;
            Debug.Log("Recieved Sheet from host");
            GameObject.Find("Canvas").GetComponent<SheetCreator.scr_SaveSheetCustom>().LoadJson(json);
        }

        [ClientRpc]
        public void RpcRequestSheetJson(string ClientName) //Send sheet data to server. Used by host
        {
            if (!isLocalPlayer)
                return;
            Debug.Log("Getting current sheet");
            string json = GameObject.Find("Canvas").GetComponent<SheetCreator.scr_SaveSheetCustom>().GetJson();
            Debug.Log("Sending sheet to server");
            Networking_Host.active.CmdUploadSheetJson(ClientName, json);
        }
    }
}