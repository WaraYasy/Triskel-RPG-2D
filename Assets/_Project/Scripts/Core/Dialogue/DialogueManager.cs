using System.Collections;
using UnityEngine;
using Ink.Runtime;

/// <summary>
/// Gestor principal del sistema de diálogos con Ink.
/// Maneja la carga de historias, navegación de diálogos, eventos y sincronización de variables.
///
/// FLUJO BÁSICO:
/// 1. Se carga el archivo JSON de Ink en Awake
/// 2. Se suscriben eventos en OnEnable
/// 3. Cuando se llama EnterDialogue(knotName), se inicia el diálogo en ese knot
/// 4. El diálogo avanza con ContinueStory()
/// 5. Al salir, se limpia todo en ExitDialogue()
///
/// Basado en el sistema de quest-system de shapedbyrainstudios.
/// Adaptado y simplificado para el proyecto Triskel.
/// </summary>
public class DialogueManager : MonoBehaviour
{
    // ===== CONFIGURACIÓN =====
    [Header("Configuración de Ink")]
    [Tooltip("Archivo JSON compilado de Ink que contiene todos los diálogos")]
    [SerializeField] private TextAsset inkJSON;

    // ===== REFERENCIAS INTERNAS =====
    private Story currentStory;                         // Historia actual de Ink
    private InkDialogueVariables dialogueVariables;     // Gestor de variables de Ink
    private InkExternalFunctions externalFunctions;     // Funciones externas para Ink

    // ===== ESTADO DEL DIÁLOGO =====
    private bool isDialogueActive = false;              // ¿Está activo un diálogo?

    // ===== PROPIEDADES PÚBLICAS =====
    /// <summary>Indica si hay un diálogo actualmente activo</summary>
    public bool IsDialogueActive => isDialogueActive;

    /// <summary>Devuelve la historia actual de Ink (útil para acceso externo)</summary>
    public Story CurrentStory => currentStory;

    #region Unity Lifecycle

    private void Awake()
    {
        // Validar que tenemos un archivo Ink asignado
        if (inkJSON == null)
        {
            Debug.LogError("[DialogueManager] ¡No hay archivo Ink JSON asignado! Asigna uno en el Inspector.");
            return;
        }

        // Crear la historia de Ink desde el JSON
        currentStory = new Story(inkJSON.text);

        // Inicializar sistemas auxiliares
        dialogueVariables = new InkDialogueVariables(currentStory);
        externalFunctions = new InkExternalFunctions();

        // Vincular funciones externas
        externalFunctions.Bind(currentStory);

        Debug.Log("[DialogueManager] Inicializado correctamente");
    }

    private void OnDestroy()
    {
        // Desvincular funciones externas al destruir
        if (currentStory != null && externalFunctions != null)
        {
            externalFunctions.Unbind(currentStory);
        }
    }

    private void OnEnable()
    {
        // Suscribirse al evento de entrada a diálogo desde GameManager
        if (GameManager.Instance != null && GameManager.Instance.dialogueEvents != null)
        {
            GameManager.Instance.dialogueEvents.onEnterDialogue += EnterDialogue;
            Debug.Log("[DialogueManager] Suscrito a eventos de diálogo");
        }
        else
        {
            Debug.LogWarning("[DialogueManager] GameManager o DialogueEvents no encontrado");
        }
    }

    private void OnDisable()
    {
        // Desuscribirse de eventos
        if (GameManager.Instance != null && GameManager.Instance.dialogueEvents != null)
        {
            GameManager.Instance.dialogueEvents.onEnterDialogue -= EnterDialogue;
        }

        // Detener la escucha de variables
        if (dialogueVariables != null)
        {
            dialogueVariables.StopListening();
        }
    }

    #endregion

    #region Dialogue Flow

    /// <summary>
    /// Inicia un diálogo en un knot específico de Ink.
    /// </summary>
    /// <param name="knotName">Nombre del knot en el archivo .ink (ej: "npc", "merchant", etc.)</param>
    private void EnterDialogue(string knotName)
    {
        if (currentStory == null)
        {
            Debug.LogError("[DialogueManager] No hay historia de Ink cargada");
            return;
        }

        if (isDialogueActive)
        {
            Debug.LogWarning("[DialogueManager] Ya hay un diálogo activo");
            return;
        }

        // Activar estado de diálogo
        isDialogueActive = true;

        // Navegar al knot especificado
        currentStory.ChoosePathString(knotName);

        // Sincronizar variables y comenzar a escuchar cambios
        dialogueVariables.SyncVariablesAndStartListening();

        Debug.Log($"[DialogueManager] Diálogo iniciado en knot: {knotName}");

        // TODO: Aquí podrías pausar el movimiento del jugador, mostrar UI, etc.
        // Ejemplo:
        // PlayerController.Instance.SetMovementEnabled(false);
        // DialogueUI.Instance.Show();

        // Comenzar a mostrar el diálogo
        ContinueStory();
    }

