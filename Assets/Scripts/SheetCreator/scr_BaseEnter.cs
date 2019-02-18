using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Linq;

namespace SheetCreator
{
    [ExecuteInEditMode]
    public class scr_BaseEnter : MonoBehaviour, IEnterField
    {
        private Text _name;
        public Text Name { get { return _name; } set { _name = value; } }

        private string _variableName = "";
        public string VariableName { get { return _variableName; } set { _variableName = value; } }

        private int _tab = 0;
        public int tab { get { return _tab; } set { _tab = value; } }

        private DataTypes _type;
        public DataTypes type { get { return _type; } set { _type = value; } }

        private GameObject[] _CreatorOnly;
        public GameObject[] CreatorOnly { get { return _CreatorOnly; } set { _CreatorOnly = value; } }

        private List<string> _options = new List<string>();
        public List<string> options
        {
            get { return _options; }
            set { _options = value; UpdateColour(); }
        }
        public void Start()
        {
            if(SceneManager.GetActiveScene().name != "SheetCreator")
            {
                foreach (GameObject a in CreatorOnly)
                {
                    a.SetActive(false);
                }
            }
        }
        void Update()
        {
            Name.text = VariableName;
        }
        public void setEdit()
        {
            scr_EditBox.active.setEdit(gameObject.transform);
        }

        public virtual void Set(string newValue)
        {
            throw new System.NotImplementedException();
        }

        public virtual void SetValue()
        {
            throw new System.NotImplementedException();
        }
        public void SetColour(Color c)
        {
            //Debug.Log("Setting " + VariableName + " to " + c.r + ":" + c.g + ":" + c.b);
            gameObject.GetComponent<Image>().color = c;
        }
        public void UpdateColour() //TODO Make it so there is the variable Text:###### for text colour. Include List for Text Components to change.
        {
            int colourAt = FirstOptionIndex(scr_EditBox.ColourOptionPrefix);
            if (colourAt > -1)
            {
                string ColourHex = options[colourAt].Substring(scr_EditBox.ColourOptionPrefix.Length);
                Color c = ContentManager.HexToColor(ColourHex);
                SetColour(c);
            }
        }
        public int FirstOptionIndex(string optionContains)
        {
            int optionAt = -1;
            for (int i = 0; i < options.Count; i++)
            {
                if (options[i].Contains(optionContains))
                {
                    optionAt = i;
                    break;
                }
            }
            return optionAt;
        }

        public List<string> GetNonOptions()
        {
            return options.Where((option) => !option.Contains(":")).ToList();
        }
    }

    public interface IEnterField
    {
        Text Name { get; set; }
        string VariableName { get; set; }
        int tab { get; set; }
        DataTypes type { get; set; }
        GameObject[] CreatorOnly { get; set; }
        List<string> options { get; set; }

        void Set(string newValue);

        void SetValue();
        void SetColour(Color c);
        int FirstOptionIndex(string optionContains);
        List<string> GetNonOptions();
    }
}