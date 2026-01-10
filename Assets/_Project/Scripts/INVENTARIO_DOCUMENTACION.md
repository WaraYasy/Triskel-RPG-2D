# 📦 Sistema de Inventario - Documentación de Integración

## 📋 Tabla de Contenidos
1. [Arquitectura del Sistema](#arquitectura-del-sistema)
2. [Componentes Principales](#componentes-principales)
3. [Integración con el Jugador](#integración-con-el-jugador)
4. [Recolección de Items](#recolección-de-items)
5. [Usando los Items](#usando-los-items)
6. [Eventos del Inventario](#eventos-del-inventario)
7. [Persistencia (Guardar/Cargar)](#persistencia)
8. [Ejemplos de Código](#ejemplos-de-código)

---

## 🏗️ Arquitectura del Sistema

```
┌─────────────────────────────────────────────────────────────┐
│                    SISTEMA DE INVENTARIO                     │
├─────────────────────────────────────────────────────────────┤
│                                                               │
│  ┌──────────────────┐      ┌──────────────────┐            │
│  │ CollectibleItem  │◄─────┤ InventoryData    │            │
│  │ (ScriptableObject)│      │ (Singleton)      │            │
│  └──────────────────┘      └────────┬─────────┘            │
│         ▲                            │                       │
│         │                            │ Eventos               │
│         │                            ▼                       │
│  ┌──────┴────────────┐      ┌──────────────────┐            │
│  │CollectibleObject  │      │  InventoryUI     │            │
│  │(En la escena)     │      │  (UI Toolkit)    │            │
│  └───────────────────┘      └──────────────────┘            │
│         ▲                                                     │
│         │                                                     │
│  ┌──────┴────────────┐      ┌──────────────────┐            │
│  │  PlayerController │      │InventoryPersist. │            │
│  │  (A implementar)  │      │ (Guardar/Cargar) │            │
│  └───────────────────┘      └──────────────────┘            │
│                                                               │
└─────────────────────────────────────────────────────────────┘
```

---

## 🧩 Componentes Principales

### 1. **CollectibleItem** (ScriptableObject)
**Ubicación:** `Assets/_Project/Resources/Items/`

Define las propiedades de cada item coleccionable (Lirio, Hacha, Manto, etc.)

```csharp
// Propiedades:
- itemID: string         // ID único ("lirio", "hacha", "manto")
- displayName: string    // Nombre visible ("Lirio")
- icon: Sprite          // Icono para la UI
- slotColor: string     // Color del slot ("blue", "green", "red", "yellow", "grey")
- description: string   // Descripción del item
```

### 2. **InventoryData** (Singleton)
**Ubicación:** `Assets/_Project/Scripts/Core/InventoryData.cs`

Gestiona el estado del inventario en memoria (qué items tiene el jugador).

```csharp
// Métodos principales:
- AddItem(CollectibleItem item): bool
- RemoveItem(string itemID): bool
- HasItem(string itemID): bool
- GetItemAtSlot(int index): CollectibleItem
- GetAllItems(): List<CollectibleItem>
- ClearInventory(): void

// Eventos:
- OnItemAdded(CollectibleItem item)
- OnItemRemoved(CollectibleItem item)
- OnInventoryCleared()
```

### 3. **CollectibleObject** (MonoBehaviour)
**Ubicación:** `Assets/_Project/Scripts/Player/CollectibleObject.cs`

Script que se adjunta a GameObjects en la escena para hacerlos recolectables.

```csharp
// Configuración:
- itemData: CollectibleItem          // Qué item representa
- autoCollect: bool                  // Auto-recoger al tocar vs presionar tecla
- interactKey: KeyCode               // Tecla para recoger (si no es auto)
- pickupParticles: GameObject        // Efectos visuales (opcional)
- pickupSound: AudioClip             // Sonido (opcional)
```

### 4. **InventoryUI** (MonoBehaviour)
**Ubicación:** `Assets/_Project/Scripts/UI/HUD/InventoryUI.cs`

Controla la interfaz visual del inventario (los 3 slots).

```csharp
// Funcionalidad:
- Muestra automáticamente los items cuando se añaden
- Permite seleccionar items con clic o teclas (1, 2, 3)
- Emite eventos cuando un item es seleccionado/deseleccionado
```

### 5. **InventoryPersistence** (MonoBehaviour)
**Ubicación:** `Assets/_Project/Scripts/Core/InventoryPersistence.cs`

Guarda y carga el inventario (local con PlayerPrefs o remoto con API).

---

## 👤 Integración con el Jugador

### Paso 1: Crear el PlayerController

Cuando crees tu script `PlayerController.cs`, necesitarás:

```csharp
using UnityEngine;
using Triskel.Core;

namespace Triskel.Player
{
    public class PlayerController : MonoBehaviour
    {
        [Header("Inventario")]
        private CollectibleItem currentEquippedItem;

        private void Start()
        {
            // Suscribirse a eventos del inventario (opcional)
            SubscribeToInventoryEvents();
        }

        private void OnDestroy()
        {
            UnsubscribeFromInventoryEvents();
        }

        #region Inventory Events

        private void SubscribeToInventoryEvents()
        {
            if (InventoryData.Instance != null)
            {
                InventoryData.Instance.OnItemAdded += OnItemCollected;
                InventoryData.Instance.OnItemRemoved += OnItemLost;
            }
        }

        private void UnsubscribeFromInventoryEvents()
        {
            if (InventoryData.Instance != null)
            {
                InventoryData.Instance.OnItemAdded -= OnItemCollected;
                InventoryData.Instance.OnItemRemoved -= OnItemLost;
            }
        }

        /// <summary>
        /// Se ejecuta cuando el jugador recoge un item.
        /// </summary>
        private void OnItemCollected(CollectibleItem item)
        {
            Debug.Log($"[Player] Recogí: {item.displayName}");

            // Aquí puedes añadir lógica cuando se recoge un item:
            // - Mostrar notificación
            // - Reproducir animación
            // - Desbloquear habilidades
            // - etc.
        }

        /// <summary>
        /// Se ejecuta cuando el jugador pierde un item.
        /// </summary>
        private void OnItemLost(CollectibleItem item)
        {
            Debug.Log($"[Player] Perdí: {item.displayName}");

            // Si el item equipado es el que se perdió, desequiparlo
            if (currentEquippedItem == item)
            {
                UnequipItem();
            }
        }

        #endregion

        #region Item Equipment

        /// <summary>
        /// Equipar un item (llamado desde InventoryUI cuando se selecciona).
        /// </summary>
        public void EquipItem(CollectibleItem item)
        {
            currentEquippedItem = item;
            Debug.Log($"[Player] Item equipado: {item.displayName}");

            // Lógica según el item:
            switch (item.itemID)
            {
                case "hacha":
                    // Habilitar ataque con hacha
                    EnableAxeAttack();
                    break;

                case "manto":
                    // Dar habilidad de invisibilidad o protección
                    EnableMantoAbility();
                    break;

                case "lirio":
                    // Dar habilidad de curación o purificación
                    EnableLilyAbility();
                    break;
            }
        }

        /// <summary>
        /// Desequipar el item actual.
        /// </summary>
        public void UnequipItem()
        {
            if (currentEquippedItem == null) return;

            Debug.Log($"[Player] Item desequipado: {currentEquippedItem.displayName}");

            // Desactivar habilidades del item
            DisableItemAbilities();

            currentEquippedItem = null;
        }

        #endregion

        #region Item Abilities (Ejemplos)

        private void EnableAxeAttack()
        {
            // TODO: Implementar ataque con hacha
            Debug.Log("[Player] Ataque con hacha habilitado");
        }

        private void EnableMantoAbility()
        {
            // TODO: Implementar habilidad del manto
            Debug.Log("[Player] Habilidad del manto habilitada");
        }

        private void EnableLilyAbility()
        {
            // TODO: Implementar habilidad del lirio
            Debug.Log("[Player] Habilidad del lirio habilitada");
        }

        private void DisableItemAbilities()
        {
            // TODO: Desactivar todas las habilidades
            Debug.Log("[Player] Habilidades desactivadas");
        }

        #endregion
    }
}
```

### Paso 2: Conectar PlayerController con InventoryUI

Modifica `InventoryUI.cs` para llamar al `PlayerController` cuando se selecciona un item:

```csharp
// En InventoryUI.cs, método OnItemSelected:

private void OnItemSelected(CollectibleItem item, int slotIndex)
{
    Debug.Log($"[InventoryUI] Item seleccionado: {item.displayName} en slot {slotIndex}");

    // Buscar PlayerController y equipar el item
    PlayerController player = FindFirstObjectByType<PlayerController>();
    if (player != null)
    {
        player.EquipItem(item);
    }
}

// En InventoryUI.cs, método OnItemDeselected:

private void OnItemDeselected(CollectibleItem item)
{
    Debug.Log($"[InventoryUI] Item deseleccionado: {item.displayName}");

    // Buscar PlayerController y desequipar el item
    PlayerController player = FindFirstObjectByType<PlayerController>();
    if (player != null)
    {
        player.UnequipItem();
    }
}
```

---

## 🎯 Recolección de Items

### Opción 1: Usar CollectibleObject (Ya implementado)

Ya tienes el script `CollectibleObject.cs` que funciona así:

1. **Crear un item en la escena:**
   - Crea un GameObject (ejemplo: "Hacha_Pickup")
   - Añade un SpriteRenderer con el sprite del item
   - Añade un Collider2D (Box Collider 2D o Circle Collider 2D)
   - Marca el Collider como **Trigger**
   - Añade el componente `CollectibleObject`
   - Asigna el `CollectibleItem` correspondiente (Hacha.asset)

2. **Configurar el jugador:**
   - Asegúrate de que tu jugador tenga un Collider2D
   - Asegúrate de que tenga el tag **"Player"**

3. **Funcionamiento automático:**
   - Cuando el jugador toca el item, se añade automáticamente al inventario
   - El item se destruye de la escena
   - La UI se actualiza automáticamente

### Opción 2: Recolección Manual desde el PlayerController

Si prefieres más control, puedes recoger items directamente:

```csharp
// En tu PlayerController.cs

private void OnTriggerEnter2D(Collider2D other)
{
    // Verificar si es un item coleccionable
    CollectibleObject collectible = other.GetComponent<CollectibleObject>();

    if (collectible != null)
    {
        // Lógica personalizada antes de recoger
        if (CanCollectItem())
        {
            // Dejar que CollectibleObject maneje la recolección
            // (ya lo hace automáticamente)
        }
    }
}

private bool CanCollectItem()
{
    // Verificar si el inventario está lleno
    if (InventoryData.Instance.IsFull())
    {
        Debug.Log("Inventario lleno!");
        return false;
    }

    return true;
}
```

### Opción 3: Recolección Manual Directa

```csharp
// Añadir un item directamente desde cualquier script

CollectibleItem itemToAdd = /* obtener el item de algún lado */;

if (InventoryData.Instance != null)
{
    bool success = InventoryData.Instance.AddItem(itemToAdd);

    if (success)
    {
        Debug.Log($"Item {itemToAdd.displayName} añadido al inventario");
    }
    else
    {
        Debug.Log("No se pudo añadir el item (inventario lleno o duplicado)");
    }
}
```

---

## 🎮 Usando los Items

### Detectar qué item está seleccionado

```csharp
// Desde cualquier script:

// Obtener el item actualmente seleccionado en la UI
InventoryUI inventoryUI = FindFirstObjectByType<InventoryUI>();
CollectibleItem selectedItem = inventoryUI.GetSelectedItem();

if (selectedItem != null)
{
    Debug.Log($"Item seleccionado: {selectedItem.displayName}");
}

// O verificar si el jugador tiene un item específico
if (InventoryData.Instance.HasItem("hacha"))
{
    Debug.Log("El jugador tiene el hacha!");
}
```

### Usar items en combate/mecánicas

```csharp
// Ejemplo: Sistema de combate que usa el item equipado

public class CombatSystem : MonoBehaviour
{
    private PlayerController player;

    private void Start()
    {
        player = GetComponent<PlayerController>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Attack();
        }
    }

    private void Attack()
    {
        // Verificar qué item tiene equipado
        InventoryUI inventoryUI = FindFirstObjectByType<InventoryUI>();
        CollectibleItem equippedItem = inventoryUI.GetSelectedItem();

        if (equippedItem != null)
        {
            // Ataque diferente según el item
            switch (equippedItem.itemID)
            {
                case "hacha":
                    AxeAttack();
                    break;
                case "manto":
                    MantoDefense();
                    break;
                default:
                    BasicAttack();
                    break;
            }
        }
        else
        {
            BasicAttack();
        }
    }

    private void AxeAttack()
    {
        Debug.Log("¡Ataque poderoso con el hacha!");
        // Daño: 10
        // Rango: 2 metros
    }

    private void MantoDefense()
    {
        Debug.Log("¡Activando escudo del manto!");
        // Bloquea daño durante 3 segundos
    }

    private void BasicAttack()
    {
        Debug.Log("Ataque básico");
        // Daño: 5
        // Rango: 1 metro
    }
}
```

---

## 📡 Eventos del Inventario

El sistema de inventario usa **eventos** para notificar cuando algo cambia. Esto es muy útil para:
- Actualizar la UI automáticamente
- Desbloquear logros
- Activar diálogos
- Modificar gameplay

### Suscribirse a eventos

```csharp
using Triskel.Core;

public class MyGameplayScript : MonoBehaviour
{
    private void Start()
    {
        // Suscribirse a eventos
        if (InventoryData.Instance != null)
        {
            InventoryData.Instance.OnItemAdded += OnItemAdded;
            InventoryData.Instance.OnItemRemoved += OnItemRemoved;
            InventoryData.Instance.OnInventoryCleared += OnInventoryCleared;
        }
    }

    private void OnDestroy()
    {
        // IMPORTANTE: Desuscribirse para evitar memory leaks
        if (InventoryData.Instance != null)
        {
            InventoryData.Instance.OnItemAdded -= OnItemAdded;
            InventoryData.Instance.OnItemRemoved -= OnItemRemoved;
            InventoryData.Instance.OnInventoryCleared -= OnInventoryCleared;
        }
    }

    private void OnItemAdded(CollectibleItem item)
    {
        Debug.Log($"Se añadió: {item.displayName}");

        // Ejemplos de uso:
        // - Desbloquear logro
        // - Mostrar tutorial
        // - Activar cutscene
        // - Cambiar música
    }

    private void OnItemRemoved(CollectibleItem item)
    {
        Debug.Log($"Se removió: {item.displayName}");
    }

    private void OnInventoryCleared()
    {
        Debug.Log("Inventario limpiado");
    }
}
```

### Ejemplos de uso de eventos

```csharp
// Ejemplo 1: Sistema de Logros
private void OnItemAdded(CollectibleItem item)
{
    if (item.itemID == "lirio")
    {
        AchievementManager.Unlock("FIRST_RELIC");
    }

    // Si tiene las 3 reliquias
    if (InventoryData.Instance.IsFull())
    {
        AchievementManager.Unlock("ALL_RELICS");
    }
}

// Ejemplo 2: Sistema de Diálogos
private void OnItemAdded(CollectibleItem item)
{
    if (item.itemID == "hacha")
    {
        DialogueManager.ShowDialogue("ghost_hacha_found");
    }
}

// Ejemplo 3: Cambiar música
private void OnItemAdded(CollectibleItem item)
{
    if (InventoryData.Instance.GetItemCount() >= 2)
    {
        AudioManager.PlayMusic("tense_music");
    }
}
```

---

## 💾 Persistencia (Guardar/Cargar)

El sistema tiene dos modos de persistencia:

### Modo 1: Local (PlayerPrefs)
Guarda en el disco del jugador

```csharp
// Configurar en InventoryPersistence (Inspector):
useAPI = false

// Guardar manualmente:
InventoryPersistence persistence = FindFirstObjectByType<InventoryPersistence>();
persistence.SaveInventory();

// Cargar manualmente:
persistence.LoadInventory();

// Se guarda automáticamente cuando recoges un item (si CollectibleObject está configurado)
```

### Modo 2: Remoto (API)
Guarda en el servidor de Triskel

```csharp
// Configurar en InventoryPersistence (Inspector):
useAPI = true

// El guardado/carga funciona igual, pero usa la API
```

### Guardar automáticamente

El sistema ya guarda automáticamente cuando:
- Recoges un item con `CollectibleObject`
- Llamas a `SaveInventory()` manualmente

```csharp
// Guardar inventario en puntos clave del juego:

public class CheckpointManager : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Guardar inventario en checkpoint
            InventoryPersistence persistence = FindFirstObjectByType<InventoryPersistence>();
            if (persistence != null)
            {
                persistence.SaveInventory();
                Debug.Log("Progreso guardado!");
            }
        }
    }
}
```

### Cargar al iniciar el juego

```csharp
// En tu GameManager o MainMenuController:

public class GameManager : MonoBehaviour
{
    private void Start()
    {
        LoadGame();
    }

    private void LoadGame()
    {
        InventoryPersistence persistence = FindFirstObjectByType<InventoryPersistence>();
        if (persistence != null)
        {
            persistence.LoadInventory();
            Debug.Log("Inventario cargado");
        }
    }
}
```

---

## 💡 Ejemplos de Código

### Ejemplo 1: Quest que requiere tener ciertos items

```csharp
public class QuestManager : MonoBehaviour
{
    public void CheckQuestCompletion()
    {
        // Quest: "Recolecta las 3 reliquias"
        if (InventoryData.Instance.HasItem("lirio") &&
            InventoryData.Instance.HasItem("hacha") &&
            InventoryData.Instance.HasItem("manto"))
        {
            CompleteQuest("collect_all_relics");
        }
    }

    private void CompleteQuest(string questID)
    {
        Debug.Log($"Quest completada: {questID}");
        // Dar recompensa, mostrar cutscene, etc.
    }
}
```

### Ejemplo 2: Puerta que solo se abre con un item

```csharp
public class MagicDoor : MonoBehaviour
{
    [SerializeField] private string requiredItemID = "lirio";
    [SerializeField] private GameObject doorSprite;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (InventoryData.Instance.HasItem(requiredItemID))
            {
                OpenDoor();
            }
            else
            {
                ShowMessage($"Necesitas el {requiredItemID} para abrir esta puerta");
            }
        }
    }

    private void OpenDoor()
    {
        doorSprite.SetActive(false);
        Debug.Log("¡Puerta abierta!");
    }

    private void ShowMessage(string message)
    {
        Debug.Log(message);
        // TODO: Mostrar en UI
    }
}
```

### Ejemplo 3: NPC que reacciona según los items del jugador

```csharp
public class NPCDialogue : MonoBehaviour
{
    public string GetDialogue()
    {
        int itemCount = InventoryData.Instance.GetItemCount();

        if (itemCount == 0)
        {
            return "Debes encontrar las reliquias para salvar este lugar.";
        }
        else if (itemCount == 1)
        {
            return "¡Encontraste una reliquia! Pero aún necesitas más.";
        }
        else if (itemCount == 2)
        {
            return "Casi lo logras... solo falta una reliquia más.";
        }
        else // itemCount == 3
        {
            return "¡Tienes las 3 reliquias! Ahora puedes enfrentar al enemigo final.";
        }
    }
}
```

### Ejemplo 4: Cambiar sprite del jugador según item equipado

```csharp
public class PlayerVisuals : MonoBehaviour
{
    [SerializeField] private SpriteRenderer playerSprite;
    [SerializeField] private Sprite normalSprite;
    [SerializeField] private Sprite hachaSprite;
    [SerializeField] private Sprite mantoSprite;

    private InventoryUI inventoryUI;

    private void Start()
    {
        inventoryUI = FindFirstObjectByType<InventoryUI>();
    }

    private void Update()
    {
        UpdatePlayerSprite();
    }

    private void UpdatePlayerSprite()
    {
        CollectibleItem equipped = inventoryUI.GetSelectedItem();

        if (equipped == null)
        {
            playerSprite.sprite = normalSprite;
        }
        else
        {
            switch (equipped.itemID)
            {
                case "hacha":
                    playerSprite.sprite = hachaSprite;
                    break;
                case "manto":
                    playerSprite.sprite = mantoSprite;
                    break;
                default:
                    playerSprite.sprite = normalSprite;
                    break;
            }
        }
    }
}
```

---

## 🎯 Checklist de Integración

Cuando crees tu PlayerController, asegúrate de:

- [ ] El jugador tiene el tag **"Player"**
- [ ] El jugador tiene un **Collider2D**
- [ ] El PlayerController se suscribe a los eventos del inventario
- [ ] El PlayerController implementa `EquipItem()` y `UnequipItem()`
- [ ] La lógica de equipar items llama a los métodos del jugador
- [ ] Los items coleccionables en la escena tienen el tag correcto
- [ ] InventoryUI llama al PlayerController cuando se selecciona un item
- [ ] El inventario se guarda en puntos clave (checkpoints, salir del juego, etc.)
- [ ] El inventario se carga al iniciar el juego

---

## 🐛 Troubleshooting

### "No se recoge el item"
- Verifica que el jugador tenga el tag "Player"
- Verifica que el Collider2D esté marcado como Trigger
- Verifica que CollectibleObject tenga el itemData asignado

### "No se ve el icono en la UI"
- Verifica que el CollectibleItem tenga el sprite asignado en el campo `icon`
- Verifica que InventoryUI tenga la referencia al UIDocument

### "El inventario se resetea al cambiar de escena"
- Verifica que InventoryData tenga `DontDestroyOnLoad` (ya lo tiene)
- Verifica que estés guardando/cargando correctamente

### "Los eventos no se disparan"
- Verifica que te suscribiste en `Start()` o después
- Verifica que te desuscribes en `OnDestroy()`
- Verifica que InventoryData.Instance no sea null

---

## 📚 Referencias

- `InventoryData.cs` - Lógica del inventario
- `CollectibleItem.cs` - Definición de items
- `CollectibleObject.cs` - Items en la escena
- `InventoryUI.cs` - Interfaz visual
- `InventoryPersistence.cs` - Guardar/Cargar
- `InventoryTester.cs` - Herramienta de pruebas
- `InventoryDebugger.cs` - Herramienta de diagnóstico

---

## 🎓 Próximos Pasos

1. **Crear PlayerController.cs** con la estructura mostrada arriba
2. **Conectar InventoryUI** para que llame a `EquipItem()` cuando selecciones un item
3. **Implementar habilidades** específicas de cada reliquia
4. **Configurar items coleccionables** en tus escenas
5. **Añadir sistema de guardado** en checkpoints/menú

---

**Última actualización:** 2026-01-10
**Autor:** Sistema de Inventario Triskel
