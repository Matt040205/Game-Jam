using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Classe est�tica para desacoplar a l�gica de dano.
/// Componentes de dano disparam esses eventos.
/// Componentes de sa�de escutam esses eventos.
/// </summary>
public static class GlobalDamageEvents
{
    // Evento para quando o Player deve receber dano.
    // Par�metros: GameObject (alvo), Dano, Posi��o da Fonte de Dano
    public static event UnityAction<GameObject, int, Vector2> OnPlayerTakeDamage;

    // Evento para quando um Inimigo (ou Boss) deve receber dano.
    // Par�metros: GameObject (alvo), Dano
    public static event UnityAction<GameObject, int> OnEnemyTakeDamage;

    public static void FirePlayerDamage(GameObject player, int damage, Vector2 damageSourcePosition)
    {
        OnPlayerTakeDamage?.Invoke(player, damage, damageSourcePosition);
    }

    public static void FireEnemyDamage(GameObject enemy, int damage)
    {
        OnEnemyTakeDamage?.Invoke(enemy, damage);
    }
}