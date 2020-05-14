//==========================================
// Title:  SpeechIn.cs
// Author: Jotaro Shigeyama (jotaro.shigeyama [at] hpi.de)
// Date:   2020.04.20
//==========================================

using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class SpeechIn
{
    VoiceCommandBase recognizer;
    public SpeechIn(VoiceCommandBase.OnRecognized onRecognized)
    {
        if (Application.platform == RuntimePlatform.OSXEditor ||
           Application.platform == RuntimePlatform.OSXPlayer)
        {
            recognizer = new MacOSSpeechIn(onRecognized);
        }
        else if (Application.platform == RuntimePlatform.WindowsEditor ||
                Application.platform == RuntimePlatform.WindowsPlayer)
        {
            recognizer = new WindowsSpeechIn(onRecognized);
        }
    }
    public SpeechIn(VoiceCommandBase.OnRecognized onRecognized, string[] commands)
    {
        if (Application.platform == RuntimePlatform.OSXEditor ||
           Application.platform == RuntimePlatform.OSXPlayer)
        {
            recognizer = new MacOSSpeechIn(onRecognized, commands);
        }
        else if (Application.platform == RuntimePlatform.WindowsEditor ||
                Application.platform == RuntimePlatform.WindowsPlayer)
        {
            recognizer = new WindowsSpeechIn(onRecognized, commands);
        }
    }
    public void StartListening()
    {
        recognizer.StartListening();
    }
    public void StartListening(string[] commands)
    {
        recognizer.StartListening(commands);
    }
    public async Task<string> Listen(string[] commands)
    {
        recognizer.StartListening(commands);
        while (!VoiceCommandBase.isRecognized())
        {
            await Task.Delay(100);
        }
        recognizer.PauseListening();
        VoiceCommandBase.setRecognized(false);
        return VoiceCommandBase.recognized;
    }
    public void PauseListening()
    {
        recognizer.PauseListening();
    }
    public void StopListening()
    {
        recognizer.StopListening();
    }
}