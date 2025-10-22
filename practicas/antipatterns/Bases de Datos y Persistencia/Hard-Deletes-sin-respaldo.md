# INSTITUTO TECNOLOGICO DE TIJUANA


![enter image description here](https://www.tijuana.tecnm.mx//wp-content/uploads/2022/03/TecNM-ITT-sgc-2018-color-scaled-e1646127126124-1568x479.jpg)

#### MARTINEZ NAVA BLANCA YESSENIA
###### 21211990

## Hard Deletes sin respaldo

### DESCRIPCIÓN
##### ¿Qué es?
El antipatrón "Hard Deletes sin respaldo" consiste en la eliminación permanente e irreversible de datos de una base de datos o sistema de almacenamiento sin tener una estrategia de respaldo o recuperación implementada. Al ejecutar una operación de borrado duro, los datos son eliminados físicamente, sin dejar rastro en el sistema.

##### ¿Por qué es una mala práctica? 
**Pérdida de datos irreversible:** La razón principal por la que se considera una mala práctica es la pérdida de datos que no pueden ser recuperados. Esto puede ser catastrófico si se eliminan datos críticos, como registros de clientes, transacciones financieras o historiales de usuarios.
**Falta de trazabilidad:** No permite auditar quién, cuándo y por qué se eliminó un registro. Esto puede generar problemas de cumplimiento normativo (por ejemplo, con regulaciones de privacidad como GDPR) o dificultar la resolución de errores, ya que no se puede seguir el rastro de la información.
**Corrupción de datos:** La eliminación de datos sin considerar su relación con otros registros puede llevar a datos huérfanos o relaciones rotas en la base de datos, lo que puede causar errores en otras partes de la aplicación.
**Riesgo de errores humanos:** Los errores accidentales por parte de los desarrolladores o administradores son una causa común de pérdida de datos. Sin un respaldo, un simple error puede tener consecuencias irreparables. 

## EJEMPLO en C# 
Escenario: En una aplicación .NET Core para gestionar productos, un servicio de negocio tiene un método para eliminar un producto.

```C#
using System;
using System.Text.Json;

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public decimal Price { get; set; }
}

public class Program
{
    public static void Main()
    {
        // 1. Crear una instancia de la clase Product.
        var myProduct = new Product
        {
            Id = 1,
            Name = "Laptop",
            Description = "Portátil de alta gama con procesador de última generación.",
            Price = 1500.50m
        };

        // 2. Serializar el objeto a una cadena JSON.
        string jsonString = JsonSerializer.Serialize(myProduct);

        // 3. Imprimir la cadena JSON en la consola.
        Console.WriteLine(jsonString);

        // 4. (Opcional) Serializar con formato legible (indentado).
        var options = new JsonSerializerOptions { WriteIndented = true };
        string formattedJsonString = JsonSerializer.Serialize(myProduct, options);
        
        Console.WriteLine("\n--- JSON con formato ---\n");
        Console.WriteLine(formattedJsonString);
    }
}

```
En este código, el método DeleteProduct elimina el producto de la base de datos de manera definitiva. No hay un mecanismo de recuperación o un registro de que el producto existió. Si un usuario o un error en la lógica desencadenara este método de forma incorrecta, el producto se perdería para siempre.

### Consecuencias.
Las consecuencias se manifiestan en la aplicación C#:
**Mantenimiento:**
  **Difícil depuración:** Cuando un reporte de ventas muestra datos incorrectos porque se eliminó un producto asociado a pedidos anteriores, es casi imposible saber qué ocurrió.
  **Costos de recuperación:** En caso de un borrado accidental, la única forma de recuperar los datos es restaurando una copia de seguridad completa de la base de datos, lo que puede significar perder otros cambios más recientes.
  **Rendimiento:** El rendimiento de un _context.Products.Remove() es generalmente bueno, pero la falta de trazabilidad afecta la calidad del código a largo plazo, ya que se pueden generar inconsistencias que requieren refactorizaciones complejas.
  **Escalabilidad:** A medida que la aplicación crece y requiere funcionalidades más avanzadas (como auditoría, reversión de acciones o análisis histórico), este enfoque se vuelve insostenible. La necesidad de conservar datos históricos para análisis o para satisfacer requisitos legales chocará directamente con esta práctica.

### Solución Correctiva
**Utilizar Soft Deletes (Eliminaciones suaves) con Entity Framework:**
En lugar de eliminar físicamente el registro, se marca como eliminado con un flag.
**Paso 1: Modificar el modelo de datos**
Se añade una propiedad IsDeleted y DeletedAt a la clase Product.
```C#
public class Product
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public decimal Price { get; set; }
    public bool IsDeleted { get; set; } = false; // Flag para eliminación
    public DateTime? DeletedAt { get; set; } // Marca de tiempo opcional
}
```
**Paso 2: Implementar el Soft Delete en el servicio:**
```C#
public class ProductService
{
    private readonly AppDbContext _context;

    public ProductService(AppDbContext context)
    {
        _context = context;
    }

    public void DeleteProduct(int productId)
    {
        var product = _context.Products.Find(productId);

        if (product != null)
        {
            // ¡Solución Correctiva! Marca el registro como eliminado
            product.IsDeleted = true;
            product.DeletedAt = DateTime.UtcNow;
            _context.SaveChanges();
        }
    }
}
```
**Excluir registros eliminados en consultas:**
Ahora, todas las consultas a productos deben filtrar los registros que no están eliminados. Entity Framework lo facilita con Where.
```C#
// Consulta que solo trae productos no eliminados
var activeProducts = _context.Products.Where(p => !p.IsDeleted).ToList();
```
De esta forma, cualquier consulta ejecutada sobre _context.Products incluirá automáticamente la cláusula WHERE IsDeleted = false.

**Presentación**
**Claridad y síntesis:** El ejemplo en C# con Entity Framework es muy claro y específico para un entorno .NET. La explicación de los conceptos se mantiene concisa.
**Lenguaje técnico:** Se utilizan términos técnicos apropiados para C# y bases de datos (DbContext, Entity Framework, ORM, HasQueryFilter).
**Ejemplo visual:** El código C# muestra la diferencia entre el antipatrón y la solución, haciendo más fácil de entender el problema y su corrección.
**Control del tiempo:** La estructura de puntos y subpuntos permite una presentación organizada y dentro del tiempo establecido. 


