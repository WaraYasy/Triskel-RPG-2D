# Sistema de Diario Narrativo - Resumen de Implementación

## Archivos Creados

### Scripts Core (Assets/_Project/Scripts/Core/Diary/)
1. **DiaryEntryData.cs**
   - ScriptableObject para definir entradas del diario
   - Campos: entryID, levelIndex, conditionID, title, text

2. **DiaryPersistence.cs**
   - Singleton para guardar/cargar entradas desbloqueadas
   - Usa PlayerPrefs (guardado local)
   - Preparado para integración con API remota

3. **DiaryManager.cs**
   - Singleton con lógica central del sistema
   - Gestiona desbloqueo de entradas según nivel y condiciones
   - Expone métodos para consultar entradas desbloqueadas

4. **DiaryIntegrationExample.cs**
   - Script de ejemplo mostrando cómo integrar el sistema
   - Incluye ejemplos de diferentes escenarios
   - Context menus para testing

5. **README.md**
   - Documentación completa del sistema
   - Instrucciones de setup
   - Ejemplos de uso

### Scripts UI (Assets/_Project/Scripts/UI/HUD/)
6. **DiaryUI.cs**
   - Controlador de la interfaz del diario (UI Toolkit)
   - Muestra lista de entradas y texto completo
   - Toggle con tecla (por defecto: J)

### Assets UI Toolkit (Assets/_Project/UI/Diary/)
7. **DiaryUI.uxml**
   - Estructura de la interfaz del diario
   - Panel dividido: lista de entradas + visualización de texto

8. **DiaryUI.uss**
   - Estilos de la interfaz
   - Tema oscuro con bordes dorados
   - Scrollviews para contenido largo

## Flujo de Trabajo Completo

### 1. Setup Inicial (Una vez)
```
1. Crear GameObject "DiaryManager" en la escena
   - Añadir DiaryManager component
   - Añadir DiaryPersistence component

2. Crear GameObject "DiaryUI" en la escena
   - Añadir UIDocument component
   - Añadir DiaryUI component
   - Asignar DiaryUI.uxml al UIDocument
```

### 2. Crear Entradas del Diario (Por cada entrada)
```
1. Click derecho en Project → Create → Triskel/Diary Entry
2. Configurar campos:
   - entryID: "level0_saved_child"
   - levelIndex: 0
   - conditionID: "saved_child"
   - title: "El niño perdido"
   - text: "Hoy salvé a un niño..."
3. Arrastrar asset al array "All Entries" del DiaryManager
```

### 3. Integración con Niveles (Código)
```csharp
using Triskel.Core;

// Durante el nivel: registrar decisiones
List<string> conditions = new List<string>();
if (playerSavedChild) conditions.Add("saved_child");

// Al finalizar nivel: notificar al diario
DiaryManager.Instance.OnLevelCompleted(currentLevelIndex, conditions.ToArray());
```

## API Principal

### DiaryManager
```csharp
// Notificar nivel completado
DiaryManager.Instance.OnLevelCompleted(int levelIndex, string[] conditionIDs);

// Desbloquear entrada manualmente
DiaryManager.Instance.UnlockEntry(string entryID);

// Verificar si está desbloqueada
bool isUnlocked = DiaryManager.Instance.IsEntryUnlocked(string entryID);

// Obtener datos de entrada
DiaryEntryData entry = DiaryManager.Instance.GetEntryData(string entryID);

// Obtener todas las entradas desbloqueadas
List<DiaryEntryData> entries = DiaryManager.Instance.GetUnlockedEntries();

// Limpiar todas las entradas
DiaryManager.Instance.ClearAllEntries();
```

### DiaryUI
```csharp
// Abrir panel
DiaryUI.OpenPanel();

// Cerrar panel
DiaryUI.ClosePanel();

// Toggle panel
DiaryUI.TogglePanel();

// Actualizar UI
DiaryUI.UpdateUI();
```

### DiaryPersistence
```csharp
// Guardar entradas
DiaryPersistence.Instance.SaveUnlockedEntries(string[] entryIDs);

// Cargar entradas
string[] entries = DiaryPersistence.Instance.LoadUnlockedEntries();

// Limpiar datos guardados
DiaryPersistence.Instance.ClearSavedData();
```

## Estructura de Datos Recomendada

### Ejemplo de Entradas por Nivel

**Nivel 0 - Decisión A**
- entryID: `level0_saved_child`
- levelIndex: `0`
- conditionID: `saved_child`
- title: `El niño perdido`

**Nivel 0 - Decisión B**
- entryID: `level0_ignored_child`
- levelIndex: `0`
- conditionID: `ignored_child`
- title: `El bosque silencioso`

**Nivel 1 - Boss Derrotado**
- entryID: `level1_defeated_boss`
- levelIndex: `1`
- conditionID: `defeated_boss`
- title: `Victoria contra las sombras`

