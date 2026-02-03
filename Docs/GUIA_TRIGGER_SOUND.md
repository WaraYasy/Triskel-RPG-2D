# Guía: Sistema de Trigger Sound

Esta guía explica cómo crear objetos de sonido que se reproduzcan automáticamente cuando el jugador pasa por ellos.

## 📋 Índice

1. [¿Qué es TriggerSound?](#qué-es-triggersound)
2. [Setup Básico](#setup-básico)
3. [Configuración Avanzada](#configuración-avanzada)
4. [Ejemplos Prácticos](#ejemplos-prácticos)
5. [Métodos Públicos](#métodos-públicos)
6. [Troubleshooting](#troubleshooting)

---

## 🎵 ¿Qué es TriggerSound?

**TriggerSound** es un componente que reproduce un sonido automáticamente cuando el jugador pasa por una zona específica.

**Casos de uso:**
- Sonidos ambientales al entrar en una zona (pájaros, viento, agua)
- Efectos de sonido al pasar por puertas o pasajes
- Triggers de audio para narrativa ambiental
- Sonidos de advertencia al entrar en zonas peligrosas

---

## ⚙️ Setup Básico

### Paso 1: Crear el GameObject

1. En la jerarquía, click derecho → **Create Empty**
2. Renombra el objeto: `TriggerSound_Ambiente` (o el nombre que prefieras)
3. Posiciona el objeto donde quieras que se active el sonido

### Paso 2: Añadir Componentes

Añade los siguientes componentes al GameObject:

1. **Collider2D** (BoxCollider2D o CircleCollider2D)
   - Marca **"Is Trigger"** ✅
   - Ajusta el tamaño para definir la zona de activación

2. **Audio Source** (se añade automáticamente)
   - No necesitas configurar nada manualmente

3. **SFXVolumeListener** (se añade automáticamente)
   - Controla el volumen global de efectos de sonido

4. **TriggerSound** (Script)
   - Este es el componente principal

### Paso 3: Configurar el Script TriggerSound

En el Inspector:

```
TriggerSound (Script)
├─ Configuración de Sonido
│  └─ Sound Clip: [Arrastra tu AudioClip aquí]
├─ Opciones de Reproducción
│  ├─ Play Once: ❌ (si quieres que se repita cada vez)
│  ├─ Play On Enter: ✅ (reproduce al entrar)
│  └─ Destroy Delay: 2 (segundos)
├─ Variación de Sonido (Opcional)
│  ├─ Randomize Volume: ❌
│  ├─ Randomize Pitch: ❌
└─ Cooldown (Opcional)
   └─ Cooldown Time: 0 (sin cooldown)
```

**¡Listo!** Ahora cuando el jugador entre en el trigger, se reproducirá el sonido.

---

## 🎛️ Configuración Avanzada

### Opciones de Reproducción

#### Play Once
- **✅ Marcado:** El sonido se reproduce solo una vez y el objeto se destruye
- **❌ Desmarcado:** El sonido se puede reproducir múltiples veces

**Ejemplo de uso:**
- ✅ Para eventos únicos (primera vez que entras en una cueva)
- ❌ Para zonas que se visitan repetidamente

#### Play On Enter vs Play On Exit
- **Play On Enter = ✅:** Sonido se reproduce al ENTRAR en el trigger
- **Play On Enter = ❌:** Sonido se reproduce al SALIR del trigger

**Ejemplo de uso:**
- Play On Enter: Sonido de puerta al entrar en una habitación
- Play On Exit: Sonido de puerta al salir de una habitación

#### Destroy Delay
- Tiempo en segundos antes de destruir el objeto (solo si `Play Once = true`)
- Debe ser mayor que la duración del AudioClip para que se escuche completo

**Recomendación:**
```
destroyDelay = duración del audio + 0.5 segundos
```

### Variación de Sonido

Para hacer los sonidos más naturales y menos repetitivos:

#### Randomize Volume
- Varía el volumen ligeramente en cada reproducción
- `volumeMin` y `volumeMax`: Rango de variación (0.8 - 1.0 recomendado)

#### Randomize Pitch
- Varía el tono ligeramente en cada reproducción
- `pitchMin` y `pitchMax`: Rango de variación (0.95 - 1.05 recomendado)

**Ejemplo de configuración:**
```
Randomize Volume: ✅
Volume Min: 0.85
Volume Max: 1.0

Randomize Pitch: ✅
Pitch Min: 0.98
Pitch Max: 1.02
```

### Cooldown Time

Tiempo mínimo entre reproducciones del sonido:

- **0:** Sin cooldown (se reproduce cada vez que se activa el trigger)
- **> 0:** Tiempo en segundos que debe pasar antes de poder reproducir de nuevo

**Ejemplo de uso:**
- **Cooldown = 0:** Pasos en grava (cada vez que pisas)
- **Cooldown = 3:** Sonido ambiental de viento (evita repetición excesiva)

---

## 🎮 Ejemplos Prácticos

### Ejemplo 1: Sonido Ambiental de Bosque (Repetible)

**Escenario:** Al entrar en una zona de bosque, se escuchan pájaros.

**Configuración:**
```
GameObject: TriggerSound_Bosque
├─ BoxCollider2D
│  ├─ Is Trigger: ✅
│  └─ Size: (5, 5) - zona grande
└─ TriggerSound
   ├─ Sound Clip: Birds_Ambient.wav
   ├─ Play Once: ❌
   ├─ Play On Enter: ✅
   ├─ Randomize Volume: ✅
   ├─ Volume Min: 0.7
   ├─ Volume Max: 1.0
   └─ Cooldown Time: 5
```

**Resultado:** Cada vez que el jugador entra, se escuchan pájaros. No se repite constantemente gracias al cooldown.

### Ejemplo 2: Puerta Crujiendo (Solo Una Vez)

**Escenario:** Al pasar por una puerta vieja, cruje la primera vez.

**Configuración:**
```
GameObject: TriggerSound_PuertaVieja
├─ BoxCollider2D
│  ├─ Is Trigger: ✅
│  └─ Size: (1, 2) - zona pequeña
└─ TriggerSound
   ├─ Sound Clip: Door_Creak.wav
   ├─ Play Once: ✅
   ├─ Play On Enter: ✅
   ├─ Destroy Delay: 2
   └─ Cooldown Time: 0
```

**Resultado:** La puerta cruje solo la primera vez que pasas. Después, el trigger se destruye.

### Ejemplo 3: Agua Goteando (Variación Natural)

**Escenario:** Zona de cueva con sonido de gotas cayendo.

**Configuración:**
```
GameObject: TriggerSound_GotaAgua
├─ CircleCollider2D
│  ├─ Is Trigger: ✅
│  └─ Radius: 3
└─ TriggerSound
   ├─ Sound Clip: Water_Drip.wav
   ├─ Play Once: ❌
   ├─ Play On Enter: ✅
   ├─ Randomize Volume: ✅
   ├─ Volume Min: 0.6
   ├─ Volume Max: 1.0
   ├─ Randomize Pitch: ✅
   ├─ Pitch Min: 0.9
   ├─ Pitch Max: 1.1
   └─ Cooldown Time: 2
```

**Resultado:** Cada vez que entras, se escucha una gota con volumen y tono ligeramente diferentes.

### Ejemplo 4: Sonido al Salir de una Zona

**Escenario:** Sonido de portal al salir de un área mágica.

**Configuración:**
```
GameObject: TriggerSound_PortalExit
├─ BoxCollider2D
│  ├─ Is Trigger: ✅
│  └─ Size: (3, 3)
└─ TriggerSound
   ├─ Sound Clip: Portal_Whoosh.wav
   ├─ Play Once: ❌
   ├─ Play On Enter: ❌ (reproduce al SALIR)
   └─ Cooldown Time: 1
```

**Resultado:** El sonido se reproduce al SALIR del trigger.

---

## 🔧 Métodos Públicos

Puedes llamar estos métodos desde otros scripts:

### `ResetTrigger()`

Resetea el estado del trigger para que pueda reproducirse de nuevo (útil si `playOnce = true`).

```csharp
using UnityEngine;
using Triskel.Audio;

public class GameManager : MonoBehaviour
{
    [SerializeField] private TriggerSound triggerSound;

    public void ReiniciarNivel()
    {
        // Permitir que el trigger suene de nuevo
        if (triggerSound != null)
        {
            triggerSound.ResetTrigger();
        }
    }
}
```

### `ForcePlay()`

Fuerza la reproducción del sonido ignorando todas las condiciones (cooldown, playOnce, etc.).

```csharp
using UnityEngine;
using Triskel.Audio;

public class EventManager : MonoBehaviour
{
    [SerializeField] private TriggerSound triggerSound;

    public void ReproducirSonidoManualmente()
    {
        if (triggerSound != null)
        {
            triggerSound.ForcePlay();
        }
    }
}
```

---

## ⚠️ Troubleshooting

### El sonido no se reproduce

**Causa 1:** El AudioClip no está asignado.

**Solución:** Arrastra un archivo de audio al campo `Sound Clip` en el Inspector.

**Causa 2:** El Collider no está marcado como Trigger.

**Solución:** En el Collider2D, marca **"Is Trigger"** ✅

**Causa 3:** El jugador no tiene el tag "Player".

**Solución:** Selecciona el GameObject del jugador → Inspector → Tag → "Player"

### El sonido se reproduce varias veces seguidas

**Causa:** No hay cooldown configurado.

**Solución:** Añade un `Cooldown Time` mayor a 0 (ejemplo: 2 segundos).

### El sonido se corta antes de terminar

**Causa:** `destroyDelay` es menor que la duración del audio.

**Solución:** Aumenta el valor de `destroyDelay`:
```
destroyDelay = duración del audio + 0.5
```

### El volumen es muy bajo o muy alto

**Causa 1:** El volumen global de SFX está bajo.

**Solución:** Ajusta el volumen en el menú de configuración del juego.

**Causa 2:** El `baseVolume` del `SFXVolumeListener` está mal configurado.

**Solución:** Selecciona el GameObject → SFXVolumeListener → Base Volume (0-1)

### El trigger no se detecta

**Causa 1:** El Collider del jugador no está configurado correctamente.

**Solución:** El jugador debe tener un Rigidbody2D y un Collider2D.

**Causa 2:** Las capas de colisión están mal configuradas.

**Solución:** Edit → Project Settings → Physics 2D → Verificar que las capas pueden colisionar.

---

## 📝 Notas Adicionales

### Performance

- Los triggers de sonido son muy eficientes en performance
- Si usas `playOnce = true`, el objeto se destruye automáticamente (libera memoria)
- Los sonidos usan `PlayOneShot()` para no bloquear el AudioSource

### Compatibilidad

- ✅ Funciona con el sistema de volumen global (`SettingsManager`)
- ✅ Compatible con Input System
- ✅ Funciona en Unity 2D y 3D (ajusta `spatialBlend` en el código si necesitas 3D)

### Mejores Prácticas

1. **Organización:** Crea una carpeta `AudioTriggers` en la jerarquía para agrupar todos los triggers
2. **Nombres descriptivos:** Usa nombres claros como `TriggerSound_Bosque_Pajaros`
3. **Ajustar tamaños:** Haz el Collider del tamaño adecuado para la zona que quieres cubrir
4. **Testing:** Prueba el cooldown para evitar repetición excesiva
5. **Volumen:** Ajusta el volumen base en `SFXVolumeListener` según la importancia del sonido

### Debugging

Para verificar que el trigger funciona:

1. Añade un Gizmo visual editando el script:
```csharp
private void OnDrawGizmos()
{
    Gizmos.color = Color.yellow;
    Gizmos.DrawWireCube(transform.position, GetComponent<BoxCollider2D>().size);
}
```

2. Revisa la consola para ver el mensaje `[TriggerSound] Reproduciendo: nombre_del_audio`

---

## 🎓 Diferencias con Otros Sistemas

| Sistema | Ventaja | Desventaja |
|---------|---------|------------|
| **TriggerSound** | Automático, sin código | Solo detecta al jugador |
| **AudioSource normal** | Control total | Requiere código manual |
| **Timeline** | Sincronización perfecta | Más complejo de configurar |
| **FMOD/Wwise** | Profesional | Requiere integración externa |

---

## 🔗 Scripts Relacionados

- `SFXVolumeListener.cs` - Controla el volumen global de efectos
- `MusicManager.cs` - Gestión de música de fondo
- `PlayerFootsteps.cs` - Ejemplo de reproducción de audio sincronizada

---

**Última actualización:** 2026-02-02
**Versión:** 1.0
**Autor:** Claude Code
