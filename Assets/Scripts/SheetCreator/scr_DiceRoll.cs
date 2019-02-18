using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SheetCreator
{
    public class scr_DiceRoll : scr_BaseEnter
    {
        public Text Output;
        public int RollBasic(int amount)
        {
            return Random.Range(1, amount + 1);
        }

        public void SendRoll(int amount)
        {
            Output.text = RollBasic(amount).ToString();
        }

        public void AdvancedRoll()
        {
            if(ContentManager.active.DiceRolling != null)
                ContentManager.active.DiceRolling.gameObject.SetActive(!ContentManager.active.DiceRolling.gameObject.activeSelf);
        }
    }
}