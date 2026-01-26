# Configuración de la Escena de Autenticación

Esta guía explica cómo configurar la escena de inicio con Login y Registro en Unity.

## Archivos Creados

### Scripts
- `Assets/_Project/Scripts/UI/AuthManager.cs` - Coordina Login y Registro
- `Assets/_Project/Scripts/UI/LoginController.cs` - Maneja la UI de login (ya existía)
- `Assets/_Project/Scripts/UI/RegisterController.cs` - Maneja la UI de registro (ya existía)

### UI (UXML/USS)
- `Assets/_Project/UI/Menus/Login.uxml` - Estructura UI de login
- `Assets/_Project/UI/Menus/Login.uss` - Estilos de login
- `Assets/_Project/UI/Menus/Register.uxml` - Estructura UI de registro
- `Assets/_Project/UI/Menus/Register.uss` - Estilos de registro

## Pasos para Configurar la Escena

### 1. Abrir/Crear la Escena de Inicio

Puedes usar la escena existente `Assets/_Project/Scenes/UI/Home.unity` o crear una nueva.

### 2. Configurar los GameObjects

Crea la siguiente jerarquía en la escena:

```
AuthScene
├── TriskelAPIClient (prefab o GameObject con el script)
├── AuthManager
│   └── UIDocument (Login)
│   └── UIDocument (Register)
└── EventSystem (si usas UI Toolkit)
```

### 3. Configurar TriskelAPIClient

1. Añade el GameObject con el componente `TriskelAPIClient`
2. Configura la URL base del API (ej: `http://localhost:8000`)
3. Marca como DontDestroyOnLoad si quieres que persista entre escenas

### 4. Configurar AuthManager

1. Crea un GameObject vacío llamado "AuthManager"
2. Añade el componente `AuthManager`
3. Crea dos GameObjects hijos:
   - "LoginUI" con componente UIDocument
   - "RegisterUI" con componente UIDocument

### 5. Configurar LoginUI

1. Selecciona el GameObject "LoginUI"
2. En el componente UIDocument:
   - Asigna `Assets/_Project/UI/Menus/Login.uxml` como Source Asset
3. Añade el componente `LoginController`
4. En LoginController:
   - Asigna el UIDocument en el campo "UI Document"

### 6. Configurar RegisterUI

1. Selecciona el GameObject "RegisterUI"
2. En el componente UIDocument:
   - Asigna `Assets/_Project/UI/Menus/Register.uxml` como Source Asset
3. Añade el componente `RegisterController`
4. En RegisterController:
   - Asigna el UIDocument en el campo "UI Document"

### 7. Conectar AuthManager

1. Selecciona el GameObject "AuthManager"
2. En el componente AuthManager:
   - Arrastra "LoginUI" al campo "Login Controller"
   - Arrastra "RegisterUI" al campo "Register Controller"
   - Configura "Hub Scene Name" con el nombre de tu escena Hub (ej: "Hub")

## Estructura Final en el Inspector

### AuthManager GameObject
```
AuthManager (MonoBehaviour)
├── Login Controller: LoginUI
├── Register Controller: RegisterUI
└── Hub Scene Name: "Hub"
```

### LoginUI GameObject
```
LoginController (MonoBehaviour)
└── UI Document: (auto-asignado)

UIDocument
└── Source Asset: Login.uxml
```

### RegisterUI GameObject
```
RegisterController (MonoBehaviour)
└── UI Document: (auto-asignado)

UIDocument
└── Source Asset: Register.uxml
```

## Flujo de Navegación

1. **Inicio**: Se muestra la pantalla de Login
2. **Login → Register**: Click en "Crear cuenta"
3. **Register → Login**: Click en "Ya tengo cuenta"
4. **Login Exitoso**: Se carga el perfil y se navega al Hub
5. **Registro Exitoso**: Se carga el perfil y se navega al Hub

## Características Implementadas

### LoginController
- ✅ Validación de campos (username, password)
- ✅ Integración con TriskelAPIClient
- ✅ Verificación de sesión existente al inicio
- ✅ Manejo de errores con mensajes traducidos
- ✅ Navegación a pantalla de registro

### RegisterController
- ✅ Validación de campos (username, password, confirmación)
- ✅ Email opcional
- ✅ Validación de longitud y formato
- ✅ Integración con TriskelAPIClient
- ✅ Navegación a pantalla de login

### AuthManager
- ✅ Coordinación entre Login y Register
- ✅ Carga de perfil tras autenticación exitosa
- ✅ Inicio de sesión de juego si hay partida activa
- ✅ Navegación automática al Hub

## Configuración de Build Settings

Asegúrate de agregar las escenas en Build Settings en este orden:

1. `Assets/_Project/Scenes/UI/Home.unity` (índice 0)
2. `Assets/_Project/Scenes/Hub/Hub.unity` (índice 1)
3. Otras escenas de niveles...

## Notas Importantes

- El `TriskelAPIClient` debe estar en la escena o ser un prefab persistente
- Los archivos UXML/USS deben estar en la carpeta correcta para que Unity los encuentre
- La fuente BoldPixels debe estar importada en el proyecto
- El background `bg_box2.png` debe existir en `Assets/_Project/Sprites/UI/Common/Backgrounds/`

## Testing

Para probar la escena:

1. Ejecuta la escena de inicio
2. Deberías ver la pantalla de Login
3. Click en "Crear cuenta" → debería mostrar el registro
4. Click en "Ya tengo cuenta" → debería volver al login
5. Registra un usuario nuevo o inicia sesión
6. Tras login exitoso → debería cargar la escena Hub

## Troubleshooting

### "UIDocument no asignado"
- Asegúrate de que cada Controller tenga el UIDocument asignado

### "Elementos UI no encontrados"
- Verifica que los nombres en UXML coincidan con los que busca el script
- Nombres importantes: UserInput, PasswordInput, LoginButton, etc.

### "Error al conectar con API"
- Verifica que la URL base en TriskelAPIClient sea correcta
- Asegúrate de que el servidor API esté corriendo

### "No carga el Hub tras login"
- Verifica que el nombre de escena en AuthManager sea correcto
- Asegúrate de que la escena Hub esté en Build Settings
