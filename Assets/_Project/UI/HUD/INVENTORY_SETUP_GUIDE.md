# 📦 GUÍA DE CONFIGURACIÓN DEL INVENTARIO - TRISKEL

**Tiempo estimado:** 20-30 minutos
**Dificultad:** Principiante
**Requisitos:** Unity abierto, escena principal lista

---

## ⚠️ ANTES DE EMPEZAR - Solución de Problemas

### Problema: "No aparece la opción Triskel en Create"

**Causa:** Unity tiene errores de compilación y no puede generar el menú.

**Solución:**
1. Abre la **Consola** de Unity (Window → General → Console)
2. Verifica que **NO haya errores rojos** ❌
3. Si hay errores, espera a que Unity recompile (esquina inferior derecha: "Compiling...")
4. Una vez sin errores, la opción "Triskel" debería aparecer

### Problema común ya resuelto:
- ✅ Error en `CollectibleObject.cs` línea 191 → **YA ARREGLADO**
- ✅ Si persiste, cierra y abre Unity de nuevo

---

## ✅ Archivos del Sistema

### Scripts creados:
```
Scripts/
├─ Core/
│  ├─ CollectibleItem.cs         (ScriptableObject para items)
│  ├─ InventoryData.cs           (Gestor de estado en memoria)
│  └─ InventoryPersistence.cs    (Guardado local/API - TOGGLE AQUÍ)
├─ API/
│  └─ TriskelAPIClient.cs        (Cliente HTTP para API)
├─ Player/
│  └─ CollectibleObject.cs       (Objetos coleccionables en mundo)
└─ UI/HUD/
   └─ InventoryUI.cs             (Controlador visual - ACTUALIZADO)
```

### Archivos UI:
```
UI/HUD/
├─ Inventory.uxml   (Estructura - 3 slots)
├─ Inventory.uss    (Estilos)
└─ INVENTORY_SETUP_GUIDE.md (Esta guía)
```

---

# 🚀 CONFIGURACIÓN PASO A PASO

---

## 🎯 PASO 1: Crear los Items (ScriptableObjects)

**¿Qué estás haciendo?** Crear 3 "fichas de información" para tus reliquias.

### 1.1 Crear carpeta para items

1. **Abre Unity**
2. En la ventana **Project** (abajo), navega a:
   ```
   Assets → _Project → Resources
   ```
3. **Si NO existe la carpeta "Resources":**
   - Click derecho en `_Project`
   - **Create → Folder**
   - Nómbrala: **Resources**

4. **Dentro de Resources**, crea otra carpeta:
   - Click derecho en `Resources`
   - **Create → Folder**
   - Nómbrala: **Items**

**Resultado:**
```
📁 Assets/_Project/
   └─ 📁 Resources/
      └─ 📁 Items/   ← Carpeta nueva
```

---

### 1.2 Crear el primer item: Lavender

1. **Click derecho dentro de `Items/`**
2. **Create → Triskel → Collectible Item**
3. Se crea un archivo llamado "NewCollectible"
4. **Renómbralo inmediatamente:** Presiona **F2** y escribe: **Lavender**

**Deberías ver:**
```
📁 Items/
   └─ 📄 Lavender
```

---

### 1.3 Configurar Lavender

1. **Haz clic en "Lavender"** (en el Project)
2. Mira el **Inspector** (panel derecho)
3. Verás esto:

```
╔════════════════════════════════════════╗
║ CollectibleItem (Script)               ║
╠════════════════════════════════════════╣
║ 📌 Identificación                      ║
║ ├─ Item ID: [        ]                 ║
║ └─ Display Name: [        ]            ║
║                                        ║
║ 🎨 Visualización                       ║
║ ├─ Icon: None (Sprite)                 ║
║ └─ Slot Color: grey                    ║
║                                        ║
║ 📝 Descripción                         ║
║ └─ [               ]                   ║
╚════════════════════════════════════════╝
```