    /// <summary>
    /// Avanza la historia de Ink y procesa el siguiente contenido.
    /// Muestra texto o choices, según corresponda.
    /// </summary>
    public void ContinueStory()
    {
        if (!isDialogueActive || currentStory == null)
        {
            Debug.LogWarning("[DialogueManager] No hay diálogo activo para continuar");
            return;
        }

        // Verificar si la historia puede continuar
        if (currentStory.canContinue)
        {
            // Obtener la siguiente línea de diálogo
            string text = currentStory.Continue();

            // Saltar líneas en blanco
            while (IsLineBlank(text) && currentStory.canContinue)
            {
                text = currentStory.Continue();
            }

            // Mostrar el texto
            Debug.Log($"[Diálogo] {text}");

            // TODO: Aquí deberías mostrar el texto en tu UI
            // Ejemplo:
            // DialogueUI.Instance.ShowText(text);

            // Si hay tags, procesarlos
            if (currentStory.currentTags.Count > 0)
            {
                foreach (string tag in currentStory.currentTags)
                {
                    Debug.Log($"[Diálogo Tag] {tag}");
                    // TODO: Procesar tags (por ejemplo, para cambiar expresiones del personaje)
                }
            }
        }

        // Verificar si hay elecciones disponibles
        if (currentStory.currentChoices.Count > 0)
        {
            Debug.Log("[Diálogo] Opciones disponibles:");
            for (int i = 0; i < currentStory.currentChoices.Count; i++)
            {
                Debug.Log($"  {i + 1}. {currentStory.currentChoices[i].text}");
            }

            // TODO: Aquí deberías mostrar las opciones en tu UI
            // Ejemplo:
            // DialogueUI.Instance.ShowChoices(currentStory.currentChoices);
        }
        // Si no hay más contenido ni elecciones, salir del diálogo
        else if (!currentStory.canContinue)
        {
            ExitDialogue();
        }
    }

    /// <summary>
    /// Selecciona una opción y continúa la historia.
    /// </summary>
    /// <param name="choiceIndex">Índice de la opción elegida (0-based)</param>
    public void MakeChoice(int choiceIndex)
    {
        if (!isDialogueActive || currentStory == null)
        {
            Debug.LogWarning("[DialogueManager] No hay diálogo activo para elegir");
            return;
        }

        if (choiceIndex < 0 || choiceIndex >= currentStory.currentChoices.Count)
        {
            Debug.LogWarning($"[DialogueManager] Índice de elección inválido: {choiceIndex}");
            return;
        }

        // Seleccionar la opción
        currentStory.ChooseChoiceIndex(choiceIndex);
        Debug.Log($"[DialogueManager] Opción seleccionada: {choiceIndex}");

        // Continuar la historia después de elegir
        ContinueStory();
    }

    /// <summary>
    /// Sale del diálogo actual y limpia el estado.
    /// </summary>
    private void ExitDialogue()
    {
        if (!isDialogueActive)
        {
            return;
        }

        Debug.Log("[DialogueManager] Saliendo del diálogo");

        // Detener la escucha de variables
        dialogueVariables.StopListening();

        // Desactivar estado de diálogo
        isDialogueActive = false;

        // TODO: Aquí deberías reactivar el movimiento del jugador, ocultar UI, etc.
        // Ejemplo:
        // PlayerController.Instance.SetMovementEnabled(true);
        // DialogueUI.Instance.Hide();
    }

    #endregion

    #region Utility Methods

    /// <summary>
    /// Verifica si una línea está en blanco o vacía.
    /// </summary>
    /// <param name="line">Línea a verificar</param>
    /// <returns>True si está en blanco</returns>
    private bool IsLineBlank(string line)
    {
        return string.IsNullOrWhiteSpace(line);
    }

    #endregion

    #region Public API

    /// <summary>
    /// Inicia un diálogo desde código externo.
    /// Usa esto si quieres iniciar un diálogo directamente sin pasar por eventos.
    /// </summary>
    /// <param name="knotName">Nombre del knot en Ink</param>
    public void StartDialogue(string knotName)
    {
        EnterDialogue(knotName);
    }

    /// <summary>
    /// Obtiene el valor de una variable de Ink.
    /// </summary>
    /// <param name="variableName">Nombre de la variable</param>
    /// <returns>Valor de la variable</returns>
    public object GetVariable(string variableName)
    {
        return dialogueVariables?.GetVariable(variableName);
    }

    /// <summary>
    /// Establece el valor de una variable de Ink.
    /// </summary>
    /// <param name="variableName">Nombre de la variable</param>
    /// <param name="value">Nuevo valor</param>
    public void SetVariable(string variableName, object value)
    {
        dialogueVariables?.SetVariable(variableName, value);
    }

    #endregion
}
