using UnityEngine;
using System;

public class Timer
{
    private float duration;
    private float elapsed;
    private bool isRunning;
    private Action onComplete;

    public float Progress => Mathf.Clamp01(elapsed / duration);
    public bool IsFinished => elapsed >= duration;
    public bool IsRunning => isRunning;

    public Timer(float duration, Action onComplete = null)
    {
        this.duration = duration;
        this.onComplete = onComplete;
        this.elapsed = 0f;
        this.isRunning = false;
    }

    public void Start()
    {
        isRunning = true;
        elapsed = 0f;
    }

    public void Tick(float deltaTime)
    {
        if (!isRunning) return;

        elapsed += deltaTime;
        if (elapsed >= duration)
        {
            isRunning = false;
            onComplete?.Invoke();
        }
    }

    public void Reset()
    {
        elapsed = 0f;
        isRunning = false;
    }
}