4. **Rellena los campos:**

   **Item ID:**
   - Escribe: `lavender`
   - ⚠️ **MUY IMPORTANTE:** Este ID debe coincidir con tu API
   - En tu API el campo es `Game.relics[]` → uno de los valores es `"lirio"`, `"hacha"`, `"manto"`
   - Según tus sprites, usa: `lavender`, `lily`, `lilyofthevalley`

   **Display Name:**
   - Escribe: `Lavanda`

   **Icon:**
   - Abre la carpeta: `Assets/_Project/Sprites/Tools/`
   - **Arrastra** el sprite **LavenderOUTLINED.png** al campo "Icon"

   **Slot Color:**
   - Puedes dejarlo en `grey` o cambiar a: `blue`, `green`, `red`, `yellow`
   - Este es el color del marco del slot (prueba varios)

   **Description:**
   - Escribe algo como: `"Una hermosa lavanda con propiedades mágicas."`

---

### 1.4 Crear el segundo item: Lily

1. **Click derecho en `Items/`**
2. **Create → Triskel → Collectible Item**
3. Renómbralo: **Lily** (F2)

4. **Configurar:**
   - Item ID: `lily`
   - Display Name: `Lirio`
   - Icon: Arrastra **Lily.png** (de `Sprites/Tools/`)
   - Slot Color: `green`
   - Description: `"Un lirio mágico con poderes curativos."`

---

### 1.5 Crear el tercer item: LilyOfTheValley

1. **Click derecho en `Items/`**
2. **Create → Triskel → Collectible Item**
3. Renómbralo: **LilyOfTheValley** (F2)

4. **Configurar:**
   - Item ID: `lilyofthevalley`
   - Display Name: `Lirio del Valle`
   - Icon: Arrastra **LilyOfTheValleyOUTLINED.png**
   - Slot Color: `yellow`
   - Description: `"Un delicado lirio del valle lleno de misterio."`

---

### ✅ Verificación PASO 1

Deberías tener esto:

```
📁 Resources/Items/
   ├─ 📄 Lavender        (itemID: lavender, icono asignado)
   ├─ 📄 Lily            (itemID: lily, icono asignado)
   └─ 📄 LilyOfTheValley (itemID: lilyofthevalley, icono asignado)
```

**¡PASO 1 COMPLETO!** ✅

---

## 🎯 PASO 2: Crear el Gestor del Inventario (GameObject)

**¿Qué estás haciendo?** Crear un objeto invisible que controla todo el inventario.

### 2.1 Crear GameObject vacío

1. Abre tu **escena principal** (SampleScene o la que uses)
2. En la ventana **Hierarchy** (izquierda), verás:
   ```
   Hierarchy
   ├─ Main Camera
   ├─ Player
   ├─ UIDocument
   └─ ...
   ```

3. **Click derecho en un espacio vacío** de Hierarchy
4. **Create Empty**
5. Se crea un GameObject llamado "GameObject"
6. **Renómbralo:** Presiona **F2** → Escribe: **InventoryManager**

**Deberías ver:**
```
Hierarchy
├─ Main Camera
├─ Player
├─ UIDocument
└─ InventoryManager  ← NUEVO
```

---

### 2.2 Añadir componente: InventoryData

1. **Selecciona "InventoryManager"** (haz clic en él)
2. En el **Inspector** (panel derecho), verás:
   ```
   ╔════════════════════════════════════╗
   ║ GameObject: InventoryManager       ║
   ╠════════════════════════════════════╣
   ║ ✓ Transform                        ║
   ║   Position (0, 0, 0)               ║
   ╚════════════════════════════════════╝
   ```

3. **Abajo del todo**, haz clic en: **Add Component**
4. En el buscador, escribe: **InventoryData**
5. Haz clic en **"InventoryData"** cuando aparezca

6. Se añade el componente. Verás:
   ```
   ╔════════════════════════════════════╗
   ║ ✓ InventoryData (Script)           ║
   ║ ├─ Script: InventoryData           ║
   ║ └─ Max Inventory Size: 3           ║
   ╚════════════════════════════════════╝
   ```

7. **Verifica que "Max Inventory Size" sea 3**

---

### 2.3 Añadir componente: InventoryPersistence

1. Con **"InventoryManager" aún seleccionado**
2. Haz clic en: **Add Component** (de nuevo)
3. Escribe: **InventoryPersistence**
4. Haz clic en **"InventoryPersistence"**

