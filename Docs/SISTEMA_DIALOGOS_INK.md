# Sistema de Diálogos con Ink - Triskel

Este documento explica cómo funciona el sistema de diálogos con Ink en el proyecto Triskel.

**Basado en:** [quest-system de shapedbyrainstudios](https://github.com/shapedbyrainstudios/quest-system) (rama 4-dialogue-implemented)
**Adaptado y simplificado para:** Proyecto Triskel

---

## 📋 Tabla de Contenidos

1. [¿Qué es Ink?](#qué-es-ink)
2. [Arquitectura del Sistema](#arquitectura-del-sistema)
3. [Componentes Principales](#componentes-principales)
4. [Configuración Inicial](#configuración-inicial)
5. [Cómo Usar el Sistema](#cómo-usar-el-sistema)
6. [Ejemplos de Código](#ejemplos-de-código)
7. [Funciones Externas Disponibles](#funciones-externas-disponibles)
8. [Variables de Ink](#variables-de-ink)
9. [Tags en Ink](#tags-en-ink)
10. [Troubleshooting](#troubleshooting)

---

## ¿Qué es Ink?

[Ink](https://www.inklestudios.com/ink/) es un lenguaje de scripting narrativo creado por Inkle Studios. Permite escribir diálogos complejos con ramificaciones, variables, y lógica condicional de forma sencilla.

**Ventajas:**
- ✅ Sintaxis simple y legible
- ✅ Sistema de opciones y ramificaciones potente
- ✅ Variables y lógica condicional integrada
- ✅ Se integra fácilmente con Unity

---

## Arquitectura del Sistema

El sistema de diálogos está compuesto por 4 componentes principales:

```
┌─────────────────────────────────────────────────────────────┐
│                       GameManager                           │
│  - Singleton global                                         │
│  - Contiene DialogueEvents                                  │
└────────────────────┬────────────────────────────────────────┘
                     │
                     │ dialogueEvents.onEnterDialogue
                     ▼
┌─────────────────────────────────────────────────────────────┐
│                    DialogueManager                          │
│  - Gestor principal del sistema                             │
│  - Carga y ejecuta historias de Ink                         │
│  - Maneja el flujo del diálogo                              │
└──────┬──────────────────────┬────────────────────┬──────────┘
       │                      │                    │
       ▼                      ▼                    ▼
┌──────────────┐    ┌──────────────────┐   ┌──────────────────┐
│InkDialogue   │    │InkExternal       │   │  Ink Story       │
│Variables     │    │Functions         │   │  (.ink/.json)    │
│- Gestiona    │    │- Funciones que   │   │- Archivo de      │
│  variables   │    │  Ink puede       │   │  diálogo         │
│  de Ink      │    │  llamar          │   │                  │
└──────────────┘    └──────────────────┘   └──────────────────┘
```

---

## Componentes Principales

### 1. **DialogueManager.cs**
**Ubicación:** `Assets/_Project/Scripts/Core/Dialogue/DialogueManager.cs`

**Responsabilidades:**
- Cargar el archivo JSON de Ink
- Iniciar y finalizar diálogos
- Avanzar la historia (ContinueStory)
- Procesar elecciones del jugador (MakeChoice)
- Coordinar InkDialogueVariables e InkExternalFunctions

**Propiedades públicas:**
- `bool IsDialogueActive` - Indica si hay un diálogo activo
- `Story CurrentStory` - Referencia a la historia de Ink actual

**Métodos públicos:**
- `StartDialogue(string knotName)` - Inicia un diálogo
- `ContinueStory()` - Avanza el diálogo
- `MakeChoice(int choiceIndex)` - Selecciona una opción
- `GetVariable(string variableName)` - Obtiene una variable de Ink
- `SetVariable(string variableName, object value)` - Establece una variable

---

### 2. **InkDialogueVariables.cs**
**Ubicación:** `Assets/_Project/Scripts/Core/Dialogue/InkDialogueVariables.cs`

**Responsabilidades:**
- Mantener una copia local de las variables de Ink
- Sincronizar variables entre Unity e Ink bidireccionalmente
- Escuchar cambios en las variables de Ink

**Métodos principales:**
- `SyncVariablesAndStartListening()` - Inicia la sincronización
- `StopListening()` - Detiene la escucha de cambios
- `GetVariable(string name)` - Obtiene una variable
- `SetVariable(string name, object value)` - Establece una variable

---

### 3. **InkExternalFunctions.cs**
**Ubicación:** `Assets/_Project/Scripts/Core/Dialogue/InkExternalFunctions.cs`

**Responsabilidades:**
- Vincular funciones de Unity con Ink
- Permitir que los scripts de Ink llamen código de Unity

**Funciones vinculadas:**
- `ModifyMoral(int delta)` - Modifica la moral del jugador
- `UnlockDiaryEntry(string entryId)` - Desbloquea entrada del diario
- `AddItem(string itemName)` - Añade item al inventario
- `Log(string message)` - Muestra mensaje en consola (debug)

---

### 4. **DialogueEvents.cs**
**Ubicación:** `Assets/_Project/Scripts/Core/Dialogue/DialogueEvents.cs`

**Responsabilidades:**
- Sistema de eventos para iniciar diálogos
- Conecta GameManager con DialogueManager

**Eventos:**
- `onEnterDialogue` - Se dispara al iniciar un diálogo

---

## Configuración Inicial

### Paso 1: Preparar el Archivo .ink

1. Crea o edita tu archivo `.ink` en `Assets/_Project/InkDialogue/`
2. Ejemplo: `all_dialogs.ink`

```ink
=== npc ===
¡Hola, viajero!

+ [¿Quién eres?]
    Soy un guardián de este bosque.
    -> END
+ [Adiós]
    ¡Hasta luego!
    -> END
```

3. Guarda el archivo. Ink lo compilará automáticamente a JSON.

### Paso 2: Configurar DialogueManager en Unity

1. Crea un GameObject en tu escena (ej: "DialogueManager")
2. Añade el componente `DialogueManager`
3. En el Inspector, asigna el campo **Ink JSON**:
   - Arrastra el archivo `.json` compilado (ej: `all_dialogs.json`)

![Configuración del Inspector](https://via.placeholder.com/600x200?text=Configurar+DialogueManager+en+Inspector)

### Paso 3: Asegurar que GameManager existe

El `DialogueManager` necesita que `GameManager` esté en la escena con `DialogueEvents` inicializado.

Ya está configurado en tu proyecto en `GameManager.cs:44`:
```csharp
dialogueEvents = new DialogueEvents();
```

---

## Cómo Usar el Sistema

### Iniciar un Diálogo

Hay **dos formas** de iniciar un diálogo:

#### Opción A: Usando Eventos (Recomendado)
```csharp
// Desde cualquier script
GameManager.Instance.dialogueEvents.EnterDialogue("npc");
```

#### Opción B: Directamente desde DialogueManager
```csharp
// Obtén la referencia al DialogueManager
DialogueManager dialogueManager = FindObjectOfType<DialogueManager>();
dialogueManager.StartDialogue("npc");
```

### Avanzar el Diálogo

Después de que el texto se muestre, llama a:
```csharp
dialogueManager.ContinueStory();
```

### Seleccionar una Opción

Cuando aparezcan opciones, selecciona una con:
```csharp
// choiceIndex es 0-based (0, 1, 2...)
dialogueManager.MakeChoice(choiceIndex);
```

### Verificar si hay Diálogo Activo

```csharp
if (dialogueManager.IsDialogueActive)
{
    // Hay un diálogo en curso
}
```

---

## Ejemplos de Código

### Ejemplo 1: Iniciar Diálogo al Interactuar con NPC

```csharp
using UnityEngine;

public class NPC : MonoBehaviour
{
    [SerializeField] private string knotName = "npc";

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Iniciar diálogo usando eventos
            GameManager.Instance.dialogueEvents.EnterDialogue(knotName);
        }
    }
}
```

### Ejemplo 2: Sistema de Input para Diálogo

```csharp
using UnityEngine;

public class DialogueInput : MonoBehaviour
{
    private DialogueManager dialogueManager;

    private void Start()
    {
        dialogueManager = FindObjectOfType<DialogueManager>();
    }

    private void Update()
    {
        if (!dialogueManager.IsDialogueActive)
            return;

        // Presionar Espacio para continuar
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (dialogueManager.CurrentStory.currentChoices.Count == 0)
            {
                dialogueManager.ContinueStory();
            }
        }

        // Presionar 1, 2, 3... para elegir opciones
        if (Input.GetKeyDown(KeyCode.Alpha1))
            dialogueManager.MakeChoice(0);
        if (Input.GetKeyDown(KeyCode.Alpha2))
            dialogueManager.MakeChoice(1);
        if (Input.GetKeyDown(KeyCode.Alpha3))
            dialogueManager.MakeChoice(2);
    }
}
```

### Ejemplo 3: Mostrar Diálogo en UI

```csharp
using UnityEngine;
using UnityEngine.UI;
using Ink.Runtime;

public class DialogueUI : MonoBehaviour
{
    [SerializeField] private Text dialogueText;
    [SerializeField] private GameObject choiceButtonPrefab;
    [SerializeField] private Transform choiceContainer;

    private DialogueManager dialogueManager;

    private void Start()
    {
        dialogueManager = FindObjectOfType<DialogueManager>();

        // Puedes suscribirte a eventos personalizados aquí
    }

    public void ShowText(string text)
    {
        dialogueText.text = text;
    }

    public void ShowChoices(System.Collections.Generic.List<Choice> choices)
    {
        // Limpiar opciones anteriores
        foreach (Transform child in choiceContainer)
        {
            Destroy(child.gameObject);
        }

        // Crear botones para cada opción
        for (int i = 0; i < choices.Count; i++)
        {
            GameObject button = Instantiate(choiceButtonPrefab, choiceContainer);
            Text buttonText = button.GetComponentInChildren<Text>();
            buttonText.text = choices[i].text;

            int choiceIndex = i; // Capturar índice
            button.GetComponent<Button>().onClick.AddListener(() =>
            {
                dialogueManager.MakeChoice(choiceIndex);
            });
        }
    }
}
```

---

## Funciones Externas Disponibles

Estas funciones se pueden llamar desde scripts de Ink usando `~`:

### 1. **ModifyMoral(int delta)**
Modifica la moral del jugador.

```ink
* [Ayudar al aldeano]
    ~ ModifyMoral(1)
    Gracias por tu ayuda.
```

### 2. **UnlockDiaryEntry(string entryId)**
Desbloquea una entrada del diario.

```ink
* [Leer el pergamino]
    ~ UnlockDiaryEntry("ancient_scroll")
    Has descubierto información importante.
```

### 3. **AddItem(string itemName)**
Añade un item al inventario.

```ink
* [Tomar la poción]
    ~ AddItem("health_potion")
    Has obtenido una poción de vida.
```

### 4. **Log(string message)**
Muestra un mensaje en la consola de Unity (útil para debug).

```ink
~ Log("El jugador llegó a este punto del diálogo")
```

---

## Variables de Ink

### Declarar Variables en Ink

```ink
VAR has_met_npc = false
VAR player_gold = 100
VAR reputation = 0
```

### Usar Variables en Condiciones

```ink
{has_met_npc:
    ¡Hola de nuevo!
- else:
    ¡Hola, soy nuevo aquí!
    ~ has_met_npc = true
}
```

### Leer/Escribir Variables desde Unity

```csharp
// Leer variable
bool hasMet = (bool)dialogueManager.GetVariable("has_met_npc");

// Escribir variable
dialogueManager.SetVariable("player_gold", 50);
```

---

## Tags en Ink

Los tags permiten añadir metadatos al diálogo (ej: speaker, emotion, animation).

### Definir Tags en Ink

```ink
=== merchant ===
# speaker:Merchant
# emotion:happy
# animation:wave

¡Bienvenido a mi tienda!
```

### Leer Tags desde Unity

```csharp
if (dialogueManager.CurrentStory.currentTags.Count > 0)
{
    foreach (string tag in dialogueManager.CurrentStory.currentTags)
    {
        string[] parts = tag.Split(':');
        string tagName = parts[0];
        string tagValue = parts[1];

        if (tagName == "speaker")
        {
            // Cambiar nombre del personaje en UI
        }
        else if (tagName == "emotion")
        {
            // Cambiar expresión facial
        }
    }
}
```

---

## Troubleshooting

### Error: "No hay archivo Ink JSON asignado"

**Solución:** Asigna el archivo `.json` compilado en el Inspector del DialogueManager.

---

### Error: "GameManager o DialogueEvents no encontrado"

**Solución:** Asegúrate de que:
1. Hay un GameObject con el componente `GameManager` en la escena
2. El `GameManager` está inicializando `dialogueEvents` en `Awake()`

---

### Las opciones no aparecen

**Problema:** Llamas a `ContinueStory()` cuando hay opciones disponibles.

**Solución:** Verifica si hay opciones antes de continuar:
```csharp
if (story.currentChoices.Count > 0)
{
    // Mostrar opciones, esperar selección
}
else if (story.canContinue)
{
    ContinueStory();
}
```

---

### Las funciones externas no funcionan

**Problema:** Las funciones no están vinculadas correctamente.

**Solución:**
1. Verifica que `InkExternalFunctions.Bind()` se llame en `DialogueManager.Awake()`
2. Asegúrate de que el nombre de la función en Ink coincide exactamente con el nombre en C#
3. Revisa la consola en busca de errores

---

### Los cambios en .ink no se reflejan

**Problema:** El archivo no se recompiló.

**Solución:**
1. Guarda el archivo `.ink`
2. Espera a que Unity recompile (verás el spinner)
3. Verifica que el archivo `.json` se haya actualizado (mira la fecha de modificación)

---

## Recursos Adicionales

- **Documentación oficial de Ink:** https://github.com/inkle/ink/blob/master/Documentation/WritingWithInk.md
- **Ink Unity Integration:** https://github.com/inkle/ink-unity-integration
- **Repositorio de referencia:** https://github.com/shapedbyrainstudios/quest-system

---

## Resumen Rápido

**Para iniciar un diálogo:**
```csharp
GameManager.Instance.dialogueEvents.EnterDialogue("knot_name");
```

**Para continuar:**
```csharp
dialogueManager.ContinueStory();
```

**Para elegir opción:**
```csharp
dialogueManager.MakeChoice(index);
```

**Para llamar funciones desde Ink:**
```ink
~ ModifyMoral(1)
~ AddItem("sword")
~ UnlockDiaryEntry("entry_001")
```

---

¡Sistema de diálogos completo y listo para usar! 🎮
