# Guía: Diálogos que se Repiten Hasta Completar una Acción

Esta guía explica cómo configurar diálogos que se repiten automáticamente hasta que el jugador complete una tarea específica (recoger un objeto, usar una habilidad, hablar con otro NPC, etc.).

## 📋 Índice

1. [Caso de Uso](#caso-de-uso)
2. [Configuración en Unity](#configuración-en-unity)
3. [Bloqueador Físico (NUEVO)](#bloqueador-físico-nuevo)
4. [Código: Completar la Condición](#código-completar-la-condición)
5. [Ejemplos Prácticos](#ejemplos-prácticos)
6. [Métodos Disponibles](#métodos-disponibles)

---

## 🎯 Caso de Uso

**Problema:** Necesitas que un NPC repita un mensaje tutorial hasta que el jugador realice una acción específica.

**Ejemplo:**
- Un NPC dice: *"Por favor, recoge la espada que está al lado del árbol"*
- El jugador sale y vuelve a entrar → El NPC repite el mensaje
- El jugador recoge la espada → El NPC ya no repite el mensaje

---

## ⚙️ Configuración en Unity

### Paso 1: Configurar el DialogueZone

1. Selecciona el GameObject con el componente `DialogueZone`
2. En el Inspector, configura:

```
DialogueZone (Script)
├─ Nodo Dialogo: "NPC_Tutorial_Espada"
├─ Dialogue Runner: [Arrastra el DialogueRunner de la escena]
├─ Opciones
│  ├─ Solo Una Vez: ❌ (DESMARCAR)
│  ├─ Freeze Player: ✅ (según prefieras)
│  └─ Persiste Entre Escenas: ❌ (normalmente no)
└─ Repetición Condicional
   └─ Repetir Hasta Condicion: ✅ (MARCAR)
```

**⚠️ IMPORTANTE:**
- `Solo Una Vez` debe estar **DESMARCADO**
- `Repetir Hasta Condicion` debe estar **MARCADO**

### Paso 2: Dale un nombre al GameObject (opcional pero recomendado)

Renombra el GameObject a algo descriptivo:
- `DialogoZone_Tutorial_Espada`
- `DialogoZone_NPC_Mision1`

---

## 🚧 Bloqueador Físico (NUEVO)

Si necesitas que un objeto **físicamente bloquee el paso** del jugador hasta que se complete la condición, usa el componente `BloqueadorCondicional`.

### Setup Completo: Diálogo + Bloqueador

#### Paso 1: Crear el Bloqueador Físico

1. Crea un GameObject (puede ser un sprite de puerta, barrera, NPC, etc.)
2. Nómbralo: `Bloqueador_Tutorial_Espada`
3. Añade componentes necesarios:
   - `Sprite Renderer` (para verlo)
   - `BoxCollider2D` o `CapsuleCollider2D`
   - **BloqueadorCondicional** (script)

#### Paso 2: Configurar el Bloqueador

En el Inspector del bloqueador:

```
BloqueadorCondicional (Script)
├─ Referencias
│  └─ Dialogo Asociado: [Arrastra el DialogueZone aquí]
└─ Configuración
   ├─ Destruir Al Completar: ✅ (si quieres que desaparezca) o ❌ (si solo se desactiva)
   └─ Efecto Desbloqueo: [Opcional: partículas, sonido, etc.]
```

**⚠️ IMPORTANTE sobre el Collider:**
- El Collider2D debe estar **desmarcado** como "Is Trigger"
- Esto permite que bloquee físicamente al jugador
- El script lo verifica automáticamente

#### Paso 3: Configurar el DialogueZone Asociado

1. Crea otro GameObject: `DialogoZone_Tutorial_Espada`
2. Añade un `BoxCollider2D` marcado como **Trigger**
3. Añade el componente `DialogueZone`
4. Configura como antes:
   - `Solo Una Vez`: ❌
   - `Repetir Hasta Condicion`: ✅

#### Paso 4: Conectar Ambos

En el `BloqueadorCondicional`:
- Arrastra el `DialogueZone` al campo **Dialogo Asociado**

### Jerarquía Recomendada en Unity

```
Tutorial_Espada (GameObject vacío)
├─ Bloqueador_Tutorial_Espada
│  ├─ Sprite Renderer (imagen de barrera/puerta)
│  ├─ BoxCollider2D (Is Trigger: NO)
│  └─ BloqueadorCondicional (Script)
│     └─ Dialogo Asociado: → DialogoZone_Tutorial_Espada
└─ DialogoZone_Tutorial_Espada
   ├─ BoxCollider2D (Is Trigger: SÍ)
   └─ DialogueZone (Script)
      ├─ Solo Una Vez: NO
      └─ Repetir Hasta Condicion: SÍ
```

### Código para Desbloquear

Cuando el jugador complete la acción (recoger objeto, matar enemigos, etc.):

```csharp
using UnityEngine;
using Triskel.Dialogue;

public class ObjetoRecogible : MonoBehaviour
{
    [SerializeField] private BloqueadorCondicional bloqueador;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Recoger objeto
            Debug.Log("¡Espada recogida!");

            // Desbloquear el paso
            if (bloqueador != null)
            {
                bloqueador.CompletarCondicion(); // Esto también completa el diálogo automáticamente
            }

            Destroy(gameObject);
        }
    }
}
```

### Ejemplo Completo en Escena

**Escenario:** NPC bloqueando un camino hasta que recojas una llave.

```
Nivel1 (Escena)
├─ Player
├─ Llave (GameObject)
│  └─ RecogerLlave (Script) → referencia a Bloqueador_NPC
├─ NPC_Guardian (GameObject padre)
│  ├─ Bloqueador_NPC
│  │  ├─ Sprite (imagen del NPC)
│  │  ├─ CapsuleCollider2D (Is Trigger: NO) ← Bloquea físicamente
│  │  └─ BloqueadorCondicional
│  │     └─ Dialogo Asociado: → DialogoZone_NPC
│  └─ DialogoZone_NPC
│     ├─ BoxCollider2D (Is Trigger: SÍ) ← Detecta al jugador para hablar
│     └─ DialogueZone
│        ├─ Nodo Dialogo: "NPC_Necesitas_Llave"
│        ├─ Solo Una Vez: NO
│        └─ Repetir Hasta Condicion: SÍ
```

**Script de la Llave:**

```csharp
using UnityEngine;
using Triskel.Dialogue;

public class RecogerLlave : MonoBehaviour
{
    [SerializeField] private BloqueadorCondicional bloqueadorNPC;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("¡Llave obtenida! El NPC te dejará pasar.");

            // Desbloquear NPC
            if (bloqueadorNPC != null)
            {
                bloqueadorNPC.CompletarCondicion();
                // Esto hace:
                // 1. Desactiva/destruye el bloqueador
                // 2. Completa el DialogueZone asociado
                // 3. El NPC ya no repite el mensaje
            }

            Destroy(gameObject);
        }
    }
}
```

### Ventajas del Sistema

✅ **Un solo método:** `bloqueador.CompletarCondicion()` desbloquea TODO
✅ **Sincronización automática:** El diálogo y el bloqueador se completan juntos
✅ **Reutilizable:** Puedes tener múltiples bloqueadores en la misma escena
✅ **Debugging:** Método `ResetearCondicion()` para testing

---

## 💻 Código: Completar la Condición

### Opción 1: Desde el Inspector (Recomendado)

**Ejemplo: Al recoger un objeto**

1. Crea un script para el objeto que activa la condición:

```csharp
using UnityEngine;
using Triskel.Dialogue;

public class ObjetoRecogible : MonoBehaviour
{
    [Header("Diálogo Asociado")]
    [Tooltip("Arrastra aquí el DialogueZone que debe dejar de repetirse")]
    [SerializeField] private DialogueZone dialogoAsociado;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Recoger el objeto
            Debug.Log("¡Objeto recogido!");

            // Completar la condición del diálogo
            if (dialogoAsociado != null)
            {
                dialogoAsociado.CompletarCondicion();
            }

            // Destruir o desactivar el objeto
            Destroy(gameObject);
        }
    }
}
```

2. En Unity:
   - Añade el script al objeto recogible
   - Arrastra el `DialogueZone` al campo `Dialogo Asociado`

### Opción 2: Buscar por Nombre de GameObject

```csharp
using UnityEngine;
using Triskel.Dialogue;

public class AccionQueCompleta : MonoBehaviour
{
    private void CompletarMision()
    {
        // Buscar el DialogueZone por nombre
        GameObject dialogoObj = GameObject.Find("DialogoZone_Tutorial_Espada");

        if (dialogoObj != null)
        {
            DialogueZone dialogo = dialogoObj.GetComponent<DialogueZone>();
            if (dialogo != null)
            {
                dialogo.CompletarCondicion();
            }
        }
    }
}
```

### Opción 3: Usar GameManager (Para sistemas complejos)

Si tienes un GameManager que controla misiones:

```csharp
using UnityEngine;
using Triskel.Dialogue;

public class MisionManager : MonoBehaviour
{
    [SerializeField] private DialogueZone dialogoMision1;

    public void CompletarMision1()
    {
        // Lógica de la misión
        Debug.Log("¡Misión 1 completada!");

        // Detener el diálogo repetitivo
        if (dialogoMision1 != null)
        {
            dialogoMision1.CompletarCondicion();
        }
    }
}
```

---

## 🎮 Ejemplos Prácticos

### Ejemplo 1: Tutorial de Recogida de Ítems

**Escenario:** Un NPC te dice que recojas 3 pociones.

```csharp
using UnityEngine;
using Triskel.Dialogue;

public class TutorialPociones : MonoBehaviour
{
    [SerializeField] private DialogueZone dialogoNPC;
    [SerializeField] private int pocionesRequeridas = 3;
    private int pocionesRecogidas = 0;

    public void RecogerPocion()
    {
        pocionesRecogidas++;
        Debug.Log($"Pociones: {pocionesRecogidas}/{pocionesRequeridas}");

        if (pocionesRecogidas >= pocionesRequeridas)
        {
            // Completar el tutorial
            if (dialogoNPC != null)
            {
                dialogoNPC.CompletarCondicion();
            }
        }
    }
}
```

### Ejemplo 2: Tutorial de Combate

**Escenario:** Un entrenador te pide derrotar 5 enemigos.

```csharp
using UnityEngine;
using Triskel.Dialogue;

public class TutorialCombate : MonoBehaviour
{
    [SerializeField] private DialogueZone dialogoEntrenador;
    private int enemigosEliminados = 0;
    private const int META = 5;

    public void RegistrarMuerteEnemigo()
    {
        enemigosEliminados++;

        if (enemigosEliminados >= META)
        {
            if (dialogoEntrenador != null)
            {
                dialogoEntrenador.CompletarCondicion();
            }
        }
    }
}
```

### Ejemplo 3: Activar Habilidad por Primera Vez

**Escenario:** El NPC te pide que uses el Dash.

```csharp
using UnityEngine;
using Triskel.Dialogue;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private DialogueZone dialogoTutorialDash;
    private bool dashUsadoPrimeraVez = false;

    private void UsarDash()
    {
        // Lógica del dash...

        // Si es la primera vez
        if (!dashUsadoPrimeraVez)
        {
            dashUsadoPrimeraVez = true;

            if (dialogoTutorialDash != null)
            {
                dialogoTutorialDash.CompletarCondicion();
            }
        }
    }
}
```

### Ejemplo 4: Hablar con Varios NPCs

**Escenario:** Un NPC te pide hablar con 3 personas diferentes.

```csharp
using UnityEngine;
using Triskel.Dialogue;
using System.Collections.Generic;

public class MisionHablarNPCs : MonoBehaviour
{
    [SerializeField] private DialogueZone dialogoMisionero;
    [SerializeField] private List<string> npcsRequeridos = new List<string>
    {
        "NPC_Herrero",
        "NPC_Comerciante",
        "NPC_Anciano"
    };

    private HashSet<string> npcsHablados = new HashSet<string>();

    public void RegistrarConversacion(string npcID)
    {
        if (npcsRequeridos.Contains(npcID))
        {
            npcsHablados.Add(npcID);

            Debug.Log($"Hablado con {npcID}. Progreso: {npcsHablados.Count}/{npcsRequeridos.Count}");

            // Verificar si se habló con todos
            if (npcsHablados.Count >= npcsRequeridos.Count)
            {
                if (dialogoMisionero != null)
                {
                    dialogoMisionero.CompletarCondicion();
                }
            }
        }
    }
}
```

---

## 🔧 Métodos Disponibles

### DialogueZone

#### `CompletarCondicion()`

Marca la condición como completada. El diálogo dejará de repetirse.

```csharp
dialogoZone.CompletarCondicion();
```

#### `ResetearCondicion()`

Resetea la condición para volver a permitir que el diálogo se repita. Útil para testing o reiniciar niveles.

```csharp
dialogoZone.ResetearCondicion();
```

---

### BloqueadorCondicional

#### `CompletarCondicion()`

Desbloquea el paso Y completa el diálogo asociado automáticamente.

```csharp
bloqueador.CompletarCondicion();
// Esto hace:
// 1. Completa el DialogueZone asociado
// 2. Destruye o desactiva el bloqueador
// 3. Reproduce efecto de desbloqueo (si está configurado)
```

#### `ResetearCondicion()`

Reactiva el bloqueador y resetea el diálogo. Útil para testing.

```csharp
bloqueador.ResetearCondicion();
```

#### `EstaCompletado()`

Verifica si el bloqueador ya fue completado.

```csharp
if (bloqueador.EstaCompletado())
{
    Debug.Log("El camino está desbloqueado");
}
```

**Ejemplo de uso en reinicio de nivel:**

```csharp
public class GameManager : MonoBehaviour
{
    [SerializeField] private DialogueZone[] dialogosCondicionales;

    public void ReiniciarNivel()
    {
        // Resetear todos los diálogos condicionales
        foreach (var dialogo in dialogosCondicionales)
        {
            if (dialogo != null)
            {
                dialogo.ResetearCondicion();
            }
        }
    }
}
```

---

## ✅ Checklist de Configuración

### Solo Diálogo (sin bloqueador físico)

- [ ] El `DialogueZone` tiene `Repetir Hasta Condicion` **MARCADO**
- [ ] El `DialogueZone` tiene `Solo Una Vez` **DESMARCADO**
- [ ] El nodo de Yarn está correctamente asignado
- [ ] El `DialogueRunner` está asignado (o se auto-detectará)
- [ ] Hay código que llama a `dialogoZone.CompletarCondicion()` cuando ocurre la acción
- [ ] (Opcional) Si el nivel se reinicia, hay código que llama a `ResetearCondicion()`

### Diálogo + Bloqueador Físico

- [ ] El objeto bloqueador tiene el componente `BloqueadorCondicional`
- [ ] El Collider2D del bloqueador **NO** está marcado como Trigger
- [ ] El `BloqueadorCondicional` tiene referencia al `DialogueZone` asociado
- [ ] El `DialogueZone` tiene `Repetir Hasta Condicion` **MARCADO**
- [ ] El `DialogueZone` tiene `Solo Una Vez` **DESMARCADO**
- [ ] Hay código que llama a `bloqueador.CompletarCondicion()` cuando ocurre la acción
- [ ] (Recomendado) Organizar en jerarquía: GameObject padre con bloqueador y diálogo como hijos

---

## ⚠️ Errores Comunes

### Diálogos

#### El diálogo solo se muestra una vez

**Causa:** `Solo Una Vez` está marcado.

**Solución:** Desmarca `Solo Una Vez` en el Inspector.

#### El diálogo nunca deja de repetirse

**Causa:** `CompletarCondicion()` nunca se llama.

**Solución:** Verifica que tu código llame al método cuando ocurra la acción.

```csharp
// Añade un Debug.Log para verificar
dialogoZone.CompletarCondicion();
Debug.Log("Condición completada!");
```

#### NullReferenceException al llamar CompletarCondicion()

**Causa:** La referencia al `DialogueZone` es null.

**Solución:** Asegúrate de arrastrar el `DialogueZone` al campo del Inspector.

```csharp
// Añade validación
if (dialogoZone != null)
{
    dialogoZone.CompletarCondicion();
}
else
{
    Debug.LogError("DialogueZone no asignado!");
}
```

---

### Bloqueador Físico

#### El jugador puede atravesar el bloqueador

**Causa 1:** El Collider2D está marcado como "Is Trigger".

**Solución:** Desmarca "Is Trigger" en el BoxCollider2D o CapsuleCollider2D del bloqueador.

**Causa 2:** No hay Rigidbody2D en el jugador.

**Solución:** Asegúrate de que el jugador tenga un Rigidbody2D.

**Causa 3:** Las capas de colisión están mal configuradas.

**Solución:** Verifica en Edit → Project Settings → Physics 2D que las capas del bloqueador y jugador pueden colisionar.

#### El bloqueador no desaparece al completar la condición

**Causa:** `CompletarCondicion()` no se llama en el bloqueador.

**Solución:** Verifica que estás llamando al método del bloqueador:

```csharp
bloqueador.CompletarCondicion(); // ✅ Correcto
dialogoZone.CompletarCondicion(); // ❌ Solo completa el diálogo, no el bloqueador
```

#### El diálogo se completa pero el bloqueador sigue ahí

**Causa:** El `DialogueZone` no está asociado al `BloqueadorCondicional`.

**Solución:** En el Inspector del bloqueador, arrastra el DialogueZone al campo "Dialogo Asociado".

#### Warning: "El Collider2D está marcado como Trigger"

**Causa:** El bloqueador tiene el collider como trigger.

**Solución:** El script lo corrige automáticamente, pero es mejor desmarcarlo manualmente en el Inspector para evitar confusiones.

---

## 🎓 Diferencias con Otras Opciones

| Opción | Diálogo | Bloqueo Físico |
|--------|---------|----------------|
| `Solo Una Vez` = ✅ | Se muestra 1 vez y nunca más | ❌ No bloquea |
| `Solo Una Vez` = ❌ | Se muestra cada vez que entras | ❌ No bloquea |
| `Repetir Hasta Condicion` = ✅ | Se repite hasta completar | ❌ No bloquea |
| `BloqueadorCondicional` | Se repite hasta completar | ✅ Bloquea físicamente |

**Combinar opciones:**
- `Solo Una Vez` + `Persiste Entre Escenas` = Diálogo único que se recuerda en el hub
- `Repetir Hasta Condicion` = Tutorial que se repite hasta completar tarea (sin bloqueo físico)
- `BloqueadorCondicional` + `DialogueZone` = Tutorial con bloqueo físico hasta completar

---

## 📝 Notas Adicionales

### General
- Los diálogos condicionales **no** se guardan en `PlayerPrefs` automáticamente
- Si necesitas persistencia entre sesiones, deberás implementar tu propio sistema de guardado
- `ResetearCondicion()` es útil para testing en el Editor
- Puedes tener múltiples `DialogueZone` y `BloqueadorCondicional` con condiciones diferentes

### Bloqueadores
- El `BloqueadorCondicional` completa automáticamente el `DialogueZone` asociado
- No necesitas llamar a ambos métodos, solo `bloqueador.CompletarCondicion()`
- Puedes reutilizar el mismo bloqueador para múltiples condiciones complejas
- Si destruyes el bloqueador (`destruirAlCompletar = true`), no podrás llamar a `ResetearCondicion()` después

### Performance
- Los diálogos condicionales usan diccionario estático en memoria (muy eficiente)
- Los bloqueadores no tienen impacto en performance una vez completados (se destruyen/desactivan)

### Compatibilidad
- ✅ Funciona con Input System
- ✅ Compatible con sistema de pausa
- ✅ Compatible con sistema de guardado del juego (requiere implementación personalizada)
- ✅ Funciona con Yarn Spinner 3.1.3+

---

**Última actualización:** 2026-01-31
**Versión:** 1.0
