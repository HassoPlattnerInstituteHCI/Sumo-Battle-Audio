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

    public async void Dialog(){
        await speechOut.Speak("Welcome to the DualPanto Coronavirus App");
        await speechOut.Speak("How do you feel?");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnApplicationQuit(){
        speechOut.Stop(); //Windows: do not remove this line.
    }

}