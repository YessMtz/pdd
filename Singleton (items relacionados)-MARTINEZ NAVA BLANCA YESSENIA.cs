/*
Singleton (items relacionados) - MARTINEZ NAVA BLANCA YESSENIA
Problemas: estado mutable, concurrencia, mal uso de global state, reciclaje indebido.
27. Singleton con dependencia externa inyectada mal
// 🚨 Código con problemas
// - Singleton mutable
// - Guarda un DbContext (o dependencia externa) como propiedad global
// - Riesgo de concurrencia y global state

using System;

namespace BadSingletonExample
{
    public class DataService
    {
        // Dependencia externa compartida peligrosamente
        public FakeDbContext Context { get; set; }

        // Instancia estática mutable
        private static DataService _instance;

        private DataService(FakeDbContext ctx)
        {
            Context = ctx;
        }

        // Implementación naive de Singleton
        public static DataService GetInstance(FakeDbContext ctx)
        {
            if (_instance == null)
            {
                _instance = new DataService(ctx);
            }
            return _instance;
        }

        public void SaveData(string data)
        {
            // Acceso directo al contexto compartido
            Context.Insert(data);
        }
    }

    // Dependencia externa simulada
    public class FakeDbContext
    {
        public void Insert(string value)
        {
            Console.WriteLine($"[DB] Insertando valor: {value}");
        }
    }
}

*/


// 🚨 Código refactorizado con Inyección de Dependencias (DI)
// - Clases sin estado
// - Dependencias inyectadas en el constructor
// - Thread-safe y fácilmente testeable

using System;

namespace GoodExample
{
    // Interfaz para la dependencia, para un buen desacoplamiento
    // ya no mantiene un estado global, para el uso de multiples hilos
    public interface IDataService
    {
        void SaveData(string data);
    }
    // 2. La clase de servicio ahora depende de una interfaz
    //    y ya no es un Singleton.
    //    Recibe el DbContext en el constructor.
    public class DataService : IDataService
    {
        // Dependencia inyectada sin necesidad de usar un singleton
        private readonly FakeDbContext _context;

        // La dependencia se inyecta por el constructor
        public DataService(FakeDbContext ctx)
        {
            _context = ctx ?? throw new ArgumentNullException(nameof(ctx));
        }

        public void SaveData(string data)
        {
            _context.Insert(data);
        }
    }

    // 3. La dependencia (simulada) sigue igual
    public class FakeDbContext
    {
        public void Insert(string value)
        {
            Console.WriteLine($"[DB] Insertando valor: {value}");
        }
    }
}

/* 4. Ejemplo de cómo se usaría en una aplicación
    En un entorno real (como ASP.NET Core), un contenedor de DI
    se encargaría de esto automáticamente.
*/
//    Ejemplo de uso manual:
//    var dbContext = new FakeDbContext();
//    var dataService = new DataService(dbContext);
//    dataService.SaveData("Hola mundo");
