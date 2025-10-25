using UnityEngine;
using UnityEngine.EventSystems;

public class CursorManager : MonoBehaviour
{
    void Start()
    {
        ShowCursor();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ShowCursor();
        }

        if (Input.GetKeyDown(KeyCode.Mouse0) && Cursor.lockState == CursorLockMode.None)
        {
            if (EventSystem.current.IsPointerOverGameObject())
            {
                Debug.Log("Clique na UI, cursor mantido.");
                return;
            }

            HideCursor();
        }
    }

    void HideCursor()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        Debug.Log("Cursor escondido e bloqueado.");
    }

    void ShowCursor()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        Debug.Log("Cursor visível e liberado.");
    }
}