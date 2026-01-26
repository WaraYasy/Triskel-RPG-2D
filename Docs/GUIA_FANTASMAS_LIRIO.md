# Guía - Sistema de Fantasmas y Lirio

## 🎯 **Objetivo:**
- Fantasmas que siguen la luz del Lirio
- Lirio funciona como toggle (ON/OFF) en lugar de temporal
- Misma animación para idle y movimiento del fantasma

---

## 📋 **Archivos Creados:**
1. **GhostAI.cs** - IA del fantasma
2. **RelicSystem.cs** (actualizado) - Lirio como toggle

---

## 👻 **Paso 1: Configurar Fantasma**

### **Crear GameObject Fantasma:**

1. **Hierarchy → Create Empty** → Nombre: `Ghost`
2. **Add Component → Sprite Renderer**
   - Asigna tu sprite del fantasma
3. **Add Component → Animator**
   - Crea un Animator Controller: `Ghost_Controller`
4. **Add Component → Rigidbody2D** (opcional, para físicas)
   - Gravity Scale: 0
   - Freeze Rotation Z: ✅
5. **Add Component → GhostAI**

### **Configurar GhostAI:**

```
Inspector → GhostAI:
  Move Speed: 2
  Detection Radius: 10  (radio en que detecta la luz)
  Stop Distance: 0.5    (distancia mínima a la luz)
  Animator: (ya está auto-asignado)
```

---

## 🎬 **Paso 2: Configurar Animación del Fantasma (4 Direcciones)**

### **Crear Animator Controller:**

1. **Project** → Create → **Animator Controller**
2. Nombre: `Ghost_Controller`
3. **Doble click** para abrirlo

### **Añadir Parámetros:**

En la pestaña **Parameters** (izquierda):

| Nombre | Tipo | Valor por Defecto |
|--------|------|-------------------|
| Horizontal | Float | 0 |
| Vertical | Float | -1 |

### **Crear Blend Tree:**

1. **Click derecho** en animator → Create State → **From New Blend Tree**
2. Nombre: `GhostMovement`
3. **Doble click** en el Blend Tree

### **Configurar Blend Tree:**

- **Blend Type:** 2D Simple Directional
- **Parameters:**
  - Horizontal
  - Vertical

### **Añadir tus 4 Animaciones:**

Click en **+ (Add Motion Field)** 4 veces:

| Motion (Animación) | Pos X | Pos Y | Dirección |
|-------------------|-------|-------|-----------|
| Fantasma_Derecha | 1 | 0 | → |
| Fantasma_Izquierda | -1 | 0 | ← |
| Fantasma_Arriba | 0 | 1 | ↑ |
| Fantasma_Abajo | 0 | -1 | ↓ |

### **Asignar al Fantasma:**

1. **Selecciona** tu GameObject `Ghost`
2. **Inspector → Animator:**
   - Controller: Arrastra `Ghost_Controller`

---

## 🌸 **Paso 3: Configurar Tag LirioLight**

**IMPORTANTE:** Los fantasmas buscan objetos con tag "LirioLight"

1. **Unity → Edit → Project Settings → Tags and Layers**
2. **Tags → +**
3. Nombre: `LirioLight`
4. **Save**

---

## 🎮 **Paso 4: Usar el Sistema**

### **En el Player:**

Ya está configurado en `RelicSystem.cs`, solo necesitas:

1. **Selecciona Player**
2. **Inspector → RelicSystem:**
   - Lirio Detection Radius: 3
   - **Lirio Light Prefab:** (opcional, si quieres luz custom)

### **Controls:**

```
Tecla 1: Seleccionar Lirio
Tecla Z: Toggle luz ON/OFF

Cuando la luz está ON:
  - Los fantasmas la ven
  - Se mueven hacia ella
  - Sigue al player
```

---

## ✨ **Paso 5: (Opcional) Prefab de Luz Custom**

Si quieres una luz más bonita:

### **Crear Prefab:**

1. **Hierarchy → Create Empty** → `LirioLight_Prefab`
2. **Add Component → Sprite Renderer**
   - Sprite: Círculo blanco/cyan
   - Color: Cyan (0, 255, 255, 100)
3. **Add Component → Light 2D** (si usas URP)
   - Color: Cyan
   - Intensity: 1.5
   - Outer Radius: 3

### **Asignar:**

1. **Arrastra** a `Assets/_Project/Prefabs/`
2. **Elimina** de Hierarchy
3. **Player → RelicSystem → Lirio Light Prefab:** Arrastra el prefab

---

## 🐛 **Troubleshooting**

### **❌ Fantasmas no se mueven:**

**Causa:** No encuentran el tag "LirioLight"
**Solución:** 
1. Verifica que creaste el tag
2. Con luz activada, busca "LirioLight" en Hierarchy
3. Inspector → Tag debe ser "LirioLight"

### **❌ Luz no aparece:**

**Causa:** No se creó correctamente
**Solución:**
1. Console → Busca errores
2. Verifica que `Lirio Detection Radius` > 0
3. Presiona Z dos veces (OFF → ON)

### **❌ Luz no sigue al player:**

**Causa:** Update() no se ejecuta
**Solución:**
1. Player debe tener RelicSystem activo
2. GameObject Player activo en Hierarchy

---

## 🎯 **Resultado Final:**

```
Player con Lirio:
  1. Presiona "1" → Selecciona Lirio
  2. Presiona "Z" → Luz ON
  3. Fantasmas detectan luz
  4. Se mueven hacia el player
  5. Presiona "Z" → Luz OFF
  6. Fantasmas se detienen
```

---

## 📊 **Diagrama del Sistema:**

```
Player (con RelicSystem)
  ↓ Presiona Z
  ↓
LirioLight GameObject (tag: LirioLight)
  ↓ Sigue al Player
  ↓
Ghost (con GhostAI)
  ↓ FindGameObjectWithTag("LirioLight")
  ↓ Si encuentra
  → Mueve hacia luz
```

---

**¡Pruébalo!** Crea un fantasma, activa el Lirio y verás cómo te sigue. 👻✨
