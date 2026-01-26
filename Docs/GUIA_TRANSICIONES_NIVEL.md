# Guía del Sistema de Transiciones de Nivel

## Descripción General

El sistema de transiciones de nivel muestra textos narrativos entre niveles, proporcionando contexto sobre las decisiones del jugador y feedback cuando muere. Utiliza UI Toolkit para la interfaz y JSON para almacenar los textos.

---

## Casos de Uso

### 1. Completar un Nivel
Cuando el jugador termina un nivel y vuelve al hub, se muestra un texto que refleja sus decisiones (moral alta/baja).

**Ejemplo:**
```
Nivel 1 completado (moral alta) → Texto: "La luz que compartiste iluminó el camino..."
Nivel 1 completado (moral baja) → Texto: "Las sombras recuerdan tu paso..."
```

### 2. Muerte del Jugador
Cuando el jugador muere, se muestra un texto contextual antes de reiniciar el nivel.

**Ejemplo:**
```
Muerte en Nivel 1 → Texto: "La oscuridad te ha consumido. Pero las sombras no pueden retener tu luz para siempre..."
```

---

## Arquitectura del Sistema

```
┌─────────────────────────────────────────────────┐
│         GameManager (Orquestador)               │
│  - IrATransicion(nivel, escena)                 │
│  - OnJugadorMuerto()                            │
└────────────┬────────────────────────────────────┘
             │
             ├─ Determina ID de transición
             │  (según moral/decisiones o muerte)
             │
             ↓
┌─────────────────────────────────────────────────┐
│     Escena: LevelTransition.unity               │
│  ┌───────────────────────────────────┐          │
│  │   TransitionManager                │          │
│  │   - Lee TransitionID estático      │          │
│  │   - Carga texto desde JSON         │          │
│  │   - Ejecuta secuencia visual       │          │
│  └───────────────────────────────────┘          │
│                                                  │
│  ┌───────────────────────────────────┐          │
│  │   UI Toolkit (UXML/USS)            │          │
│  │   - Fondo negro                    │          │
│  │   - Texto centrado                 │          │
│  │   - Animaciones de fade            │          │
│  └───────────────────────────────────┘          │
└─────────────────────────────────────────────────┘
             │
             ↓
        Carga escena destino
     (Hub o reinicio de nivel)
```

---

## Estructura de Archivos

```
Assets/_Project/
├── Scripts/Core/Transition/
│   ├── TransitionDataStructure.cs    # Clases para JSON
│   └── TransitionManager.cs          # Lógica de transición
│
├── Resources/TransitionData/
│   └── TransitionTexts.json          # Textos de transiciones
│
├── UI/
│   ├── LevelTransition.uxml          # Estructura UI
│   ├── LevelTransition.uss           # Estilos CSS-like
│   └── TransitionPanelSettings.asset # Configuración UI
│
└── Scenes/UI/
    └── LevelTransition.unity         # Escena de transición
```

---

## Formato del JSON

**Ubicación:** `Assets/_Project/Resources/TransitionData/TransitionTexts.json`

```json
{
  "transitions": [
    {
      "id": "nivel1_bueno",
      "level": 1,
      "text": "La estatua les dio calor. Las sombras se apartaron.",
      "displayTime": 0
    },
    {
      "id": "nivel1_malo",
      "level": 1,
      "text": "Gritaron cuando alcé la flor. Eran ellos o yo.",
      "displayTime": 0
    },
    {
      "id": "muerte_nivel1",
      "level": 1,
      "text": "La oscuridad te ha consumido. Pero las sombras no pueden retener tu luz para siempre...",
      "displayTime": 4
    }
  ]
}
```

### Campos:
- **id** (string): Identificador único (ej: `nivel1_bueno`, `muerte_nivel2`)
- **level** (int): Número de nivel (0 = hub, 1-4 = niveles)
- **text** (string): Texto a mostrar (soporte multilínea)
- **displayTime** (float): Tiempo en pantalla (0 = usa el valor por defecto de 4 segundos)

---

## Uso desde Código

### 1. Completar un Nivel

En tu script de victoria/final de nivel:

```csharp
// Cuando el jugador completa el nivel
GameManager.Instance.IrATransicion(1, "MainHub");
```

El sistema automáticamente:
1. Determina si la moral es buena/mala
2. Elige el texto correspondiente (`nivel1_bueno` o `nivel1_malo`)
3. Muestra la transición
4. Carga el hub

### 2. Muerte del Jugador

**Ya está implementado automáticamente** en `PlayerHealth.cs`:

```csharp
private void Die()
{
    isDead = true;
    Debug.Log("[PlayerHealth] El jugador ha muerto.");

    if (GameManager.Instance != null)
    {
        GameManager.Instance.OnJugadorMuerto(); // ← Automático
    }
}
```

El sistema automáticamente:
1. Detecta en qué escena murió
2. Elige el texto de muerte correspondiente
3. Muestra la transición
4. Reinicia el nivel

---

## Lógica de Selección de Textos

### Para Completar Niveles

**Método:** `GameManager.ObtenerIDTransicion(int nivel)`

```csharp
private string ObtenerIDTransicion(int nivel)
{
    // Basado en la moral del jugador
    string sufijo = moralScore >= 50 ? "bueno" : "malo";
    return $"nivel{nivel}_{sufijo}";
}
```

**Resultado:**
- Moral ≥ 50 → `nivel1_bueno`, `nivel2_bueno`, etc.
- Moral < 50 → `nivel1_malo`, `nivel2_malo`, etc.

