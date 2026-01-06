using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

public class AnimationEvent
{
    public Action EventFire;
    float _triggerInSec = 0;
    float _secLeft = 0;
    bool _running = false;

    public AnimationEvent() { }
    public AnimationEvent(float triggerInFrames)
    {
        _triggerInSec = triggerInFrames;
        _secLeft = _triggerInSec;
    }
    public void ManualProcess(double delta)
    {
        if (!_running) { return; }

        _secLeft -= (float)delta;
        if (_secLeft <= 0)
        {
            EventFire?.Invoke();
            _running = false;
        }
    }
    public void Restart()
    {
        _secLeft = _triggerInSec;
        _running = true;
    }
}