5. Se añade el componente. Verás:
   ```
   ╔════════════════════════════════════════════╗
   ║ ✓ InventoryPersistence (Script)            ║
   ╠════════════════════════════════════════════╣
   ║ 🔧 TOGGLE: Modo de Persistencia           ║
   ║ └─ Use API: ☐                              ║ ← Checkbox
   ║                                            ║
   ║ Referencias                                ║
   ║ └─ Available Items                         ║
   ║    └─ Size: 0                              ║
   ╚════════════════════════════════════════════╝
   ```

6. **IMPORTANTE - Configurar:**

   **Use API:**
   - **DESMARCA** el checkbox (debe estar **☐ vacío**)
   - Esto significa que guardará en local (PlayerPrefs) por ahora

   **Available Items:**
   - Haz clic en el número "0" al lado de "Size"
   - Cámbialo a: **3**
   - Presiona **Enter**

7. **Ahora verás 3 campos vacíos:**
   ```
   Available Items
   ├─ Size: 3
   ├─ Element 0: None (CollectibleItem)
   ├─ Element 1: None (CollectibleItem)
   └─ Element 2: None (CollectibleItem)
   ```

8. **Arrastra los items que creaste:**
   - Desde **Project** → **Resources/Items/**
   - Arrastra **Lavender** → Suéltalo en **Element 0**
   - Arrastra **Lily** → Suéltalo en **Element 1**
   - Arrastra **LilyOfTheValley** → Suéltalo en **Element 2**

9. **Resultado final:**
   ```
   Available Items
   ├─ Size: 3
   ├─ Element 0: Lavender (CollectibleItem)
   ├─ Element 1: Lily (CollectibleItem)
   └─ Element 2: LilyOfTheValley (CollectibleItem)
   ```

---

### 2.4 Añadir componente: TriskelAPIClient

1. Con **"InventoryManager" aún seleccionado**
2. Haz clic en: **Add Component**
3. Escribe: **TriskelAPIClient**
4. Haz clic en **"TriskelAPIClient"**

5. Se añade el componente. Verás:
   ```
   ╔════════════════════════════════════════════╗
   ║ ✓ TriskelAPIClient (Script)                ║
   ╠════════════════════════════════════════════╣
   ║ Configuración de la API                    ║
   ║ └─ Base URL: https://localhost:8000        ║
   ║                                            ║
   ║ Autenticación                              ║
   ║ ├─ Player ID:                              ║
   ║ └─ Player Token:                           ║
   ║                                            ║
   ║ Partida Actual                             ║
   ║ └─ Current Game ID:                        ║
   ╚════════════════════════════════════════════╝
   ```

6. **Por ahora, déjalo todo vacío**
   - Lo configurarás cuando tu API esté funcionando

---

### ✅ Verificación PASO 2

Tu **InventoryManager** debería verse así en el Inspector:

```
╔════════════════════════════════════════════╗
║ GameObject: InventoryManager                ║
╠════════════════════════════════════════════╣
║ ✓ Transform                                 ║
║                                             ║
║ ✓ InventoryData (Script)                    ║
║ └─ Max Inventory Size: 3                    ║
║                                             ║
║ ✓ InventoryPersistence (Script)             ║
║ ├─ Use API: ☐ (DESMARCADO)                 ║
║ └─ Available Items: [Lavender, Lily, Lily] ║
║                                             ║
║ ✓ TriskelAPIClient (Script)                 ║
║ └─ (Campos vacíos por ahora)                ║
╚════════════════════════════════════════════╝
```

**¡PASO 2 COMPLETO!** ✅

---

## 🎯 PASO 3: Verificar el UI

**¿Qué estás haciendo?** Asegurarte de que la interfaz visual esté bien conectada.

### 3.1 Encontrar el GameObject UIDocument

1. En **Hierarchy**, busca el GameObject llamado **"UIDocument"**
   - Si no existe, créalo: Click derecho → UI Toolkit → UI Document

2. **Selecciona "UIDocument"**

3. En el **Inspector**, deberías ver:
   ```
   ╔════════════════════════════════════════════╗
   ║ GameObject: UIDocument                      ║
   ╠════════════════════════════════════════════╣
   ║ ✓ Panel Settings                            ║
   ║                                             ║
   ║ ✓ UI Document (Script)                      ║
   ║ └─ Source Asset: None (Visual Tree...)     ║
   ║                                             ║
   ║ ✓ InventoryUI (Script)                      ║
   ║ └─ UI Document: None (UIDocument)           ║
   ╚════════════════════════════════════════════╝
   ```

---

### 3.2 Asignar el archivo UXML

1. En el componente **"UI Document"**, busca el campo **"Source Asset"**

2. **Haz clic en el círculo pequeño** (⊙) a la derecha del campo

3. Se abre una ventana. **Busca:** `Inventory`

4. **Selecciona:** `Inventory` (tipo: Visual Tree Asset)

5. El campo ahora dice: **Source Asset: Inventory**

---

### 3.3 Conectar InventoryUI con UI Document

1. En el componente **"InventoryUI"**, busca el campo **"UI Document"**

2. **Opción A - Arrastrar:**
   - Arrastra el componente **"UI Document"** (el de arriba)
   - Suéltalo en el campo "UI Document" de InventoryUI

   **Opción B - Usar el selector:**
   - Haz clic en el círculo (⊙)
   - Selecciona "UIDocument (UI Document)"

3. El campo ahora dice: **UI Document: UIDocument (UI Document)**

---

### ✅ Verificación PASO 3

Tu **UIDocument** debería verse así:

```
╔════════════════════════════════════════════╗
║ GameObject: UIDocument                      ║
╠════════════════════════════════════════════╣
║ ✓ UI Document (Script)                      ║
║ └─ Source Asset: Inventory ✓                ║
║                                             ║
║ ✓ InventoryUI (Script)                      ║
║ └─ UI Document: UIDocument ✓                ║
╚════════════════════════════════════════════╝
```

**Prueba visual:**
- En la vista **Game** (pestaña arriba), deberías ver **3 slots grises** en la parte inferior

**¡PASO 3 COMPLETO!** ✅

---

## 🎯 PASO 4: Crear un Objeto Coleccionable

**¿Qué estás haciendo?** Crear un objeto en el mundo que el jugador pueda recoger.

### 4.1 Crear el GameObject

1. En **Hierarchy**, click derecho
2. **2D Object → Sprite** (o GameObject → 2D Object → Sprite)
3. Se crea un objeto llamado "New Sprite"
4. **Renómbralo:** **Pickup_Lavender** (F2)

---

### 4.2 Configurar el sprite visual

1. Con **"Pickup_Lavender" seleccionado**
2. En el **Inspector**, busca el componente **"Sprite Renderer"**
3. En el campo **"Sprite"**, haz clic en el círculo (⊙)
4. Busca: **LavenderOUTLINED**
5. Selecciona el sprite **LavenderOUTLINED**

6. **Posicionar en la escena:**
   - En **Transform**, configura:
     - Position X: **5** (o donde quieras)
     - Position Y: **0**
     - Position Z: **0**

---

### 4.3 Añadir el script CollectibleObject

1. Con **"Pickup_Lavender" seleccionado**
2. **Add Component**
3. Escribe: **CollectibleObject**
4. Haz clic en **"Collectible Object"**

5. Se añade el componente:
   ```
   ╔════════════════════════════════════════════╗
   ║ ✓ CollectibleObject (Script)                ║
   ╠════════════════════════════════════════════╣
   ║ Configuración del Item                     ║
   ║ └─ Item Data: None (CollectibleItem)       ║
   ║                                            ║
   ║ Método de Recolección                      ║
   ║ ├─ Auto Collect: ☑                        ║
   ║ └─ Interact Key: E                         ║
   ║                                            ║
   ║ Efectos (Opcional)                         ║
   ║ ├─ Pickup Particles: None                  ║
   ║ ├─ Pickup Sound: None                      ║
   ║ └─ Destroy Delay: 0.5                      ║
   ╚════════════════════════════════════════════╝
   ```

---

### 4.4 Asignar el item data

1. En el campo **"Item Data"**
2. Desde **Project → Resources/Items/**
3. **Arrastra "Lavender"** → Suéltalo en **"Item Data"**

4. Debería decir: **Item Data: Lavender (CollectibleItem)**

5. **Auto Collect:** Déjalo **marcado ☑**
   - Esto significa que se recoge automáticamente al tocar

---

### 4.5 Añadir Collider

1. Con **"Pickup_Lavender" seleccionado**
2. **Add Component**
3. Escribe: **Box Collider 2D** (o Circle Collider 2D)
4. Selecciónalo

5. Se añade el componente:
   ```
   ╔════════════════════════════════════════════╗
   ║ ✓ Box Collider 2D                           ║
   ╠════════════════════════════════════════════╣
   ║ ├─ Is Trigger: ☑ ← MARCA ESTO             ║
   ║ └─ Size: (1, 1)                            ║
   ╚════════════════════════════════════════════╝
   ```

6. **IMPORTANTE:** Marca **"Is Trigger": ☑**

---

### 4.6 Configurar el tag del jugador

**CRÍTICO:** El script detecta al jugador por su tag.

1. En **Hierarchy**, busca tu GameObject **"Player"**
2. **Selecciónalo**
3. En el **Inspector**, arriba del todo verás:
   ```
   Tag: Untagged ▼
   ```
4. Haz clic en **"Untagged"**
5. Selecciona: **Player**
   - Si no existe, selecciona **"Add Tag..."**
   - Haz clic en el **+**
   - Escribe: **Player**
   - Vuelve al GameObject Player y asigna el tag

---

### ✅ Verificación PASO 4

Tu **Pickup_Lavender** debería verse así:

```
╔════════════════════════════════════════════╗
║ GameObject: Pickup_Lavender                 ║
╠════════════════════════════════════════════╣
║ ✓ Transform (Position: 5, 0, 0)             ║
║                                             ║
║ ✓ Sprite Renderer                           ║
║ └─ Sprite: LavenderOUTLINED ✓               ║
║                                             ║
║ ✓ CollectibleObject (Script)                ║
║ └─ Item Data: Lavender ✓                    ║
║ └─ Auto Collect: ☑                         ║
║                                             ║
║ ✓ Box Collider 2D                           ║
║ └─ Is Trigger: ☑ ✓                         ║
╚════════════════════════════════════════════╝
```

**Y tu Player:**
```
╔════════════════════════════════════════════╗
║ GameObject: Player                          ║
║ Tag: Player ✓                               ║
╚════════════════════════════════════════════╝
```

**¡PASO 4 COMPLETO!** ✅

---

## 🎯 PASO 5: ¡PROBAR TODO!

### 5.1 Preparar para probar

1. **Guarda la escena:** Ctrl + S
2. Asegúrate de que tu **Player** pueda moverse (debe tener un script de movimiento)

---

### 5.2 Ejecutar el juego

1. Haz clic en el botón **Play** ▶ (arriba en el centro)

2. **Mueve al jugador** hacia el objeto **Pickup_Lavender**

---

### 5.3 ¿Qué debería pasar?

✅ **Al tocar el objeto:**
- El objeto **desaparece** del mundo
- En la **Consola** (Window → General → Console) ves:
  ```
  [InventoryData] Item añadido: Lavanda (1/3)
  [InventoryPersistence] ✓ Guardado LOCAL: lavender
  ```

✅ **En la UI:**
- En la vista **Game**, parte inferior
- Aparece un **slot con el icono** de lavanda
- El slot cambia de color según el "slotColor" que configuraste

---

### 5.4 Probar las teclas

1. Presiona la tecla **1**
   - El slot 1 se **selecciona** (cambia a grey_pressed.png)
   - En Consola: `[InventoryUI] Slot 1 seleccionado: Lavanda`

2. Presiona **1 de nuevo**
   - El slot se **deselecciona** (vuelve a grey.png)

3. Presiona **2** o **3**
   - Si esos slots están vacíos: `[InventoryUI] Slot X está vacío.`

---

### 5.5 Probar el guardado

1. **Stop** el juego (botón ▶ de nuevo)

2. **Play** de nuevo ▶

3. En **Hierarchy**, selecciona **"InventoryManager"**

4. En el **Inspector**, en el componente **InventoryPersistence**:
   - **Click derecho** sobre el nombre del componente
   - Selecciona: **"Debug: Cargar Inventario"**

5. **Deberías ver:**
   - En Consola: `[InventoryPersistence] ✓ Cargado LOCAL: lavender`
   - En la UI: El slot con lavanda aparece de nuevo

**¡El guardado funciona!** ✅

---

### ✅ Verificación PASO 5

Si todo funciona:
- ✅ Recoger items funciona
- ✅ Items aparecen en UI
- ✅ Teclas 1, 2, 3 funcionan
- ✅ Guardado/cargado funciona

**¡SISTEMA COMPLETO Y FUNCIONANDO!** 🎉

---

## 🔌 PASO 6 (OPCIONAL): Crear más objetos coleccionables

Puedes repetir el **Paso 4** para crear:

### Pickup_Lily
- Sprite: **Lily.png**
- Item Data: **Lily**
- Position: X=10, Y=0

### Pickup_LilyOfTheValley
- Sprite: **LilyOfTheValleyOUTLINED.png**
- Item Data: **LilyOfTheValley**
- Position: X=15, Y=0

---

## 🚀 INTEGRACIÓN CON API (Cuando esté lista)

### Configurar credenciales

1. Selecciona **"InventoryManager"** en Hierarchy

2. En el componente **TriskelAPIClient**, rellena:
   ```
   Base URL: https://tu-api.com
   Player ID: (el UUID que te da la API al crear jugador)
   Player Token: (el token que te da la API)
   Current Game ID: (el UUID de la partida actual)
   ```

### Activar modo API

1. En el componente **InventoryPersistence**:
   ```
   Use API: ☑ (MARCA EL CHECKBOX)
   ```

### ¡Listo!

Ahora el sistema:
- **Guarda** en la API cuando recoges items
- **Carga** desde la API al iniciar
- **Fallback** a local si la API falla

---

## 🐛 Solución de Problemas

### "No aparece el item en la UI"
**Causa:** InventoryUI no está conectado correctamente.
**Solución:** Verifica Paso 3 - UIDocument debe tener Source Asset y UI Document asignados.

### "No se recoge el item al tocarlo"
**Causa 1:** El Player no tiene el tag "Player".
**Solución:** Verifica Paso 4.6.

**Causa 2:** El Collider no está marcado como Trigger.
**Solución:** Verifica Paso 4.5 - Is Trigger debe estar ☑.

### "Error: InventoryData no está inicializado"
**Causa:** El InventoryManager no existe en la escena.
**Solución:** Verifica Paso 2 - Debe existir el GameObject con los 3 componentes.

### "No se guarda el progreso"
**Causa:** InventoryPersistence no está configurado.
**Solución:** Verifica Paso 2.3 - Available Items deben estar asignados.

### "La API no responde"
**Causa 1:** Credenciales incorrectas.
**Solución:** Verifica que Player ID, Token y Game ID sean correctos.

**Causa 2:** API no está corriendo.
**Solución:** Inicia tu servidor FastAPI.

**El sistema usa PlayerPrefs como fallback automático.**

---

## 📝 Checklist Final

- [ ] 3 CollectibleItem creados (Lavender, Lily, LilyOfTheValley)
- [ ] Icons asignados en cada item
- [ ] InventoryManager en escena con 3 componentes
- [ ] InventoryPersistence: useAPI = ☐ FALSE
- [ ] InventoryPersistence: Available Items asignados (3)
- [ ] UIDocument configurado con Inventory.uxml
- [ ] InventoryUI conectado a UIDocument
- [ ] Al menos 1 Pickup_Lavender en escena
- [ ] CollectibleObject con Item Data asignado
- [ ] Collider2D con Is Trigger = ☑
- [ ] Player con Tag = "Player"
- [ ] Probado en Play Mode (recoger funciona)
- [ ] Probado teclas 1, 2, 3
- [ ] Probado guardar/cargar

---

## 🎓 Para Otros Desarrolladores

### UI/UX Designer
- Edita: `Inventory.uxml` y `Inventory.uss` en UI Builder
- NO tocar código C#

### Level Designer
- Usa: CollectibleObject en GameObjects
- Solo arrastra los CollectibleItem assets
- Configura efectos visuales/audio

### Backend Developer
- Trabaja en: API FastAPI
- NO tocar código Unity
- Coordinar IDs: `Game.relics[]` debe coincidir con `CollectibleItem.itemID`

### Gameplay Programmer
- Modifica: `InventoryUI.cs` línea 363 → `OnItemSelected()`
- Usa: `InventoryData.Instance.GetItemIDs()` para leer reliquias
- Usa: `InventoryUI.GetSelectedItem()` para item equipado

---

**¡Sistema listo para producción!** 🎮✨
