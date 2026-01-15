# Guía - Migración a Input System

## 🎯 **Objetivo**
Migrar tu proyecto de Input Manager antiguo al nuevo Input System de Unity

---

## 📦 **Paso 1: Instalar Input System**

1. **Window → Package Manager**
2. **Packages: Unity Registry**
3. Busca: **"Input System"**
4. **Install**
5. Unity preguntará: **"Do you want to enable the new input system backend?"**
   - **Yes** (reiniciará Unity)

**Alternativa si ya tienes el viejo sistema:**
- Edit → Project Settings → Player
- Other Settings → Active Input Handling
- Selecciona: **Both** (soporta ambos mientras migras)
- Restart Unity

✅ **Verificar:** Aparece `UnityEngine.InputSystem` disponible

---

## 🎮 **Paso 2: Crear Input Actions Asset**

1. **Project → Assets/_Project/** (carpeta raíz del proyecto)
2. **Create → Input Actions**
3. **Nombre:** `PlayerInputActions`
4. **Doble click** para abrir el editor

---

## ⚙️ **Paso 3: Configurar Actions**

### **En el Input Actions Editor:**

1. **Action Maps → + → Nombre:** `Player`

2. **Actions (dentro de Player):**

#### **Movement:**
- **+ → Add Action** → Nombre: `Move`
- Action Type: **Value**
- Control Type: **Vector 2**
- Bindings:
  - **+ → Add Up/Down/Left/Right Composite → 2D Vector**
    - Up: W, Arrow Up
    - Down: S, Arrow Down
    - Left: A, Arrow Left
    - Right: D, Arrow Right
  - **+ → Add Binding**: `<Gamepad>/leftStick`

#### **Dash:**
- **+ → Add Action** → Nombre: `Dash`
- Action Type: **Button**
- Bindings:
  - Space
  - `<Gamepad>/buttonSouth`

#### **SelectRelic1 (PC - Selección Directa):**
- **+ → Add Action** → Nombre: `SelectRelic1`
- Action Type: **Button**
- Bindings: `<Keyboard>/1`

#### **SelectRelic2 (PC - Selección Directa):**
- **+ → Add Action** → Nombre: `SelectRelic2`
- Action Type: **Button**
- Bindings: `<Keyboard>/2`

#### **SelectRelic3 (PC - Selección Directa):**
- **+ → Add Action** → Nombre: `SelectRelic3`
- Action Type: **Button**
- Bindings: `<Keyboard>/3`

#### **CycleRelic (Móvil - Botón Next):**
- **+ → Add Action** → Nombre: `CycleRelic`
- Action Type: **Button**
- Bindings:
  - `<Keyboard>/tab` (para testing en PC)
  - `<Gamepad>/buttonWest` (para On-Screen Button móvil)

**💡 Cómo funciona:**
- **PC:** Teclas 1/2/3 seleccionan reliquia directamente
- **Móvil:** Botón Next cicla entre reliquias (Lirio → Hacha → Manto)

#### **UseRelic:**
- **+ → Add Action** → Nombre: `UseRelic`
- Action Type: **Button**
- Bindings: `<Keyboard>/z`

#### **OpenDiary:**
- **+ → Add Action** → Nombre: `OpenDiary`
- Action Type: **Button**
- Bindings: `<Keyboard>/j`

3. **Save Asset** (arriba en el editor)
4. **Generate C# Class** (checkbox arriba) ✅
5. **Apply**

Se creará: `PlayerInputActions.cs` automáticamente

---

## 🔄 **Paso 4: Desactivar Input Manager (Opcional)**

Si quieres forzar uso del nuevo sistema:

1. **Edit → Project Settings → Player**
2. **Other Settings → Active Input Handling**
3. Selecciona: **Input System Package (New)**
4. **Restart Unity**

---

## ✅ **Verificación:**

- [ ] Input System instalado
- [ ] PlayerInputActions.asset creado
- [ ] PlayerInputActions.cs generado
- [ ] Actions configuradas:
  - [ ] Move (Vector2)
  - [ ] Dash (Button)
  - [ ] SelectRelic1/2/3 (Button) - PC
  - [ ] CycleRelic (Button) - Móvil
  - [ ] UseRelic (Button)
  - [ ] OpenDiary (Button)
- [ ] Bindings añadidos a cada action

---

**Siguiente:** Refactorizar PlayerController y RelicSystem

**Tiempo:** 15 minutos
**Dificultad:** Media ⭐⭐⭐
