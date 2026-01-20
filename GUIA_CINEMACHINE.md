# Guía - Configuración Cinemachine para Juego 2D

## 🎯 **Objetivo**
Configurar una cámara suave que sigue al jugador con Cinemachine.

---

## 📦 **Paso 1: Instalar Cinemachine**

1. **Window → Package Manager**
2. **Packages: Unity Registry**
3. **Busca:** "Cinemachine"
4. **Install**
5. **Espera** que se instale (~30 segundos)

✅ **Verificar:** Aparece menu "Cinemachine" arriba

---

## 🎬 **Paso 2: Crear Virtual Camera**

### **Opción A: Cinemachine 2D Camera (Recomendado para 2D)**

1. **Hierarchy → Cinemachine → 2D Camera**
2. **Nombre:** `CM vcam_Player`

### **O si no aparece "2D Camera":**

1. **Hierarchy → Cinemachine → Virtual Camera**
2. Nombre: `CM vcam_Player`
3. Inspector → Add Extension → **Cinemachine Pixel Perfect**

---

## ⚙️ **Paso 3: Configurar la Virtual Camera**

**Selecciona** `CM vcam_Player`:

### **Follow (Seguir al Player):**
```
Follow: Arrastra Player desde Hierarchy
```

### **Body (Cómo sigue):**
```
Body: Framing Transposer (para 2D)

Settings:
  - Lookahead Time: 0.2
  - Lookahead Smoothing: 10
  - Damping X: 1
  - Damping Y: 1
  - Screen X: 0.5
  - Screen Y: 0.5
  - Dead Zone Width: 0.1
  - Dead Zone Height: 0.1
```

### **Aim (Hacia dónde mira):**
```
Aim: Do Nothing (para top-down)
```

### **Lens (Configuración de cámara):**
```
Orthographic Size: 5 (ajusta según necesites)
```

---

## 📐 **Paso 4: Confinar la Cámara (Opcional)**

Para que NO salga del mapa:

### **Crear Confiner:**

1. **Hierarchy → Create Empty** → `CameraConfiner`
2. **Add Component → Polygon Collider 2D**
3. **Edit Collider:** Dibuja el límite de tu mapa
4. **Is Trigger:** ✅

### **Asignar a Virtual Camera:**

1. **Selecciona** `CM vcam_Player`
2. **Add Extension → CinemachineConfiner2D**
3. **Bounding Shape 2D:** Arrastra `CameraConfiner`
4. **Damping:** 0.5

---

## 🎮 **Configuraciones Recomendadas por Estilo**

### **📊 Para Exploración Lenta:**
```
Body: Framing Transposer
  Damping X: 2
  Damping Y: 2
  Dead Zone: 0.2 x 0.2
```

### **⚡ Para Acción Rápida:**
```
Body: Framing Transposer
  Damping X: 0.5
  Damping Y: 0.5
  Lookahead Time: 0.5
  Dead Zone: 0.05 x 0.05
```

### **🎯 Para Top-Down Fijo:**
```
Body: Framing Transposer
  Damping X: 1.5
  Damping Y: 1.5
  Dead Zone: 0.15 x 0.15
```

---

## ✨ **Efectos Adicionales**

### **Camera Shake (Temblor):**

1. **Virtual Camera → Add Extension → CinemachineImpulseListener**
2. Para causar shake desde código:
```csharp
using Cinemachine;

CinemachineImpulseSource impulse = GetComponent<CinemachineImpulseSource>();
impulse.GenerateImpulse();
```

### **Smooth Zoom (Cambiar tamaño):**

Desde código:
```csharp
CinemachineVirtualCamera vcam = FindObjectOfType<CinemachineVirtualCamera>();
vcam.m_Lens.OrthographicSize = 7f; // Zoom out suave
```

---

## 🐛 **Troubleshooting**

### **❌ Cámara no sigue al player:**
- Verifica que **Follow** tenga al Player asignado
- Check que Player tenga tag "Player"

### **❌ Cámara muy lenta/rápida:**
- Ajusta **Damping** (más bajo = más rápido)

### **❌ Cámara sale del mapa:**
- Añade **CinemachineConfiner2D**
- Verifica que Polygon Collider cubra el mapa

### **❌ Cámara "jittery" (con saltos):**
- Main Camera → Add Component → **CinemachineBrain**
- Update Method: **Late Update**

---

## 📋 **Checklist Final**

- [ ] Cinemachine instalado
- [ ] Virtual Camera 2D creada
- [ ] Follow apunta al Player
- [ ] Body: Framing Transposer
- [ ] Damping configurado
- [ ] Orthographic Size ajustado
- [ ] (Opcional) Confiner configurado
- [ ] CinemachineBrain en Main Camera

---

## 🎯 **Resultado Final**

**Antes (sin Cinemachine):**
- Cámara rígida
- Saltos bruscos
- Script custom necesario

**Después (con Cinemachine):**
- Movimiento suave
- Lookahead (anticipa movimiento)
- Dead zone (no se mueve si player está cerca del centro)
- Fácil de ajustar

---

**Tiempo:** 5 minutos
**Dificultad:** Fácil ⭐⭐

¡Tu cámara ahora se ve profesional! 🎬✨
