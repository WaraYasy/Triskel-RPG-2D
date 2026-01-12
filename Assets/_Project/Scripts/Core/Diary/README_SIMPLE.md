# Sistema de Diario - Versión Simplificada

## Estructura del JSON

El diario usa un JSON simple en `Assets/_Project/Resources/DiaryData/DiaryEntries.json`:

```json
{
  "entries": [
    {
      "id": "level0_bueno",
      "level": 0,
      "title": "Título de la entrada",
      "text": "Texto de la entrada del diario"
    }
  ]
}
```

### Convención de IDs
El ID debe seguir el formato: `level{X}_{decision}`

Ejemplos:
- `level0_bueno` - Nivel 0, decisión buena
- `level1_malo` - Nivel 1, decisión mala
- `level2_neutral` - Nivel 2, decisión neutral

## Cómo Usar

### 1. Desbloquear una entrada desde tu código

```csharp
// Cuando el jugador complete un nivel con una decisión:
DiaryManager.Instance.UnlockEntry(nivel, decision);

// Ejemplos:
DiaryManager.Instance.UnlockEntry(0, "bueno");  // Desbloquea level0_bueno
DiaryManager.Instance.UnlockEntry(1, "malo");   // Desbloquea level1_malo
```

### 2. Verificar si una entrada está desbloqueada

```csharp
DiaryEntry entry = DiaryManager.Instance.GetEntry("level0_bueno");
if (entry != null)
{
    Debug.Log(entry.title);
    Debug.Log(entry.text);
}
```

### 3. Obtener todas las entradas desbloqueadas

```csharp
List<DiaryEntry> unlocked = DiaryManager.Instance.GetUnlockedEntries();
foreach (var entry in unlocked)
{
    Debug.Log($"Nivel {entry.level}: {entry.title}");
}
```

## Pruebas

1. Abre la escena `Diario.unity`
2. Dale Play
3. Usa las teclas:
   - **1** = Nivel 0 Bueno
   - **2** = Nivel 0 Malo
   - **3** = Nivel 1 Bueno
   - **4** = Nivel 1 Malo
   - **5** = Nivel 2 Bueno
   - **6** = Nivel 2 Malo
   - **J** = Abrir/Cerrar Diario
   - **0** = Limpiar todo

## Añadir Nuevas Entradas

1. Abre `DiaryEntries.json`
2. Añade una nueva entrada:

```json
{
  "id": "level3_heroico",
  "level": 3,
  "title": "Un Acto Heroico",
  "text": "Hoy hice algo heroico..."
}
```

3. Guarda el archivo
4. Desde tu código:

```csharp
DiaryManager.Instance.UnlockEntry(3, "heroico");
```

¡Listo!
