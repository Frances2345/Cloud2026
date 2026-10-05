using UnityEngine;
using Cloud2026.Core;
using Cloud2026.Models;
using Cloud2026.Services;

namespace Cloud2026.Gameplay
{
    public class GameSaveTriggers : MonoBehaviour
    {
        [SerializeField] private UGSCloudSaveService cloudSaveService;

        private PerfilJugador _perfil = new PerfilJugador();
        private InventarioJugador _inventario = new InventarioJugador();
        private ProgresoJugador _progreso = new ProgresoJugador();

        private int _contadorAcciones = 0;
        private const int LimiteAcciones = 5;

        private async void Start()
        {
            if (cloudSaveService != null)
            {
                // LECTURA ÚNICA AL ARRANCAR
                var (p, i, pr) = await cloudSaveService.CargarEstadoCompletoAsync();
                _perfil = p;
                _inventario = i;
                _progreso = pr;
            }
        }

        // MOMENTO 1: Al terminar un combate
        public async void OnCombateFinalizado()
        {
            Debug.Log("[MOMENTO 1] Combate finalizado -> Sincronizando con Cloud Save.");
            await cloudSaveService.GuardarEstadoCompletoAsync(_perfil, _inventario, _progreso);
        }

        // MOMENTO 2: Cada N acciones del jugador
        public async void RegistrarAccion()
        {
            _contadorAcciones++;
            if (_contadorAcciones >= LimiteAcciones)
            {
                _contadorAcciones = 0;
                Debug.Log($"[MOMENTO 2] Se alcanzaron {LimiteAcciones} acciones -> Guardando en Cloud Save.");
                await cloudSaveService.GuardarEstadoCompletoAsync(_perfil, _inventario, _progreso);
            }
        }

        // MOMENTO 3: Al cerrar la aplicación
        private async void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus && cloudSaveService != null)
            {
                Debug.Log("[MOMENTO 3] Aplicación pausada -> Guardando en Cloud Save.");
                await cloudSaveService.GuardarEstadoCompletoAsync(_perfil, _inventario, _progreso);
            }
        }

        private async void OnApplicationQuit()
        {
            if (cloudSaveService != null)
            {
                Debug.Log("[MOMENTO 3] Aplicación cerrada -> Guardando en Cloud Save.");
                await cloudSaveService.GuardarEstadoCompletoAsync(_perfil, _inventario, _progreso);
            }
        }
    }
}