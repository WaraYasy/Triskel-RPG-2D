# Guía de Pruebas - Sistema de Diario Narrativo

## Setup Inicial (Paso a Paso)

### Paso 1: Crear los GameObjects en la Escena

1. **Crear DiaryManager:**
   ```
   - Hierarchy → Click derecho → Create Empty
   - Nombre: "DiaryManager"
   - Add Component → DiaryManager
   - Add Component → DiaryPersistence
   ```

2. **Crear DiaryUI:**
   ```
   - Hierarchy → Click derecho → UI Toolkit → UI Document
   - Nombre: "DiaryUI"
   - En el Inspector:
     - UIDocument component → Source Asset → Seleccionar "DiaryUI.uxml"
   - Add Component → DiaryUI
   - En DiaryUI component:
     - Toggle Key: J (o la tecla que prefieras)
   ```

### Paso 2: Crear Entradas de Prueba

1. **Crear primera entrada:**
   ```
   Project → Click derecho en carpeta Assets/_Project/Data (o donde prefieras)
   → Create → Triskel/Diary Entry

   Nombre del archivo: "Level0_SavedChild"

   En el Inspector:
   - Entry ID: "level0_saved_child" (se auto-genera)
   - Level Index: 0
   - Condition ID: "saved_child"
   - Title: "El niño perdido"
   - Text: "Hoy encontré a un niño llorando en el bosque oscuro.
           Sus ojos reflejaban miedo pero también esperanza.
           Decidí ayudarlo a encontrar su camino de vuelta a casa.
           Su sonrisa al ver a su familia... eso hizo que todo valiera la pena."
   ```

2. **Crear segunda entrada (decisión alternativa):**
   ```
   Crear otro Diary Entry
   Nombre del archivo: "Level0_IgnoredChild"

   En el Inspector:
   - Entry ID: "level0_ignored_child"
   - Level Index: 0
   - Condition ID: "ignored_child"
   - Title: "El bosque silencioso"
   - Text: "Escuché un llanto lejano en el bosque, pero el tiempo apremia.
           Mi misión es más importante que las distracciones del camino.
           A veces debemos hacer sacrificios difíciles por el bien mayor.
           O al menos eso me digo a mí mismo para poder dormir esta noche."
   ```

3. **Crear tercera entrada (nivel 1):**
   ```
   Crear otro Diary Entry
   Nombre del archivo: "Level1_DefeatedBoss"

   En el Inspector:
   - Entry ID: "level1_defeated_boss"
   - Level Index: 1
   - Condition ID: "defeated_boss"
   - Title: "Victoria contra las sombras"
   - Text: "La batalla fue intensa. La criatura de las sombras casi me vence,
           pero recordé por qué estoy aquí. Cada golpe, cada esquiva,
           cada momento de duda... todo culminó en esta victoria.
           Las sombras se disiparon, pero sé que esto es solo el comienzo."
   ```

4. **Asignar entradas al DiaryManager:**
   ```
   - Selecciona el GameObject "DiaryManager" en Hierarchy
   - En el Inspector, componente DiaryManager
   - En el array "All Entries":
     - Size: 3
     - Element 0: Arrastrar "Level0_SavedChild"
     - Element 1: Arrastrar "Level0_IgnoredChild"
     - Element 2: Arrastrar "Level1_DefeatedBoss"
   ```

### Paso 3: Crear GameObject de Prueba (Opcional)

```
- Hierarchy → Create Empty
- Nombre: "DiaryTester"
- Add Component → DiaryIntegrationExample
```

## Métodos de Prueba

### Método 1: Context Menus (MÁS RÁPIDO)

1. **Selecciona DiaryManager en Hierarchy**
2. **Click derecho en el componente DiaryManager (en Inspector)**
3. **Verás estos menús:**
   ```
   Debug: Desbloquear Entrada de Prueba   → Desbloquea la primera entrada
   Debug: Mostrar Entradas Desbloqueadas  → Muestra en consola qué está desbloqueado
   Debug: Limpiar Todas las Entradas      → Resetea todo
   Debug: Simular Nivel Completado        → Simula completar un nivel
   ```

4. **Probar:**
   ```
   a) Click en "Debug: Desbloquear Entrada de Prueba"
   b) Entrar en Play Mode
   c) Presionar tecla J
   d) Ver la entrada desbloqueada en el diario
   ```

### Método 2: Usando DiaryIntegrationExample

1. **Selecciona el GameObject "DiaryTester" en Hierarchy**
2. **En el Inspector, componente DiaryIntegrationExample**
3. **Verás estos Context Menus:**
   ```
   Debug: Simular Nivel - Salvar NPC          → Completa nivel con decisión "salvar"
   Debug: Simular Nivel - Ignorar NPC         → Completa nivel con decisión "ignorar"
   Debug: Simular Nivel - Múltiples Decisiones → Completa nivel con varias condiciones
   Debug: Simular Nivel - Solo Completado     → Completa nivel sin decisiones específicas
   Debug: Resetear Nivel Actual               → Vuelve al nivel 0
   ```

