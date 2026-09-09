---
description: Cierra un ticket: verifica, promueve lo aprendido y escribe la descripción del PR
---

Cierra el trabajo en curso en `ns-store`. `$ARGUMENTS` = título del cambio; si viene vacío,
dedúcelo del diff y confírmalo.

Sin este paso, `.work/` es un cementerio y el conocimiento del repo no aprende nada. Este es el
mecanismo que convierte el ruido de un ticket en conocimiento durable.

## 1. Verificar

```
just check
```

**Si falla, para acá.** Reporta qué falló con su salida y pregunta si arreglar o registrar. No
promuevas nada ni escribas la descripción del PR con el check en rojo, y no lo hagas
"provisionalmente".

Lo que `just check` no puede cubrir, revísalo leyendo el diff, no asumiendo:

- ¿`errorCode` nuevo? Entonces `ns-store-ui` necesita su mensaje en español. Corre `just check`
  **también allá** — es el repo que verifica ese contrato.
- ¿Endpoint nuevo? Thin: resolver servicio, llamar, envolver. Sin códigos de estado ni mensajes
  escritos a mano.
- ¿Colección nueva? `PageRequest` y `PagedResult<T>`.
- ¿Consulta que llega a una tabla hija? Tiene que pasar por el padre, que es el que tiene el filtro
  de soft delete.

## 2. Promover lo que salió del trabajo

Lee `.work/<ticket>/findings.md` y `decisions.md` si existen, y hazte tres preguntas:

- ¿Qué aprendí que no estaba escrito en ningún lado?
- ¿Qué decisión tomé que restringe trabajo futuro?
- ¿Qué me costó más de 30 minutos por no ser obvio?

Para cada respuesta, elige destino **en este orden de preferencia**. El destino por defecto **no**
es prosa:

1. ¿Puede ser un test, una regla o una restricción de tipo? → `tests/NsStore.Architecture.Tests`,
   `src/BannedSymbols.txt`, `.editorconfig`
2. ¿Puede ser un target del runner? → `Justfile`
3. ¿Es específico de un módulo? → el `README.md` de ese directorio
4. ¿Restringe decisiones futuras? → entrada nueva `D-nn` en `.agent/decisions.md`
5. ¿Nada de lo anterior? → `.agent/findings.md`, con línea de salida

Todo lo demás se descarta. La mayor parte de un ticket es ruido; eso es normal.

Actualiza además:

- **Hallazgos dejados fuera de alcance** → `.agent/findings.md` con el siguiente `DT-nn` libre
  (o usa `/finding`). Un hallazgo que solo se menciona en el PR se pierde.
- **Hallazgos cerrados** → cambia su estado a `resuelto` con la fecha y el commit. No borres la
  sección ni renumeres nada.
- **Términos de negocio nuevos** → glosario de `.agent/domain.md`, con su identificador en inglés.

**Propón los diffs y espera aprobación.** No escribas directo en `.agent/`.

## 3. Descripción del PR

En inglés, con dos secciones que no son de relleno:

- **What changed** — en términos de negocio, no de archivos. "Collecting from the sale panel now
  issues a receipt", no "modified SaleService.cs".
- **What was left out and why** — la parte más valiosa. Un hueco declarado con su razón vale más
  que una lista de logros: es lo que evita que la próxima sesión lo descubra sola.

Menciona los IDs tocados: `closes DT-03`, `adds D-16`.

## 4. Limpiar

Borra `.work/<ticket>/`. El ticket muere; el destilado queda.

**No hagas push sin que te lo pidan.**
