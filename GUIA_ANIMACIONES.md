# Guía - Sistema de Animaciones del Jugador

## 📋 Sistema de Animaciones

Tu Animator Controller tiene **8 animaciones**:

### **Andando (4 direcciones):**
- AnimacionAndarDelante
- AnimacionAndarDerecha
- AnimacionAndarIzquierda
- AnimacionAndarDetras

### **Parado/Idle (4 direcciones):**
- AnimacionParado (frente)
- AnimacionParadoDerecha
- AnimacionParadoIzquierda
- AnimacionParadoAtras

---

## 🎯 Configuración del Animator Controller

### **Paso 1: Añadir Parámetros**

1. Abre tu Animator Controller: `Project/Animations/Characters/Player 1_controller`
2. En la pestaña **Parameters** (izquierda), añade:

| Nombre | Tipo | Valor por Defecto |
|--------|------|-------------------|
| Speed | Float | 0 |
| Horizontal | Float | 0 |
| Vertical | Float | 0 |
| LastHorizontal | Float | 0 |
| LastVertical | Float | -1 |

---

## 🔀 Paso 2: Crear Blend Trees

### **2.1: Blend Tree para ANDAR**

1. Click derecho en el Animator → Create State → From New Blend Tree
2. Nombre: `Movement`
3. Doble click en `Movement`

#### Configurar Blend Tree:
- **Blend Type:** 2D Simple Directional
- **Parameters:**
  - Horizontal
  - Vertical

#### Añadir Animaciones (botón +):
| Motion | Pos X | Pos Y |
|--------|-------|-------|
| AnimacionAndarDerecha | 1 | 0 |
| AnimacionAndarIzquierda | -1 | 0 |
| AnimacionAndarDelante | 0 | 1 |
| AnimacionAndarDetras | 0 | -1 |

---

### **2.2: Blend Tree para IDLE**

1. Click derecho → Create State → From New Blend Tree
2. Nombre: `Idle`
3. Doble click en `Idle`

#### Configurar Blend Tree:
- **Blend Type:** 2D Simple Directional
- **Parameters:**
  - LastHorizontal
  - LastVertical

#### Añadir Animaciones:
| Motion | Pos X | Pos Y |
|--------|-------|-------|
| AnimacionParadoDerecha | 1 | 0 |
| AnimacionParadoIzquierda | -1 | 0 |
| AnimacionParado | 0 | 1 |
| AnimacionParadoAtras | 0 | -1 |

---

## 🔗 Paso 3: Crear Transiciones

Vuelve al nivel principal del Animator (flecha atrás ←)

### **De Entry → Idle:**
- Ya existe por defecto
- Set as Layer Default State

### **De Idle → Movement:**
- Click derecho en Idle → Make Transition → Click en Movement
- **Conditions:**
  - Speed **Greater** 0.01
- **Settings:**
  - Has Exit Time: ❌
  - Transition Duration: 0.1

### **De Movement → Idle:**
- Click derecho en Movement → Make Transition → Idle
- **Conditions:**
  - Speed **Less** 0.01
- **Settings:**
  - Has Exit Time: ❌
  - Transition Duration: 0.1

---

## 🎮 Paso 4: Añadir Scripts al Player

1. Selecciona tu **Player** en Hierarchy
2. **Add Component → Animator**
   - Controller: Arrastra tu Animator Controller
   - Avatar: None (es 2D)
3. **Add Component → PlayerAnimator** (el script nuevo)

---

## ✅ Paso 5: Probar

1. **Play** ▶️
2. Mueve al jugador con **WASD**
3. Deberías ver:
   - **W** → Animación andar arriba
   - **S** → Animación andar abajo
   - **A** → Animación andar izquierda
   - **D** → Animación andar derecha
   - **Soltar** → Idle en la última dirección

---

## 🐛 Troubleshooting

**❌ No cambia de animación:**
- Verifica que los parámetros estén creados correctamente
- Revisa que las transiciones tengan las condiciones correctas

**❌ Animación incorrecta:**
- Revisa las posiciones (Pos X, Pos Y) en los Blend Trees
- Asegúrate que cada animación esté en la dirección correcta

**❌ Transiciones abruptas:**
- Aumenta Transition Duration a 0.2
- Activa "Interpolate" en los Blend Trees

---

## 📊 Diagrama del Sistema

```
Entry → Idle ⇄ Movement

Idle:
  - LastHorizontal/LastVertical
  - 4 animaciones parado

Movement:
  - Horizontal/Vertical  
  - 4 animaciones andando

Speed:
  - 0 = Idle
  - >0 = Movement
```

---

**¿Todo configurado?** ¡Prueba el movimiento! 🎮
