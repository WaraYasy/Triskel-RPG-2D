# Guía - Nuevos Sistemas del Merge (Develop_integracion)

## 📚 **Sistemas Implementados**

Tu compañera añadió:

1. **Sistema de Inventario** (Reliquias coleccionables)
2. **Sistema de Diario** (Entradas narrativas)
3. **Scripts de Testing** para probar todo

---

## 🎒 1. Sistema de Inventario

### **Funcionalidad:**
- Recoger reliquias en el mundo
- Máximo 3 items (las 3 reliquias)
- Persistencia automática
- Eventos cuando se añade/quita items

### **Scripts Principales:**
- `InventoryData.cs` - Gestor del inventario (Singleton)
- `CollectibleObject.cs` - Script para objetos que se recogen
- `CollectibleItem.cs` - ScriptableObject con datos del item
- `InventoryPersistence.cs` - Guardado/Carga
- `InventoryUI.cs` - UI para mostrar items

### **📋 Cómo Probar:**

#### **Paso 1: Crear un Objeto Coleccionable**

1. **Hierarchy** → Create Empty → Nombre: `Lirio_Coleccionable`
2. **Add Component → Sprite Renderer**
   - Asigna un sprite (ej: círculo cyan)
3. **Add Component → Circle Collider 2D**
   - Is Trigger: ✅
4. **Add Component → CollectibleObject**
   - Auto Collect: ✅ (se recoge al tocar)
   - Interact Key: E (si Auto Collect = false)

#### **Paso 2: Crear el CollectibleItem Asset**

1. **Project → Assets/_Project/Resources/Items/**
2. **Create → Triskel → Collectible Item**
3. **Nombre:** `Lirio`
4. **Configurar:**
   ```
   Item ID: lirio
   Display Name: Lirio Azul
   Description: Detecta energías oscuras
   Icon: (sprite del lirio)
   Item Type: Relic
   ```

#### **Paso 3: Asignar al GameObject**

1. Selecciona `Lirio_Coleccionable`
2. Inspector → **CollectibleObject:**
   - **Item Data:** Arrastra el asset `Lirio`

#### **Paso 4: Añadir Manager a la Escena**

1. **Hierarchy** → Create Empty → **InventoryManager**
2. **Add Component → InventoryData**
3. **Add Component → InventoryPersistence**

#### **Paso 5: ¡Probar!**

1. **Play** ▶️
2. **Camina hacia el Lirio**
3. Verás en Console: `✓ Item recogido: Lirio Azul`
4. Se guarda automáticamente

---

## 📖 2. Sistema de Diario

### **Funcionalidad:**
- Entradas narrativas por nivel/decisión
- Carga desde JSON
- Desbloqueo según moral
- Persistencia entre sesiones

### **Scripts Principales:**
- `DiaryManager.cs` - Gestor del diario (Singleton)
- `DiaryEntryData.cs` - Estructura de una entrada
- `DiaryDataStructure.cs` - Estructura del JSON
- `DiaryPersistence.cs` - Guardado/Carga
- `DiaryUI.cs` - UI para mostrar entradas

### **📋 Cómo Probar:**

#### **Paso 1: Crear JSON de Entradas**

1. **Assets/_Project/Resources/DiaryData/**
2. **Create → Text File → Nombre: DiaryEntries.json**
3. **Contenido:**

```json
{
  "entries": [
    {
      "id": "level0_bueno",
      "level": 0,
      "decision": "bueno",
      "phrase": "La luz guía mis pasos hacia la esperanza."
    },
    {
      "id": "level0_malo",
      "level": 0,
      "decision": "malo",
      "phrase": "Las sombras me susurran secretos oscuros."
    },
    {
      "id": "level1_bueno",
      "level": 1,
      "decision": "bueno",
      "phrase": "Mi compasión ilumina el camino."
    },
    {
      "id": "level1_malo",
      "level": 1,
      "decision": "malo",
      "phrase": "El poder corrompe, pero me hace fuerte."
    }
  ]
}
```

#### **Paso 2: Añadir Manager a la Escena**

1. **Hierarchy** → Create Empty → **DiaryManager**
2. **Add Component → DiaryManager**
   - Json File Name: `DiaryEntries`

#### **Paso 3: Añadir Script de Testing**

1. **Hierarchy** → Create Empty → **DiaryTester**
2. **Add Component → QuickDiaryTest**

#### **Paso 4: ¡Probar!**

1. **Play** ▶️
2. **Presiona teclas:**
   - **F1** → Desbloquea "Nivel 0 Bueno"
   - **F2** → Desbloquea "Nivel 0 Malo"
   - **F3** → Desbloquea "Nivel 1 Bueno"
   - **F4** → Desbloquea "Nivel 1 Malo"
   - **F12** → Limpiar todas las entradas

3. Verás en Console:
   ```
   [Diario] ✓ 'level0_bueno' desbloqueada
   ```

---

## 🧪 3. Testing Rápido con Scripts de Debug

### **InventoryDebugger.cs**

**Teclas:**
- **I** → Añadir Lirio
- **O** → Añadir Hacha
- **P** → Añadir Manto
- **C** → Limpiar inventario
- **L** → Ver contenido del inventario

### **Cómo usar:**

1. **Hierarchy** → Create Empty → **InventoryDebugger**
2. **Add Component → InventoryDebugger**
3. **Play** ▶️
4. **Presiona I/O/P** para añadir items
5. Verás en Console el estado del inventario

---

## 📊 Resumen de Archivos Nuevos

```
Scripts/
├── Core/
│   ├── Diary/
│   │   ├── DiaryManager.cs ⭐
│   │   ├── DiaryEntryData.cs
│   │   ├── DiaryDataStructure.cs
│   │   └── DiaryPersistence.cs
│   ├── Inventory/
│   │   ├── InventoryData.cs ⭐
│   │   ├── CollectibleItem.cs
│   │   └── InventoryPersistence.cs
│   └── Testing/
│       ├── QuickDiaryTest.cs ⭐
│       ├── QuickInventoryTest.cs ⭐
│       ├── InventoryDebugger.cs
│       └── InventoryUIDebugger.cs
├── Player/
│   └── CollectibleObject.cs ⭐
└── UI/
    ├── DiaryUI.cs
    └── HUD/
        └── InventoryUI.cs
```

⭐ = Archivos más importantes para empezar

---

## ✅ **Quick Start - Probar Todo en 5 Minutos**

1. **Crear GameObjects:**
   - InventoryManager (con InventoryData + InventoryPersistence)
   - DiaryManager (con DiaryManager)
   - InventoryDebugger (con InventoryDebugger)
   - DiaryTester (con QuickDiaryTest)

2. **Crear JSON:**
   - `Resources/DiaryData/DiaryEntries.json` (con entradas de ejemplo)

3. **Play:**
   - **I/O/P** → Probar inventario
   - **F1-F6** → Probar diario
   - **L** → Ver inventario en Console

---

**¿Por dónde quieres empezar?** 🎮
1. 🎒 Sistema de Inventario
2. 📖 Sistema de Diario
3. 🧪 Testing rápido con debuggers
