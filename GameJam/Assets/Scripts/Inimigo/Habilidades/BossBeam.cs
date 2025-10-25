using UnityEngine;

public class BossBeam : MonoBehaviour
{
    public float duration = 2f;
    public float debuffDuration = 5f;

    void Start()
    {
        Destroy(gameObject, duration);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerDebuffs playerDebuffs = other.GetComponent<PlayerDebuffs>();
            if (playerDebuffs != null && !playerDebuffs.powersDisabled)
            {
                Debug.Log("Feixe acertou o Player! Desabilitando poderes.");
                playerDebuffs.ApplyDisablePowers(debuffDuration);
            }
        }
    }
}