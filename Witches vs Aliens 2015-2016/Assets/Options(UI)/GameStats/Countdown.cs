﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Countdown : MonoBehaviour, ISpawnable {

    [SerializeField]
    protected Color startTextColor;
    [SerializeField]
    protected Color endTextColor;
    [SerializeField]
    protected Color startOutlineColor;
    [SerializeField]
    protected Color endOutlineColor;
    [SerializeField]
    protected float duration = 1;

    protected Text text;
    protected Outline outline;
    public string count { set { text.text = value; } }
	// Use this for initialization
	protected virtual void Awake () {
        text = GetComponentInChildren<Text>();
        outline = GetComponentInChildren<Outline>();
	}

    public void Create()
    {
        Callback.DoLerpRealtime(lerpIn, 0.5f, this);

        Callback.FireAndForgetRealtime(Despawn, duration, this);
    }

    protected virtual void lerpIn(float l)
    {
        text.color = Color.Lerp(startTextColor, endTextColor, l);
        outline.effectColor = Color.Lerp(startOutlineColor, endOutlineColor, l);
    }

    protected virtual void Despawn()
    {
        SimplePool.Despawn(this.gameObject);
    }
}