4. **Flujo de prueba completo:**
   ```
   a) Asegúrate de estar fuera de Play Mode
   b) Click en "Debug: Simular Nivel - Salvar NPC"
   c) Entrar en Play Mode
   d) Presionar J para abrir el diario
   e) Verificar que aparece "El niño perdido"
   f) Salir de Play Mode
   g) Click en "Debug: Resetear Nivel Actual"
   h) Click en "Debug: Simular Nivel - Ignorar NPC"
   i) Entrar en Play Mode
   j) Presionar J
   k) Verificar que aparece "El bosque silencioso"
   ```

### Método 3: Código Manual en Play Mode

1. **Crear script de prueba temporal:**

```csharp
using UnityEngine;
using Triskel.Core;

public class QuickDiaryTest : MonoBehaviour
{
    void Update()
    {
        // Presiona 1 para desbloquear entrada de nivel 0 (salvar)
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            DiaryManager.Instance.OnLevelCompleted(0, new string[] { "saved_child" });
            Debug.Log("Desbloqueada: saved_child");
        }

        // Presiona 2 para desbloquear entrada de nivel 0 (ignorar)
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            DiaryManager.Instance.OnLevelCompleted(0, new string[] { "ignored_child" });
            Debug.Log("Desbloqueada: ignored_child");
        }

        // Presiona 3 para desbloquear entrada de nivel 1
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            DiaryManager.Instance.OnLevelCompleted(1, new string[] { "defeated_boss" });
            Debug.Log("Desbloqueada: defeated_boss");
        }

        // Presiona 0 para limpiar todo
        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            DiaryManager.Instance.ClearAllEntries();
            Debug.Log("Diario limpiado");
        }
    }
}
```

2. **Usar:**
   ```
   - Añade este script a cualquier GameObject en la escena
   - Enter Play Mode
   - Presiona 1 → Abre diario (J) → Ver entrada "El niño perdido"
   - Presiona 0 → Limpiar
   - Presiona 2 → Abre diario (J) → Ver entrada "El bosque silencioso"
   ```

## Cómo Escribir el Texto de las Entradas

### Opción 1: Directamente en el Inspector (Recomendado para textos cortos)

```
1. Selecciona el DiaryEntryData asset en Project
2. En el Inspector, campo "Text"
3. Escribe directamente (se auto-ajusta el tamaño del campo)
```

**Ventaja:** Rápido para textos cortos
**Desventaja:** Difícil de editar textos largos

### Opción 2: Editor Externo + Copy/Paste (Recomendado para textos largos)

```
1. Abre tu editor de texto favorito (Notepad++, VS Code, Word, etc.)
2. Escribe el texto narrativo completo
3. Copia el texto (Ctrl+C)
4. En Unity, selecciona el DiaryEntryData asset
5. En el Inspector, campo "Text"
6. Pega el texto (Ctrl+V)
```

**Ejemplo de texto largo:**
```
La noche caía sobre las montañas cuando finalmente encontré refugio.

Mis pies dolían por la larga caminata, pero mi corazón estaba ligero.
La decisión que tomé hoy cambiará todo. Lo sé.

El anciano me dijo: "No todos los caminos llevan a casa, joven viajero.
Algunos te llevan a convertirte en quien necesitas ser."

Esas palabras resuenan en mi mente mientras escribo estas líneas.
¿Quién necesito ser? ¿Quién quiero ser?

Mañana continuaré mi viaje. Por ahora, descanso.
```

### Opción 3: Archivo de Texto como Referencia

```
1. Crea un archivo de texto: "diary_entries.txt"
2. Organiza por secciones:

=== NIVEL 0 - SALVAR NIÑO ===
Título: El niño perdido
---
Hoy encontré a un niño llorando en el bosque oscuro...
(texto completo)

=== NIVEL 0 - IGNORAR NIÑO ===
Título: El bosque silencioso
---
Escuché un llanto lejano en el bosque...
(texto completo)

3. Copia/pega cada sección al DiaryEntryData correspondiente
```

### Opción 4: Script Custom Editor (Avanzado)

Si vas a tener MUCHAS entradas (50+), puedo crear un editor custom que:
- Importe textos desde archivos .txt
- Tenga un editor más cómodo
- Permita búsqueda y filtrado

¿Lo necesitas?

## Consejos para el Texto

### Formato del Texto

1. **Saltos de línea:**
   ```
   Unity respeta los saltos de línea (Enter).

   Puedes crear párrafos separados.

   O diálogos:
   - "¿Estás seguro?" preguntó.
   - "Completamente", respondí.
   ```

2. **Longitud recomendada:**
   ```
   - Mínimo: 100-200 palabras (1-2 párrafos)
   - Óptimo: 300-500 palabras (3-5 párrafos)
   - Máximo: Sin límite (el scroll funciona bien)
   ```

3. **Estilo narrativo:**
   ```
   Primera persona (recomendado):
   "Hoy encontré..."
   "Mi decisión fue..."
   "No puedo olvidar..."

   Tercera persona:
   "El viajero encontró..."
   "Su decisión fue..."
   ```

## Verificar que Todo Funciona

### Checklist de Prueba:

