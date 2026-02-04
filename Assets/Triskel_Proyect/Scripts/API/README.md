# Triskel API Client - Documentacion

Sistema de comunicacion con la API REST de Triskel para Unity.

---

## Estructura de Archivos

```
Assets/_Project/Scripts/API/
├── TriskelAPIClient.cs      # Clase principal (Singleton)
├── HttpService.cs           # Servicio HTTP interno
├── APIConstants.cs          # Constantes (niveles, decisiones, eventos)
├── README.md                # Esta documentacion
└── Models/
    ├── PlayerModels.cs      # DTOs de jugadores
    ├── GameModels.cs        # DTOs de partidas
    └── EventModels.cs       # DTOs de eventos
```

---

## Setup Inicial

### 1. Crear GameObject

En tu escena inicial (ej: `MainMenu`), crea un GameObject vacio:

```
GameObject: "TriskelAPI"
  └── TriskelAPIClient (Component)
```

### 2. Configurar en Inspector

| Campo | Valor |
|-------|-------|
| Base URL | `https://triskel-api.railway.app` (produccion) |
| Base URL | `http://localhost:8000` (desarrollo) |

### 3. El componente persiste entre escenas automaticamente (DontDestroyOnLoad)

---

## Uso Basico

### Importar el namespace

```csharp
using Triskel.API;
using Triskel.API.Models;
```

### Acceder al cliente

```csharp
TriskelAPIClient.Instance.MetodoQueQuieras();
```

---

## Flujos de Uso

### 1. Login / Registro

```csharp
void Start()
{
    // Verificar si ya hay sesion guardada
    if (TriskelAPIClient.Instance.IsLoggedIn)
    {
        // Validar que las credenciales siguen siendo validas
        TriskelAPIClient.Instance.VerifySession(
            onSuccess: profile => {
                Debug.Log($"Sesion valida: {profile.username}");
                CargarMenuPrincipal();
            },
            onError: error => {
                Debug.Log("Sesion expirada, mostrar registro");
                MostrarPantallaRegistro();
            }
        );
    }
    else
    {
        MostrarPantallaRegistro();
    }
}

// Cuando el usuario introduce su nombre
public void Registrar(string username)
{
    TriskelAPIClient.Instance.RegisterPlayer(
        username: username,
        email: null,  // Opcional
        onSuccess: response => {
            Debug.Log($"Registrado: {response.username}");
            Debug.Log($"Player ID: {response.player_id}");
            // Las credenciales se guardan automaticamente
            CargarMenuPrincipal();
        },
        onError: error => {
            Debug.LogError($"Error de registro: {error}");
        }
    );
}

// Cerrar sesion
public void CerrarSesion()
{
    TriskelAPIClient.Instance.Logout();
}
```

---

### 2. Menu Principal - Gestionar Partidas

```csharp
// Obtener partidas activas (para boton "Continuar")
public void CargarOpcionesMenu()
{
    TriskelAPIClient.Instance.GetActiveGames(
        onSuccess: partidas => {
            if (partidas.Length > 0)
            {
                // Hay partidas en progreso
                mostrarBotonContinuar = true;
                ultimaPartida = partidas[0];
            }
        },
        onError: error => {
            Debug.LogError(error);
        }
    );
}

// Crear nueva partida
public void NuevaPartida()
{
    TriskelAPIClient.Instance.CreateGame(
        onSuccess: game => {
            Debug.Log($"Partida creada: {game.game_id}");
            SceneManager.LoadScene("HubCentral");
        },
        onError: error => {
            Debug.LogError($"Error: {error}");
        }
    );
}

// Continuar partida existente
public void ContinuarPartida(string gameId)
{
    TriskelAPIClient.Instance.LoadGame(gameId);

    // Obtener datos para saber donde estaba
    TriskelAPIClient.Instance.GetCurrentGame(
        onSuccess: game => {
            SceneManager.LoadScene(game.current_level);
        }
    );
}
```

---

### 3. Gameplay - Niveles

```csharp
// Al entrar a un nivel
public void OnEntrarNivel(string nombreNivel)
{
    TriskelAPIClient.Instance.StartLevel(nombreNivel);
}

// Al completar un nivel
public void OnCompletarNivel()
{
    TriskelAPIClient.Instance.CompleteLevel(
        level: "senda_ebano",
        timeSeconds: 342,           // Tiempo en segundos
        deaths: 5,                  // Muertes en este nivel
        choice: "sanar",            // Decision moral (opcional)
        relic: "lirio",             // Reliquia obtenida (opcional)
        onSuccess: game => {
            Debug.Log($"Progreso: {game.completion_percentage}%");
        }
    );
}
```

---

### 4. Gameplay - Eventos

