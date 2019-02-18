using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SheetCreator {
    public class CreateButton : MonoBehaviour {
         
        //Used to store the id for adding input areas
        
        public scr_EditBox edit;

        public int id;

        public void Invoke()
        {
            edit.CreateOption(id);
        }
    }
}