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

## 🔧 Próximas Mejoras

Una vez funcione el movimiento básico, añadiremos:

1. **Dash** (Space para dash)
2. **Animaciones** (caminar en 4 direcciones)
3. **Input System** (mejor manejo de inputs)
4. **Reliquias básicas** (cambio con 1/2/3)

---

**¿Todo funcionando?** Avísame cuando lo pruebes y seguimos con el Dash! 🎮
