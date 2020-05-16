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
        if (transform.position.y < -2)
        {
            GameObject closestEnemy = GetClosestObject("Enemy", player.transform.position);
            StartCoroutine(GameObject.Find("Panto")
                    .GetComponent<LowerHandle>()
                    .SwitchTo(closestEnemy, 0.2f));
            Destroy(gameObject);
        }
    }

    GameObject GetClosestObject(string tag, Vector3 position)
    {
        GameObject[] gos = GameObject.FindGameObjectsWithTag(tag);
        GameObject closest = null;
        float distance = Mathf.Infinity;
        foreach (GameObject go in gos)
        {
            Vector3 diff = go.transform.position - position;
            float curDistance = diff.sqrMagnitude;
            if (curDistance < distance)
            {
                closest = go;
                distance = curDistance;
            }
        }
        return closest;
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