### Para Muerte

**Método:** `GameManager.ObtenerIDMuerte(string nombreEscena)`

```csharp
private string ObtenerIDMuerte(string nombreEscena)
{
    if (nombreEscena.Contains("Cuadrante1"))
        return "muerte_nivel1";
    else if (nombreEscena.Contains("Cuadrante2"))
        return "muerte_nivel2";
    else if (nombreEscena.Contains("Cuadrante3"))
        return "muerte_nivel3";
    else if (nombreEscena.Contains("Cuadrante4"))
        return "muerte_nivel4";
    else if (nombreEscena.Contains("Hub"))
        return "muerte_hub";
    else
        return "muerte_hub"; // Default
}
```

---

## Secuencia de Transición

```
1. Fade In (1 segundo)
   ↓
2. Mostrar Texto (4 segundos por defecto)
   ↓
3. Fade Out (1 segundo)
   ↓
4. Cargar Escena Destino
```

### Configuración de Tiempos

En `TransitionManager` (Inspector):
- **Tiempo Fade In**: 1 segundo
- **Tiempo Texto Por Defecto**: 4 segundos
- **Tiempo Fade Out**: 1 segundo

O en el JSON por transición individual:
```json
{
  "id": "muerte_nivel1",
  "displayTime": 5  // Sobrescribe el tiempo por defecto
}
```

---

## Agregar Nuevas Transiciones

### Paso 1: Editar el JSON

Abre `TransitionTexts.json` y agrega una nueva entrada:

```json
{
  "id": "nivel5_bueno",
  "level": 5,
  "text": "Tu nuevo texto aquí...",
  "displayTime": 0
}
```

### Paso 2: Listo

No necesitas recompilar ni modificar código. El sistema cargará automáticamente el nuevo texto.

---

## Personalización de Estilos

### Cambiar Tamaño de Fuente

Edita `LevelTransition.uss`:

```css
#transition-text {
    font-size: 42px;  /* Cambiar de 36px a 42px */
}
```

### Cambiar Color de Fondo

Edita `LevelTransition.uss`:

```css
#background {
    background-color: rgb(20, 20, 30);  /* Azul oscuro en vez de negro */
}
```

### Cambiar Padding del Texto

Edita `LevelTransition.uss`:

```css
#transition-text {
    padding: 80px;  /* Más espacio alrededor del texto */
}
```

---

## Testing en el Editor

### Probar una Transición Específica

1. Abre `TransitionManager.cs`
2. Agrega código temporal en `Start()`:

```csharp
private void Start()
{
    // TESTING - Borrar después
    if (string.IsNullOrEmpty(TransitionID))
    {
        TransitionID = "muerte_nivel1";
        SiguienteEscena = "MainHub";
    }
    // FIN TESTING

    // ... resto del código
}
```

3. Abre la escena `LevelTransition.unity`
4. Presiona Play
5. Verás la transición con el texto especificado

### Saltar Transición

Durante Play Mode, presiona **SPACE** para saltar inmediatamente a la siguiente escena.

---

## Integración con Otros Sistemas

### Con el Sistema de Diario

El sistema de transiciones usa el mismo patrón que el diario:
- **Datos en JSON** (fácil de editar)
- **Carga desde Resources**
- **Basado en decisiones del jugador**

Puedes usar las mismas decisiones que desbloquean entradas del diario para determinar los textos de transición.

### Con el Sistema de Moral

El `GameManager.moralScore` determina qué texto de completar nivel se muestra:
- Moral ≥ 50 → Textos "buenos"
- Moral < 50 → Textos "malos"

---

## Solución de Problemas

### El texto no aparece

**Causa:** El ID no existe en el JSON

**Solución:**
1. Verifica que el ID en el JSON coincida exactamente
2. Revisa la consola para ver qué ID está buscando
3. Asegúrate de que el JSON esté en `Resources/TransitionData/`

### La escena no carga después de la transición

**Causa:** El nombre de la escena es incorrecto

**Solución:**
1. Verifica que la escena destino exista en Build Settings
2. Revisa que el nombre coincida exactamente (case-sensitive)

### Error de Input System

**Causa:** Usando `Input` antiguo en vez del nuevo Input System

**Solución:**
Ya corregido. El código usa `Keyboard.current.spaceKey.wasPressedThisFrame`

### JSON no se carga

**Causa:** Archivo no está en la carpeta correcta

**Solución:**
El archivo DEBE estar en: `Assets/_Project/Resources/TransitionData/TransitionTexts.json`

---

## Extensiones Futuras

### Agregar Imágenes de Fondo

Modifica el UXML para incluir un `<ui:VisualElement>` con background-image.

### Soporte Multi-idioma

Crea JSONs separados por idioma:
- `TransitionTexts_ES.json`
- `TransitionTexts_EN.json`

Y carga el correcto según configuración.

### Efectos de Sonido

Agrega un `AudioSource` al `TransitionManager` y reproduce sonidos según el tipo de transición.

### Animaciones Personalizadas

Usa las transiciones USS de UI Toolkit para efectos más complejos (blur, slide, etc.).

---

## Referencias Relacionadas

- **GUIA_SISTEMAS_NUEVOS.md** - Sistema de diario (patrón similar)
- **GameManager.cs** - Orquestador central
- **PlayerHealth.cs** - Integración con muerte
- **TransitionManager.cs** - Lógica de transición
- **TransitionTexts.json** - Base de datos de textos
