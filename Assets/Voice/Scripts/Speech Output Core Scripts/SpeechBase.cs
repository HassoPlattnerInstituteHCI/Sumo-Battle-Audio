
public abstract class SpeechBase
{
    public static bool isSpeaking = false;
    public abstract void Init();
    public abstract void Speak(string text);
    public abstract void Stop();
}
