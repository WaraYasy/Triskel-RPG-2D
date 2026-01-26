# Guía - Añadir Música al Nivel

## 🎵 **Objetivo**
Añadir música de fondo a tu nivel con o sin fade in/out

---

## 🎧 **Preparación del Audio:**

### **Formatos Recomendados:**
- **.ogg** → Mejor compresión para música larga ⭐
- **.mp3** → Compatible, buen tamaño
- **.wav** → Alta calidad pero pesado (evitar para música)

### **Importar Música:**

1. **Coloca tu archivo** en:
   ```
   Assets/_Project/Audio/Music/
   ```

2. **Selecciona el archivo** en Project
3. **Inspector → Import Settings:**
   ```
   Load Type: Streaming ⭐ (para música larga)
   Preload Audio Data: ✓
   Compression Format: Vorbis (para .ogg) o MP3
   Quality: 70% (ajustar según necesidad)
   Sample Rate Setting: Override Sample Rate
   Sample Rate: 44100 Hz
   ```

4. **Apply**

---

## 🎮 **Opción 1: Método Simple (Recomendado)**

### **Paso 1: Crear GameObject de Música**

1. **Hierarchy → Create Empty**
2. **Nombre:** `BackgroundMusic`
3. **Add Component → Audio Source**

### **Paso 2: Configurar Audio Source**

```
AudioClip: Arrastra tu música desde Project
Output: AudioMixer (Master) - o dejarlo en None
Mute: ❌
Bypass Effects: ❌
Bypass Listener Effects: ❌
Bypass Reverb Zones: ❌
Play On Awake: ✅
Loop: ✅

Volume: 0.5 (50%)
Pitch: 1
Stereo Pan: 0
Spatial Blend: 0 (2D - importante para música)
Reverb Zone Mix: 1
```

### **Paso 3: Probar**

1. **Play** ▶️
2. La música empieza automáticamente
3. Ajusta **Volume** mientras está en Play para encontrar el nivel perfecto
4. **Copia el valor** y pégalo con Play detenido

---

## 🎛️ **Opción 2: Con Script (Más Control)**

Para fade in/out, control de volumen dinámico, etc.

### **Paso 1: Usar MusicManager**

1. **Hierarchy → Create Empty**
2. **Nombre:** `MusicManager`
3. **Add Component → Music Manager** (script creado)

### **Paso 2: Configurar en Inspector**

```
Background Music: Arrastra tu clip
Volume: 0.5
Play On Awake: ✓
Loop: ✓

--- Fade (Opcional) ---
Use Fade In: ✓
Fade In Duration: 2 (segundos)
```

### **Paso 3: Métodos Disponibles**

Desde otros scripts puedes controlar:

```csharp
MusicManager music = FindObjectOfType<MusicManager>();

music.Play();              // Reproducir
music.Stop();              // Detener
music.Pause();             // Pausar
music.Resume();            // Reanudar
music.SetVolume(0.7f);     // Cambiar volumen
music.FadeOut(2f);         // Fade out en 2 segundos
```

---

## 🔊 **Recomendaciones de Volumen:**

| Tipo | Volumen Recomendado |
|------|---------------------|
| **Música ambiente** | 0.3 - 0.5 |
| **Música intensa** | 0.5 - 0.7 |
| **Música boss** | 0.6 - 0.8 |
| **SFX (efectos)** | 0.7 - 1.0 |

---

## 🎨 **Consejos Avanzados:**

### **1. Persistir Música Entre Escenas:**

Si quieres que la música continúe al cambiar de escena:

```csharp
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    private static MusicManager instance;
    
    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }
        
        instance = this;
        DontDestroyOnLoad(gameObject); // ← IMPORTANTE
    }
}
```

### **2. Música Diferente por Zona:**

Usa script para cambiar música según zona:

```csharp
public void ChangeMusic(AudioClip newClip, float fadeDuration = 1f)
{
    StartCoroutine(ChangeMusicCoroutine(newClip, fadeDuration));
}

private IEnumerator ChangeMusicCoroutine(AudioClip newClip, float duration)
{
    // Fade out
    yield return StartCoroutine(FadeOutCoroutine(duration));
    
    // Cambiar clip
    audioSource.clip = newClip;
    
    // Fade in
    Play();
}
```

### **3. Ajuste de Volumen en Settings:**

Conecta con PlayerPrefs:

```csharp
private void Start()
{
    float savedVolume = PlayerPrefs.GetFloat("MusicVolume", 0.5f);
    SetVolume(savedVolume);
}

public void SetVolume(float newVolume)
{
    volume = newVolume;
    audioSource.volume = volume;
    PlayerPrefs.SetFloat("MusicVolume", volume);
}
```

---

## 🐛 **Troubleshooting:**

### **❌ No suena nada:**

**Verifica:**
1. AudioListener en Main Camera (debe existir)
2. Volume > 0
3. Mute desactivado
4. AudioClip asignado
5. Play On Awake activado

### **❌ Música se corta o se oye mal:**

**Solución:**
- Load Type: **Streaming** (no Decompress On Load)
- Sample Rate: **44100 Hz**
- Compression: **Vorbis** para .ogg

### **❌ Música muy pesada:**

**Optimizar:**
- Usar **.ogg** en vez de .wav
- Compression Format: Vorbis
- Quality: 50-70% (suficiente para música ambiente)
- Sample Rate: 44100 Hz (no mayor)

### **❌ Lag al empezar:**

**Solución:**
- Preload Audio Data: ✓
- O usar Fade In para ocultar el inicio

---

## ✅ **Checklist:**

- [ ] Audio importado en Assets/_Project/Audio/Music/
- [ ] Import Settings: Streaming + Vorbis
- [ ] GameObject "BackgroundMusic" creado
- [ ] AudioSource configurado
- [ ] AudioClip asignado
- [ ] Play On Awake: ✓
- [ ] Loop: ✓
- [ ] Spatial Blend: 0 (2D)
- [ ] Volume ajustado
- [ ] Probado en Play

---

## 🎯 **Quick Start (30 segundos):**

1. Arrastra música a `Assets/_Project/Audio/Music/`
2. Hierarchy → Create Empty → "BackgroundMusic"
3. Add Component → Audio Source
4. Arrastra clip + Play On Awake ✓ + Loop ✓
5. Volume: 0.5
6. Play ▶️

**¡Listo!** 🎵

---

**Tiempo:** 2-5 minutos
**Dificultad:** Fácil ⭐
