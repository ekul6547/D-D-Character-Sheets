using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SheetCreator
{
    [ExecuteInEditMode]
    public class EnterHooks : MonoBehaviour {

        public Text Name;
        public int tab;
        public string VariableName;
        public DataTypes type;
        public GameObject[] CreatorOnly;
        public List<string> options;

        private void Awake()
        {
            Update();
            this.enabled = false;
        }

        [ContextMenu("Force Update")]
        public void Update() {
            IEnterField field = gameObject.GetComponent<IEnterField>();
            field.options = this.options;
            field.Name = this.Name;
            field.tab = this.tab;
            field.VariableName = this.VariableName;
            field.type = this.type;
            field.CreatorOnly = this.CreatorOnly;
            this.enabled = false;
        }
    }
}
