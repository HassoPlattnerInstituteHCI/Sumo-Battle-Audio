﻿/// Hint: Commenting or uncommenting in VS
/// On Mac: CMD + SHIFT + 7
/// On Windows: CTRL + K and then CTRL + C

using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private Rigidbody playerRb;
    public float maxSpeed = 10f;
    public float speed = 1f;
    public GameObject focalPoint;

    //public bool hasPowerup;
    private PowerupType activePowerup;
    public float powerupStrength = 15f;
    private int powerupTime = 7;
    public GameObject powerupIndicator;

    private PlayerSoundEffect soundEffects;
    private bool playerFellDown = false;

    public LayerMask pointAndClickAffected;
    public float pointAndClickDistance = 20f;

    private Vector3 targetPosition;
    private bool targetSelected;

    //public float explosionPower = 10f;
    //public float explosionRadius = 5f;
    //public float explosionUpwardForce = 2f;
    //public LayerMask explosionAffected;

    private SpeechIn speech;

    private bool movementFrozen = false;

    void Start()
    {
        // looks in the (current) gameObject for component of type Rigidbody
        // gameObject.GetComponent<Rigidbody>() would work as well
        playerRb = GetComponent<Rigidbody>();
        soundEffects = GetComponent<PlayerSoundEffect>();
        //activatePlayer();

        speech = new SpeechIn(onSpeechRecognized);
        speech.StartListening(new string[]{"help", "resume"});
    }

    void OnApplicationQuit()
    {
        speech.StopListening();
    }

    void onSpeechRecognized(string command) {
        Debug.Log("Recognized: " + command);
        if (command == "resume" && movementFrozen) {
            StartCoroutine(ResumeAfterPause());
        } else if (command == "help" && !movementFrozen) {
            toggleMovementFrozen();
            var powerups = GameObject.FindGameObjectsWithTag("Powerup");
            if (powerups.Length > 0) {
                StartCoroutine(GameObject.Find("Panto").GetComponent<LowerHandle>().SwitchTo(powerups[0], 0.2f));
            }
        }
    }

    IEnumerator ResumeAfterPause() {
        var enemy = Enemy.GetClosestObject("Enemy", transform.position);
        if (enemy != null) {
            yield return GameObject.Find("Panto").GetComponent<LowerHandle>().SwitchTo(enemy, 0.2f);
        }
        toggleMovementFrozen();
    }

    public void activatePlayer() {
        UpperHandle upperHandle = GameObject.Find("Panto").GetComponent<UpperHandle>();
        StartCoroutine(upperHandle.SwitchTo(gameObject, 0.2f));
        upperHandle.FreeRotation();
    }

    void toggleMovementFrozen()
    {
        playerRb.constraints = !movementFrozen ? RigidbodyConstraints.FreezeAll : RigidbodyConstraints.None;
        foreach (var enemy in GameObject.FindGameObjectsWithTag("Enemy"))
        {
            enemy.GetComponent<Rigidbody>().constraints = !movementFrozen ? RigidbodyConstraints.FreezeAll : RigidbodyConstraints.None;
        }
        movementFrozen = !movementFrozen;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown("h")) {
            onSpeechRecognized("help");
        }
        if (Input.GetKeyDown("r")) {
            onSpeechRecognized("resume");
        }

        if (!GameObject.FindObjectOfType<SpawnManager>().gameStarted) return;
        powerupIndicator.transform.position = transform.position + new Vector3(0f, -0.5f, 0f);

        if (transform.position.y < -10f && !playerFellDown)
        {
            playerFellDown = true;
            float clipTime = soundEffects.PlayerFellDown();
            Destroy(gameObject, clipTime);
        }

        if (Input.GetMouseButton(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, pointAndClickAffected))
            {
                targetPosition = hit.point;
                targetSelected = true;
                Debug.Log(targetPosition);
            }
        }
    }

    // FixedUpdate is called on a fixed physics loop
    void FixedUpdate()
    {
        if (!GameObject.FindObjectOfType<SpawnManager>().gameStarted) return;
        //PointAndClickMovement();
        PantoMovement();
    }

    void PantoMovement()
    {
        float rotation = GameObject.Find("Panto").GetComponent<UpperHandle>().getRotation();
        // Rotate the forward direction around the Y Axis
        Vector3 direction = Quaternion.Euler(0, rotation, 0) * Vector3.forward;
        playerRb.velocity = speed * direction;
    }

    /// <summary>
    /// Pushes the player to the mouse position.
    /// </summary>
    void PushMovement()
    {
        if (!targetSelected) return;
        Vector3 direction = (targetPosition - transform.position).normalized;
        direction.y = 0;
        playerRb.AddForce(direction * maxSpeed, ForceMode.Acceleration);
    }

    /// <summary>
    /// Moves the player to the mouse position and then stops.
    /// </summary>
    void PointAndClickMovement()
    {
        if (!targetSelected) return;
        float distance = Vector3.Distance(transform.position, targetPosition);
        Vector3 direction = (targetPosition - transform.position).normalized;
        direction.y = 0;
        float moveForce = Mathf.Clamp(speed * distance, 0, maxSpeed);
        playerRb.velocity = moveForce * direction;
    }

    /// <summary>
    /// Moves the player using keyboard input.
    /// </summary>
    void KeyboardMovement()
    {
        // gets the vertical user input from joystick/keyboard (w/s or up/down)
        // as a float between -1 and 1
        float forwardInput = Input.GetAxis("Vertical");
        // gets the normalized forward direction of the focal point aka camera
        // normalized means values of the vector range between -1 and 1
        Vector3 forwardDirection = focalPoint.transform.forward.normalized;
        // adds force to the player in the direction the camera is facing
        playerRb.AddForce(forwardDirection * forwardInput * maxSpeed);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Powerup"))
        {
            Powerup powerup = other.GetComponent<Powerup>();
            activePowerup = powerup.powerupType;
            //hasPowerup = true;
            powerupIndicator.gameObject.SetActive(true);
            soundEffects.PlayPowerupCollect();
            Destroy(other.gameObject);
            ResetCountdown();
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        GameObject other = collision.gameObject;
        /// challenge: when other has tag "Enemy" and we have a powerup
        /// get the enemyRigidbody and push the enemy away from the player
        if (other.CompareTag("Enemy"))
        {
            Enemy enemy = other.GetComponent<Enemy>();

            soundEffects.PlayEnemyHitClip(enemy);

            if (HasPowerup())
            {
                soundEffects.PlayPowerupHit();
                Rigidbody enemyRigidbody = other.GetComponent<Rigidbody>();
                Vector3 awayFromPlayer = other.transform.position - transform.position;
                enemyRigidbody.AddForce(awayFromPlayer.normalized * powerupStrength, ForceMode.Impulse);
            } else
            { 
                soundEffects.PlayHit();
            }
        }
    }

    private void PowerupCountdown()
    {
        //hasPowerup = false;
        ResetPowerupType();
        powerupIndicator.gameObject.SetActive(false);
    }

    private void ResetCountdown()
    {
        CancelInvoke("PowerupCountdown"); // if we previously picked up an powerup
        Invoke("PowerupCountdown", powerupTime);
    }

    private bool HasPowerup()
    {
        return activePowerup != PowerupType.None;
    }

    private void ResetPowerupType()
    {
        activePowerup = PowerupType.None;
    }

    //public void ExplosionPowerup()
    //{
    //    //if (activePowerup != PowerupType.OneTime) return;

    //    Vector3 explosionPos = transform.position;
    //    Collider[] colliders = Physics.OverlapSphere(explosionPos, explosionRadius, explosionAffected);
    //    foreach (Collider hit in colliders)
    //    {
    //        Rigidbody rb = hit.GetComponent<Rigidbody>();
    //        if (rb != null)
    //            rb.AddExplosionForce(explosionPower, explosionPos, explosionRadius, explosionUpwardForce);
    //    }

    //    ResetPowerupType();
    //}
}
