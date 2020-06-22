using System.Threading.Tasks;
using System.Text.RegularExpressions;
using UnityEngine;
public class MacOSSPeechOut : SpeechBase
{
    System.Diagnostics.Process speechProcess;
    public override void Init(){}
    public override void Stop(){
        if (!speechProcess.HasExited)
            speechProcess.Kill();
    }
    public override async void Speak(string text)
    {
        string cmdArgs;
        string voice = GetLangVoice();
        Debug.Log("[Mac] Speaking: " + text);
        int rate = (int)(speed * 170); //[mac]default say : 170 wpm.
        int outputChannel = 48;
        if (text.ToLower().Contains("dualpanto"))
        { //TODO: make more general list of user defined pronounciations and find how to do this in windows‚
            text = Regex.Replace(text,"dualpanto", "[[emph +]] dual-panto[[emph -]]", RegexOptions.IgnoreCase);
        }
        if (text == "crazylaugh")
        {
            cmdArgs = string.Format("-v Hysterical \"muhahahaha\" ");     //couldnt help myself ;)
        }
        else
        {
            cmdArgs = string.Format("-r {2} -v {0} \"{1}\" ", voice, text.Replace("\"", ","), rate);
        }
        speechProcess =  System.Diagnostics.Process.Start("/usr/bin/say", cmdArgs);
        SpeechBase.isSpeaking = true;
        while (!speechProcess.HasExited)    // now wait until finished speaking
        {
            await Task.Delay(100);
        }
        SpeechBase.isSpeaking = false;
        return;
    }
    private string GetLangVoice(){
        switch(Language){
            case LANGUAGE.DUTCH:
                return "Xander";
            case LANGUAGE.ENGLISH:
                return "Samantha";
            case LANGUAGE.GERMAN:
                return "Anna";
            case LANGUAGE.JAPANESE:
                return "Kyoko"; //京子さん
            default: return "Samantha";
        }
    }
}
