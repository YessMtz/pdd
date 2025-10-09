
### Identificación del patrón que falta

**Template Method** define la estructura de un algoritmo en una clase base y deja que las subclases decidan cómo implementar algunos pasos, sin cambiar el orden general del proceso.

En este caso, el plugin sigue una secuencia fija de pasos:

1. Lectura
2. Validación
3. Transformación
4. Enriquecimiento
5. Persistencia

El problema es que todo está metido directamente dentro del método `Execute()`, lo que provoca varios dolores de cabeza:

* El código es rígido: no se pueden cambiar los pasos.
* Hay duplicación: la validación se repite en los pasos 2 y 4.
* Hay mucho acoplamiento: cada paso depende del anterior.
* No se pueden reutilizar o extender pasos de forma aislada.

#### Por qué usar Template Method

Este patrón soluciona el problema porque:

* Mantiene la estructura general del flujo en una clase base abstracta (PluginProcessorBase).
* Define métodos abstractos o virtuales para cada paso (Read, Validate, Transform, Enrich, Persist).
* Permite que las subclases personalicen o reemplacen pasos sin romper la secuencia principal.


```C#
// Archivo: BadPlugin.cs
// Este ejemplo es intencionalmente espagueti: cada "Paso" está acoplado y no es reutilizable.

using System;
using System.Collections.Generic;
using System.IO;

namespace BadPluginExample
{
    public class PluginProcessor
    {
        // Estado interno compartido entre pasos
        private Dictionary<string, object> context = new();

        public void Execute(string inputFile)
        {
            Console.WriteLine("Iniciando plugin...");

            // Paso 1: Lectura de archivo (mezcla de I/O y parsing)
            try
            {
                var content = File.ReadAllText(inputFile);
                // parsing rudimentario
                context["raw"] = content;
                Console.WriteLine("Paso 1 completado: lectura");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error lectura: {ex.Message}");
                return;
            }

            // Paso 2: Validación (código duplicado con paso 4)
            {
                var raw = context["raw"] as string ?? "";
                if (string.IsNullOrWhiteSpace(raw))
                {
                    Console.WriteLine("Validación fallida: contenido vacío");
                    return;
                }
                // regla de validación hardcodeada
                if (!raw.Contains("BEGIN"))
                {
                    Console.WriteLine("Validación fallida: falta BEGIN");
                    return;
                }
                Console.WriteLine("Paso 2 completado: validación");
            }

            // Paso 3: Transformación (acopla con detalles de formato)
            {
                var raw = context["raw"] as string ?? "";
                // hace reemplazos y formatea
                var transformed = raw.Replace("BEGIN", "").Replace("END", "").Trim();
                // agrega metadata
                transformed = $"#TRANSFORMED#\n{transformed}";
                context["transformed"] = transformed;
                Console.WriteLine("Paso 3 completado: transformación");
            }

            // Paso 4: Enriquecimiento (duplica validación y escribe log)
            {
                // duplicación: nuevamente valida estructura
                var transformed = context.ContainsKey("transformed") ? context["transformed"] as string : null;
                if (string.IsNullOrEmpty(transformed) || !transformed.StartsWith("#TRANSFORMED#"))
                {
                    Console.WriteLine("Enriquecimiento abortado: formato incorrecto");
                    return;
                }

                // enriquecimiento rudimentario
                context["enriched"] = transformed + "\n// enriched at " + DateTime.UtcNow;
                // logging inline
                Console.WriteLine("Paso 4 completado: enriquecimiento");
            }

            // Paso 5: Persistencia (IO directo)
            try
            {
                var outPath = Path.Combine(Directory.GetCurrentDirectory(), "output.txt");
                File.WriteAllText(outPath, context["enriched"] as string ?? "");
                Console.WriteLine($"Paso 5 completado: persistido en {outPath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error persistencia: {ex.Message}");
            }

            Console.WriteLine("Plugin finalizado.");
        }
    }

    // Clase de prueba rápida (no es parte de la librería)
    public static class Runner
    {
        public static void Main()
        {
            var processor = new PluginProcessor();
            // Cambia la ruta al probar localmente
            processor.Execute("input.txt");
        }
    }
}

```


