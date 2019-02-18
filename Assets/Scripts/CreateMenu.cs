using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CreateMenu : MonoBehaviour {

    [System.Serializable]
    public class ButtonOption
    {
        public string name;
        public int id;
    }

    public GameObject ButtonTemplate;
    RectTransform t;
    public Color OddColour;
    public Color EvenColor;

    public ButtonOption[] Options;

	void Start () //Create list of buttons to add input areas
    {
        t = ButtonTemplate.transform.parent.GetComponent<RectTransform>();
        float gapheight = 45f;
		for(int i = 0; i < Options.Length; i++)
        {
            t.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, t.rect.height + gapheight);
            GameObject newButton = Instantiate(ButtonTemplate, ButtonTemplate.transform.parent);
            newButton.SetActive(true);
            RectTransform newRectTransform = newButton.GetComponent<RectTransform>();
            Vector3 pos = newRectTransform.anchoredPosition;
            pos.y = t.localPosition.y - (i * gapheight) - 5;
            newRectTransform.anchoredPosition = pos;

            SheetCreator.CreateButton b = newButton.GetComponent<SheetCreator.CreateButton>();
            b.id = Options[i].id;
            newButton.transform.Find("Text").GetComponent<Text>().text = Options[i].name;

            ColorBlock C = newButton.GetComponent<Button>().colors;
            if (i % 2 == 0)
            {
                C.normalColor = EvenColor;
            }
            else
            {
                C.normalColor = OddColour;
            }
            newButton.GetComponent<Button>().colors = C; //Set color depending if even or odd in list
        }
	}
}
