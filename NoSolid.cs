using System;
using System.Collections.Generic;
using System.Linq;

// ==============================================================================
// NoSolid.cs-Rocha Castañeda Gerardo
// ==============================================================================

var sistema = new SistemaVeterinaria();

Console.WriteLine("=== Caso 1: Mascota común ===");
sistema.AtenderMascota("Lilo", "Perro", 8);

Console.WriteLine("\n=== Caso 2: Tipo nuevo no contemplado ===");
sistema.AtenderMascota("Piolín", "Ave", 2);

Console.WriteLine("\n=== Caso 3: Sistema especial ===");
SistemaVeterinaria sistemaEspecial = new SistemaVeterinariaEspecial();
sistemaEspecial.AtenderMascota("Rex", "Perro", 5);

Console.ReadLine();

public class Mascota
{
    public string Nombre { get; }
    public string Tipo { get; }
    public int Edad { get; }

    public Mascota(string nombre, string tipo, int edad)
    {
        Nombre = nombre;
        Tipo = tipo;
        Edad = edad;
    }

    public bool EsValida()
    {
        return !string.IsNullOrEmpty(Nombre) && Edad > 0;
    }

    // [VIOLACIÓN OCP - Abierto a modificación / Cerrado a extensión]
    // Si queremos añadir un hámster o cambiar precios, tenemos que MODIFICAR este código. 
    // Debería ser extensible sin tocar la clase base.
    public decimal CalcularCostoVacuna()
    {
        if (Tipo.StartsWith("P")) return 200;
        if (Tipo.StartsWith("G")) return 180;
        if (Tipo.Contains("tuga")) return 500;

        return Edad * 50;
    }
}

// [VIOLACIÓN LSP - Principio de Sustitución de Liskov / Mal UpperCasting]
// Esta clase hereda de SistemaVeterinaria, pero ALTERA drásticamente el comportamiento 
// esperado (rechaza perros). Una subclase debe poder sustituir a su clase base 
// sin romper la funcionalidad o fortalecer precondiciones.
public class SistemaVeterinariaEspecial : SistemaVeterinaria
{
    public override void AtenderMascota(string nombre, string tipo, int edad)
    {
        if (tipo == "Perro")
        {
            Console.WriteLine("Los perros no se atienden en este sistema especial");
            return;
        }
        base.AtenderMascota(nombre, tipo, edad);
    }
}

// [VIOLACIÓN ISP - Segregación de Interfaces] y [VIOLACIÓN SRP]
// La clase se llama EmailService pero también envía SMS. Obliga a depender de métodos
// que tal vez no se necesiten, mezclando canales de comunicación.
public class EmailService
{
    public void Enviar(string mensaje)
    {
        Console.WriteLine($"[EMAIL] {mensaje}");
    }

    public void EnviarSMS(string mensaje)
    {
        Console.WriteLine($"[SMS] {mensaje}");
    }
}

public class Notificador
{
    // [VIOLACIÓN DIP - Inversión de Dependencias]
    // Se está instanciando EmailService directamente (fuerte acoplamiento).
    // Depende de una implementación concreta en lugar de una abstracción (interfaz).
    private EmailService email = new EmailService();

    // [VIOLACIÓN OCP] 
    // Si en el futuro queremos notificar por WhatsApp, hay que modificar esta clase.
    public void Notificar(Mascota mascota)
    {
        if (mascota.Edad > 5)
        {
            email.Enviar($"Mascota mayor: {mascota.Nombre}");
        }
        else
        {
            email.EnviarSMS($"Mascota joven: {mascota.Nombre}");
        }
    }
}

public class SistemaVeterinaria
{
    private List<Mascota> mascotas = new List<Mascota>();

    // [VIOLACIÓN DIP] Fuerte acoplamiento instanciando 'Notificador' con 'new'.
    private Notificador notificador = new Notificador();

    // [VIOLACIÓN SRP - Responsabilidad Única]
    // Este método hace demasiado: Crea objetos, valida, calcula costos, notifica 
    // y además genera reportes. Tiene múltiples razones para cambiar.
    public virtual void AtenderMascota(string nombre, string tipo, int edad)
    {
        var mascota = new Mascota(nombre, tipo, edad);

        if (!mascota.EsValida())
        {
            Console.WriteLine("Mascota inválida");
            return;
        }

        mascotas.Add(mascota);
        var costo = mascota.CalcularCostoVacuna();
        Console.WriteLine($"Costo calculado: {costo}");

        notificador.Notificar(mascota);

        Console.WriteLine("Resumen actual:");

        // [VIOLACIÓN OCP] Regla de negocio quemada ("hardcoded").
        // Si queremos excluir otra mascota del resumen, tenemos que tocar este código.
        foreach (var m in mascotas.Where(x => x.Tipo != "Tortuga"))
        {
            Console.WriteLine($"{m.Nombre} - {m.Tipo}");
        }
    }
}