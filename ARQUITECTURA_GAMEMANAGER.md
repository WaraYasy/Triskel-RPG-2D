# 🏗️ Arquitectura del GameManager Orquestador

## 📋 Orden de Inicialización Garantizado

### Fase 1: Awake() - Todos los Singletons se establecen

```
Unity ejecuta Awake() de todos los scripts (orden NO garantizado):

✓ GameManager.Awake()
   └─ Establece Singleton Instance
   └─ DontDestroyOnLoad

✓ InventoryData.Awake()
   └─ Establece Singleton Instance
   └─ DontDestroyOnLoad

✓ InventoryPersistence.Awake()
   └─ Establece Singleton Instance
   └─ DontDestroyOnLoad
   └─ BuildItemDictionary() ← Inicialización interna

✓ DiaryManager.Awake()
   └─ Establece Singleton Instance
   └─ DontDestroyOnLoad
   └─ LoadEntriesFromJSON() ← Inicialización interna

✓ DiaryPersistence.Awake()
   └─ Establece Singleton Instance
   └─ DontDestroyOnLoad
```

### Fase 2: Start() - GameManager obtiene referencias

```
Unity ejecuta Start() de todos los scripts (DESPUÉS de todos los Awake()):

✓ GameManager.Start()
   └─ InitializeManagers() ← AQUÍ se obtienen las referencias
      ├─ FindFirstObjectByType<DiaryManager>()
      ├─ FindFirstObjectByType<InventoryData>()
      ├─ FindFirstObjectByType<InventoryPersistence>()
      ├─ FindFirstObjectByType<DiaryPersistence>()
      └─ FindFirstObjectByType<TriskelAPIClient>()
```

**GARANTÍA:** Cuando GameManager.Start() se ejecuta, **TODOS** los Awake() ya terminaron.

---

## 🎯 ¿Por qué es importante?

### ❌ ANTES (Awake):
```csharp
private void Awake()
{
    Instance = this;
    DontDestroyOnLoad(gameObject);
    InitializeManagers(); // ❌ Puede ejecutarse ANTES que otros Awake()
}
```

**Problema:**
- Si GameManager.Awake() se ejecuta primero
- FindFirstObjectByType() encuentra los componentes
- Pero sus Instance pueden ser NULL aún ❌

### ✅ AHORA (Start):
```csharp
private void Awake()
{
    Instance = this;
    DontDestroyOnLoad(gameObject);
    // NO inicializa managers aquí
}

private void Start()
{
    InitializeManagers(); // ✅ Todos los Awake() ya terminaron
}
```

**Ventaja:**
- Garantiza que todos los Singletons estén listos
- InventoryPersistence.itemDictionary ya está construido
- DiaryManager.entries ya está cargado del JSON
- Más robusto y predecible

---

## 📊 Diagrama de Dependencias

```
┌─────────────────────────────────────────────────────────┐
│                    GAMEMANAGER                          │
│                  (Orquestador Central)                  │
│                                                          │
│  Awake: Singleton + DontDestroyOnLoad                  │
│  Start: InitializeManagers() ← Obtiene referencias     │
│                                                          │
└────────────┬──────────┬──────────┬──────────┬──────────┘
             │          │          │          │
             ▼          ▼          ▼          ▼
    ┌────────────┐ ┌────────────┐ ┌────────────┐ ┌────────────┐
    │  Diary     │ │ Inventory  │ │   Diary    │ │ Inventory  │
    │  Manager   │ │   Data     │ │Persistence │ │Persistence │
    └────────────┘ └────────────┘ └────────────┘ └────────────┘
         │              │               │               │
         │              │               │               │
      Awake()        Awake()        Awake()         Awake()
         │              │               │               │
         ├─ Singleton   ├─ Singleton    ├─ Singleton    ├─ Singleton
         ├─ DDOL        ├─ DDOL         ├─ DDOL         ├─ DDOL
         └─ LoadJSON    └─ (nada)       └─ (nada)       └─ BuildDict
```

