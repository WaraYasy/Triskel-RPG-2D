# Guía - Crear Controles Móviles con Input System

## 🎯 **Objetivo**
Crear botones táctiles y joystick usando los componentes On-Screen del Input System

---

## ⚠️ **ANTES DE EMPEZAR:**

**Asegúrate de tener:**
- ✅ Input System instalado
- ✅ PlayerInputActions configurado y SIN ERRORES
- ✅ PlayerController y RelicSystem refactorizados
- ✅ Player tiene component **Player Input**

**Si hay errores de compilación, PRIMERO arregla esos.** Esta guía es para DESPUÉS.

---

## 📱 **Layout Final:**

```
┌────────────────────────────────────┐
│  ┌────┐                   ┌─────┐ │
│  │📖  │                   │ 🌸  │ │ Diary / Next Relic
│  └────┘                   └─────┘ │
│                                    │
│                           ┌─────┐ │
│                           │ ⚡  │ │ Use Relic
│  🕹️                       └─────┘ │
│  Move                              │
│                           ┌─────┐ │
│                           │DASH │ │ Dash
│                           └─────┘ │
└────────────────────────────────────┘
```

---

## 🎨 **Paso 1: Crear Canvas**

1. **Hierarchy → UI → Canvas**
2. **Nombre:** `MobileControls_UI`
3. **Canvas Scaler (Component):**
   ```
   UI Scale Mode: Scale With Screen Size
   Reference Resolution: 1920 x 1080
   Screen Match Mode: Match Width Or Height
   Match: 0.5
   ```

---

## 🕹️ **Paso 2: Crear Joystick Virtual**

1. **Click derecho en MobileControls_UI**
2. **UI → Input System → On-Screen Stick**
3. **Nombre:** `Joystick_Move`

### **Configurar RectTransform:**
```
Anchors: Bottom-Left (Alt+Shift+Click en cuadrado abajo-izquierda)
Pos X: 180
Pos Y: 180
Width: 200
Height: 200
```

### **Configurar On-Screen Stick Component:**
```
Control Path: <Gamepad>/leftStick
Movement Range: 50
```

### **Personalizar Visual (Opcional):**

**Hijo: Background**
- Image → Color: Blanco (255, 255, 255, 80)
- Sprite: Círculo (busca "Knob" en assets)

**Hijo: Handle**
- Image → Color: Blanco (255, 255, 255, 200)
- Width: 80, Height: 80

---

## 🔘 **Paso 3: Crear Botón DASH**

1. **Click derecho en MobileControls_UI**
2. **UI → Input System → On-Screen Button**
3. **Nombre:** `Button_Dash`

### **RectTransform:**
```
Anchors: Bottom-Right
Pos X: -180
Pos Y: 180
Width: 130
Height: 130
```

### **On-Screen Button Component:**
```
Control Path: <Gamepad>/buttonSouth
```

### **Visual:**
- **Image:** Color cyan (0, 255, 255, 150)
- **Añadir texto:**
  - Click derecho en Button_Dash → UI → Text - TextMeshPro
  - Text: "DASH"
  - Font Size: 28
  - Alignment: Center
  - Anchors: Stretch

---

## 🌸 **Paso 4: Crear Botón NEXT RELIC**

1. **Click derecho en MobileControls_UI**
2. **UI → Input System → On-Screen Button**
3. **Nombre:** `Button_NextRelic`

### **RectTransform:**
```
Anchors: Top-Right
Pos X: -180
Pos Y: -450
Width: 130
Height: 130
```

### **On-Screen Button Component:**
```
Control Path: <Gamepad>/buttonWest
```

### **Visual:**
- **Image:** Color según reliquia actual
- **Texto:** "NEXT" o icono 🌸

---

## ⚡ **Paso 5: Crear Botón USE RELIC**

1. **Click derecho en MobileControls_UI**
2. **UI → Input System → On-Screen Button**
3. **Nombre:** `Button_UseRelic`

### **RectTransform:**
```
Anchors: Middle-Right
Pos X: -180
Pos Y: -150
Width: 130
Height: 130
```

### **On-Screen Button Component:**
```
Control Path: <Gamepad>/buttonEast
```

### **Visual:**
- **Image:** Color amarillo/dorado (255, 200, 0, 150)
- **Texto:** "USE"

---

## 📖 **Paso 6: Crear Botón DIARY (Opcional)**

1. **Click derecho en MobileControls_UI**
2. **UI → Input System → On-Screen Button**
3. **Nombre:** `Button_Diary`

### **RectTransform:**
```
Anchors: Top-Left
Pos X: 100
Pos Y: -100
Width: 100
Height: 100
```

### **On-Screen Button Component:**
```
Control Path: <Gamepad>/buttonNorth
```

### **Visual:**
- **Image:** Color púrpura (150, 100, 200, 150)
- **Texto:** "📖" o "DIARY"

---

## ⚙️ **Paso 7: Conectar con Player**

**IMPORTANTE:** El Player debe tener el component **Player Input**.

