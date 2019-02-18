using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DiceRoller : MonoBehaviour {

    public QuickFloat Rolls;
    public QuickFloat DiceSize;
    public Text Output; //The output area
    public string[] LastResultsSent; //Last results
	
    public int[] RollDice(int amount, int limit) //Roll a (limit) dice (amount) times
    {
        List<int> results = new List<int>();
        for(int i = amount; i > 0; i--)
        {
            results.Add(RollDie(limit));
        }
        return results.ToArray();
    }

    int RollDie(int amount) //Roll a die
    {
        int i = Random.Range(1, amount + 1);
        Debug.Log("Dice Rolled: " + i);
        return i;
    }

    public void Send(int[] join) //Send list of integers to the output
    {
        List<string> T = new List<string>();
        string S = "";
        foreach(int i in join)
        {
            S += ", "  + i.ToString();
            T.Add(i.ToString());
        }
        S = S.Substring(1);
        Output.text = S;
        LastResultsSent = T.ToArray();
    }

    public void Send(string S) //Send the text to the output
    {
        Output.text = S;
    }

    public void RollSelected() //Roll chosen options
    {
        Send(RollDice(Rolls.AsInt(), DiceSize.AsInt()));
    }

    public void PercentageRoll() //Roll a percentage roll
    {
        Send((RollDice(1, 10)[0] * 10).ToString());
    }
}
