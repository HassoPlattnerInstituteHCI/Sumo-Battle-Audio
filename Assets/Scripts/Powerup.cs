using UnityEngine;

public enum PowerupType
{
    None,
    OneTime,
    Duration
}

public class Powerup : MonoBehaviour
{
    public PowerupType powerupType;
    public float duration;
}
