using UnityEngine;

public class EnemyAttackTrigger : MonoBehaviour
{
    private EnemyAttack parentAttackScript;

    void Start()
    {
        parentAttackScript = GetComponentInParent<EnemyAttack>();
        if (parentAttackScript == null)
        {
            Debug.LogError($"[EnemyAttackTrigger] em '{gameObject.name}' N�O ENCONTROU o script 'EnemyAttack' no seu 'pai'. Verifique se 'EnemyAttack.cs' est� no objeto pai. OS ATAQUES DO INIMIGO N�O V�O FUNCIONAR.");
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
            Debug.LogError($"[EnemyAttackTrigger] Tentou 'OnTriggerExit2D' mas 'parentAttackScript' � NULL. O Inimigo n�o pode atacar.");
        }
    }
}