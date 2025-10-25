using System.Collections;
using UnityEngine;

public class PlayerDebuffs : MonoBehaviour
{
    public bool powersDisabled = false;
    public bool controlsInverted = false;

    private PlayerMovement playerMovement;
    private PlayerCombat playerCombat;

    void Start()
    {
        playerMovement = GetComponent<PlayerMovement>();
        playerCombat = GetComponent<PlayerCombat>();
    }

    public void ApplyDisablePowers(float duration)
    {
        StartCoroutine(DisablePowersRoutine(duration));
    }

    public void ApplyInvertControls(float duration)
    {
        StartCoroutine(InvertControlsRoutine(duration));
    }

    private IEnumerator DisablePowersRoutine(float duration)
    {
        powersDisabled = true;
        playerCombat.enabled = false;
        Debug.Log("PODERES DESABILITADOS!");

        yield return new WaitForSeconds(duration);

        powersDisabled = false;
        playerCombat.enabled = true;
        Debug.Log("Poderes reativados.");
    }

    private IEnumerator InvertControlsRoutine(float duration)
    {
        controlsInverted = true;
        playerMovement.invertControls = true;
        Debug.Log("CONTROLES INVERTIDOS!");

        yield return new WaitForSeconds(duration);

        controlsInverted = false;
        playerMovement.invertControls = false;
        Debug.Log("Controles normalizados.");
    }
}