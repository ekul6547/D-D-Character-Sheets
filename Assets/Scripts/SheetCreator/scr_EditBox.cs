using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace SheetCreator
{
    public class scr_EditBox : MonoBehaviour
    {

        public static scr_EditBox active;
        void Awake()
        {
            active = this;
        }

        public Transform currentEdit; //Currently editing input area

        public Slider xSlider;
        public Slider ySlider;
        public InputField xInput;
        public InputField yInput;
        public InputField NameInput;

        public RectTransform main; //The page you are editing on

        public void OnSlidersChange()
        {
            if (currentEdit != null) //if theres an edit
            {
                if (xSlider.gameObject.activeSelf) //if sliders are active
                {
                    RectTransform rectT = currentEdit.GetComponent<RectTransform>();
                    Vector2 size = new Vector2(rectT.rect.width, rectT.rect.height); //Size of area the box can fit in
                    Vector2 from = new Vector2(main.rect.xMin + (size.x * rectT.pivot.x), main.rect.yMax - (size.y * (1 - rectT.pivot.y))); //The XY point the box moves from
                    Vector2 limits = new Vector2(main.rect.width - size.x, main.rect.height - size.y); //The size the box can move to (so it doesn't overlap the edges)

                    Vector3 pos = currentEdit.localPosition;
                    pos.x = Mathf.Round(from.x + ((xSlider.value / xSlider.maxValue) * limits.x));
                    pos.y = Mathf.Round(from.y - ((ySlider.value / ySlider.maxValue) * limits.y));
                    currentEdit.localPosition = pos;
                }
            }
        }
        //Compatibility between input fields and sliders
        public void SetX(string i)
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
            SetX(a);
        }
        public void SetX(float i)
        {
            if (currentEdit != null)
            {
                RectTransform rectT = currentEdit.GetComponent<RectTransform>();
                Vector2 size = new Vector2(rectT.rect.width, rectT.rect.height);
                Vector2 from = new Vector2(main.rect.xMin + (size.x * rectT.pivot.x), main.rect.yMax - (size.y * (1 - rectT.pivot.y)));
                Vector2 limits = new Vector2(main.rect.width - size.x, main.rect.height - size.y);

                if (i < 0)
                    i = 0;
                if (i > limits.x)
                    i = limits.x;

                Vector3 pos = currentEdit.localPosition;
                pos.x = Mathf.Round(from.x + i);
                currentEdit.localPosition = pos;
            }
        }
        public void SetY(string i)
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
            SetY(a);
        }
        public void SetY(float i)
        {
            if (currentEdit != null)
            {
                RectTransform rectT = currentEdit.GetComponent<RectTransform>();
                Vector2 size = new Vector2(rectT.rect.width, rectT.rect.height);
                Vector2 from = new Vector2(main.rect.xMin + (size.x * rectT.pivot.x), main.rect.yMax - (size.y * (1 - rectT.pivot.y)));
                Vector2 limits = new Vector2(main.rect.width - size.x, main.rect.height - size.y);

                if (i < 0)
                    i = 0;
                if (i > limits.y)
                    i = limits.y;

                Vector3 pos = currentEdit.localPosition;
                pos.y = Mathf.Round(from.y - i);
                currentEdit.localPosition = pos;
            }
        }

        public void setEdit(Transform obj) //Set an input area to be the selected one to edit
        {
            if (currentEdit != null) //If an input is selected, then revert it's color
            {
                SaveOptions();
                currentEdit.GetComponent<Image>().color = CurrentColor;
            }
            if (currentEdit == obj || obj == null) //If the new area is already selected, then deselect it
            {
                currentEdit = null;
                UpdateInputs();
                NameInput.text = "";
                SetColourSliders(Color.white);
                OptionsInput.text = "";
                return;
            }

            currentEdit = obj; //Set to the new edit
            UpdateInputs(); //update sliders and text input

            string VarName = obj.Find("VariableName").GetComponent<Text>().text; //Update the Variable Name Input Field
            if (VarName.EndsWith(":"))
            {
                VarName = VarName.Substring(0, VarName.Length - 1);
            }
            Debug.Log(VarName + " Selected.");

            NameInput.text = VarName;
            SetColourSliders(currentEdit.GetComponent<Image>().color); //Store previous color
            currentEdit.GetComponent<Image>().color = HighlightedColor; //Set to selected color

            IEnterField inputType = currentEdit.gameObject.GetComponent<IEnterField>();

            string[] optionsTo = inputType.options.ToArray();
            if (optionsTo.Length > 0)
            {
                int colourAt = -1;
                for(int i = 0; i < optionsTo.Length; i++)
                {
                    if (optionsTo[i].Contains(ColourOptionPrefix))
                    {
                        colourAt = i;
                        break;
                    }
                }
                if (colourAt > -1){
                    string ColourHex = optionsTo[colourAt].Substring(ColourOptionPrefix.Length);
                    CurrentColor = ContentManager.HexToColor(ColourHex);
                }
                optionsTo = inputType.GetNonOptions().ToArray();

                if (optionsTo.Length > 0)
                {
                    OptionsInput.text = string.Join("\n", optionsTo);
                }
            }

            //Disable the Variable Name input
            bool dis = false;
            if (currentEdit.GetComponent<scr_NameEnter>() != null)
                dis = true;
            if (currentEdit.GetComponent<scr_VersionEnter>() != null)
                dis = true;

            if (dis)
            {
                NameInput.DeactivateInputField();
            }
            else
            {
                NameInput.ActivateInputField();
            }
        }

        public void SetName(string n) //Set the variable name
        {
            if (currentEdit != null)
            {
                currentEdit.GetComponent<scr_BaseEnter>().VariableName = n;
            }
        }

        public void Delete() //Delete the input area
        {
            if (currentEdit != null)
            {
                DestroyObject(currentEdit.gameObject);
                setEdit(null);
            }
        }

        public void CreateOption(int i) //create a new input area specified to i. 0 Should be character name
        {
            GameObject[] options = GameObject.FindObjectOfType<scr_SaveSheetCustom>().OPTIONS_DO_NOT_CHANGE;
            if (i < options.Length)
            {
                GameObject instant = Instantiate(options[i], main); //create input area
                instant.GetComponent<scr_BaseEnter>().tab = int.Parse(main.name.Substring(0, 1)); //set which tab it appears on
                setEdit(instant.transform); //set to the current edit
            }
        }
        //public GameObject[] ToSwitch;

        public void Switch() //Switch between sliders and text entry
        {
            xSlider.gameObject.SetActive(!xSlider.gameObject.activeSelf);
            ySlider.gameObject.SetActive(!ySlider.gameObject.activeSelf);
            xInput.gameObject.SetActive(!xInput.gameObject.activeSelf);
            yInput.gameObject.SetActive(!yInput.gameObject.activeSelf);
            UpdateInputs();
        }

        public void UpdateInputs() //Update the Sliders and Text entries
        {
            if (currentEdit != null)
            {
                RectTransform rectT = currentEdit.GetComponent<RectTransform>();
                Vector2 size = new Vector2(rectT.rect.width, rectT.rect.height); //Size of object
                Vector2 from = new Vector2(main.rect.xMin + (size.x * rectT.pivot.x), main.rect.yMax - (size.y * (1 - rectT.pivot.y))); //XY of Area it can move in
                Vector2 limits = new Vector2(main.rect.width - size.x, main.rect.height - size.y); //Size of area it can move in
                Vector3 pos = currentEdit.localPosition;
                xSlider.value = ((pos.x - from.x) / limits.x) * xSlider.maxValue;
                ySlider.value = ((from.y - pos.y) / limits.y) * ySlider.maxValue;
                xInput.text = pos.x.ToString();
                yInput.text = (0 - pos.y).ToString();
            }
            else
            {
                xSlider.value = 0;
                ySlider.value = 0;
                xInput.text = "0";
                yInput.text = "0";
            }
        }

        [ContextMenu("Zero In")]
        public void ZeroAll() //Set all input area positions to 0,0
        {
            GameObject[] edits = GameObject.FindGameObjectsWithTag("InputArea");
            foreach (GameObject obj in edits)
            {
                obj.transform.localPosition = Vector3.zero;
            }
            UpdateInputs();
        }


        public InputField OptionsInput;

        public void SaveOptions()
        {
            if (currentEdit == null) return;
            string[] hexStart = new string[] { ColourOptionPrefix + ContentManager.ColorToHex(CurrentColor) };
            hexStart[0] = hexStart[0].Replace("-", "");
            string[] options = hexStart.Concat(OptionsInput.text.Split('\n')).ToArray();
            var main = currentEdit.GetComponent<scr_BaseEnter>();
            main.options.Clear();
            main.options.AddRange(options);
        }

        [Header("Colour Options")]
        public Color HighlightedColor;
        public Color CurrentColor;
        public Slider RedSlider;
        public Slider GreenSlider;
        public Slider BlueSlider;
        public Image ShowColour;
        public const string ColourOptionPrefix = "Colour:";
        //CurrentColor

        public void SetColourSliders(Color c)
        {
            RedSlider.value = c.r * 255;
            GreenSlider.value = c.g * 255;
            BlueSlider.value = c.b * 255;
            ShowColour.color = c;
            CurrentColor = c;
        }

        public void ColourSliders()
        {
            CurrentColor.r = RedSlider.value / 255f;
            CurrentColor.g = GreenSlider.value / 255f;
            CurrentColor.b = BlueSlider.value / 255f;
            CurrentColor.a = 1;
            ShowColour.color = CurrentColor;
        }
    }
}
