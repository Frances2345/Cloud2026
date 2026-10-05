using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Unity.Services.CloudSave;
using Cloud2026.Core;
using Cloud2026.Models;

namespace Cloud2026.Services
{
    public class UGSCloudSaveService : MonoBehaviour, ICloudSaveService
    {
        public bool IsReady => true;

        // Miembros requeridos por la interfaz ICloudSaveService
        public event Action<string> OnCallFailed;

        // CORRECCIÓN: Retorna IDictionary<string, string> coincidiendo exactamente con la interfaz
        public IDictionary<string, string> CurrentWriteLocks => _writeLocks;
        private readonly Dictionary<string, string> _writeLocks = new Dictionary<string, string>();

        private const string KeyPerfil = "perfil";
        private const string KeyInventario = "inventario";
        private const string KeyProgreso = "progreso";

        private bool _yaCargado = false;

        // Métodos de compatibilidad requeridos por ICloudSaveService
        public async Task<PlayerProfileLoadResult> LoadProfileAsync()
        {
            var (perfil, _, _) = await CargarEstadoCompletoAsync();
            return new PlayerProfileLoadResult(new PlayerProfile(), default);
        }

        public async Task<bool> SaveProfileAsync(PlayerProfile profile)
        {
            var (p, i, pr) = await CargarEstadoCompletoAsync();
            return await GuardarEstadoCompletoAsync(p, i, pr);
        }

        /// <summary>
        /// PASO 6: Leer una sola vez al arrancar desde UGS o la caché local.
        /// </summary>
        public async Task<(PerfilJugador perfil, InventarioJugador inventario, ProgresoJugador progreso)> CargarEstadoCompletoAsync()
        {
            // Si ya se leyó al arrancar, devuelve directamente desde PlayerPrefs (Caché Local)
            if (_yaCargado && PlayerPrefs.HasKey(KeyPerfil))
            {
                Debug.Log("[Caché Local] Leyendo datos guardados localmente.");
                var pLocal = JsonUtility.FromJson<PerfilJugador>(PlayerPrefs.GetString(KeyPerfil));
                var iLocal = JsonUtility.FromJson<InventarioJugador>(PlayerPrefs.GetString(KeyInventario));
                var prLocal = JsonUtility.FromJson<ProgresoJugador>(PlayerPrefs.GetString(KeyProgreso));
                return (pLocal ?? new PerfilJugador(), iLocal ?? new InventarioJugador(), prLocal ?? new ProgresoJugador());
            }

            try
            {
                var keys = new HashSet<string> { KeyPerfil, KeyInventario, KeyProgreso };
                var data = await CloudSaveService.Instance.Data.Player.LoadAsync(keys);

                PerfilJugador perfil = data.TryGetValue(KeyPerfil, out var itemP)
                    ? itemP.Value.GetAs<PerfilJugador>() : new PerfilJugador();

                InventarioJugador inventario = data.TryGetValue(KeyInventario, out var itemI)
                    ? itemI.Value.GetAs<InventarioJugador>() : new InventarioJugador();

                ProgresoJugador progreso = data.TryGetValue(KeyProgreso, out var itemPr)
                    ? itemPr.Value.GetAs<ProgresoJugador>() : new ProgresoJugador();

                GuardarEnCacheLocal(perfil, inventario, progreso);
                _yaCargado = true;

                Debug.Log("[Cloud Save] Carga inicial de la nube completada y guardada en caché local.");
                return (perfil, inventario, progreso);
            }
            catch (Exception ex)
            {
                OnCallFailed?.Invoke(ex.Message);
                Debug.LogWarning($"[Cloud Save] Error conectando a la nube. Cargando copia local: {ex.Message}");
                _yaCargado = true;
                return (
                    PlayerPrefs.HasKey(KeyPerfil) ? JsonUtility.FromJson<PerfilJugador>(PlayerPrefs.GetString(KeyPerfil)) : new PerfilJugador(),
                    PlayerPrefs.HasKey(KeyInventario) ? JsonUtility.FromJson<InventarioJugador>(PlayerPrefs.GetString(KeyInventario)) : new InventarioJugador(),
                    PlayerPrefs.HasKey(KeyProgreso) ? JsonUtility.FromJson<ProgresoJugador>(PlayerPrefs.GetString(KeyProgreso)) : new ProgresoJugador()
                );
            }
        }

        /// <summary>
        /// PASO 6: Guarda localmente y sube en lote a Cloud Save.
        /// </summary>
        public async Task<bool> GuardarEstadoCompletoAsync(PerfilJugador perfil, InventarioJugador inventario, ProgresoJugador progreso)
        {
            try
            {
                // 1. Guardar localmente
                GuardarEnCacheLocal(perfil, inventario, progreso);

                // 2. Guardar en nube
                var datosAGuardar = new Dictionary<string, object>
                {
                    { KeyPerfil, perfil },
                    { KeyInventario, inventario },
                    { KeyProgreso, progreso }
                };

                await CloudSaveService.Instance.Data.Player.SaveAsync(datosAGuardar);
                Debug.Log("[Cloud Save] Guardado en la nube exitoso.");
                return true;
            }
            catch (Exception ex)
            {
                OnCallFailed?.Invoke(ex.Message);
                Debug.LogError($"[Cloud Save] Error al guardar en la nube: {ex.Message}");
                return false;
            }
        }

        private void GuardarEnCacheLocal(PerfilJugador perfil, InventarioJugador inventario, ProgresoJugador progreso)
        {
            PlayerPrefs.SetString(KeyPerfil, JsonUtility.ToJson(perfil));
            PlayerPrefs.SetString(KeyInventario, JsonUtility.ToJson(inventario));
            PlayerPrefs.SetString(KeyProgreso, JsonUtility.ToJson(progreso));
            PlayerPrefs.Save();
        }
    }
}