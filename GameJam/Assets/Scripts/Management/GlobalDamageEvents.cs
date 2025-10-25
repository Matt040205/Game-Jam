using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Classe estática para desacoplar a lógica de dano.
/// Componentes de dano disparam esses eventos.
/// Componentes de saúde escutam esses eventos.
/// </summary>
public static class GlobalDamageEvents
{
    // Evento para quando o Player deve receber dano.
    // Parâmetros: GameObject (alvo), Dano, Posição da Fonte de Dano
    public static event UnityAction<GameObject, int, Vector2> OnPlayerTakeDamage;

    // Evento para quando um Inimigo (ou Boss) deve receber dano.
    // Parâmetros: GameObject (alvo), Dano
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