**DDOL = DontDestroyOnLoad**

---

## 🔧 Componentes y sus responsabilidades

### GameManager (Orquestador)
**Awake:**
- Establece Singleton
- DontDestroyOnLoad

**Start:**
- Obtiene referencias a otros managers
- Valida que existan

**Responsabilidades:**
- Coordinar SaveGame/LoadGame/NewGame
- Gestionar estado global (moral, nivel, tiempo)
- Auto-guardado periódico
- Sincronización con API

---

### InventoryData (Datos)
**Awake:**
- Establece Singleton
- DontDestroyOnLoad

**Responsabilidades:**
- Gestionar lista de items en memoria
- Disparar eventos cuando cambia el inventario
- NO guarda (eso es InventoryPersistence)

---

### InventoryPersistence (Persistencia)
**Awake:**
- Establece Singleton
- DontDestroyOnLoad
- **Construye diccionario de items disponibles** ← IMPORTANTE

**Responsabilidades:**
- Guardar/cargar inventario en PlayerPrefs
- Convertir entre IDs y CollectibleItems
- NO gestiona lógica de negocio

---

### DiaryManager (Datos + Lógica)
**Awake:**
- Establece Singleton
- DontDestroyOnLoad
- **Carga entradas desde JSON** ← IMPORTANTE

**Responsabilidades:**
- Gestionar entradas del diario en memoria
- Desbloquear entradas según decisiones
- NO guarda (eso es DiaryPersistence)

---

### DiaryPersistence (Persistencia)
**Awake:**
- Establece Singleton
- DontDestroyOnLoad

**Responsabilidades:**
- Guardar/cargar entradas desbloqueadas en PlayerPrefs
- NO gestiona lógica narrativa

---

## ✅ Checklist de Configuración

Para que el sistema funcione correctamente en una escena:

```
✓ GameObject "GameManager" con:
  └─ GameManager.cs

✓ GameObject "DiarySystem" con:
  ├─ DiaryManager.cs
  └─ DiaryPersistence.cs ← MISMO GameObject

✓ GameObject "InventorySystem" con:
  ├─ InventoryData.cs
  └─ InventoryPersistence.cs ← MISMO GameObject
      └─ availableItems[] asignado en Inspector (3 items)

✓ GameObject "DiaryUI" con:
  ├─ UIDocument (DiaryUI.uxml)
  └─ DiaryUI.cs

✓ GameObject "InventoryUI" con:
  ├─ UIDocument (Inventory.uxml)
  └─ InventoryUI.cs
```

---

## 🧪 Verificación en Consola

Al darle Play, deberías ver este orden en la consola:

```
1. [DiaryManager] 6 entradas cargadas desde JSON
2. [InventoryPersistence] Items disponibles registrados: 3
3. [GameManager] Managers inicializados: Diary=True, Inventory=True...
4. [GameManager] Inicializado. Usa LoadGame() para cargar partida guardada.
```

**Orden correcto:** Los managers se inicializan ANTES que GameManager los busque.

---

## 🎓 Buenas Prácticas

### ✅ DO (Hacer):
- Establecer Singleton en Awake()
- DontDestroyOnLoad en Awake()
- Inicialización interna en Awake() (cargar JSON, construir diccionarios)
- Obtener referencias externas en Start() (como hace GameManager)

### ❌ DON'T (No hacer):
- Acceder a otros Singletons en Awake() (orden no garantizado)
- Llamar a métodos de otros componentes en Awake()
- Usar FindFirstObjectByType() en Awake() para dependencias

---

## 📚 Referencias

- **GameManager.cs** - Orquestador central
- **InventoryData.cs** - Gestión de items en memoria
- **DiaryManager.cs** - Gestión del diario
- **InventoryPersistence.cs** - Guardado de inventario
- **DiaryPersistence.cs** - Guardado de diario

---

**Última actualización:** 2026-01-13
**Versión:** 2.0 - GameManager Orquestador con orden de inicialización garantizado
