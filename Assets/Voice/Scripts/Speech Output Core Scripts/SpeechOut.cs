using UnityEngine;
using System.Threading.Tasks;

public class SpeechOut
{
    SpeechBase speech;
    System.Diagnostics.Process speechProcess;

    // Use this for initialization
    public SpeechOut()
    {
        if (Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.OSXPlayer)
        {
            speech = new MacOSSPeechOut();
        }
        else if (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.WindowsPlayer)
        {
            speech = new WindowsSpeechOut();
        }
        Init();
    }
    public void Init(){
        speech.Init();
    }
    public void Stop(){
        speech.Stop();
    }
    public async Task Speak(string text)
    {
        speech.Speak(text); 

        while (SpeechBase.isSpeaking)    // now wait until finished speaking
        {
            await Task.Delay(100);
        }
        return;
    }

}
