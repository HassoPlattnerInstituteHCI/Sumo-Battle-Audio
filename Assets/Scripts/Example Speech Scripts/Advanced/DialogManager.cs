using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

public class DialogManager : MonoBehaviour
{
    SpeechIn speechIn;
    SpeechOut speechOut;
    public List<string> metaCommands = new List<string> { "repeat", "quit", "options" };
    // Start is called before the first frame update
    void Start()
    {
        speechIn = new SpeechIn(onRecognized);
        speechIn.setMetaCommands(metaCommands);
        speechOut = new SpeechOut();

        GenerateDialog().play(speechIn, speechOut);
    }
    DialogNode GenerateDialog() {
        DialogNode start = new DialogNode("Welcome to the DualPanto Corona Diagnosis App... Time to greet me");
        DialogNode howareyou = new DialogNode("How are you?");
        DialogNode healthy = new DialogNode("I am happy to hear that!");
        DialogNode sick = new DialogNode("You should go see a doctor then!", OnTriggerTest);
        DialogNode alright = new DialogNode("hang in there");
        start.addOption(new List<string> { "hey", "hi", "hello" }, howareyou);
        howareyou.addOption("I'm fine", healthy);
        howareyou.addOption("I'm sick", sick);
        howareyou.addOption("so-so", alright);
        return start;
    }
    private Task OnTriggerTest(object v) {
        Debug.Log("managed to trigger a function :)");
        return Task.CompletedTask;
    }
    async void onRecognized(string message)
    {
        // handle defined meta-commands
        switch (message) {
            case "repeat":
                await speechOut.Repeat();
                break;
            case "quit":
                await speechOut.Speak("Thanks for using our application. Closing down now...");
                OnApplicationQuit();
                Application.Quit();
                break;
            case "options":
                string commandlist = "";
                foreach (string command in speechIn.getActiveCommands()) {
                    commandlist += command + ", ";
                }
                await speechOut.Speak("currently available commands: " + commandlist);
                break;
        }
    }

    public void OnApplicationQuit()
    {
        speechIn.StopListening(); // [mac] do not delete this line!
        speechOut.Stop();
    }
}


