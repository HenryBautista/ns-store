---
description: Registra un hallazgo nuevo en .agent/findings.md con el siguiente ID libre
---

Registra un hallazgo (bug, deuda o mejora) en `.agent/findings.md`. `$ARGUMENTS` lo describe; si
viene vacío, usa lo que se acabe de descubrir en esta sesión.

## 1. Antes de registrar, pregúntate si debería bajar un escalón

Un hallazgo en prosa es la última opción, no la primera. Si lo que descubriste se puede expresar
como un test de `tests/NsStore.Architecture.Tests`, una entrada en `src/BannedSymbols.txt` o una
severidad en `.editorconfig`, **hazlo así y no registres nada**: una regla que falla sola vale más
que un párrafo que alguien tiene que acordarse de leer.

## 2. Verificar

**Abre el código y confírmalo.** Un hallazgo es una afirmación sobre el sistema: si entra sin
verificar, contamina la priorización de todos los que vienen después. Anota el archivo y, si ayuda,
la línea o el fragmento exacto.

Si al mirarlo resulta que no es un defecto, dilo y no registres nada.

## 3. Comprobar que no es un duplicado

Lee la tabla resumen de `.agent/findings.md`. Si ya está, amplía el existente con lo que aporte
esta observación en vez de crear otro. Si es un caso distinto del mismo defecto de fondo,
regístralo aparte y enlaza los dos.

Mira también `ns-store-ui/.agent/findings.md`: varios defectos se ven desde los dos lados
(`DT-03` y `DT-04` son el mismo), y conviene enlazarlos.

## 4. Asignar ID

El siguiente `DT-nn` libre = el mayor que exista **entre los dos repos** + 1. Los IDs no se
reciclan ni se renumeran nunca: un commit que dice "cierra DT-03" tiene que seguir significando lo
mismo dentro de un año.

## 5. Escribir

Agrega la fila a la tabla resumen y la sección al cuerpo, **antes** de "Trampas transversales":

```markdown
<a id="dt-nn"></a>
## DT-nn — <título en una línea, afirmativo>

**Tipo:** Bug | Mejora | Deuda · **Severidad:** Alta | Media | Baja · **Estado:** abierto ·
**Detectado:** AAAA-MM-DD

**Dónde:** `ruta/al/archivo.cs`

<Qué pasa y por qué importa, en términos del negocio. Incluye el fragmento que lo demuestra si
aclara. Si ya causó un problema real, dilo — es lo que justifica la severidad.>

**Arreglo propuesto:** <qué haría falta. Si hay más de un camino, ponlos y di cuál preferirías.>
```

Si en cambio es una **trampa transversal** —no un bug arreglable, sino algo que muerde mientras
trabajas— va en la sección "Trampas transversales", con fecha y **línea de salida**: el test que la
eliminaría.

Criterio de severidad, calibrado con los que ya están:

- **Alta** — el sistema pierde de vista dinero real (`DT-01`, `DT-02`) o dejó pasar un fallo a
  producción (`DT-09`).
- **Media** — resultado incorrecto o incompleto que el usuario puede notar, sin pérdida de dinero.
- **Baja** — funciona mal o de forma incómoda, con rodeo posible.

Revisa si el hallazgo nuevo cambia el orden sugerido de prioridad; si no, déjalo.

Commit en inglés: `docs: record DT-nn (<título corto>)`. **Sin push.**
