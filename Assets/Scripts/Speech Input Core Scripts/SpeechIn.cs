//==========================================
// Title:  SpeechIn.cs
// Author: Jotaro Shigeyama, Corinna Jascheck and Thijs Roumen (firstname.lastname [at] hpi.de)
// Date:   2020.04.20
//==========================================

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class SpeechIn
{
    VoiceCommandBase recognizer;
    List<string> metaCommands = new List<string> { "repeat", "quit", "options" };
    string[] activeCommands;
    public SpeechIn(VoiceCommandBase.OnRecognized onRecognized) {
        if (Application.platform == RuntimePlatform.OSXEditor ||
           Application.platform == RuntimePlatform.OSXPlayer) {
            recognizer = new MacOSSpeechIn(onRecognized);
        }
        else if (Application.platform == RuntimePlatform.WindowsEditor ||
                Application.platform == RuntimePlatform.WindowsPlayer) {
            recognizer = new WindowsSpeechIn(onRecognized);
        }
    }
    public SpeechIn(VoiceCommandBase.OnRecognized onRecognized, string[] commands) {
        if (Application.platform == RuntimePlatform.OSXEditor ||
           Application.platform == RuntimePlatform.OSXPlayer) {
            recognizer = new MacOSSpeechIn(onRecognized, commands);
        }
        else if (Application.platform == RuntimePlatform.WindowsEditor ||
                Application.platform == RuntimePlatform.WindowsPlayer) {
            recognizer = new WindowsSpeechIn(onRecognized, commands);
        }
    }
    public List<string> getMetaCommands() {
        return metaCommands;
    }
    public string[] getActiveCommands() {
        return activeCommands;
    }
    public void setMetaCommands(List<string> commands) {
        metaCommands = commands;
    }
    public string[] appendMetaCommands(string[] commands){
        return commands.Concat(metaCommands.ToArray()).ToArray();
    }
    public void StartListening(){
        recognizer.StartListening();
    }
    public void StartListening(string[] commands){
        recognizer.StartListening(appendMetaCommands(commands));
    }
    public async Task<string> Listen(string[] commands){
        activeCommands = commands; 
        recognizer.StartListening(appendMetaCommands(commands));
        while(!VoiceCommandBase.isRecognized()){
            await Task.Delay(100);
        }
        recognizer.PauseListening();
        VoiceCommandBase.setRecognized(false);
        return VoiceCommandBase.recognized;
    }
    public void PauseListening(){
        recognizer.PauseListening();
    }
    public void StopListening(){
        recognizer.StopListening();
    }
}