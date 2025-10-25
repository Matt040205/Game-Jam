using UnityEngine;

public class EnemyAttackTrigger : MonoBehaviour
{
    private EnemyAttack parentAttackScript;

    void Start()
    {
        parentAttackScript = GetComponentInParent<EnemyAttack>();
        if (parentAttackScript == null)
        {
            Debug.LogError($"[EnemyAttackTrigger] em '{gameObject.name}' NÃO ENCONTROU o script 'EnemyAttack' no seu 'pai'. Verifique se 'EnemyAttack.cs' está no objeto pai. OS ATAQUES DO INIMIGO NÃO VÃO FUNCIONAR.");
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (parentAttackScript != null)
        {
            parentAttackScript.OnChildTriggerEnter(other);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (parentAttackScript != null)
        {
            parentAttackScript.OnChildTriggerExit(other);
        }
        else
        {
            Debug.LogError($"[EnemyAttackTrigger] Tentou 'OnTriggerExit2D' mas 'parentAttackScript' é NULL. O Inimigo não pode atacar.");
        }
    }
}