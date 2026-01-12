# 📖 Sistema de Diario - Guía de Uso

## 🎯 ¿Qué es esto?

Este sistema te permite gestionar todas las entradas del diario de tu juego desde **un único archivo JSON**. Cada nivel puede tener múltiples entradas según las decisiones del jugador.

---

## 📁 Estructura del JSON

El archivo `DiaryEntries.json` tiene esta estructura simple:

```json
{
  "levels": [
    {
      "levelIndex": 0,
      "levelName": "Nivel 1 - Descripción",
      "entries": [
        {
          "entryID": "level1_good",
          "conditionID": "helped_someone",
          "title": "Título de la entrada",
          "text": "Texto completo de la entrada..."
        },
        {
          "entryID": "level1_bad",
          "conditionID": "ignored_someone",
          "title": "Otro título",
          "text": "Otro texto..."
        }
      ]
    }
  ]
}
```

---

## 📝 Campos Explicados

### Nivel (DiaryLevel)

| Campo | Descripción | Ejemplo |
|-------|-------------|---------|
| `levelIndex` | Número del nivel (empieza en 0) | `0` = Nivel 1, `1` = Nivel 2 |
| `levelName` | Nombre descriptivo (solo para ti) | `"Nivel 1 - El Bosque"` |
| `entries` | Lista de entradas posibles | (ver abajo) |

### Entrada (DiaryEntry)

| Campo | Descripción | Ejemplo |
|-------|-------------|---------|
| `entryID` | ID único de la entrada | `"level1_saved_child"` |
| `conditionID` | ID de la decisión que activa esta entrada | `"saved_child"` |
| `title` | Título que aparece en la lista del diario | `"Héroe del Bosque"` |
| `text` | Texto completo de la entrada (puede ser largo) | `"Hoy salvé a un niño..."` |

---

## ✏️ Cómo Añadir una Nueva Entrada

### Paso 1: Abre el archivo JSON
Navega a `Assets/_Project/Resources/DiaryData/DiaryEntries.json`

### Paso 2: Añade tu entrada
Copia este template dentro de `"entries": []` del nivel correspondiente:

```json
{
  "entryID": "level2_tu_decision",
  "conditionID": "tu_condicion",
  "title": "Tu Título Aquí",
  "text": "Tu texto aquí. Puede ser muy largo y tener múltiples líneas.\n\nUsa \\n para saltos de línea."
}
```

### Paso 3: Rellena los campos
- **entryID**: Nombre único (ej: `level2_helped_elder`)
- **conditionID**: Nombre de la condición que activa esta entrada (ej: `helped_elder`)
- **title**: Título corto (ej: `"Bondad Inesperada"`)
- **text**: El texto completo de la entrada

### Paso 4: Guarda el archivo
Unity detectará automáticamente los cambios.

---

## 🎮 Cómo se Desbloquean las Entradas

Cuando un nivel termina, tu código de juego debe llamar a:

```csharp
DiaryManager.Instance.OnLevelCompleted(levelIndex, conditionIDs);
```

**Ejemplo:**
```csharp
// El jugador completó el nivel 1 y salvó al niño
string[] decisiones = new string[] { "saved_child" };
DiaryManager.Instance.OnLevelCompleted(0, decisiones);
```

El sistema automáticamente:
1. Busca el nivel 0 (Nivel 1)
2. Busca una entrada con `conditionID = "saved_child"`
3. Desbloquea esa entrada
4. La guarda para que aparezca en el diario

---

## 🔍 IDs de Condiciones

Los `conditionID` son textos que defines tú. Deben coincidir entre:
- El JSON (campo `conditionID`)
- Tu código de juego (lo que pasas a `OnLevelCompleted`)

**Recomendaciones:**
- Usa nombres descriptivos: `"helped_elder"` ✅ (no `"h1"` ❌)
- Usa snake_case: `"saved_child"` ✅ (no `"SavedChild"` ❌)
- Sé consistente

---

## ⚠️ Errores Comunes

### "No se encontró el archivo JSON"
- Verifica que el archivo esté en `Assets/_Project/Resources/DiaryData/`
- El nombre debe ser exactamente `DiaryEntries.json`

### "Error al deserializar el JSON"
- Verifica que el JSON tenga la sintaxis correcta
- Usa un validador online: https://jsonlint.com/

### "No se encontró entrada para nivel X"
- Verifica que el `levelIndex` en el JSON coincida con el nivel
- Verifica que el `conditionID` coincida con lo que envías desde el código

---

## 🧪 Testing

### Desbloquear una entrada de prueba
En Unity, selecciona el GameObject con `DiaryManager` → Click derecho en el Inspector → **Debug: Desbloquear Entrada de Prueba**

### Ver todas las entradas desbloqueadas
Click derecho → **Debug: Mostrar Entradas Desbloqueadas**

### Limpiar todas las entradas
Click derecho → **Debug: Limpiar Todas las Entradas**

---

## 📊 Ejemplo Completo

```json
{
  "levels": [
    {
      "levelIndex": 0,
      "levelName": "Nivel 1 - El Bosque",
      "entries": [
        {
          "entryID": "level1_saved_child",
          "conditionID": "saved_child",
          "title": "Héroe del Bosque",
          "text": "Hoy salvé a un niño perdido. Sus padres me agradecieron con lágrimas en los ojos."
        },
        {
          "entryID": "level1_ignored_child",
          "conditionID": "ignored_child",
          "title": "El Precio de la Indiferencia",
          "text": "Pasé junto a un niño llorando, pero no tenía tiempo. A veces me pregunto qué le habrá pasado."
        }
      ]
    }
  ]
}
```

---

## 💡 Consejos

1. **Usa saltos de línea** con `\n` para mejorar la legibilidad
2. **Agrupa entradas por nivel** para facilitar la edición
3. **Usa nombres descriptivos** para los IDs
4. **Escribe primero todos los títulos** para ver la estructura general
5. **Luego rellena los textos** largos

---

## 🔧 Configuración en Unity

1. Selecciona el GameObject con `DiaryManager`
2. En el Inspector, verás:
   - **JSON File Name**: `DiaryEntries` (no cambies esto a menos que cambies el nombre del archivo)
   - **All Entries**: Déjalo vacío si usas JSON

¡Listo! El sistema cargará automáticamente desde el JSON.

---

¿Necesitas ayuda? Revisa los logs de Unity para ver mensajes de error detallados.
