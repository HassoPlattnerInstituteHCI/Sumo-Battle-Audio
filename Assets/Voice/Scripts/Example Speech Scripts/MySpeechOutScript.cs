using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks; //note that you need to include this if you want to use Task.Delay.

public class MySpeechOutScript : MonoBehaviour
{
    // Start is called before the first frame update
    SpeechOut speechOut;
    void Start()
    {
        speechOut = new SpeechOut();
        Dialog();
    }

    async void Dialog()
    {
        await speechOut.Speak("Muhahaha");
        await speechOut.Speak("Do you hear me?");
        await speechOut.Speak("I hope you really enjoy the class this year as well. Take care.");
    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnApplicationQuit()
    {
        speechOut.Stop(); //Windows: do not remove this line.
    }

}