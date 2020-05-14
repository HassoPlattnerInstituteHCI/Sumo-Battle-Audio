using System.Threading.Tasks;
    public class MacOSSPeechOut : SpeechBase
    {
        public override void Init(){}
        public override void Stop(){}
        public override async void Speak(string text)
        {
            string cmdArgs;
            string voice = "Samantha";
            int outputChannel = 48;
            if (text == "crazylaugh")
            {
                cmdArgs = string.Format("-a {0} -v Hysterical \"muhahahaha\" ", outputChannel);     //couldnt help myself ;)
            }
            else
            {
                cmdArgs = string.Format("-a {2} -v {0} \"{1}\" ", voice, text.Replace("\"", ","), outputChannel);
            }
            System.Diagnostics.Process speechProcess =  System.Diagnostics.Process.Start("/usr/bin/say", cmdArgs);
            SpeechBase.isSpeaking = true;
            while (!speechProcess.HasExited)    // now wait until finished speaking
            {
                await Task.Delay(100);
            }
            SpeechBase.isSpeaking = false;
            return; 
        }

}