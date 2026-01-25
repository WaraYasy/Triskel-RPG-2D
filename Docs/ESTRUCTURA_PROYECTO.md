# Estructura de Proyecto Unity - 4 Niveles + Hub

## 📁 Estructura Recomendada

```
tu-proyecto-unity/
│
├── .gitignore
├── README.md
├── CHANGELOG.md
├── LICENSE
│
├── Assets/
│   ├── _Project/                          # Todo tu código del proyecto aquí
│   │   │
│   │   ├── Scenes/                        # Escenas del juego
│   │   │   ├── Hub/
│   │   │   │   └── MainHub.unity
│   │   │   ├── Levels/
│   │   │   │   ├── Level01.unity
│   │   │   │   ├── Level02.unity
│   │   │   │   ├── Level03.unity
│   │   │   │   └── Level04.unity
│   │   │   └── UI/
│   │   │       ├── MainMenu.unity
│   │   │       └── LoadingScreen.unity
│   │   │
│   │   ├── Scripts/                       # Scripts organizados por funcionalidad
│   │   │   ├── Core/                      # Sistemas base del juego
│   │   │   │   ├── GameManager.cs
│   │   │   │   ├── LevelManager.cs
│   │   │   │   └── SceneLoader.cs
│   │   │   │
│   │   │   ├── Player/                    # Todo relacionado con el jugador
│   │   │   │   ├── PlayerController.cs
│   │   │   │   ├── PlayerMovement.cs
│   │   │   │   ├── PlayerStats.cs
│   │   │   │   └── PlayerInventory.cs
│   │   │   │
│   │   │   ├── Characters/                # NPCs y otros personajes
│   │   │   │   ├── CharacterBase.cs
│   │   │   │   ├── NPCController.cs
│   │   │   │   ├── EnemyAI.cs
│   │   │   │   └── CharacterAnimator.cs
│   │   │   │
│   │   │   ├── Hub/                       # Lógica específica del Hub
│   │   │   │   ├── HubManager.cs
│   │   │   │   ├── LevelSelector.cs
│   │   │   │   └── HubInteractables.cs
│   │   │   │
│   │   │   ├── Levels/                    # Lógica de niveles
│   │   │   │   ├── LevelController.cs
│   │   │   │   ├── ObjectiveManager.cs
│   │   │   │   ├── CheckpointSystem.cs
│   │   │   │   └── LevelCompletion.cs
│   │   │   │
│   │   │   ├── API/                       # 🔌 Conexión con tu API
│   │   │   │   ├── ApiManager.cs          # Gestor principal de API
│   │   │   │   ├── ApiClient.cs           # Cliente HTTP (UnityWebRequest)
│   │   │   │   ├── Endpoints/             # Endpoints organizados
│   │   │   │   │   ├── AuthEndpoints.cs
│   │   │   │   │   ├── PlayerEndpoints.cs
│   │   │   │   │   ├── LevelEndpoints.cs
│   │   │   │   │   └── LeaderboardEndpoints.cs
│   │   │   │   ├── Models/                # Modelos de datos (DTOs)
│   │   │   │   │   ├── UserData.cs
│   │   │   │   │   ├── LevelData.cs
│   │   │   │   │   ├── PlayerProgress.cs
│   │   │   │   │   └── ApiResponse.cs
│   │   │   │   └── Utils/
│   │   │   │       ├── JsonHelper.cs
│   │   │   │       └── ApiConfig.cs       # URLs, tokens, configuración
│   │   │   │
│   │   │   ├── UI/                        # Scripts de interfaz
│   │   │   │   ├── Menus/
│   │   │   │   │   ├── MainMenuController.cs
│   │   │   │   │   ├── PauseMenu.cs
│   │   │   │   │   └── SettingsMenu.cs
│   │   │   │   ├── HUD/
│   │   │   │   │   ├── HealthBar.cs
│   │   │   │   │   ├── ScoreDisplay.cs
│   │   │   │   │   └── MinimapController.cs
│   │   │   │   └── Dialogs/
│   │   │   │       └── DialogueSystem.cs
│   │   │   │
│   │   │   ├── Audio/                     # Sistema de audio
│   │   │   │   ├── AudioManager.cs
│   │   │   │   └── MusicController.cs
│   │   │   │
│   │   │   └── Utils/                     # Utilidades generales
│   │   │       ├── Singleton.cs
│   │   │       ├── ObjectPool.cs
│   │   │       └── Constants.cs
│   │   │
│   │   ├── Prefabs/                       # Prefabs organizados
│   │   │   ├── Characters/
│   │   │   │   ├── Player.prefab
│   │   │   │   └── Enemies/
│   │   │   ├── Environment/
│   │   │   ├── UI/
│   │   │   └── Managers/
│   │   │       └── GameManager.prefab
│   │   │
│   │   ├── Materials/                     # Materiales del proyecto
│   │   │   ├── Characters/
│   │   │   └── Environment/
│   │   │
│   │   ├── Models/                        # Modelos 3D
│   │   │   ├── Characters/
│   │   │   └── Environment/
│   │   │
│   │   ├── Animations/                    # Animaciones y controladores
│   │   │   ├── Player/
│   │   │   └── Characters/
│   │   │
│   │   ├── Audio/                         # Archivos de audio
│   │   │   ├── Music/
│   │   │   ├── SFX/
│   │   │   └── Voices/
│   │   │
│   │   ├── Sprites/                       # Sprites e imágenes 2D
│   │   │   └── UI/
│   │   │
│   │   └── Resources/                     # Assets cargados dinámicamente
│   │       └── Config/
│   │           └── GameSettings.asset
│   │
│   ├── Plugins/                           # Plugins de terceros
│   │   └── (DOTween, etc.)
│   │
│   └── TextMesh Pro/                      # TMP (si lo usas)
│
├── Packages/                              # Package Manager
│   └── manifest.json
│
├── ProjectSettings/                       # Configuración de Unity
│
└── Documentation/                         # 📚 Documentación del proyecto
    ├── API_INTEGRATION.md                 # Guía de integración API
    ├── GAME_DESIGN.md                     # Documento de diseño
    ├── SETUP.md                           # Instrucciones de setup
    └── ARCHITECTURE.md                    # Arquitectura del código
```

