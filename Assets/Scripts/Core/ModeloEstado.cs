using System;
using System.Collections.Generic;

namespace Cloud2026.Models
{
    /// <summary>
    /// Identificadores de las llaves independientes en Cloud Save.
    /// </summary>
    public static class Llaves
    {
        public const string Perfil = "perfil";
        public const string Inventario = "inventario";
        public const string Progreso = "progreso";
        public const string Defensa = "defensa"; // Reservado para el Laboratorio 9
    }

    /// <summary>
    /// LLAVE "perfil": Datos generales de la cuenta.
    /// Cambia poco (al subir de nivel). Es la llave que más se lee y menos se escribe.
    /// </summary>
    [Serializable]
    public class PerfilJugador
    {
        public int version = 1; // Campo obligatorio para versionado/migración de esquemas
        public string nombre;
        public int nivelCuenta;
        public int experiencia;
        public string fechaCreacionUtc; // Formato ISO
    }

    /// <summary>
    /// Estructura de datos para los héroes dentro del inventario.
    /// </summary>
    [Serializable]
    public class Heroe
    {
        public string id;
        public string nombre;
        public int nivel;
    }

    /// <summary>
    /// LLAVE "inventario": Colecciones de héroes u objetos que crecen con el tiempo.
    /// Se separa en su propia llave para no tener que reescribir perfil ni progreso
    /// cada vez que se obtiene un nuevo héroe u objeto.
    /// </summary>
    [Serializable]
    public class InventarioJugador
    {
        public int version = 1; // Campo obligatorio
        public List<Heroe> heroes = new List<Heroe>();
        public List<string> objetos = new List<string>();
    }

    /// <summary>
    /// LLAVE "progreso": Estadísticas de combate y trofeos.
    /// Cambia en cada combate. Es la llave que dicta la frecuencia y costo de escritura.
    /// </summary>
    [Serializable]
    public class ProgresoJugador
    {
        public int version = 1; // Campo obligatorio
        public int combatesGanados;
        public int combatesPerdidos;
        public int trofeos;
        public List<string> desbloqueos = new List<string>();
    }

    /// <summary>
    /// LLAVE "defensa": Única de lectura pública (para el Laboratorio 9).
    /// Se deja definida y vacía por ahora. Solo guardará lo justo para poder ser atacado.
    /// </summary>
    [Serializable]
    public class DefensaPublica
    {
        public int version = 1;
    }
}