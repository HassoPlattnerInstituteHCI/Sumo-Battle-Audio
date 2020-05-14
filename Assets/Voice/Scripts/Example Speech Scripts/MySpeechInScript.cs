using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Windows.Speech;

public class MySpeechInScript : MonoBehaviour
{
    SpeechIn speechIn;
    string[] commands = new string[] { "apple", "banana", "orange" };
    void Start()
    {
        speechIn = new SpeechIn(onRecognized, commands);
        speechIn.Listen(commands);
    }
    void onRecognized(string message)
    {
        Debug.Log("[MyScript]: " + message);
    }
    public void OnApplicationQuit()
    {
        speechIn.StopListening(); // [macOS] do not delete this line!
    }
}
