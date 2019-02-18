using System.Collections;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;

namespace SheetCreator
{

    public class ContentManager : MonoBehaviour
    {

        public static ContentManager active; //Instantof itself

        [Header("Client")]
        public Networking_Client LocalClient; //The local player client


        [Header("Character List")]
        public List<ContentInstance> contentList = new List<ContentInstance>(); //Currently active character instants

        public ContentInstance activeInstant; //Currently selected character
        public Color CurrentColor; //Active color
        Color PreColor; //Storage for the option's previous color

        public RectTransform ScrollArea; //Area where listing appears
        public GameObject Template; //Template of character listing
        public Color evenColor;
        public Color oddColor;

        public InputField newCharName;

        [Header("Other")]
        public DiceRoller DiceRolling; //Window for advanced die rolling
        public GameObject UpdateArrow; //Object that means "Update Available"
        float startY;

        public GameObject AddDropDown; //Window to input new drop down
        public GameObject DownloadButton; //Download sheet from host

        public GameObject PasswordPanel;

        void Awake()
        {
            //Setup
            active = this;
            if (UpdateArrow != null)
            {
                startY = UpdateArrow.transform.localPosition.y;
            }
        }
        void Update()
        {
            //Move the arrow up and down when update is available
            if (UpdateArrow != null)
            {
                if (LocalClient != null)
                {
                    UpdateArrow.SetActive(LocalClient.newUpdates);
                    Vector3 pos = UpdateArrow.transform.localPosition;
                    pos.y = startY + (Mathf.Sin(Time.time * 3) * 5);
                    UpdateArrow.transform.localPosition = pos;
                }
                else
                {
                    UpdateArrow.SetActive(false);
                }
            }
            if (messageQueue.Count > 0 && !messageIsShowing) ShowNextMessage();
        }
        public void CheckLoad() //Checks if correct scene before loading the list
        {
            //Needs to activate here as it is after host is set
            if (DownloadButton != null)
            {
                if (LocalClient.Name == Networking_Host.active.Host)
                {
                    DownloadButton.SetActive(false);
                }
            }
            if (SceneManager.GetActiveScene().name == "Main")
                LoadList(); //If main scene and not sheet creator, load the list of characters

        }
        public void SetAsActive(ContentInstance I) //Set the active character
        {
            if (I != null)
            {
                ColorBlock C;
                if (activeInstant != null) //Reset old one
                {
                    C = activeInstant.GetComponent<Button>().colors;
                    C.normalColor = PreColor;
                    activeInstant.GetComponent<Button>().colors = C;
                }
                activeInstant = I; //Set here
                C = activeInstant.GetComponent<Button>().colors;
                PreColor = C.normalColor; //Store old colors
                C.normalColor = CurrentColor; //Selection Color
                activeInstant.GetComponent<Button>().colors = C;
            }
        }
        public void SaveActive() //Calls the active character to save
        {
            activeInstant.Save();
        }
        public void ReloadList() //Recreates the list (Refresh button)
        {
            foreach (ContentInstance I in contentList)
            {
                Destroy(I.gameObject); //Destroy all in list
            }
            contentList.Clear();
            ScrollArea.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 0); //Sets scroll size to 0, so that it wont infinetally expand
            LocalClient.StopUpdates();
            LoadList(); //Loads the list
        }

        [ContextMenu("Load List")]
        public void LoadList() //Request list from server
        {
            Networking_Host.active.LoadList(LocalClient.Name);
        }

        public void SaveCharacter(int CharacterID, string CharacterName, bool IsNPC)
        {
            Networking_Host.active.SaveCharacter(LocalClient.name, CharacterID, CharacterName, IsNPC);
        }
        public void SaveVariable(string CharacterName, string VariableName, object Value)
        {
            Networking_Host.active.SaveVariable(LocalClient.Name, CharacterName, VariableName, Value);
        }
        public void SaveVariables(int CharID, string data)
        {
            Debug.Log("Sending: " + data);
            Networking_Host.active.SaveVariables(LocalClient.Name, CharID, data);
        }


        public void SaveSheetInstance(string Name, string Folder, object instance, bool neat) //Save files, either on server or client side
        {
            string json = SaveInstance.active.TurnToJson(instance, neat);
            if (json != null)
            {
                SaveSheetToFile(Name + ".cfg", Folder, json);
            }
        }
        public void SaveSheetToFile(string Name, string folder, string json) //Save on client
        {
            SaveInstance.active.WriteToFile(json, Application.dataPath + "/Settings/" + folder, Name);
        }


