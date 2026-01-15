using System.Collections.Generic;
using Ink.Runtime;
using UnityEngine;

/// <summary>
/// Gestiona las variables globales de Ink.
/// Mantiene una copia local de las variables y las sincroniza bidireccionalmente con Ink Story.
/// Basado en el sistema de quest-system de shapedbyrainstudios.
/// </summary>
public class InkDialogueVariables
{
    // Diccionario local que almacena todas las variables de Ink
    private Dictionary<string, object> variables;

    // Referencia a la historia de Ink
    private Story story;

    /// <summary>
    /// Constructor: Inicializa el diccionario extrayendo todas las variables globales de la historia de Ink.
    /// </summary>
    /// <param name="inkStory">La historia de Ink desde la cual extraer variables</param>
    public InkDialogueVariables(Story inkStory)
    {
        story = inkStory;
        variables = new Dictionary<string, object>();

        // Extraer todas las variables globales de la historia
        foreach (string variableName in story.variablesState)
        {
            object value = story.variablesState[variableName];
            variables.Add(variableName, value);

            Debug.Log($"[InkDialogueVariables] Variable '{variableName}' inicializada con valor: {value}");
        }
    }

    /// <summary>
    /// Sincroniza las variables locales con Ink y comienza a escuchar cambios.
    /// Llama a esto al iniciar un diálogo.
    /// </summary>
    public void SyncVariablesAndStartListening()
    {
        // Escribir todas las variables locales a Ink
        SyncVariablesToStory();

        // Suscribirse a los cambios de variables en Ink
        story.variablesState.variableChangedEvent += UpdateVariableState;
    }

    /// <summary>
    /// Deja de escuchar cambios en las variables de Ink.
    /// Llama a esto al salir de un diálogo.
    /// </summary>
    public void StopListening()
    {
        story.variablesState.variableChangedEvent -= UpdateVariableState;
    }

    /// <summary>
    /// Callback que se ejecuta cuando una variable cambia en Ink.
    /// Actualiza el diccionario local solo si la variable ya existía.
    /// </summary>
    /// <param name="variableName">Nombre de la variable que cambió</param>
    /// <param name="newValue">Nuevo valor de la variable</param>
    private void UpdateVariableState(string variableName, object newValue)
    {
        // Solo actualizar si la variable ya está en nuestro diccionario
        if (variables.ContainsKey(variableName))
        {
            variables[variableName] = newValue;
            Debug.Log($"[InkDialogueVariables] Variable '{variableName}' actualizada a: {newValue}");
        }
    }

    /// <summary>
    /// Sincroniza todas las variables locales hacia Ink Story.
    /// Útil para actualizar el estado de Ink desde Unity.
    /// </summary>
    private void SyncVariablesToStory()
    {
        foreach (KeyValuePair<string, object> variable in variables)
        {
            story.variablesState[variable.Key] = variable.Value;
        }
    }

    /// <summary>
    /// Obtiene el valor de una variable.
    /// </summary>
    /// <param name="variableName">Nombre de la variable</param>
    /// <returns>Valor de la variable, o null si no existe</returns>
    public object GetVariable(string variableName)
    {
        if (variables.ContainsKey(variableName))
        {
            return variables[variableName];
        }

        Debug.LogWarning($"[InkDialogueVariables] Variable '{variableName}' no encontrada");
        return null;
    }

    /// <summary>
    /// Establece el valor de una variable (tanto local como en Ink).
    /// </summary>
    /// <param name="variableName">Nombre de la variable</param>
    /// <param name="value">Nuevo valor</param>
    public void SetVariable(string variableName, object value)
    {
        if (variables.ContainsKey(variableName))
        {
            variables[variableName] = value;
            story.variablesState[variableName] = value;
            Debug.Log($"[InkDialogueVariables] Variable '{variableName}' establecida a: {value}");
        }
        else
        {
            Debug.LogWarning($"[InkDialogueVariables] Intentando establecer variable inexistente: '{variableName}'");
        }
    }
}