```csharp
// Muerte del jugador
public void OnPlayerDeath(Vector2 posicion, string causa)
{
    TriskelAPIClient.Instance.SendDeathEvent(
        level: "senda_ebano",
        cause: causa,               // "fall", "enemy_attack", "trap"
        position: posicion,
        enemyType: "goblin"         // Opcional
    );
}

// Checkpoint alcanzado
public void OnCheckpoint(string checkpointId)
{
    TriskelAPIClient.Instance.SendCheckpointEvent(
        level: "senda_ebano",
        checkpointId: checkpointId
    );
}

// Item/Reliquia recogida
public void OnItemCollected(string itemType, string relicName)
{
    TriskelAPIClient.Instance.SendItemCollectedEvent(
        level: "senda_ebano",
        itemType: itemType,         // "relic", "key", etc
        relicName: relicName        // "lirio", "hacha", "manto"
    );
}

// Interaccion con NPC
public void OnNPCInteraction(string npcId)
{
    TriskelAPIClient.Instance.SendNPCInteractionEvent(
        level: "senda_ebano",
        npcId: npcId,
        action: "talk"
    );
}

// Encuentro con jefe
public void OnBossEncounter()
{
    TriskelAPIClient.Instance.SendBossEncounterEvent(
        level: "claro_almas",
        bossName: "guardian"
    );
}
```

---

### 5. Final del Juego

```csharp
public void OnBossDefeated(int numeroFinal)
{
    // 1. Registrar que final obtuvo
    TriskelAPIClient.Instance.SendGameEndingEvent(numeroFinal);

    // 2. Marcar partida como completada
    TriskelAPIClient.Instance.CompleteGame(
        bossDefeated: true,
        onSuccess: game => {
            Debug.Log("Partida completada!");
            MostrarCinematicaFinal(numeroFinal);
        }
    );
}

// Calcular el final basado en decisiones
public int CalcularFinal(GameChoices choices)
{
    int buenas = 0;
    if (choices.senda_ebano == "sanar") buenas++;
    if (choices.fortaleza_gigantes == "construir") buenas++;
    if (choices.aquelarre_sombras == "revelar") buenas++;

    return APIConstants.Endings.CalculateEnding(buenas);
    // 3 buenas = Final 1, 2 = Final 2, 1 = Final 3, 0 = Final 4
}
```

---

## Constantes Disponibles

Usa `APIConstants` para evitar errores de escritura:

### Niveles

```csharp
APIConstants.Levels.HUB_CENTRAL         // "hub_central"
APIConstants.Levels.SENDA_EBANO         // "senda_ebano"
APIConstants.Levels.FORTALEZA_GIGANTES  // "fortaleza_gigantes"
APIConstants.Levels.AQUELARRE_SOMBRAS   // "aquelarre_sombras"
APIConstants.Levels.CLARO_ALMAS         // "claro_almas"
```

### Estados de Partida

```csharp
APIConstants.GameStatus.IN_PROGRESS     // "in_progress"
APIConstants.GameStatus.COMPLETED       // "completed"
APIConstants.GameStatus.ABANDONED       // "abandoned"
```

### Decisiones Morales

```csharp
// Senda Ebano
APIConstants.Choices.SANAR              // "sanar" (buena)
APIConstants.Choices.FORZAR             // "forzar" (mala)

// Fortaleza Gigantes
APIConstants.Choices.CONSTRUIR          // "construir" (buena)
APIConstants.Choices.DESTRUIR           // "destruir" (mala)

// Aquelarre Sombras
APIConstants.Choices.REVELAR            // "revelar" (buena)
APIConstants.Choices.OCULTAR            // "ocultar" (mala)

// Verificar si es buena
APIConstants.Choices.IsGoodChoice("sanar")  // true
```

### Reliquias

```csharp
APIConstants.Relics.LIRIO               // "lirio" (Senda Ebano)
APIConstants.Relics.HACHA               // "hacha" (Fortaleza Gigantes)
APIConstants.Relics.MANTO               // "manto" (Aquelarre Sombras)
```

### Tipos de Evento

```csharp
APIConstants.EventTypes.PLAYER_DEATH        // "player_death"
APIConstants.EventTypes.LEVEL_START         // "level_start"
APIConstants.EventTypes.LEVEL_END           // "level_end"
APIConstants.EventTypes.CHECKPOINT_REACHED  // "checkpoint_reached"
APIConstants.EventTypes.ITEM_COLLECTED      // "item_collected"
APIConstants.EventTypes.NPC_INTERACTION     // "npc_interaction"
APIConstants.EventTypes.BOSS_ENCOUNTER      // "boss_encounter"
APIConstants.EventTypes.CUSTOM_EVENT        // "custom_event"
```

### Causas de Muerte