        //TODO Add character buton adds blank to database, not list, and updates others

        public void AddOptionFromPopup()
        {
            AddCharacterOption(0, newCharName.text);
            newCharName.text = "";
        }

        [ContextMenu("AddBlank")]
        public void AddBlankOption() //Add blank option to character list. Will not update for other clients until character is saved
        {
            //Networking_Host.active.CmdAddNewOption("ALL","");
            AddCharacterOption(-1,"");
        }

        public void AddCharacterOption(int charID, string CharName) //Add option to character list with name CharName
        {
            float gapHeight = 45; //height of buttons in list + gap between
            ScrollArea.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, ScrollArea.rect.height + gapHeight); //Add this height to the scroll area
            GameObject newOption = Instantiate(Template, Template.transform.parent); //Create new button
            newOption.SetActive(true);

            RectTransform NORectTrans = newOption.GetComponent<RectTransform>();
            Vector3 newPos = NORectTrans.anchoredPosition;
            newPos.y -= contentList.Count * gapHeight; //Move down to latest point
            NORectTrans.anchoredPosition = newPos;

            ContentInstance inst = newOption.GetComponent<ContentInstance>(); //Character Data
            inst.ID = charID;
            inst.CharacterName = CharName; //Set the name
            //inst.ReloadFile(); //Load any Data from file
            Button newButton = newOption.GetComponent<Button>();
            ColorBlock C = newButton.colors;
            if (contentList.Count % 2 == 0)
            {
                C.normalColor = evenColor;
            }
            else
            {
                C.normalColor = oddColor;
            }
            newButton.colors = C; //Set color depending if even or odd in list
            contentList.Add(inst); //Add to instant list
        }

        public void DownloadCurrentSheet() //Request to download the currently used sheet by host
        {
            LocalClient.Download();
        }

        int passwordCheck_CharID = -1;
        public void StartPasswordCheck(int CharacterID)
        {
            PasswordPanel.SetActive(true);
            passwordCheck_CharID = CharacterID;
        }

        public void OnPasswordSubmit()
        {
            InputField passwordInput = PasswordPanel.transform.Find("PasswordInput").GetComponent<InputField>();

            LocalClient.TestPassword(passwordCheck_CharID, passwordInput.text);
            passwordCheck_CharID = -1;
        }

        [Header("Messages")]
        public GameObject MessagePanel;
        public Text MessageOutput;
        public List<string> messageQueue = new List<string>();
        public bool messageIsShowing = false;

        public void AddMessage(string message)
        {
            messageQueue.Add(message);
        }

        public void ShowNextMessage()
        {
            if(messageQueue.Count > 0)
            {
                MessagePanel.SetActive(true);
                MessageOutput.text = messageQueue[0];
                messageQueue.RemoveAt(0);
                messageIsShowing = true;
            }
            else
            {
                MessagePanel.SetActive(false);
                messageIsShowing = false;
            }
        }


        public const string hexCharacters = "0123456789ABCDEF";
        public static string ColorToHex(Color color)
        {
            string hex_r = byteToHex(byte.Parse((Mathf.RoundToInt(color.r * 255)).ToString()));
            string hex_g = byteToHex(byte.Parse((Mathf.RoundToInt(color.g * 255)).ToString()));
            string hex_b = byteToHex(byte.Parse((Mathf.RoundToInt(color.b * 255)).ToString()));
            return hex_r + hex_g + hex_b;
        }
        public static string byteToHex(byte b)
        {
            int char1 = 0;
            while(b - 16 > 0)
            {
                char1++;
                b -= 16;
            }
            int char2 = b;
            string hex = hexCharacters.Substring(char1, 1) + hexCharacters.Substring(char2, 1);
            return hex;
        }
        public static Color HexToColor(string hex)
        {
            int r = HexToByte(hex.Substring(0, 2));
            int g = HexToByte(hex.Substring(2, 2));
            int b = HexToByte(hex.Substring(4, 2));
            return new Color(r/255f, g/255f, b/255f);
        }
        public static byte HexToByte(string hex)
        {
            if(hex.Length != 2)
            {
                throw new Exception("Hex String Invalid!");
            }
            char[] chars = hex.ToUpper().ToCharArray();
            int char1 = hexCharacters.IndexOf(chars[0])*16;
            int char2 = hexCharacters.IndexOf(chars[1]);
            byte final = (byte)(char1 + char2);
            return final;
        }
    }
}