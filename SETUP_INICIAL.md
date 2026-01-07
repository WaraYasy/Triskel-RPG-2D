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

1. **RelicSystem** - Cambio entre 3 reliquias (teclas 1/2/3)
2. **Habilidades básicas** - Usar reliquia con Z
3. **Visual feedback** - Sprite diferente por reliquia equipada
4. **GameManager expandido** - Guardar moral, reliquias obtenidas
5. **DiarioEvolutivo** - Sistema de frases narrativas

---

**¿Todo funcionando?** Avísame cuando esté listo y seguimos con las reliquias! 🎮