1. **Selecciona Player** en Hierarchy
2. Si **NO tiene** Player Input:
   - **Add Component → Player Input**
3. **Configurar Player Input:**
   ```
   Actions: Arrastra PlayerInputActions asset
   Default Map: Player
   Behavior: Send Messages (o Invoke Unity Events)
   ```

**El Input System conecta automáticamente:**
```
On-Screen Stick → <Gamepad>/leftStick → PlayerInputActions.Player.Move
Button_Dash → <Gamepad>/buttonSouth → PlayerInputActions.Player.Dash
Button_NextRelic → <Gamepad>/buttonWest → PlayerInputActions.Player.CycleRelic
Button_UseRelic → <Gamepad>/buttonEast → PlayerInputActions.Player.UseRelic
Button_Diary → <Gamepad>/buttonNorth → PlayerInputActions.Player.OpenDiary
```

---

## 📱 **Paso 8: Ocultar en PC (Opcional)**

Crea un script simple:

```csharp
using UnityEngine;

public class HideMobileControlsOnPC : MonoBehaviour
{
    private void Start()
    {
        #if !UNITY_ANDROID && !UNITY_IOS
            gameObject.SetActive(false);
        #endif
    }
}
```

Añádelo al Canvas `MobileControls_UI`.

---

## 🧪 **Paso 9: Probar en Editor**

### **Configurar Game View:**

1. **Game tab → Aspect Ratio → Free Aspect**
2. O mejor: **Window → General → Device Simulator**
   - Selecciona un dispositivo Android
   - Verás los controles como en móvil

### **Probar:**

1. **Play** ▶️
2. **Click y arrastra** el joystick → Player se mueve
3. **Click** en botón DASH → Player hace dash
4. **Click** en NEXT → Cambia reliquia
5. **Click** en USE → Usa reliquia activa

---

## 🎨 **Personalización Visual:**

### **Cambiar Colores:**

```csharp
// Para cada botón:
Image → Color: Tu color
```

**Paleta recomendada:**
- DASH: Cyan (0, 255, 255)
- NEXT: Verde (100, 255, 100)
- USE: Amarillo (255, 200, 0)
- DIARY: Púrpura (150, 100, 200)

### **Añadir Iconos:**

Puedes usar sprites custom o emojis en TextMeshPro:
```
DASH: 💨
NEXT: 🔄
USE: ⚡
DIARY: 📖
```

---

## 🐛 **Troubleshooting:**

### **❌ Botones no responden:**

**Causa:** Control Path incorrecto
**Solución:** Verifica que coincide con los bindings en PlayerInputActions

**Ejemplo:**
```
Button_Dash → Control Path: <Gamepad>/buttonSouth
PlayerInputActions.Dash → Binding: <Gamepad>/buttonSouth ✅ COINCIDE
```

### **❌ Joystick no mueve al player:**

**Causa 1:** Control Path incorrecto
**Solución:** `<Gamepad>/leftStick` (exacto)

**Causa 2:** Player no tiene Player Input
**Solución:** Player → Add Component → Player Input

### **❌ No veo los controles:**

**Causa:** Canvas desactivado o fuera de cámara
**Solución:** 
- Verifica que Canvas está activo ✅
- Canvas → Render Mode: Screen Space - Overlay

### **❌ "PlayerInputActions not found":**

**Causa:** C# no generado o con errores
**Solución:** Vuelve a GUIA_INPUT_SYSTEM_SETUP.md y regenera

---

## ✅ **Checklist Final:**

- [ ] Canvas creado
- [ ] On-Screen Stick añadido (leftStick)
- [ ] Button_Dash (buttonSouth)
- [ ] Button_NextRelic (buttonWest)
- [ ] Button_UseRelic (buttonEast)
- [ ] Button_Diary (buttonNorth) - opcional
- [ ] Player tiene Player Input component
- [ ] Actions asignado en Player Input
- [ ] Probado en Game View
- [ ] Todo funciona sin errores

---

## 🎯 **Resultado Final:**

**En Editor:**
- ✅ Controles visibles
- ✅ Clickeables con mouse
- ✅ Player responde

**En Build Android:**
- ✅ Controles táctiles
- ✅ Joystick funcional
- ✅ Botones responden al touch

---

## 📊 **Mapeo Completo:**

| UI Element | Control Path | Input Action | Código |
|------------|--------------|--------------|--------|
| Joystick   | `<Gamepad>/leftStick` | Move | `inputActions.Player.Move.ReadValue()` |
| DASH       | `<Gamepad>/buttonSouth` | Dash | `OnDashPerformed()` |
| NEXT       | `<Gamepad>/buttonWest` | CycleRelic | `OnCycleRelicPerformed()` |
| USE        | `<Gamepad>/buttonEast` | UseRelic | `OnUseRelicPerformed()` |
| DIARY      | `<Gamepad>/buttonNorth` | OpenDiary | Event en DiaryUI |

---

**¡Tu juego ahora tiene controles móviles profesionales!** 📱✨

**Tiempo:** 20-30 minutos
**Dificultad:** Media ⭐⭐⭐
