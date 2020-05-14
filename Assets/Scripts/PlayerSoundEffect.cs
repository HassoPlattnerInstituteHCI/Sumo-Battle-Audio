using UnityEngine;

public class PlayerSoundEffect : MonoBehaviour
{
    public AudioClip powerupClip;
    public AudioClip powerupHitClip;
    public AudioClip hitClip;
    public AudioClip warningClip;
    public AudioClip gameOverClip;

    public float maxPitch = 1.2f;
    public float minPitch = 0.8f;

    private AudioSource audioSource;

    private GameObject previousEnemy;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
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
