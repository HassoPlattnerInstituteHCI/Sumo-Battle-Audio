using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class MyRockPaperScissorsScript : MonoBehaviour
{
    SpeechIn speechIn;
    SpeechOut speechOut;
    string[] commands = new string[] { "rock", "paper", "scissors" };
    // Start is called before the first frame update
    void Start()
    {
        speechIn = new SpeechIn(onRecognized);
        speechOut = new SpeechOut();
        Dialog(); //start async process.
    }

    private async void Dialog()
    {
        speechIn.StartListening(); //startup native speech plugin.
        int me, pc;
        int round = 1;
        do
        {
            await Task.Delay(1000);
            await speechOut.Speak("Round " + round + " : Rock, Paper, Scissors, GO!");
            string input = await speechIn.Listen(commands);

            me = System.Array.IndexOf(commands, input);
            pc = Random.Range(0, 3); //[0,3) = {0,1,2} see unity docs.
            await speechOut.Speak("I have " + commands[pc]);

            round++;
        } while (Judge(me, pc) != 1);
        await speechOut.Speak("Congrats! good bye");
        speechIn.StopListening();
    }

    void onRecognized(string message)
    {
    }

    private int Judge(int me, int pc)
    {
        if (me == pc) return 0; //draw
        else if ((me == 0 && pc == 1) || (me == 1 && pc == 2) || (me == 2 && pc == 0)) return 2; //PC win
        else return 1; //me win
    }
    public void OnApplicationQuit()
    {
        speechIn.StopListening(); // [mac] do not delete this line!
        speechOut.Stop(); // [win] do not delete this line!
    }
}