- [ ] **DiaryManager existe en la escena**
  - Verificar: Hierarchy → DiaryManager está presente

- [ ] **DiaryUI existe en la escena**
  - Verificar: Hierarchy → DiaryUI está presente

- [ ] **UIDocument tiene el UXML asignado**
  - Verificar: Seleccionar DiaryUI → Inspector → UIDocument → Source Asset debe mostrar "DiaryUI"

- [ ] **DiaryManager tiene entradas asignadas**
  - Verificar: DiaryManager → Inspector → All Entries → Size > 0

- [ ] **Las entradas tienen texto**
  - Verificar: Seleccionar DiaryEntryData en Project → Inspector → Text no está vacío

- [ ] **Desbloquear entrada funciona**
  - Acción: Context Menu → Debug: Desbloquear Entrada de Prueba
  - Verificar: Consola muestra "✓ Entrada desbloqueada: [ID]"

- [ ] **UI se abre con tecla**
  - Acción: Play Mode → Presionar J
  - Verificar: Panel del diario aparece

- [ ] **Entrada aparece en la lista**
  - Acción: Abrir diario
  - Verificar: Botón "Nivel 1: [Título]" aparece en lista izquierda

- [ ] **Texto se muestra completo**
  - Acción: Click en entrada de la lista
  - Verificar: Texto completo aparece en panel derecho

- [ ] **Scroll funciona con textos largos**
  - Acción: Crear entrada con 500+ palabras
  - Verificar: Scrollbar aparece y funciona

- [ ] **Guardado persiste entre sesiones**
  - Acción: Desbloquear entrada → Cerrar Unity → Abrir Unity → Play Mode
  - Verificar: Entrada sigue desbloqueada

- [ ] **Limpiar datos funciona**
  - Acción: Context Menu → Debug: Limpiar Todas las Entradas
  - Verificar: Consola muestra "Todas las entradas han sido limpiadas"
  - Verificar: Abrir diario → "No hay entradas desbloqueadas aún"

## Problemas Comunes y Soluciones

### Problema: "Panel no aparece al presionar J"

**Solución:**
```
1. Verificar que DiaryUI está en la escena
2. Verificar que UIDocument tiene DiaryUI.uxml asignado
3. Verificar consola por errores
4. Cambiar la tecla en Inspector → DiaryUI → Toggle Key
```

### Problema: "No hay entradas en la lista"

**Solución:**
```
1. Verificar que desbloqueaste al menos una entrada
2. DiaryManager → Context Menu → Debug: Mostrar Entradas Desbloqueadas
3. Si aparece en consola pero no en UI → Cerrar y reabrir el panel
```

### Problema: "El texto no se ve completo"

**Solución:**
```
1. Verificar que el texto está en el campo "Text" del DiaryEntryData
2. Click en la entrada en la lista
3. Verificar que el scroll funciona (arrastra con mouse)
```

### Problema: "Las entradas no se guardan"

**Solución:**
```
1. Verificar que DiaryPersistence está en la escena
2. DiaryPersistence → Context Menu → Debug: Guardar Diario
3. Verificar consola: debe mostrar "✓ Guardado LOCAL: [IDs]"
```

### Problema: "Texto con caracteres raros"

**Solución:**
```
1. Asegúrate de usar UTF-8 si copias desde archivo
2. Evita caracteres especiales no soportados
3. Unity soporta: á, é, í, ó, ú, ñ, ¿, ¡
```

## Ejemplo de Flujo Completo

```
1. Setup inicial (una vez):
   - Crear DiaryManager en escena
   - Crear DiaryUI en escena
   - Crear 3 DiaryEntryData assets
   - Asignar entradas al DiaryManager

2. Escribir contenido (por cada entrada):
   - Escribir texto en editor externo
   - Copiar/pegar al campo "Text" en Inspector

3. Probar:
   - Context Menu → Debug: Desbloquear Entrada de Prueba
   - Play Mode
   - Presionar J
   - Leer entrada
   - Cerrar diario (J o botón X)

4. Probar persistencia:
   - Desbloquear varias entradas
   - Salir de Play Mode
   - Volver a Play Mode
   - Verificar que siguen ahí

5. Limpiar y repetir:
   - Context Menu → Debug: Limpiar Todas las Entradas
   - Probar con diferentes decisiones
```

## Integración en tu Juego Real

Cuando estés listo para integrar en tu sistema de niveles:

```csharp
// En tu LevelController o GameManager:
using Triskel.Core;

public void OnLevelFinished()
{
    // Recopilar decisiones tomadas
    List<string> decisions = CollectPlayerDecisions();

    // Notificar al diario
    DiaryManager.Instance.OnLevelCompleted(currentLevel, decisions.ToArray());
}

private List<string> CollectPlayerDecisions()
{
    List<string> conditions = new List<string>();

    if (playerSavedNPC) conditions.Add("saved_npc");
    if (playerDefeatedBoss) conditions.Add("defeated_boss");
    if (playerFoundSecret) conditions.Add("found_secret");

    return conditions;
}
```

---

¿Necesitas ayuda con algún paso específico?
