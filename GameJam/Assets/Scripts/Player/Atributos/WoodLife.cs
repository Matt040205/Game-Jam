using UnityEngine;

public class WoodLife : MonoBehaviour
{
    [Header("Heal Settings")]
    public int healAmount = 25;

    [Header("Audio")]
    public AudioClip collectSound;

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Verifica se o objeto que colidiu � o Player
        if (other.CompareTag("Player"))
        {
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();

            if (playerHealth != null)
            {
                // Cura o jogador
                playerHealth.Heal(healAmount);

                // Toca o som de coleta (se houver)
                if (collectSound != null)
                {
                    // Usa AudioSource.PlayClipAtPoint para que o som toque mesmo ap�s a destrui��o do objeto
                    AudioSource.PlayClipAtPoint(collectSound, transform.position);
                }

                Debug.Log($"Item WoodLife coletado. Curou {healAmount} de vida.");

                // Destr�i o item
                Destroy(gameObject);
            }
            else
            {
                Debug.LogWarning("Player tag encontrado, mas sem componente PlayerHealth.");
            }
        }
        // Opcional: Voc� pode adicionar outras tags (como "Wall") para casos onde o item deve se destruir
    }
}