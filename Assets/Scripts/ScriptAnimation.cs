using System;
using UnityEngine;

public class ScriptAnimation
{
    private float startTime;
    private float duration;
    private bool looped;

    public bool isPlaying { get; private set; }
    public bool hasEnded { get; private set; }

    public event Action<float> OnStep;
    public event Action OnEnd;

    public ScriptAnimation(float duration, bool looped = false)
    {
        this.duration = duration;
        this.looped = looped;

        isPlaying = false;
    }

    public float GetAlpha()
    {
        return Mathf.InverseLerp(startTime, startTime + duration, Time.time); 
    }

    public void Play()
    {
        startTime = Time.time;
        isPlaying = true;
    }

    private void End()
    {
        isPlaying = false;
        if (looped)
            Play();
        else 
        {  
            hasEnded = true;
            OnEnd?.Invoke();
        }
    }

    public void Step()
    {
        if (!isPlaying) return;

        float alpha = GetAlpha();
        OnStep.Invoke(alpha);

        if (alpha >= 1)
        {
            End();
            return;
        }
    }
}