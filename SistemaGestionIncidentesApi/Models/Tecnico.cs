﻿public class Tecnico
{
    public int Id { get; set; }
    public string Nombre { get; set; }
    public string Correo { get; set; }
    public string? Estado { get; set; }

    public string? Clave { get; set; } // ahora es opcional
}
