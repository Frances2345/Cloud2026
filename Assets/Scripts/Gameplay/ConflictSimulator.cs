using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Unity.Services.CloudSave;
using Unity.Services.CloudSave.Models; // Contiene SaveItem y CloudSaveConflictException
using Cloud2026.Models;

namespace Cloud2026.Gameplay
{
    public class ConflictSimulator : MonoBehaviour
    {
        private const string TargetKey = "perfil";

        /// <summary>
        /// PASO 7: Simula el conflicto enviando un WriteLock caducado/viejo.
        /// Se prueba desde Unity haciendo Clic Derecho en el componente -> "Simular Conflicto Paso 7"
        /// </summary>
        [ContextMenu("Simular Conflicto Paso 7")]
        public async void SimularConflicto()
        {
            Debug.Log("=== [PASO 7] INICIANDO PRUEBA DE CONFLICTO LOCAL-NUBE ===");

            try
            {
                // 1. Cargar estado de UGS y obtener el WriteLock actual (string) del servidor
                var keys = new HashSet<string> { TargetKey };
                var data = await CloudSaveService.Instance.Data.Player.LoadAsync(keys);

                if (!data.TryGetValue(TargetKey, out var item))
                {
                    Debug.LogError("[Paso 7] Primero debes haber guardado la llave 'perfil' al menos una vez.");
                    return;
                }

                string writeLockViejo = item.WriteLock; // Es un string
                Debug.Log($"[1] WriteLock viejo guardado localmente: {writeLockViejo}");

                // 2. Generar cambio en el servidor (Escribir desde otro lugar con DATO DISTINTO)
                var perfilServidor = item.Value.GetAs<PerfilJugador>();
                perfilServidor.experiencia += 100; // Cambio en el servidor

                var datosServidor = new Dictionary<string, object> { { TargetKey, perfilServidor } };
                await CloudSaveService.Instance.Data.Player.SaveAsync(datosServidor);
                Debug.Log("[2] Modificación en servidor exitosa -> El WriteLock en UGS ha caducado para nuestro cliente.");

                // 3. Modificar dato localmente
                var perfilLocal = item.Value.GetAs<PerfilJugador>();
                perfilLocal.experiencia += 50; // Cambio local desacoplado

                // 4. Intentar guardar enviando el SaveItem con el WriteLock viejo (Provoca CloudSaveConflictException 409)
                Debug.Log($"[3] Enviando SaveAsync con el WriteLock viejo ({writeLockViejo})...");

                var datosLocal = new Dictionary<string, SaveItem>
                {
                    { TargetKey, new SaveItem(perfilLocal, writeLockViejo) }
                };

                await CloudSaveService.Instance.Data.Player.SaveAsync(datosLocal);
            }
            catch (CloudSaveConflictException ex)
            {
                // CAPTURA DEL CONFLICTO 409
                Debug.LogError("[409 CONFLICTO DETECTADO] Excepción capturada correctamente: CloudSaveConflictException");

                foreach (var detail in ex.Details)
                {
                    Debug.LogWarning($"[Detalles] Llave: {detail.Key} | Lock intentado: {detail.AttemptedWriteLock} | Lock real en nube: {detail.ExistingWriteLock}");
                }

                // RESOLUCIÓN DEL CONFLICTO
                await AplicarPoliticaResolucionFusion();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Error] Ocurrió una excepción: {ex.Message}");
            }
        }

        /// <summary>
        /// POLÍTICA ELEGIDA: Fusión por campo (Field Merger)
        /// Se leen los datos frescos de la nube y se combinan los valores con los cambios locales.
        /// </summary>
        private async Task AplicarPoliticaResolucionFusion()
        {
            Debug.Log("[Resolución] Aplicando Política: Fusión por campo (Field Merger)...");

            // 1. Recargar los datos más recientes de la nube junto a su NUEVO WriteLock
            var data = await CloudSaveService.Instance.Data.Player.LoadAsync(new HashSet<string> { TargetKey });
            var perfilNube = data[TargetKey].Value.GetAs<PerfilJugador>();
            string nuevoWriteLock = data[TargetKey].WriteLock;

            // 2. Fusión por campo (Tomar el máximo acumulado de experiencia)
            perfilNube.experiencia = Math.Max(perfilNube.experiencia, 50);

            // 3. Reintentar el guardado con el nuevo WriteLock usando SaveItem
            var datosNube = new Dictionary<string, SaveItem>
            {
                { TargetKey, new SaveItem(perfilNube, nuevoWriteLock) }
            };

            await CloudSaveService.Instance.Data.Player.SaveAsync(datosNube);

            Debug.Log("[Resolución] ¡Conflicto resuelto y sincronizado con éxito usando el WriteLock actualizado!");
        }
    }
}