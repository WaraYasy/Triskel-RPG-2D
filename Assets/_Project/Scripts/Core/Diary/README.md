# Sistema de Diario Narrativo - Triskel

Sistema completo de diario narrativo con entradas desbloqueables basadas en decisiones del jugador.

## Arquitectura

### 1. DiaryEntryData (ScriptableObject)
Define los datos de una entrada del diario.

**Campos:**
- `entryID`: Identificador único (ej: "level1_saved_child")
- `levelIndex`: Nivel que desbloquea esta entrada (0 = Nivel 1)
- `conditionID`: Condición que activa esta entrada (ej: "saved_child")
- `title`: Título visible en la UI
- `text`: Texto completo de la entrada

**Crear una entrada:**
1. Click derecho en Project → Create → Triskel/Diary Entry
2. Configurar los campos en el Inspector
3. Asignar el asset al DiaryManager

### 2. DiaryManager (Singleton)
Lógica central del sistema. Gestiona desbloqueo y acceso a entradas.

**Métodos principales:**
```csharp
// Notificar nivel completado con condiciones
DiaryManager.Instance.OnLevelCompleted(levelIndex, conditionIDs);

// Desbloquear entrada manualmente
DiaryManager.Instance.UnlockEntry("level1_saved_child");

// Verificar si una entrada está desbloqueada
bool isUnlocked = DiaryManager.Instance.IsEntryUnlocked("level1_saved_child");

// Obtener datos de una entrada
DiaryEntryData entry = DiaryManager.Instance.GetEntryData("level1_saved_child");

// Obtener todas las entradas desbloqueadas
List<DiaryEntryData> entries = DiaryManager.Instance.GetUnlockedEntries();
```

### 3. DiaryPersistence
Gestiona guardado y carga de entradas desbloqueadas.

**Características:**
- Guardado local con PlayerPrefs
- Preparado para integración con TriskelAPIClient (guardado remoto)
- Formato CSV: "level1_saved_child,level2_ignored_npc,level3_completed"

### 4. DiaryUI (UI Toolkit)
Interfaz visual del diario.

**Características:**
- Panel que se abre/cierra con tecla (por defecto: J)
- Lista de entradas desbloqueadas
- Visualización de texto completo con scroll
- Actualización automática al desbloquear entradas

**Métodos públicos:**
```csharp
DiaryUI diaryUI = FindObjectOfType<DiaryUI>();

// Abrir panel
diaryUI.OpenPanel();

// Cerrar panel
diaryUI.ClosePanel();

// Toggle panel
diaryUI.TogglePanel();

// Actualizar UI (después de desbloquear entrada)
diaryUI.UpdateUI();
```

## Setup en Unity

### Paso 1: Crear GameObjects necesarios

1. **Crear DiaryManager:**
   - GameObject vacío: "DiaryManager"
   - Añadir componente: DiaryManager
   - Añadir componente: DiaryPersistence
   - Asignar todas las DiaryEntryData creadas en el campo "All Entries"

2. **Crear DiaryUI:**
   - GameObject con UIDocument: "DiaryUI"
   - Añadir componente: UIDocument
   - Añadir componente: DiaryUI
   - Asignar el archivo UXML en UIDocument.sourceAsset
   - Configurar la tecla de toggle (por defecto: J)

### Paso 2: Crear entradas del diario

Para cada entrada narrativa:
1. Click derecho → Create → Triskel/Diary Entry
2. Configurar:
   - `entryID`: ID único (ej: "level1_saved_child")
   - `levelIndex`: Nivel asociado (0, 1, 2...)
   - `conditionID`: Condición que la desbloquea (ej: "saved_child")
   - `title`: "El niño perdido"
   - `text`: Texto narrativo completo

3. Arrastrar el asset al array "All Entries" del DiaryManager

### Paso 3: Integrar con tu sistema de niveles

En tu código de fin de nivel:

```csharp
using Triskel.Core;

public class LevelController : MonoBehaviour
{
    private void OnLevelCompleted()
    {
        int levelIndex = 0; // Nivel actual

        // Recopilar condiciones cumplidas durante el nivel
        List<string> conditions = new List<string>();

        if (playerSavedChild)
            conditions.Add("saved_child");
        else
            conditions.Add("ignored_child");

        if (playerFoundSecret)
            conditions.Add("found_secret");

        // Notificar al diario
        DiaryManager.Instance.OnLevelCompleted(levelIndex, conditions.ToArray());
    }
}
```

## Ejemplo de Estructura de Entradas

### Nivel 0 - Decisión A (salvar niño)
- entryID: "level0_saved_child"
- levelIndex: 0
- conditionID: "saved_child"
- title: "El niño perdido"
- text: "Hoy salvé a un niño del bosque oscuro. Sus ojos reflejaban esperanza..."

### Nivel 0 - Decisión B (ignorar niño)
- entryID: "level0_ignored_child"
- levelIndex: 0
- conditionID: "ignored_child"
- title: "El bosque silencioso"
- text: "Escuché un llanto en el bosque, pero seguí mi camino. El tiempo no esperaba..."

### Nivel 1 - Completado
- entryID: "level1_completed"
- levelIndex: 1
- conditionID: "level_completed"
- title: "La ciudad olvidada"
- text: "Llegué a la ciudad olvidada. Las ruinas cuentan historias de tiempos antiguos..."

## Debugging

### Context Menus disponibles:

**DiaryManager:**
- Debug: Desbloquear Entrada de Prueba
- Debug: Mostrar Entradas Desbloqueadas
- Debug: Limpiar Todas las Entradas
- Debug: Simular Nivel Completado

**DiaryPersistence:**
- Debug: Guardar Diario
- Debug: Cargar Diario
- Debug: Limpiar Datos Guardados

### Logs útiles:

```csharp
// Ver entradas desbloqueadas
DiaryManager.Instance.DebugShowUnlockedEntries();

// Limpiar todo (reset)
DiaryManager.Instance.ClearAllEntries();
DiaryPersistence.Instance.ClearSavedData();
```

## Integración con API (Futuro)

El sistema está preparado para guardado remoto:

```csharp
// En DiaryPersistence.cs (cuando TriskelAPIClient tenga endpoint)
public void SaveToRemote(string[] entryIDs, Action onSuccess, Action<string> onError)
{
    if (TriskelAPIClient.Instance != null)
    {
        // Llamar endpoint de la API
        TriskelAPIClient.Instance.UpdateDiaryEntries(entryIDs, onSuccess, onError);
    }
}
```

## Personalización de UI

Edita los estilos en `DiaryUI.uss` para cambiar:
- Colores (background-color, border-color, color)
- Tamaños de fuente (font-size)
- Espaciado (padding, margin)
- Bordes (border-width, border-radius)

## Checklist de Implementación

- [ ] Crear GameObject DiaryManager con componentes necesarios
- [ ] Crear GameObject DiaryUI con UIDocument
- [ ] Crear al menos 2 DiaryEntryData por nivel con diferentes condiciones
- [ ] Asignar todas las entradas al DiaryManager
- [ ] Integrar llamada a `OnLevelCompleted()` al finalizar niveles
- [ ] Probar desbloqueo de entradas
- [ ] Verificar guardado/carga con PlayerPrefs
- [ ] Ajustar estilos de UI según diseño del juego

## Notas Importantes

- Las entradas NO se pueden modificar después de desbloquearse (inmutables)
- Solo se desbloquea UNA entrada por nivel
- El sistema elige la primera condición que coincida
- Los textos soportan múltiples líneas (TextArea)
- El scroll automático permite textos muy largos
- El guardado es automático al desbloquear una entrada