```csharp
APIConstants.DeathCauses.FALL           // "fall"
APIConstants.DeathCauses.ENEMY_ATTACK   // "enemy_attack"
APIConstants.DeathCauses.TRAP           // "trap"
APIConstants.DeathCauses.BOSS           // "boss"
APIConstants.DeathCauses.ENVIRONMENTAL  // "environmental"
```

---

## Propiedades Utiles

```csharp
// ID del jugador actual
string id = TriskelAPIClient.Instance.PlayerID;

// Token del jugador (no lo muestres al usuario)
string token = TriskelAPIClient.Instance.PlayerToken;

// ID de la partida actual
string gameId = TriskelAPIClient.Instance.CurrentGameID;

// Verificar si hay sesion activa
bool loggedIn = TriskelAPIClient.Instance.IsLoggedIn;
```

---

## Eventos del Cliente

```csharp
void Start()
{
    // Suscribirse a eventos
    TriskelAPIClient.Instance.OnError += OnAPIError;
    TriskelAPIClient.Instance.OnLoggedIn += OnLogin;
    TriskelAPIClient.Instance.OnLoggedOut += OnLogout;
}

void OnAPIError(string error)
{
    Debug.LogError($"Error de API: {error}");
    // Mostrar mensaje al usuario
}

void OnLogin()
{
    Debug.Log("Usuario logueado");
}

void OnLogout()
{
    Debug.Log("Usuario deslogueado");
    // Volver a pantalla de inicio
}
```

---

## Debug

En el Inspector del componente `TriskelAPIClient`, hay opciones de debug:

- **Debug: Test Connection** - Verifica la conexion con la API
- **Debug: Clear All Data** - Borra todas las credenciales guardadas

---

## Diagrama de Flujo Completo

```
┌─────────────────────────────────────────────────────────────┐
│                      INICIO DE APP                          │
└─────────────────────────────────────────────────────────────┘
                              │
                              ▼
                    ┌─────────────────┐
                    │  IsLoggedIn?    │
                    └─────────────────┘
                     │              │
                    NO             YES
                     │              │
                     ▼              ▼
            ┌────────────┐  ┌────────────────┐
            │ Registro   │  │ VerifySession  │
            │ UI         │  └────────────────┘
            └────────────┘         │
                   │          ┌────┴────┐
                   │         OK       ERROR
                   │          │         │
                   ▼          │         ▼
          ┌────────────────┐  │  ┌────────────┐
          │ RegisterPlayer │  │  │ Limpiar    │
          └────────────────┘  │  │ credencial │
                   │          │  └────────────┘
                   │          │         │
                   └──────────┴─────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│                     MENU PRINCIPAL                          │
├─────────────────────────────────────────────────────────────┤
│  [Nueva Partida]              [Continuar] (si hay activa)   │
└─────────────────────────────────────────────────────────────┘
         │                              │
         ▼                              ▼
   CreateGame()                   LoadGame(id)
         │                              │
         └──────────────┬───────────────┘
                        │
                        ▼
┌─────────────────────────────────────────────────────────────┐
│                       GAMEPLAY                              │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│  Entrar nivel ──────────► StartLevel("nivel")               │
│                                                             │
│  Muerte ────────────────► SendDeathEvent(...)               │
│                                                             │
│  Checkpoint ────────────► SendCheckpointEvent(...)          │
│                                                             │
│  Recoger item ──────────► SendItemCollectedEvent(...)       │
│                                                             │
│  Hablar NPC ────────────► SendNPCInteractionEvent(...)      │
│                                                             │
│  Completar nivel ───────► CompleteLevel(...)                │
│                                                             │
└─────────────────────────────────────────────────────────────┘
                        │
                        ▼
┌─────────────────────────────────────────────────────────────┐
│                    JEFE FINAL                               │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│  Encuentro ─────────────► SendBossEncounterEvent(...)       │
│                                                             │
│  Victoria ──────────────► SendGameEndingEvent(final)        │
│                          CompleteGame()                     │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

---

## Notas Importantes

1. **Las credenciales se guardan automaticamente** en `PlayerPrefs` al registrar
2. **El cliente es Singleton** - accede siempre via `TriskelAPIClient.Instance`
3. **Los eventos son asincronos** - usa callbacks `onSuccess` y `onError`
4. **La partida actual se guarda** - si el jugador cierra el juego, puede continuar
5. **Usa las constantes** - evita errores de escritura en strings

---

## Errores Comunes

| Error | Causa | Solucion |
|-------|-------|----------|
| "No hay partida activa" | Llamar a metodos de nivel sin `CreateGame()` | Crear partida primero |
| "No hay credenciales" | Llamar a metodos sin registrar | Registrar jugador primero |
| 401 Unauthorized | Token invalido o expirado | Llamar a `Logout()` y re-registrar |
| 404 Not Found | ID de partida/jugador no existe | Verificar IDs |
| Connection refused | API no disponible | Verificar URL y que la API este corriendo |
