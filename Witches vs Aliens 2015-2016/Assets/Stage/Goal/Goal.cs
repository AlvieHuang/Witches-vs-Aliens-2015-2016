﻿using UnityEngine;
using System.Collections;

public class Goal : MonoBehaviour {
    [SerializeField]
    protected Side mySide;

    ParticleSystem vfx;
    void Start()
    {
        vfx = GetComponent<ParticleSystem>();
    }
	
	// Update is called once per frame
	void OnCollisionEnter2D (Collision2D other) {
        if (!other.collider.CompareTag(Tags.puck))
            return;
        Debug.Log("GOOOOOOOOOOOOOOOOOOOOOOAL!");
        other.gameObject.GetComponent<PuckFX>().Hide();
        vfx.Play();
        ScreenShake.RandomShake(this, 0.1f, 0.25f);
        Observers.Post(new GoalScoredMessage(mySide));
	}
}

public enum Side
{
    LEFT,
    RIGHT
}

public class GoalScoredMessage : Message
{
    public readonly Side side;
    public const string classMessageType = "GoalScoredMessage";
    public GoalScoredMessage(Side side) : base(classMessageType)
    {
        this.side = side;
    }
}

