using Ink.Runtime;
using UnityEngine;

/// <summary>
/// Vincula funciones externas de Unity con Ink.
/// Permite que los scripts de Ink (.ink) llamen funciones de Unity como modificar moral,
/// desbloquear entradas del diario, etc.
///
/// Ejemplo de uso en Ink:
/// ~ ModifyMoral(1)
/// ~ UnlockDiaryEntry("entry_001")
///
/// Basado en el sistema de quest-system de shapedbyrainstudios.
/// </summary>
public class InkExternalFunctions
{
    /// <summary>
    /// Vincula todas las funciones externas a la historia de Ink.
    /// Llama a esto durante la inicialización del DialogueManager.
    /// </summary>
    /// <param name="story">La historia de Ink a la cual vincular las funciones</param>
    public void Bind(Story story)
    {
        // Vincular función para modificar moral
        story.BindExternalFunction("ModifyMoral", (int delta) => ModifyMoral(delta));

        // Vincular función para desbloquear entradas del diario
        story.BindExternalFunction("UnlockDiaryEntry", (string entryId) => UnlockDiaryEntry(entryId));

        // Vincular función para añadir items al inventario
        story.BindExternalFunction("AddItem", (string itemName) => AddItem(itemName));

        // Vincular función para mostrar logs en consola (útil para debug)
        story.BindExternalFunction("Log", (string message) => Log(message));

        Debug.Log("[InkExternalFunctions] Funciones externas vinculadas correctamente");
    }

    /// <summary>
    /// Desvincula todas las funciones externas.
    /// Llama a esto al destruir el DialogueManager.
    /// </summary>
    /// <param name="story">La historia de Ink desde la cual desvincular las funciones</param>
    public void Unbind(Story story)
    {
        story.UnbindExternalFunction("ModifyMoral");
        story.UnbindExternalFunction("UnlockDiaryEntry");
        story.UnbindExternalFunction("AddItem");
        story.UnbindExternalFunction("Log");

        Debug.Log("[InkExternalFunctions] Funciones externas desvinculadas");
    }

    // ===== FUNCIONES EXTERNAS =====
    // Estas son las funciones que Ink puede llamar

    /// <summary>
    /// Modifica la moral del jugador.
    /// Ejemplo en Ink: ~ ModifyMoral(1)  // Incrementa moral en 1
    /// </summary>
    /// <param name="delta">Cantidad a modificar (+1, -1, etc.)</param>
    private void ModifyMoral(int delta)
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ModifyMoral(delta);
            Debug.Log($"[InkExternalFunctions] Moral modificada en {delta}");
        }
        else
        {
            Debug.LogWarning("[InkExternalFunctions] GameManager no encontrado");
        }
    }

    /// <summary>
    /// Desbloquea una entrada del diario.
    /// Ejemplo en Ink: ~ UnlockDiaryEntry("entry_001")
    /// </summary>
    /// <param name="entryId">ID de la entrada a desbloquear</param>
    private void UnlockDiaryEntry(string entryId)
    {
        // Aquí deberías implementar la lógica para desbloquear una entrada del diario
        // Por ejemplo, si tienes un DiaryManager:
        // DiaryManager.Instance.UnlockEntry(entryId);

        Debug.Log($"[InkExternalFunctions] Entrada del diario desbloqueada: {entryId}");

        // TODO: Implementar la lógica real cuando tengas el sistema de diario
        // Ejemplo:
        // if (DiaryManager.Instance != null)
        // {
        //     DiaryManager.Instance.UnlockEntry(entryId);
        // }
    }

    /// <summary>
    /// Añade un item al inventario.
    /// Ejemplo en Ink: ~ AddItem("health_potion")
    /// </summary>
    /// <param name="itemName">Nombre del item a añadir</param>
    private void AddItem(string itemName)
    {
        // Aquí deberías implementar la lógica para añadir items al inventario
        // Por ejemplo, si tienes un InventoryManager:
        // InventoryManager.Instance.AddItem(itemName);

        Debug.Log($"[InkExternalFunctions] Item añadido al inventario: {itemName}");

        // TODO: Implementar la lógica real cuando tengas el sistema de inventario
        // Ejemplo:
        // if (InventoryData.Instance != null)
        // {
        //     InventoryData.Instance.AddItem(itemName);
        // }
    }

    /// <summary>
    /// Muestra un mensaje en la consola desde Ink (útil para debug).
    /// Ejemplo en Ink: ~ Log("Este es un mensaje de debug")
    /// </summary>
    /// <param name="message">Mensaje a mostrar</param>
    private void Log(string message)
    {
        Debug.Log($"[Ink Log] {message}");
    }
}
