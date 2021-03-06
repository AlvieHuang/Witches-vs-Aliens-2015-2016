﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
[RequireComponent(typeof(Collider2D))]
public class MeteorCrater : MonoBehaviour, ISpawnable {
    [SerializeField]
    protected float duration;

    [SerializeField]
    protected float speedNerf;

    [SerializeField]
    [AutoLink(childPath = "Glow")]
    protected SpriteRenderer glowSprite;

    SpriteRenderer rend;
    Material glowMat;
    ParticleSystem vfx;
    Collider2D coll;
    Side _side;
    public Side side {
        set { _side = value; }
    }

    Dictionary<InputToAction, FloatStat> modifiers = new Dictionary<InputToAction, FloatStat>();

    void Awake()
    {
        rend = transform.Find("Crater").GetComponent<SpriteRenderer>();
        vfx = GetComponent<ParticleSystem>();
        coll = GetComponent<Collider2D>();
        glowMat = Instantiate(glowSprite.material);
        glowSprite.material = glowMat;
    }

	// Use this for initialization
	public void Create () { //called on spawn
        Callback.FireAndForget(despawn, duration, this);
        vfx.Play();
        Callback.DoLerp((float l) => rend.color = rend.color.setAlphaFloat(l), 0.5f, this);
        Callback.DoLerp((float l) => glowMat.SetFloat(Tags.ShaderParams.cutoff, 1 / (20 * l + 1)), duration, this);
        ScreenShake.RandomShake(this, 0.05f, 0.1f);
        //vfx stuff
	}
	
	// Update is called once per frame
	void OnTriggerEnter2D (Collider2D other) {
        if(other.CompareTag(Tags.player))
        {
            if (other.GetComponentInParent<Stats>().side != _side)
            {
                InputToAction otherController = other.GetComponentInParent<InputToAction>();
                if (!modifiers.ContainsKey(otherController))
                {
                    modifiers[otherController] = otherController.maxSpeedTracker.addModifier(speedNerf);
                }
            }
        }
	}

    void OnTriggerExit2D(Collider2D other)
    {
        if(other.CompareTag(Tags.player))
        {
            InputToAction otherController = other.GetComponentInParent<InputToAction>();
            if (modifiers.ContainsKey(otherController))
            {
                otherController.maxSpeedTracker.removeModifier(modifiers[otherController]);
                modifiers.Remove(otherController);
            }
        }
    }

    void despawn()
    {
        foreach (var element in modifiers)
        {
            element.Key.maxSpeedTracker.removeModifier(element.Value);
        }
        coll.enabled = false;
        float startingGlowCutoff = glowMat.GetFloat(Tags.ShaderParams.cutoff);
        Callback.DoLerp((float l) => { rend.color = rend.color.setAlphaFloat(l); glowMat.SetFloat(Tags.ShaderParams.cutoff, l * startingGlowCutoff); }, 1f, this, reverse: true).FollowedBy(() => { coll.enabled = true; SimplePool.Despawn(this.gameObject); }, this);
    }
}
