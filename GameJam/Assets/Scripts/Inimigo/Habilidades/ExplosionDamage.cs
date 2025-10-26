using UnityEngine;
using FMODUnity; // <- Adicionado

public class ExplosionDamage : MonoBehaviour
{
    public int damage = 25;
    public float explosionDuration = 0.2f;

    [Header("FMOD Events")]
    [EventRef] public string explosionSoundEvent; // <- Adicionado

    void Start()
    {
        // --- FMOD ---
        if (!string.IsNullOrEmpty(explosionSoundEvent)) RuntimeManager.PlayOneShot(explosionSoundEvent, transform.position);
        // --- FIM FMOD ---
        Destroy(gameObject, explosionDuration);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            GlobalDamageEvents.FirePlayerDamage(other.gameObject, damage, transform.position);
        }
    }
}