---

## 🔑 Conceptos Clave de la Estructura

### 1. **Carpeta `_Project/`**
- Prefijo `_` mantiene tu código al principio en Unity
- Todo tu código personalizado va aquí
- Fácil de identificar vs assets de terceros

### 2. **Scripts Organizados por Dominio**
- `Core/` → Sistemas fundamentales
- `Player/` → Todo del jugador
- `Characters/` → NPCs y enemigos
- `API/` → Comunicación con backend
- `UI/` → Interfaz de usuario

### 3. **Gestión de API (Estructura Recomendada)**

```csharp
// API/ApiManager.cs - Singleton principal
public class ApiManager : MonoBehaviour
{
    public static ApiManager Instance { get; private set; }
    
    private ApiClient client;
    
    public AuthEndpoints Auth { get; private set; }
    public PlayerEndpoints Player { get; private set; }
    public LevelEndpoints Level { get; private set; }
}

// API/ApiClient.cs - Cliente HTTP
public class ApiClient
{
    private string baseUrl;
    private string authToken;
    
    public async Task<T> Get<T>(string endpoint) { }
    public async Task<T> Post<T>(string endpoint, object data) { }
}

// API/Endpoints/LevelEndpoints.cs
public class LevelEndpoints
{
    private ApiClient client;
    
    public async Task<LevelData> GetLevelData(int levelId) { }
    public async Task<bool> SaveProgress(PlayerProgress progress) { }
}
```

---

## 📋 Archivos Importantes en la Raíz

### **README.md** (Ejemplo)
```markdown
# Nombre del Proyecto

## Descripción
Juego de 4 niveles + hub con integración API

## Características
- 4 niveles jugables
- Sistema de hub central
- Integración con API REST
- Sistema de personajes

## Requisitos
- Unity 2022.3 LTS o superior
- .NET Framework 4.x

## Instalación
1. Clonar repositorio
2. Abrir con Unity
3. Configurar API_URL en ApiConfig.cs

## Estructura del Proyecto
Ver Documentation/ARCHITECTURE.md
```

### **CHANGELOG.md**
```markdown
# Changelog

## [1.0.0] - 2026-01-05
### Added
- Sistema de hub implementado
- 4 niveles completados
- Integración API completa
```

---

## 🎯 Mejores Prácticas

### **Namespaces en C#**
```csharp
namespace TuProyecto.Core { }
namespace TuProyecto.Player { }
namespace TuProyecto.API { }
namespace TuProyecto.UI { }
```

### **ScriptableObjects para Configuración**
```csharp
// Resources/Config/GameSettings.asset
[CreateAssetMenu(fileName = "GameSettings", menuName = "Config/Game Settings")]
public class GameSettings : ScriptableObject
{
    public string apiUrl;
    public int maxLevels = 4;
    public float playerSpeed = 5f;
}
```

### **Patrón de Eventos para Desacoplar**
```csharp
// Evita dependencias directas entre sistemas
public static class GameEvents
{
    public static event Action<int> OnLevelCompleted;
    public static event Action<PlayerData> OnPlayerDataUpdated;
}
```

---

## 🔐 Seguridad API

### **NO subir a GitHub:**
- API Keys
- Tokens de autenticación
- Contraseñas

### **Usar:**
```csharp
// ApiConfig.cs - Lee de archivo local o variables de entorno
public class ApiConfig
{
    #if UNITY_EDITOR
        public const string API_URL = "http://localhost:3000";
    #else
        public const string API_URL = "https://api.tudominio.com";
    #endif
}
```

Agregar a `.gitignore`:
```
**/ApiKeys.cs
**/Secrets.json
```

---

## 📦 Gestión de Ramas Git

```bash
main              # Producción estable
├── develop       # Desarrollo activo
    ├── feature/hub-system
    ├── feature/level-1
    ├── feature/api-integration
    └── feature/character-system
```

---

## 🚀 Próximos Pasos

1. **Crea el README.md** con info de tu proyecto
2. **Organiza tus scripts** según esta estructura
3. **Implementa ApiManager** como singleton
4. **Documenta tus endpoints** en API_INTEGRATION.md
5. **Usa ScriptableObjects** para configuración
6. **Implementa sistema de eventos** para desacoplar código
