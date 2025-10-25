using UnityEngine;
// Alias para que o c�digo fique limpo, referenciando o Enum p�blico do PlayerCombat
using AbilityType = PlayerCombat.AbilityType;

public class AbilityUnlockItem : MonoBehaviour
{
    [Header("Configura��o de Desbloqueio")]
    [Tooltip("Define qual habilidade ser� desbloqueada ao coletar este item.")]
    // O Enum AbilityType ser� renderizado no Inspector
    public AbilityType abilityToUnlock;
    public AudioClip collectSound;

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Verifica se o objeto que colidiu � o Player
        if (other.CompareTag("Player"))
        {
            PlayerCombat playerCombat = other.GetComponent<PlayerCombat>();

            if (playerCombat != null)
            {
                // O item tenta desbloquear a habilidade
                bool unlockedSuccessfully = playerCombat.UnlockAbility(abilityToUnlock);

                if (unlockedSuccessfully)
                {
                    // Se desbloqueou com sucesso OU se a habilidade j� estava desbloqueada, coleta o item.

                    if (collectSound != null)
                    {
                        AudioSource.PlayClipAtPoint(collectSound, transform.position);
                    }

                    // Destr�i o item APENAS se o desbloqueio foi aceito.
                    Destroy(gameObject);
                }
                else
                {
                    // Se o slot estiver cheio (unlockedSuccessfully � false)
                    Debug.Log($"N�o foi poss�vel pegar o item {abilityToUnlock}. Slots de habilidade cheios.");
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