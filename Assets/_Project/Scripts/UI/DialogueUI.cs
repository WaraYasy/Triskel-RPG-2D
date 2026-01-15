using UnityEngine;

/// <summary>
/// UI SUPER SIMPLE de diálogos - Solo usa la consola y teclas
/// Presiona ESPACIO para continuar, o 1/2/3/4 para elegir opciones
/// </summary>
public class DialogueUI : MonoBehaviour
{
    private DialogueManager dialogueManager;

    private void Start()
    {
        dialogueManager = FindFirstObjectByType<DialogueManager>();
    }

    private void Update()
    {
        if (!dialogueManager.IsDialogueActive)
            return;

        // Presionar ESPACIO para continuar
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (dialogueManager.CurrentStory.currentChoices.Count == 0)
            {
                dialogueManager.ContinueStory();
            }
        }

        // Presionar 1, 2, 3, 4 para elegir opciones
        if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1))
            dialogueManager.MakeChoice(0);
        if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2))
            dialogueManager.MakeChoice(1);
        if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3))
            dialogueManager.MakeChoice(2);
        if (Input.GetKeyDown(KeyCode.Alpha4) || Input.GetKeyDown(KeyCode.Keypad4))
            dialogueManager.MakeChoice(3);
    }
}
