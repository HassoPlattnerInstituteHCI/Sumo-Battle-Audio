using UnityEngine;
using System.Threading.Tasks; //note that you need to include this if you want to use Task.Delay.

public class PlayerSoundEffect : MonoBehaviour
{
    public AudioClip powerupClip;
    public AudioClip powerupHitClip;
    public AudioClip hitClip;
    //public AudioClip warningClip;
    public AudioClip gameOverClip;

    public float maxPitch = 1.2f;
    public float minPitch = 0.8f;

    private AudioSource audioSource;

    private GameObject previousEnemy;

    private SpeechOut speechOut;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        speechOut = new SpeechOut();
    }

    /// <summary>
    /// Says enemy name.
    /// </summary>
    /// <param name="clip"></param>
    /// <param name="enemy">(optional) If enemy reference gets passed in</param>
    public void PlayEnemyHitClip(string enemyName, GameObject enemy = null)
    {
        if (enemy)
        {
            if (previousEnemy && enemy.Equals(previousEnemy))
            {
                return;
            }
            previousEnemy = enemy;
        }
        Say(enemyName + " hit me!");
    }

    private async void Say(string input)
    {
        speechOut.Stop();
        await speechOut.Speak(input);
    }

    void OnApplicationQuit()
    {
        speechOut.Stop();
    }

    /// <summary>
    /// Plays the enemy hit clip.
    /// </summary>
    /// <param name="clip"></param>
    /// <param name="enemy">(optional) If enemy reference gets passed in</param>
    public void PlayEnemyHitClip(AudioClip clip, GameObject enemy = null)
    {
        if (enemy)
        {
            if (previousEnemy && enemy.Equals(previousEnemy))
            {
                return;
            }
            previousEnemy = enemy;
        }

        audioSource.Stop();
        audioSource.clip = clip;
        audioSource.Play();
    }

    public void PlayOneShot(AudioClip clip)
    {
        audioSource.PlayOneShot(clip);
    }

    public void PlayHit()
    {
        PlayClipPitched(hitClip, minPitch, maxPitch);
    }

    public void PlayPowerupHit()
    {
        PlayClipPitched(powerupHitClip, minPitch, maxPitch);
    }

    public void PlayPowerupCollect()
    {
        PlayClipPitched(powerupClip, minPitch, maxPitch);
    }

    public float PlayerFellDown()
    {
        audioSource.PlayOneShot(gameOverClip);
        return gameOverClip.length;
    }

    public void PlayClipPitched(AudioClip clip, float minPitch, float maxPitch)
    {
        // little trick to make clip sound less redundant
        audioSource.pitch = Random.Range(minPitch, maxPitch);
        // plays same clip only once, this way no overlapping
        audioSource.PlayOneShot(clip);
        audioSource.pitch = 1f;
    }
}
