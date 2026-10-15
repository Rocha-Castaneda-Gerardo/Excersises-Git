using System;
using System.Collections.Generic;
using System.Linq;

// ==============================================================================
// SOLID.cs-Rocha Castañeda Gerardo
// ==============================================================================

// Configuramos las inyecciones de dependencias iniciales
ICanalComunicacion canalEmail = new EmailCanal();
ICanalComunicacion canalSms = new SmsCanal();
INotificador notificador = new NotificadorVeterinaria(canalEmail, canalSms);

SistemaVeterinaria sistema = new SistemaVeterinaria(notificador);

Console.WriteLine("=== Caso 1: Mascota común ===");
// Correcto UpperCasting: Pasamos un 'Perro' que es tratado como 'IMascota'
sistema.AtenderMascota(new Perro("Lilo", 8));

Console.WriteLine("\n=== Caso 2: Tipo nuevo (Cerrado a modificación, Abierto a extensión) ===");
sistema.AtenderMascota(new Ave("Piolín", 2));

Console.WriteLine("\n=== Caso 3: Sistema especial (Cumpliendo Liskov/UpperCasting) ===");
SistemaVeterinaria sistemaEspecial = new SistemaVeterinariaEspecial(notificador);
sistemaEspecial.AtenderMascota(new Perro("Rex", 5));

Console.ReadLine();

// ---------------------------------------------------------
// 1. Abstracciones (Implementación de Interfaces)
// ---------------------------------------------------------

public interface IMascota
{
    string Nombre { get; }
    int Edad { get; }
    bool EsValida();
    decimal CalcularCostoVacuna();
    bool IncluirEnResumen { get; } // Reemplaza la regla hardcodeada de "Tortuga"
}

public interface ICanalComunicacion
{
    void Enviar(string mensaje);
}

public interface INotificador
{
    void Notificar(IMascota mascota);
}

// ---------------------------------------------------------
// 2. Extensión de Mascotas (Abierto/Cerrado)
// ---------------------------------------------------------

public abstract class MascotaBase : IMascota
{
    public string Nombre { get; }
    public int Edad { get; }

    // Por defecto todas se incluyen, a menos que la clase hija diga lo contrario
    public virtual bool IncluirEnResumen => true;

    protected MascotaBase(string nombre, int edad)
    {
        Nombre = nombre;
        Edad = edad;
    }

    public bool EsValida() => !string.IsNullOrEmpty(Nombre) && Edad > 0;

    // Cada mascota sabrá calcular su propio costo
    public abstract decimal CalcularCostoVacuna();
}

public class Perro : MascotaBase
{
    public Perro(string nombre, int edad) : base(nombre, edad) { }
    public override decimal CalcularCostoVacuna() => 200m;
}

public class Gato : MascotaBase
{
    public Gato(string nombre, int edad) : base(nombre, edad) { }
    public override decimal CalcularCostoVacuna() => 180m;
}

public class Tortuga : MascotaBase
{
    // Ocultamos la tortuga extendiendo su comportamiento, no tocando el sistema central
    public override bool IncluirEnResumen => false;
    public Tortuga(string nombre, int edad) : base(nombre, edad) { }
    public override decimal CalcularCostoVacuna() => 500m;
}

public class Ave : MascotaBase
{
    public Ave(string nombre, int edad) : base(nombre, edad) { }
    public override decimal CalcularCostoVacuna() => Edad * 50m;
}

// ---------------------------------------------------------
// 3. Canales de Notificación (Responsabilidad Única e Interfaces)
// ---------------------------------------------------------

public class EmailCanal : ICanalComunicacion
{
    public void Enviar(string mensaje) => Console.WriteLine($"[EMAIL] {mensaje}");
}

public class SmsCanal : ICanalComunicacion
{
    public void Enviar(string mensaje) => Console.WriteLine($"[SMS] {mensaje}");
}

// ---------------------------------------------------------
// 4. Notificador (Inversión de Dependencias)
// ---------------------------------------------------------

public class NotificadorVeterinaria : INotificador
{
    private readonly ICanalComunicacion _canalEmail;
    private readonly ICanalComunicacion _canalSms;

    // Dependemos de abstracciones, no de clases concretas
    public NotificadorVeterinaria(ICanalComunicacion canalEmail, ICanalComunicacion canalSms)
    {
        _canalEmail = canalEmail;
        _canalSms = canalSms;
    }

    public void Notificar(IMascota mascota)
    {
        if (mascota.Edad > 5)
            _canalEmail.Enviar($"Mascota mayor: {mascota.Nombre}");
        else
            _canalSms.Enviar($"Mascota joven: {mascota.Nombre}");
    }
}

// ---------------------------------------------------------
// 5. Sistema Principal (Liskov y UpperCasting)
// ---------------------------------------------------------

public class SistemaVeterinaria
{
    private readonly List<IMascota> _mascotas = new List<IMascota>();
    private readonly INotificador _notificador;

    // Inyección de dependencias
    public SistemaVeterinaria(INotificador notificador)
    {
        _notificador = notificador;
    }

    // Diseñado para respetar Liskov (LSP). Damos a las subclases una 
    // forma oficial y predecible de aplicar reglas de admisión.
    protected virtual bool PuedeAtender(IMascota mascota)
    {
        return true;
    }

    public void AtenderMascota(IMascota mascota)
    {
        if (!PuedeAtender(mascota)) return;

        if (!mascota.EsValida())
        {
            Console.WriteLine("Mascota inválida");
            return;
        }

        _mascotas.Add(mascota);
        var costo = mascota.CalcularCostoVacuna();
        Console.WriteLine($"Costo calculado: {costo}");

        _notificador.Notificar(mascota);

        Console.WriteLine("Resumen actual:");
        foreach (var m in _mascotas.Where(x => x.IncluirEnResumen))
        {
            // Usamos GetType().Name para reemplazar la propiedad "Tipo" del original
            Console.WriteLine($"{m.Nombre} - {m.GetType().Name}");
        }
    }
}

public class SistemaVeterinariaEspecial : SistemaVeterinaria
{
    public SistemaVeterinariaEspecial(INotificador notificador) : base(notificador) { }

    // Ya no rompemos el comportamiento central. Solo extendemos la regla de admisión.
    // Esto es un excelente uso de Liskov y UpperCasting (tratando a Perro como IMascota).
    protected override bool PuedeAtender(IMascota mascota)
    {
        if (mascota is Perro)
        {
            Console.WriteLine("Los perros no se atienden en este sistema especial");
            return false;
        }
        return base.PuedeAtender(mascota);
    }
}