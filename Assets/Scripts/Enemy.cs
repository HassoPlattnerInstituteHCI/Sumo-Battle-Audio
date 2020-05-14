using UnityEngine;

public class Enemy : MonoBehaviour
{
    public float speed = 8f;
    private Rigidbody enemyRb;
    private GameObject player;
    // clip already assigned in AudioSource
    private AudioSource audioSource;
    [HideInInspector]
    public AudioClip nameClip;
    [HideInInspector]
    public string enemyName = "Enemy";

    void Start()
    {
        enemyRb = GetComponent<Rigidbody>();
        player = GameObject.Find("Player");
        audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        // if the game object falls below -10 on the y-axis we gonna remove it
        // from the game
        if (transform.position.y < -10)
        {
            Destroy(gameObject);
        }
    }

    void FixedUpdate()
    {
        // if no player is found
        if (!player) return;
        Vector3 lookDirection = player.transform.position - transform.position;
        enemyRb.AddForce(lookDirection.normalized * speed);
    }

    private void OnCollisionEnter(Collision collision)
    {
        audioSource.pitch = Random.Range(0.8f, 1.2f);
        audioSource.Play();
    }
}
