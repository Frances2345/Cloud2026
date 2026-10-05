using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cloud2026.Core;
using Cloud2026.Models;

namespace Cloud2026.Services
{
    /// <summary>
    /// Contrato para guardar y cargar el perfil del jugador en Cloud Save. Desacopla la UI y
    /// el gameplay del SDK, igual que ICloudCodeService con Cloud Code.
    /// </summary>
    public interface ICloudSaveService
    {
        /// <summary>Se dispara cuando una llamada falla, con un mensaje apto para la UI.</summary>
        event Action<string> OnCallFailed;

        /// <summary>
        /// True si los servicios están inicializados y hay sesión iniciada.
        /// </summary>
        bool IsReady { get; }

        /// <summary>
        /// Diccionario de Write Locks devueltos por el servidor tras la última operación.
        /// </summary>
        IDictionary<string, string> CurrentWriteLocks { get; }

        /// <summary>
        /// Carga el perfil comparando con la caché local.
        /// </summary>
        Task<PlayerProfileLoadResult> LoadProfileAsync();

        /// <summary>
        /// Sella el perfil y lo sube a Cloud Save.
        /// </summary>
        Task<bool> SaveProfileAsync(PlayerProfile profile);

        /// <summary>
        /// PASO 3: Guardado unificado de las tres llaves (perfil, inventario, progreso)
        /// en una sola llamada batch HTTP.
        /// </summary>
        Task<bool> GuardarEstadoCompletoAsync(PerfilJugador perfil, InventarioJugador inventario, ProgresoJugador progreso);

        /// <summary>
        /// PASO 4: Carga unificada de las tres llaves con LoadAsync.
        /// Gestiona automáticamente el caso de jugador nuevo (diccionario vacío) devolviendo instancias por defecto.
        /// </summary>
        Task<(PerfilJugador perfil, InventarioJugador inventario, ProgresoJugador progreso)> CargarEstadoCompletoAsync();
    }
}