# 🎮 Triskel RPG 2D

<div align="center">

<img src="Assets/_Project/Sprites/UI/Icons/triskel_logo.png" alt="Triskel Logo" width="200"/>

![Unity](https://img.shields.io/badge/Unity-2022.3_LTS-000000?style=for-the-badge&logo=unity&logoColor=white)
![C#](https://img.shields.io/badge/C%23-.NET_4.x-239120?style=for-the-badge&logo=c-sharp&logoColor=white)
![Platform](https://img.shields.io/badge/Platform-Windows%20%7C%20Android-blue?style=for-the-badge)

**Un RPG 2D narrativo con mecánicas de exploración, combate y decisiones morales**

[🎯 Características](#-características-principales) • [🚀 Instalación](#-instalación) • [📖 Documentación](#-documentación) • [🤝 Créditos](#-créditos)

</div>

---

## 📋 Descripción

**Triskel RPG 2D** es un juego de rol 2D desarrollado en Unity como proyecto educativo del instituto Ethazi. El jugador explora un mundo mágico, enfrenta enemigos, toma decisiones morales que afectan la narrativa y recolecta reliquias mágicas con habilidades únicas.

El juego combina:
- 🗺️ **Exploración** de múltiples niveles interconectados
- 📖 **Narrativa** ramificada con sistema de diálogos
- 🎭 **Decisiones morales** que determinan el final del juego
- ✨ **Reliquias mágicas** con habilidades especiales
- 📊 **Integración API** para tracking de progreso y estadísticas

---

## ✨ Características Principales

### 🎮 Gameplay

- **Sistema de Movimiento**: Controles fluidos con soporte para teclado/gamepad y controles táctiles
- **Mecánica de Dash**: Impulso rápido con cooldown (3x velocidad)
- **Sistema de Salud**: Gestión de vida con UI visual
- **IA de Enemigos**: Fantasmas atraídos por la luz del jugador
- **Jefes Finales**: Patrones de ataque complejos con zonas de peligro

### 🎭 Sistemas Narrativos

- **Diálogos Interactivos**: Sistema basado en Yarn Spinner con árboles de decisión
- **Diario del Jugador**: Entradas narrativas desbloqueables según progreso
- **Decisiones Morales**: 3 elecciones clave que determinan 4 finales posibles
- **Transiciones Cinemáticas**: Textos narrativos entre niveles

### ✨ Reliquias Mágicas

1. **🌸 Lirio de Luz** (Senda del Ébano)
   - Emite luz que atrae enemigos
   - Revela caminos ocultos

2. **🪓 Hacha Ancestral** (Fortaleza de los Gigantes)
   - Habilidad de combate mejorada
   - Destruye obstáculos especiales

3. **🌙 Manto de Sombras** (Aquelarre de las Sombras)
   - Habilidad de ocultación
   - Interacción con elementos mágicos

### 🌐 Integración API

- **Autenticación**: Sistema de login/registro con backend REST
- **Guardado en la Nube**: Progreso sincronizado automáticamente
- **Partidas Múltiples**: Soporte para continuar partidas guardadas
- **Analytics**: Tracking de eventos de gameplay (muertes, decisiones, tiempo)
- **Sesiones**: Registro de tiempo jugado y plataforma

---

## 🛠️ Tecnologías

### Motor y Lenguaje
- **Unity 2022.3 LTS** con Universal Render Pipeline (URP) v17.0.4
- **C# .NET Framework 4.x**

### Paquetes y Sistemas
- **Input System 1.14.2** - Control multiplataforma (teclado, gamepad, táctil)
- **Cinemachine 3.1.5** - Sistema de cámaras dinámicas
- **Yarn Spinner 3.1.3** - Sistema de diálogos narrativos
- **UI Toolkit** - Interfaces modernas con UXML/USS
- **UnityWebRequest** - Comunicación HTTP con API REST

### Backend
- **API REST** personalizada en `http://localhost:8000` (desarrollo)
- Endpoints para jugadores, partidas, sesiones y eventos
- Autenticación con tokens personalizados

---

## 📁 Estructura del Proyecto

```
Assets/_Project/
├── 📂 Scenes/
│   ├── UI/              # Menús, login, transiciones
│   ├── Hub/             # Área central (3 escenas interconectadas)
│   └── Levels/          # 4 niveles jugables
│
├── 📂 Scripts/
│   ├── Core/            # GameManager, Diary, Inventory, Dialogue
│   ├── Player/          # Movimiento, salud, reliquias
│   ├── Enemies/         # IA de enemigos
│   ├── Boss/            # Sistema de jefes
│   ├── API/             # Cliente REST y modelos de datos
│   ├── UI/              # Controladores de interfaces
│   └── Audio/           # Gestión de música y SFX
│
├── 📂 Prefabs/          # Objetos reutilizables
├── 📂 Audio/            # Música, efectos de sonido y voces
├── 📂 Resources/        # DiaryData, TransitionData (JSON)
└── 📂 Animations/       # Controladores de animación
```

---

## 🚀 Instalación

### Requisitos Previos

- **Unity 2022.3 LTS** o superior
- **Visual Studio 2022** o **Visual Studio Code** con extensiones de C#
- **.NET Framework 4.x**
- **(Opcional)** Backend API corriendo en `localhost:8000`

### Pasos de Instalación

1. **Clonar el repositorio**
   ```bash
   git clone https://github.com/tu-usuario/triskel-rpg-2d.git
   cd triskel-rpg-2d
   ```

2. **Abrir en Unity Hub**
   - Abre Unity Hub
   - Click en "Add" → Selecciona la carpeta del proyecto
   - Asegúrate de usar Unity 2022.3 LTS

3. **Configurar la API (Opcional)**
   - Si tienes el backend, inícialo en `http://localhost:8000`
   - En Unity, ve a `GameManager` → Configurar `baseURL` en el Inspector

4. **Ejecutar el juego**
   - Abre la escena `Assets/_Project/Scenes/UI/Home.unity`
   - Presiona Play ▶️

---

## 🎯 Cómo Jugar

### Controles de Escritorio

| Acción | Teclado | Gamepad |
|--------|---------|---------|
| **Movimiento** | WASD / Flechas | Stick Izquierdo |
| **Dash** | Espacio | Botón A |
| **Interactuar** | E | Botón X |
| **Pausa** | ESC / P | Start |

### Controles Móviles

- **Joystick virtual** para movimiento
- **Botón de dash** en pantalla
- **Botón de pausa** en esquina superior

### Flujo del Juego

1. **🔐 Login/Registro** - Crea una cuenta o inicia sesión
2. **🏠 Hub Central** - Explora y habla con NPCs
3. **🗺️ Niveles** - Completa 4 niveles en orden:
   - Senda del Ébano
   - Fortaleza de los Gigantes
   - Aquelarre de las Sombras
   - Claro de las Almas (Final)
4. **⚖️ Decisiones** - Toma decisiones morales en cada nivel
5. **🏆 Final** - Obtén uno de 4 finales según tus elecciones

---

## 🎮 Sistemas Principales

### 🎛️ GameManager (Singleton)
Orquestador central con persistencia `DontDestroyOnLoad`. Gestiona:
- Referencias a sistemas principales
- Carga/guardado de partidas
- Transiciones entre escenas

### 📦 Sistema de Inventario
- Recogida automática de reliquias
- Persistencia con ScriptableObjects
- UI visual de ítems equipados

### 📖 Sistema de Diario
- Entradas narrativas desbloqueables
- Almacenamiento en JSON (`DiaryData.json`)
- Persistencia entre sesiones

### 🎬 Sistema de Transiciones
- Textos narrativos entre niveles
- Pantalla de muerte con reinicio automático
- Basado en UI Toolkit (UXML/USS)
- Textos configurables en JSON

### 🗣️ Sistema de Diálogos
- Árboles de decisión con Yarn Spinner
- Persistencia en memoria para el hub
- Diálogos únicos o repetibles
- Temas personalizables

### ⚙️ Sistema de Configuración
- Volumen de música (0-100%)
- Volumen de efectos de sonido (0-100%)
- Tamaño de fuente (Normal/Grande) para accesibilidad
- Persistencia automática en PlayerPrefs

### 🌐 Integración API

#### Flujo de Nueva Partida
```
Login → CreateGame() → StartSession() → Jugar → UpdateGame() cada 30s
```

#### Flujo de Continuar Partida
```
Login (detecta active_game_id) → GetGame() → Restaurar estado → StartSession()
```

#### Eventos Registrados
- Muertes del jugador (con causa y posición)
- Checkpoints alcanzados
- Ítems recogidos
- Interacciones con NPCs
- Encuentros con jefes
- Decisiones morales

---

## 📖 Documentación

El proyecto incluye **13 guías completas** en español:

### Guías de Sistemas
1. `ESTRUCTURA_PROYECTO.md` - Organización y buenas prácticas
2. `ARQUITECTURA_GAMEMANAGER.md` - Orden de inicialización
3. `GUIA_SISTEMAS_NUEVOS.md` - Inventario y diario
4. `GUIA_FANTASMAS_LIRIO.md` - Mecánicas de IA
5. `GUIA_ANIMACIONES.md` - Sistema de animaciones 8 direcciones
6. `GUIA_TRANSICIONES_NIVEL.md` - Sistema de transiciones

### Guías Técnicas
7. `GUIA_MUSICA.md` - Integración de audio
8. `GUIA_CINEMACHINE.md` - Sistema de cámaras
9. `GUIA_INPUT_SYSTEM_SETUP.md` - Fundamentos de Input System
10. `GUIA_CONTROLES_MOVILES_INPUT_SYSTEM.md` - Input móvil

### Guías de Testing y Setup
11. `DIARY_TESTING_GUIDE.md` - Testing del diario
12. `SETUP_INICIAL.md` - Setup inicial del jugador
13. `SETUP_AUTH_SCENE.md` - Configuración de autenticación

### Documentación de la API
Ubicación: `C:\Users\Wara\Documents\Ethazi\Api\TriskelApi\docs\`

- `GAME_INTEGRATION_API.md` - Guía completa de integración
- `UNITY_INTEGRATION.md` - Ejemplos específicos de Unity
- `UNITY_QUICK_START.md` - Código listo para copiar
- `QUICK_REFERENCE.md` - Referencia rápida de endpoints
- `RESUMEGAME_FLOWCHART.md` - Diagramas de flujo

---

## 🎨 Assets y Recursos

### Arte y Animaciones
- **Sprites 2D** con animaciones de 8 direcciones
- **Tilesets** para niveles
- **Efectos visuales** para habilidades y magia

### Audio
- **Música de fondo** por nivel/área
- **Efectos de sonido** contextuales
- **Voces** para diálogos importantes
- Formatos: OGG, MP3, WAV

### Fuentes
- Sistema de tamaño configurable (Normal/Grande)
- Integrado con sistema de accesibilidad

---

## 🌟 Características Destacadas

### ♿ Accesibilidad
- Tamaño de fuente ajustable
- Controles remapeables
- UI clara y legible
- Soporte para múltiples dispositivos de entrada

### 📱 Multiplataforma
- Windows (teclado/gamepad)
- Android (controles táctiles)
- Sistema de input unificado

### 🔒 Seguridad
- Autenticación con tokens
- Credenciales almacenadas localmente (cifradas en PlayerPrefs)
- Validación de sesiones

### 📊 Analytics
- Tracking completo de eventos
- Métricas de gameplay
- Estadísticas por jugador
- Dashboard de progreso (backend)

---

## 🐛 Resolución de Problemas

### El juego no inicia
- Verifica que usas Unity 2022.3 LTS
- Asegúrate de abrir la escena `Home.unity`

### Error de conexión API
- Verifica que el backend esté corriendo
- Comprueba la URL en GameManager (`baseURL`)
- Revisa logs de consola para detalles

### Controles no responden
- Verifica que Input System esté instalado
- Revisa la configuración en Edit → Project Settings → Input System Package

### Audio no se escucha
- Verifica volumen en menú de Settings
- Comprueba que los AudioSource tengan `SFXVolumeListener` (para SFX)

---

## 🤝 Créditos

### Desarrollo
- [@GaizkaDM](https://github.com/GaizkaDM)
- [@UnaiZugaza](https://github.com/UnaiZugaza)
- [@WaraYasy](https://github.com/WaraYasy)

### Herramientas y Librerías
- Unity Technologies - Motor Unity
- Yarn Spinner Team - Sistema de diálogos
- Cinemachine - Sistema de cámaras

---

## 🔗 Enlaces Útiles

- 📚 [Documentación del Proyecto](./docs/)
- 🐛 [Reportar Bug](https://github.com/tu-usuario/triskel-rpg-2d/issues)
- 💡 [Sugerir Funcionalidad](https://github.com/tu-usuario/triskel-rpg-2d/issues/new)

---

<div align="center">

**Hecho con ❤️ por Madrágora**


</div>
