# 🎮 Guía de Configuración - Sistema de Diálogos

Esta guía te ayudará paso a paso a configurar y probar el sistema de diálogos.

---

## 📋 PASO 1: Configurar DialogueManager

1. **En tu escena principal**, haz clic derecho en Hierarchy
2. Selecciona **Create Empty**
3. Renómbralo a **"DialogueManager"**
4. Con el objeto seleccionado, en el Inspector haz clic en **Add Component**
5. Busca y añade el script **DialogueManager**
6. En el campo **"Ink JSON"**, arrastra el archivo:
   - 📁 `Assets/_Project/InkDialogue/all_dialogs.json`

✅ **Resultado:** Deberías ver en el Inspector que el campo "Ink JSON" tiene el archivo asignado.

---

## 📋 PASO 2: Configurar DialogueUI

1. **En el mismo objeto "DialogueManager"** (o crea uno nuevo llamado "DialogueUI")
2. Haz clic en **Add Component**
3. Busca y añade el script **DialogueUI**

✅ **Resultado:** Este script es super simple, solo maneja el input de teclado.

---

## 📋 PASO 3: Crear un NPC de Prueba

1. En Hierarchy, haz clic derecho
2. Selecciona **2D Object → Sprite → Square** (o usa cualquier sprite)
3. Renómbralo a **"NPC"**
4. **Posiciónalo** en tu escena donde lo puedas ver
5. Con el NPC seleccionado, haz clic en **Add Component**
6. Busca y añade el script **NPC**
7. En el Inspector del NPC:
   - **Dialogue Knot Name:** Deja "npc" (o cambia a "merchant" o "test_dialogue")
   - **Interaction Range:** 2 (distancia para interactuar)
   - **Player Tag:** "Player" (asegúrate de que tu jugador tenga este tag)

✅ **Resultado:** Deberías ver un círculo amarillo alrededor del NPC cuando lo seleccionas (es el rango de interacción).

---

## 📋 PASO 4: Verificar que tienes GameManager

1. En Hierarchy, busca un objeto llamado **"GameManager"**
2. Si NO existe, créalo:
   - Clic derecho en Hierarchy → Create Empty
   - Renómbralo a "GameManager"
   - Add Component → busca "GameManager"
   - **IMPORTANTE:** Marca "DontDestroyOnLoad" si quieres que persista entre escenas

✅ **Resultado:** Debes tener un GameManager en la escena.

---

## 📋 PASO 5: Verificar tu Jugador

1. Asegúrate de que tu jugador tiene el **Tag "Player"**
2. Para verificar/cambiar el tag:
   - Selecciona tu jugador en Hierarchy
   - En el Inspector, arriba verás un dropdown que dice "Tag"
   - Selecciona **"Player"**

✅ **Resultado:** El NPC podrá detectar cuando el jugador está cerca.

---

## 🎮 PASO 6: ¡PROBAR EL SISTEMA!

### Cómo Probarlo:

1. **Presiona Play ▶** en Unity
2. **Acércate al NPC** con tu jugador
3. **Presiona la tecla E** para iniciar el diálogo

### Controles:

- **E:** Iniciar diálogo (cerca del NPC)
- **ESPACIO:** Continuar el diálogo
- **1, 2, 3, 4:** Seleccionar opciones (cuando aparezcan)

### ¿Dónde ver el diálogo?

📺 **Por ahora, todo aparece en la CONSOLA de Unity** (no hay UI visual todavía)

Para ver la consola:
- Ve al menú: **Window → General → Console**

Verás algo como:
```
[DialogueManager] Diálogo iniciado en knot: npc
[Diálogo] ¡Hola, viajero! Es la primera vez que te veo por aquí.
[Diálogo] Opciones disponibles:
  1. Cuéntame sobre este lugar
  2. ¿Tienes alguna misión para mí?
  3. Adiós
```

---

## 🐛 Troubleshooting

### ❌ "No se encontró DialogueManager en la escena"
**Solución:** Asegúrate de haber creado el GameObject con el componente DialogueManager (PASO 1).

---

### ❌ "No hay archivo Ink JSON asignado"
**Solución:** Arrastra el archivo `all_dialogs.json` al campo "Ink JSON" del DialogueManager.

---

### ❌ "El diálogo no inicia cuando presiono E"
**Solución:**
- Verifica que tu jugador tenga el tag "Player"
- Acércate más al NPC (debe estar dentro del círculo amarillo)
- Verifica en la consola si hay errores

---

### ❌ "GameManager o DialogueEvents no encontrado"
**Solución:** Asegúrate de que hay un GameManager en la escena (PASO 4).

---

## 🎨 Próximos Pasos (Opcional)

Una vez que el sistema funcione con la consola, puedes:

1. **Crear una UI visual** con Canvas, Panel, Text
2. **Modificar DialogueUI.cs** para mostrar el texto en pantalla
3. **Añadir botones** para las opciones
4. **Añadir efectos** como escritura letra por letra

Por ahora, prueba el sistema básico con la consola para verificar que todo funciona.

---

## 📝 Resumen de Archivos Necesarios

✅ DialogueManager.cs (en GameObject "DialogueManager")
✅ DialogueUI.cs (en GameObject "DialogueUI" o junto a DialogueManager)
✅ NPC.cs (en GameObject "NPC")
✅ GameManager (debe existir en la escena)
✅ all_dialogs.json (asignado en DialogueManager)

---

¡Listo! 🎉 Prueba el sistema y verás los diálogos funcionando en la consola.
