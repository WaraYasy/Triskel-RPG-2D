# Guía Rápida - Configuración Inicial

## ✅ Paso 1: Crear Escena de Prueba

1. En Unity: `Assets/_Project/Scenes/` → Click derecho → Create → Scene
2. Nombre: `TestMovement`
3. Abrir la escena

---

## ✅ Paso 2: Configurar GameObject del Jugador

### Crear Player:
1. Hierarchy → Click derecho → Create Empty
2. Nombre: `Player`
3. Transform → Position: `(0, 0, 0)`

### Añadir componentes:
4. Add Component → **Rigidbody 2D**
   - Gravity Scale: `0` (es top-down, no cae)
   - Constraints → Freeze Rotation Z: ✅ (no rota)

5. Add Component → **Circle Collider 2D**
   - Radius: `0.5`

6. Add Component → **Sprite Renderer** (temporal para ver algo)
   - Sprite: Circle (buscar "Circle" en assets por defecto)
   - Color: Azul o el que quieras

7. Add Component → **PlayerController** (tu script)
   - Move Speed: `5`

---

## ✅ Paso 3: Crear GameManager

1. Hierarchy → Click derecho → Create Empty
2. Nombre: `GameManager`
3. Add Component → **GameManager** (tu script)

---

## ✅ Paso 4: Configurar Cámara

1. Seleccionar `Main Camera` en Hierarchy
2. Inspector:
   - Position: `(0, 0, -10)`
   - Projection: `Orthographic`
   - Size: `5` (o el que te guste)

---

## 🎮 Paso 5: ¡PROBAR!

1. Click en **Play** ▶️
2. Usa **WASD** o **Flechas** para mover al jugador
3. Deberías ver el círculo azul moviéndose

---

## ✅ ¿Funciona? Siguiente Paso

Si el movimiento funciona correctamente:

### Guardar como Prefab:
1. Drag el `Player` desde Hierarchy a `Assets/_Project/Prefabs/`
2. Ahora tienes un Player prefab reutilizable

### Hacer commit:
```bash
git add Assets/_Project/Scripts/
git commit -m "feat: Add basic PlayerController and GameManager"
git push origin Dev_GRB
```

---

## 🚀 Paso 6: Añadir Dash (v2.0)

El PlayerController ahora incluye sistema de Dash:

### **Nuevas Características:**
- Presiona **SPACE** para hacer dash
- Dash en la última dirección de movimiento
- Cooldown de 1.5 segundos
- Velocidad: 3x más rápido que caminar

### **Configuración en Inspector:**
```
Configuración de Movimiento:
  Move Speed: 5

Configuración de Dash:  ← NUEVO
  Dash Speed: 15        ← Velocidad del dash
  Dash Duration: 0.2    ← Duración del dash
  Dash Cooldown: 1.5    ← Tiempo entre dashes
```

### **Cómo Probarlo:**
1. Play ▶️
2. Muévete con **WASD**
3. Presiona **SPACE** → ¡Dash!
4. Intenta spamear Space → Verás que tiene cooldown

### **Ajustes Recomendados:**
Experimenta con estos valores en Inspector:

| Para más acción | Para más estratégico |
|----------------|---------------------|
| Dash Speed: 20 | Dash Speed: 12 |
| Cooldown: 1.0  | Cooldown: 2.0 |

---

## 🎯 Próximas Mejoras

Una vez funcione movimiento + dash:

1. ✅ **RelicSystem** - Cambio entre 3 reliquias (SIGUIENTE)
2. **Habilidades básicas** - Usar reliquia con Z
3. **Visual feedback** - Sprite diferente por reliquia equipada
4. **GameManager expandido** - Guardar moral, reliquias obtenidas
5. **DiarioEvolutivo** - Sistema de frases narrativas

---

## 🌸 Paso 7: Sistema de Reliquias (v1.0)

### **Nuevas Características:**
- Cambia entre 3 reliquias con teclas **1/2/3**
- Presiona **0** para desequipar
- Visual feedback con colores
- Enum para tipos de reliquias

