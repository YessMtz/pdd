## 27. Crear nodos compuestos que no contienen hijos

#### El siguiente CODESMELLS 
En el código se detectan algunos code smells: por un lado, el uso de números mágicos (como 42, 7, 16 o 2) escritos directamente en el código en lugar de definirse como constantes; también se repite lógica en métodos casi idénticos para añadir hijos, lo que genera duplicación de código; y por último, aparecen composites vacíos, sin hijos, que se usan únicamente para ilustrar el requerimiento.
```C#
using System;
using System.Collections.Generic;

namespace CompositeBadSmells
{
    // Tipo de componente (simulación de enum simple)
    public enum ComponentType
    {
        Leaf = 1,
        Composite = 2
    }

    // Clase base
    public abstract class Component
    {
        public string Name { get; set; }
        public ComponentType Type { get; set; }

        public abstract void Render(int depth);

        // Métodos virtuales para Composite
        public virtual void AddChild(Component child) { }
        public virtual void RemoveChild(Component child) { }
    }

    // Implementación de Leaf
    public class Leaf : Component
    {
        public int Value { get; set; }  // Números mágicos aquí

        public Leaf(string name, int value)
        {
            this.Name = name;
            this.Value = value;
            this.Type = ComponentType.Leaf;
        }

        public override void Render(int depth)
        {
            for (int i = 0; i < depth; i++) Console.Write(" ");
            Console.WriteLine($"- Leaf: {Name} (value={Value})");
        }
    }

    // Implementación de Composite
    public class Composite : Component
    {
        private List<Component> children;
        private int capacity; // Se inicializa con número mágico

        public Composite(string name)
        {
            this.Name = name;
            this.Type = ComponentType.Composite;
            this.capacity = 16;  // número mágico
            this.children = new List<Component>(capacity);
        }

        public override void Render(int depth)
        {
            for (int i = 0; i < depth; i++) Console.Write(" ");
            Console.WriteLine($"+ Composite: {Name} (children={children.Count})");

            foreach (var child in children)
            {
                child.Render(depth + 2);
            }
        }

        // Primer método AddChild
        public override void AddChild(Component child)
        {
            if (children.Count >= capacity)
            {
                capacity = capacity * 2; // número mágico
            }
            children.Add(child);
        }

        // Segundo método casi duplicado (code smell)
        public void AddChildAlt(Component child)
        {
            if (children.Count >= capacity)
            {
                capacity = capacity * 2; // duplicación
            }
            children.Add(child);
        }

        public override void RemoveChild(Component child)
        {
            children.Remove(child);
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            // Creamos composites (algunos vacíos)
            Composite root = new Composite("root");
            Composite emptyComposite1 = new Composite("empty-1"); // no se le agregan hijos
            Composite emptyComposite2 = new Composite("empty-2"); // tampoco

            // Creamos hojas con números mágicos
            Leaf leafA = new Leaf("leaf-A", 42); // número mágico
            Leaf leafB = new Leaf("leaf-B", 7);  // número mágico

            // Agregamos algunos hijos al root
            root.AddChild(emptyComposite1);
            root.AddChildAlt(leafA); // usamos la versión duplicada
            root.AddChild(leafB);

            Console.WriteLine("Estructura del árbol (nota composites vacíos con children=0):");
            root.Render(0);

            Console.WriteLine("\nRenderizando composites vacíos:");
            emptyComposite1.Render(0);
            emptyComposite2.Render(0);

            Console.WriteLine("\nFin del programa.");
        }
    }
}

```
Uno de los atributos ComponentType ya que el patron Composite usa polimorfismo para distinguir LEAF y COMPOSITE, reemplazando numeros magicos con constantes sifnificativas (InitialCapacity, GrowthFactor, DefaultValue).
Se elimino AddChildAlt y se agrego una interfaz comun IComponent para mayor extensibilidad, facil de leer y mantener.

```C#
using System;
using System.Collections.Generic;

namespace CompositeRefactored
{
    public interface IComponent
    {
        string Name { get; }
        void Render(int depth);
    }
    public class Leaf : IComponent
    {
        private const int DefaultValue = 10; // Constante para evitar número mágico
        public string Name { get; }
        public int Value { get; }

        public Leaf(string name, int value = DefaultValue)
        {
            Name = name;
            Value = value;
        }

        public void Render(int depth)
        {
            Console.WriteLine($"{new string(' ', depth)}- Leaf: {Name} (value={Value})");
        }
    }

    public class Composite : IComponent
    {
        private const int InitialCapacity = 16; // Constante definida
        private const int GrowthFactor = 2;     // Constante definida

        public string Name { get; }
        private readonly List<IComponent> _children;

        private int _capacity;

        public Composite(string name)
        {
            Name = name;
            _capacity = InitialCapacity;
            _children = new List<IComponent>(_capacity);
        }

        public void Render(int depth)
        {
            Console.WriteLine($"{new string(' ', depth)}+ Composite: {Name} (children={_children.Count})");

            foreach (var child in _children)
            {
                child.Render(depth + 2);
            }
        }

        public void AddChild(IComponent child)
        {
            if (_children.Count >= _capacity)
            {
                _capacity *= GrowthFactor;
            }
            _children.Add(child);
        }

        public void RemoveChild(IComponent child)
        {
            _children.Remove(child);
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            // Raíz del árbol
            var root = new Composite("root");

            // Composites vacíos (válidos dentro del patrón Composite)
            var emptyComposite1 = new Composite("empty-1");
            var emptyComposite2 = new Composite("empty-2");

            // Hojas con valores claros (sin números mágicos directos)
            var leafA = new Leaf("leaf-A", 42);
            var leafB = new Leaf("leaf-B", 7);

            // Agregamos hijos al root
            root.AddChild(emptyComposite1);
            root.AddChild(leafA);
            root.AddChild(leafB);

            Console.WriteLine("Estructura del árbol:");
            root.Render(0);

            Console.WriteLine("\nRenderizando composites vacíos:");
            emptyComposite1.Render(0);
            emptyComposite2.Render(0);

            Console.WriteLine("\nFin del programa.");
        }
    }
}

```


