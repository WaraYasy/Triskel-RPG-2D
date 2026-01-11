# Primera Prueba del Diario (5 minutos)

## Setup Rápido

### 1. Crear GameObjects (2 minutos)

**DiaryManager:**
```
Hierarchy → Click derecho → Create Empty
Nombre: "DiaryManager"
Add Component → DiaryManager
Add Component → DiaryPersistence
```

**DiaryUI:**
```
Hierarchy → Click derecho → UI Toolkit → UI Document
Nombre: "DiaryUI"
Inspector → UIDocument → Source Asset → Buscar "DiaryUI" (el archivo .uxml)
Add Component → DiaryUI
```

**QuickTester:**
```
Hierarchy → Click derecho → Create Empty
Nombre: "QuickTester"
Add Component → QuickDiaryTest
```

### 2. Crear una Entrada de Prueba (2 minutos)

```
Project → Click derecho en cualquier carpeta
Create → Triskel/Diary Entry

Nombre del archivo: "TestEntry"

En el Inspector:
✓ Entry ID: (se auto-genera como "testentry")
✓ Level Index: 0
✓ Condition ID: "saved_child"
✓ Title: "Mi primera entrada"
✓ Text: (escribe cualquier cosa aquí)
```

**Ejemplo de texto:**
```
Esta es mi primera entrada del diario.
Estoy probando el sistema.

Puedo escribir varias líneas.
Y crear párrafos.

¡Funciona!
```

### 3. Conectar la Entrada (1 minuto)

```
Selecciona "DiaryManager" en Hierarchy
Inspector → DiaryManager component
All Entries → Size: 1
Element 0 → Arrastra "TestEntry" aquí
```

## Probar (AHORA)

### Opción A: Context Menu (Sin Play Mode)

```
1. Selecciona "DiaryManager" en Hierarchy
2. Inspector → DiaryManager component → Click derecho
3. "Debug: Desbloquear Entrada de Prueba"
4. Presiona Play ▶
5. Presiona tecla J
6. ¡VER TU ENTRADA EN EL DIARIO!
```

### Opción B: Con Play Mode

```
1. Presiona Play ▶
2. Presiona tecla 1 (desbloquea entrada)
3. Presiona tecla J (abre diario)
4. ¡VER TU ENTRADA EN EL DIARIO!
```

## Controles en Play Mode

```
1 = Desbloquear entrada nivel 0 (saved_child)
2 = Desbloquear entrada nivel 0 (ignored_child)
3 = Desbloquear entrada nivel 1 (defeated_boss)
J = Abrir/Cerrar diario
0 = Limpiar todo
H = Ayuda
```

## Qué Deberías Ver

**Al presionar J:**
- Panel oscuro aparece en pantalla
- Título: "Diario del Viajero"
- Panel izquierdo: Lista de entradas
- Panel derecho: Texto de la entrada seleccionada

**En la lista (izquierda):**
```
┌─────────────────────────────┐
│ Nivel 1: Mi primera entrada │  ← Botón clickable
└─────────────────────────────┘
```

**En el texto (derecha):**
```
Mi primera entrada
─────────────────────────────────────
Esta es mi primera entrada del diario.
Estoy probando el sistema.

Puedo escribir varias líneas.
Y crear párrafos.

¡Funciona!
```

## Si No Funciona

### Panel no aparece:
```
✓ Verifica que DiaryUI existe en Hierarchy
✓ Verifica que UIDocument tiene "DiaryUI.uxml" asignado
✓ Mira la consola (puede haber error)
```

### No hay entradas en la lista:
```
✓ Verifica que desbloqueaste algo (tecla 1 o Context Menu)
✓ Mira la consola: debe decir "✓ Entrada desbloqueada"
```

### No veo el texto:
```
✓ Click en el botón de la entrada en la lista izquierda
✓ Verifica que escribiste algo en el campo "Text" del asset
```

## Siguiente Paso

**Crear segunda entrada con diferente decisión:**

```
1. Duplicar "TestEntry" (Ctrl+D)
2. Renombrar a "TestEntry2"
3. En Inspector:
   - Entry ID: "testentry2" (cambiar manualmente)
   - Condition ID: "ignored_child"
   - Title: "Mi segunda entrada"
   - Text: "Este es un texto diferente para otra decisión..."
4. Añadir al DiaryManager:
   - All Entries → Size: 2
   - Element 1 → Arrastra "TestEntry2"
5. Limpiar diario (tecla 0)
6. Presionar tecla 2 (en vez de 1)
7. Abrir diario (J)
8. ¡Ver la segunda entrada!
```

## Escribir Texto Largo

### Método 1: Directamente en Unity
```
1. Selecciona el DiaryEntryData en Project
2. Inspector → campo "Text"
3. Escribe (o pega con Ctrl+V)
4. El campo crece automáticamente
```

### Método 2: Desde archivo externo (RECOMENDADO)
```
1. Abre Notepad/Word/cualquier editor
2. Escribe tu texto narrativo:

   ─────────────────────────────────
   El día comenzó tranquilo.

   La brisa del mar me recordó a casa,
   pero sabía que no podía volver atrás.

   La decisión estaba tomada.
   ─────────────────────────────────

3. Selecciona todo (Ctrl+A)
4. Copia (Ctrl+C)
5. En Unity → DiaryEntryData → Inspector → Text
6. Pega (Ctrl+V)
7. ✓ Listo
```

### Consejos para el Texto

**Formato:**
- Los saltos de línea (Enter) se respetan
- Puedes crear párrafos vacíos
- Soporta acentos: á, é, í, ó, ú, ñ
- Soporta símbolos: ¿, ¡, ", ', -

**Longitud:**
- Mínimo: 50 palabras
- Ideal: 200-400 palabras (3-5 párrafos)
- Máximo: Sin límite (hay scroll)

**Estilo:**
```
✓ BIEN: "Hoy encontré un niño en el bosque..."
✓ BIEN: "El viajero descubrió una verdad terrible..."
✗ EVITAR: Textos muy cortos ("Encontré algo.")
✗ EVITAR: Diálogos sin contexto ("- Hola - dijo")
```

## Checklist Completo

- [ ] DiaryManager creado con componentes
- [ ] DiaryUI creado con UIDocument
- [ ] DiaryUI.uxml asignado en Source Asset
- [ ] QuickTester creado con componente
- [ ] Al menos 1 DiaryEntryData creado
- [ ] Entrada tiene texto escrito
- [ ] Entrada asignada a DiaryManager
- [ ] Context Menu funciona (desbloquea)
- [ ] Play Mode funciona
- [ ] Tecla J abre el panel
- [ ] Entrada aparece en la lista
- [ ] Texto se muestra completo
- [ ] Tecla 0 limpia todo

## ¿Todo Listo?

Si completaste el checklist:
✓ El sistema funciona
✓ Puedes crear más entradas
✓ Puedes integrar en tu juego

**Lee DIARY_TESTING_GUIDE.md para más detalles**