**Nivel 1 - Huyó del Boss**
- entryID: `level1_fled_boss`
- levelIndex: `1`
- conditionID: `fled_boss`
- title: `La retirada estratégica`

## Testing y Debug

### Context Menus Disponibles

**DiaryManager (Inspector)**
- Debug: Desbloquear Entrada de Prueba
- Debug: Mostrar Entradas Desbloqueadas
- Debug: Limpiar Todas las Entradas
- Debug: Simular Nivel Completado

**DiaryPersistence (Inspector)**
- Debug: Guardar Diario
- Debug: Cargar Diario
- Debug: Limpiar Datos Guardados

**DiaryIntegrationExample (Inspector)**
- Debug: Simular Nivel - Salvar NPC
- Debug: Simular Nivel - Ignorar NPC
- Debug: Simular Nivel - Múltiples Decisiones
- Debug: Simular Nivel - Solo Completado
- Debug: Resetear Nivel Actual

### Comandos de Consola
```csharp
// Desbloquear entrada de prueba
DiaryManager.Instance.DebugUnlockTestEntry();

// Ver entradas desbloqueadas
DiaryManager.Instance.DebugShowUnlockedEntries();

// Limpiar todo
DiaryManager.Instance.DebugClearEntries();
```

## Checklist de Implementación

### Setup
- [ ] Crear GameObject DiaryManager con componentes necesarios
- [ ] Crear GameObject DiaryUI con UIDocument
- [ ] Asignar DiaryUI.uxml al UIDocument
- [ ] Configurar tecla de toggle en DiaryUI (por defecto: J)

### Contenido
- [ ] Crear al menos 2 DiaryEntryData por nivel
- [ ] Configurar entryID, levelIndex, conditionID para cada entrada
- [ ] Escribir título y texto para cada entrada
- [ ] Asignar todas las entradas al array "All Entries" del DiaryManager

### Integración
- [ ] Identificar dónde se completan los niveles en tu código
- [ ] Añadir sistema de tracking de decisiones del jugador
- [ ] Llamar a `DiaryManager.Instance.OnLevelCompleted()` al finalizar nivel
- [ ] Probar desbloqueo con diferentes decisiones

### Testing
- [ ] Probar abrir/cerrar panel del diario
- [ ] Verificar que las entradas se desbloquean correctamente
- [ ] Probar guardado/carga (cerrar y reabrir Unity)
- [ ] Verificar scroll en listas largas
- [ ] Verificar texto largo con scroll

### Opcionales
- [ ] Ajustar estilos de UI en DiaryUI.uss
- [ ] Implementar guardado remoto con TriskelAPIClient
- [ ] Añadir animaciones de entrada/salida del panel
- [ ] Añadir sonidos de UI (abrir diario, seleccionar entrada)

## Notas Importantes

1. **Inmutabilidad**: Las entradas NO cambian después de desbloquearse
2. **Una entrada por nivel**: Solo se desbloquea UNA entrada por nivel completado
3. **Prioridad de condiciones**: El sistema elige la primera condición que coincida
4. **Guardado automático**: Las entradas se guardan automáticamente al desbloquearse
5. **Singleton**: DiaryManager y DiaryPersistence persisten entre escenas (DontDestroyOnLoad)

## Próximos Pasos Recomendados

1. Crear 3-5 entradas de prueba para el primer nivel
2. Probar el sistema completo con el DiaryIntegrationExample
3. Integrar con tu controlador de nivel existente
4. Ajustar estilos de UI según tu estética
5. Crear todas las entradas para todos los niveles
6. Implementar guardado remoto (opcional)

## Soporte y Debugging

Si encuentras problemas:
1. Verifica que DiaryManager y DiaryPersistence están en la escena
2. Revisa que las entradas tienen entryID únicos
3. Verifica que levelIndex coincide con tu sistema de niveles
4. Usa los Context Menus para testing rápido
5. Revisa la consola para logs y errores

## Personalización

### Cambiar tecla de toggle
```csharp
// En DiaryUI Inspector
Toggle Key: KeyCode.J (cambiar según necesites)
```

### Cambiar colores de UI
```css
/* En DiaryUI.uss */
.diary-panel {
    background-color: rgba(20, 20, 25, 0.95); /* Cambiar color de fondo */
    border-color: rgb(200, 180, 140); /* Cambiar color de borde */
}
```

### Añadir más campos a DiaryEntryData
```csharp
// En DiaryEntryData.cs
[Header("Extras")]
public Sprite entryImage; // Imagen de la entrada
public AudioClip entrySound; // Sonido al abrir
public string entryDate; // Fecha narrativa
```

---

**Sistema creado para:** Triskel (Unity C#)
**Arquitectura:** Modular, separación de responsabilidades, escalable
**Compatibilidad:** Unity 2021.3+, UI Toolkit
