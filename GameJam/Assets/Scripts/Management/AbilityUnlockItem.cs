using UnityEngine;
// Alias para que o código fique limpo, referenciando o Enum público do PlayerCombat
using AbilityType = PlayerCombat.AbilityType;

public class AbilityUnlockItem : MonoBehaviour
{
    [Header("Configuração de Desbloqueio")]
    [Tooltip("Define qual habilidade será desbloqueada ao coletar este item.")]
    // O Enum AbilityType será renderizado no Inspector
    public AbilityType abilityToUnlock;
    public AudioClip collectSound;

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Verifica se o objeto que colidiu é o Player
        if (other.CompareTag("Player"))
        {
            PlayerCombat playerCombat = other.GetComponent<PlayerCombat>();

            if (playerCombat != null)
            {
                // O item tenta desbloquear a habilidade
                bool unlockedSuccessfully = playerCombat.UnlockAbility(abilityToUnlock);

                if (unlockedSuccessfully)
                {
                    // Se desbloqueou com sucesso OU se a habilidade já estava desbloqueada, coleta o item.

                    if (collectSound != null)
                    {
                        AudioSource.PlayClipAtPoint(collectSound, transform.position);
                    }

                    // Destrói o item APENAS se o desbloqueio foi aceito.
                    Destroy(gameObject);
                }
                else
                {
                    // Se o slot estiver cheio (unlockedSuccessfully é false)
                    Debug.Log($"Não foi possível pegar o item {abilityToUnlock}. Slots de habilidade cheios.");
                    // O item permanece no mapa.
                }
            }
            else
            {
                Debug.LogWarning("Player tag encontrado, mas sem componente PlayerCombat. O item de desbloqueio falhou.");
            }
        }
    }
}