### **Configuración en Unity:**

#### 1. Añadir RelicSystem al Player:
1. Seleccionar `Player` en Hierarchy
2. Add Component → **RelicSystem**

#### 2. Crear Visual Indicator (Opcional):
1. Click derecho en Player → 2D Object → Sprites → Circle
2. Nombre: `RelicIndicator`
3. Transform → Scale: `(0.3, 0.3, 1)`
4. Transform → Position: `(0, 0.7, 0)` (encima del jugador)

#### 3. Configurar RelicSystem:
En Inspector → RelicSystem:
```
Configuración:
  Current Relic: None

Visual Feedback (Opcional):
  Relic Indicator: [Arrastrar RelicIndicator aquí]
  Color Lirio: Cyan
  Color Hacha: Red
  Color Manto: Purple
```

### **Cómo Probarlo:**
1. Play ▶️
2. Presiona **1** → Indicador se vuelve Cyan (Lirio Azul)
3. Presiona **2** → Indicador se vuelve Rojo (Hacha Sagrada)
4. Presiona **3** → Indicador se vuelve Púrpura (Manto de Luna)
5. Presiona **0** → Indicador desaparece (Sin reliquia)
6. Verás logs en Console indicando el cambio

### **Tipos de Reliquias:**
```
0 = None (Sin reliquia)
1 = Lirio Azul (Cyan)
2 = Hacha Sagrada (Rojo)
3 = Manto de Luna (Púrpura)
```

---

## 🎯 Siguiente: Habilidades Básicas

Próximo paso será añadir una habilidad simple por cada reliquia:

1. ✅ **Lirio Azul (Z)** → Detectar objetos cerca (HECHO)
2. ✅ **Hacha Sagrada (Z)** → Golpe direccional (HECHO)
3. ✅ **Manto de Luna (Z)** → Toggle invisibilidad (HECHO)

---

## ⚡ Paso 8: Habilidades de Reliquias (v2.0)

### **Nuevas Características:**
- Presiona **Z** para usar la reliquia equipada
- Cada reliquia tiene su habilidad única
- Cooldown de 2 segundos entre usos

### **Configuración Adicional:**

1. **Selecciona Player** en Hierarchy
2. **Inspector → RelicSystem:**

```
Habilidades:  ← NUEVO
  Ability Cooldown: 2           ← Tiempo entre usos
  Lirio Detection Radius: 3     ← Radio del Lirio
  Player Sprite: [Arrastra el SpriteRenderer del Player aquí]
```

**Importante:** Para que funcione la invisibilidad del Manto, **arrastra** el componente **Sprite Renderer** del Player al campo "Player Sprite".

---

### **Cómo Probar las Habilidades:**

#### **🌸 Lirio Azul:**
1. Presiona **1** (equipar Lirio)
2. Presiona **Z**
3. Verás una **esfera cyan** aparecer alrededor del jugador por 1 segundo

#### **⚔️ Hacha Sagrada:**
1. Presiona **2** (equipar Hacha)
2. **Muévete** en una dirección (WASD)
3. Presiona **Z**
4. Verás un **cubo rojo** dispararse en esa dirección

#### **🌙 Manto de Luna:**
1. Presiona **3** (equipar Manto)
2. Presiona **Z** → Player se vuelve semi-transparente (invisibilidad ON)
3. Presiona **Z** otra vez → Player vuelve a ser opaco (invisibilidad OFF)

---

### **Cooldown:**
- Si presionas Z varias veces seguidas, verás en Console que hay un cooldown de 2 segundos
- Puedes ajustar el tiempo en Inspector → Ability Cooldown

---

## 🎯 Siguiente Paso: GameManager Expandido

Ahora que las mecánicas básicas funcionan, podemos añadir:

1. **GameManager** → Guardar moral, reliquias obtenidas
2. **GameData** → Estructura de datos persistente
3. **DiarioEvolutivo** → Frases narrativas básicas

---

**¿Funcionan las habilidades?** Pruébalas todas y avísame! 🌸⚔️🌙
