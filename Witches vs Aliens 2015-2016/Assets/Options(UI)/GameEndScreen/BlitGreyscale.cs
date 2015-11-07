﻿using UnityEngine;
using System.Collections;

public class BlitGreyscale : MonoBehaviour {

    float intensity = 0;
    float value = 1;
    public float time;
    Material material;

    static int valueShader = Shader.PropertyToID("_Value");

    // Creates a private material used to the effect
    void Awake()
    {
        material = new Material(Shader.Find("Unlit/Greyscale"));
        Callback.DoLerpRealtime((float l) => intensity = l, time/3, this)
            .FollowedBy(() => Callback.DoLerpRealtime((float l) => value = l, 2*time/3, this, reverse: true), this);
    }

    // Postprocess the image
    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (intensity == 0)
        {
            Graphics.Blit(source, destination);
            return;
        }
        material.SetFloat(Tags.ShaderParams.cutoff, intensity);
        material.SetFloat(valueShader, value);
        Graphics.Blit(source, destination, material);
    }
}