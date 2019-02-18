using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QuickFloat : MonoBehaviour {

    public Text Display;
    public InputField InputDisplay;

    protected float _Value;
    public float Value
    {
        get
        {
            return _Value;
        }
        set
        {
            float T = (float) Math.Round(value, DecimalLimit);
            if (T < LimitsLowHigh.x)
                T = LimitsLowHigh.x;
            if (T > LimitsLowHigh.y)
                T = LimitsLowHigh.y;
            _Value = T;
        }
    }
    [Range(0,10)]
    public int DecimalLimit;

    public Vector2 LimitsLowHigh;

    void Update()
    {
        if(LimitsLowHigh.y < LimitsLowHigh.x)
        {
            Debug.LogError("LimitsLowHigh Y Value is less than X value. Y represents upper bound.");
        }
    }

    void Awake()
    {
        Value = Value;
    }

	void LateUpdate () {
		if(Display != null)
        {
            Display.text = Value.ToString();
        }
        if (InputDisplay != null)
        {
            if (!InputDisplay.isFocused)
            {
                InputDisplay.text = Value.ToString();
            }
        }
	}
    public void Add(float i)
    {
        Value += i;
    }
    public void Set(float i)
    {
        Value = i;
    }
    public void SetFloat(string TO)
    {
        Set(float.Parse(TO));
    }

    public int AsInt()
    {
        return Mathf.FloorToInt(Value);
    }
}