En la versión original, todos los pasos estaban acoplados dentro de un solo método Execute(), lo que hacía el código rígido y difícil de mantener. 
Ahora, cada paso se separó en métodos individuales, y la validación —que antes se repetía— se centralizó en un único método Validate(). 
Además, se separó la lógica del flujo de las operaciones de entrada y salida, de modo que cada parte del proceso se encarga solo de su propia responsabilidad. 
Gracias a esta estructura, el sistema es más extensible, ya que se pueden crear nuevos plugins heredando de PluginProcessorBase, y también más fácil de probar, porque cada paso se puede testear de forma independiente.

```C#
using System;
using System.Collections.Generic;
using System.IO;

namespace PluginTemplateExample
{
    // Clase base que define el esqueleto del proceso
    public abstract class PluginProcessorBase
    {
        protected readonly Dictionary<string, object> context = new();

        // Método plantilla: define los pasos generales
        public void Execute(string inputFile)
        {
            Console.WriteLine("Iniciando plugin...");

            if (!ReadFile(inputFile)) return;
            if (!Validate()) return;
            Transform();
            if (!Enrich()) return;
            SaveOutput();

            Console.WriteLine("Plugin finalizado.");
        }

        // Métodos que pueden redefinirse
        protected abstract bool ReadFile(string path);
        protected abstract bool Validate();
        protected abstract void Transform();
        protected abstract bool Enrich();
        protected abstract void SaveOutput();
    }

    // Implementación concreta del plugin
    public class TextPluginProcessor : PluginProcessorBase
    {
        protected override bool ReadFile(string path)
        {
            try
            {
                var content = File.ReadAllText(path);
                context["raw"] = content;
                Console.WriteLine("Paso 1 completado: lectura");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error lectura: {ex.Message}");
                return false;
            }
        }

        protected override bool Validate()
        {
            var raw = context["raw"] as string ?? "";
            if (string.IsNullOrWhiteSpace(raw))
            {
                Console.WriteLine("Validación fallida: contenido vacío");
                return false;
            }

            if (!raw.Contains("BEGIN"))
            {
                Console.WriteLine("Validación fallida: falta BEGIN");
                return false;
            }

            Console.WriteLine("Paso 2 completado: validación");
            return true;
        }

        protected override void Transform()
        {
            var raw = context["raw"] as string ?? "";
            var transformed = raw.Replace("BEGIN", "").Replace("END", "").Trim();
            transformed = $"#TRANSFORMED#\n{transformed}";
            context["transformed"] = transformed;

            Console.WriteLine("Paso 3 completado: transformación");
        }

        protected override bool Enrich()
        {
            var transformed = context.ContainsKey("transformed") ? context["transformed"] as string : null;
            if (string.IsNullOrEmpty(transformed) || !transformed.StartsWith("#TRANSFORMED#"))
            {
                Console.WriteLine("Enriquecimiento abortado: formato incorrecto");
                return false;
            }

            context["enriched"] = $"{transformed}\n// enriched at {DateTime.UtcNow}";
            Console.WriteLine("Paso 4 completado: enriquecimiento");
            return true;
        }

        protected override void SaveOutput()
        {
            try
            {
                var outPath = Path.Combine(Directory.GetCurrentDirectory(), "output.txt");
                File.WriteAllText(outPath, context["enriched"] as string ?? "");
                Console.WriteLine($"Paso 5 completado: persistido en {outPath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error persistencia: {ex.Message}");
            }
        }
    }

 // Clase de prueba
    public static class Runner
    {
        public static void Main()
        {
            var processor = new TextPluginProcessor();
            processor.Execute("input.txt");
        }
    }
}
```
