using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;
using TMPro;

public class AnonymousAuthManager : MonoBehaviour
{
    [SerializeField] private TMP_Text playerIdText;

    // Referencias a las cajas de texto de la UI (Paso 4)
    [SerializeField] private TMP_InputField usernameInput;
    [SerializeField] private TMP_InputField passwordInput;

    private async void Start()
    {
        await InitializeAndSignInAsync();
    }

    private async Task InitializeAndSignInAsync()
    {
        try
        {
            // 1. Inicializar Unity Services si aún no lo está
            if (UnityServices.State == ServicesInitializationState.Uninitialized)
            {
                await UnityServices.InitializeAsync();
            }

            // 2. Iniciar sesión anónima
            if (!AuthenticationService.Instance.IsSignedIn)
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
            }

            // 3. Obtener y mostrar el Player ID
            string playerId = AuthenticationService.Instance.PlayerId;
            Debug.Log($"Player ID: {playerId}");

            if (playerIdText != null)
            {
                playerIdText.text = $"Player ID: {playerId}";
            }
        }
        catch (AuthenticationException ex)
        {
            Debug.LogError($"Error de autenticación: {ex.ErrorCode} - {ex.Message}");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Error general: {ex.Message}");
        }
    }

    // --- NUEVO MÉTODO PARA PASOS 4 Y 5 ---
    public async void LinkAccount()
    {
        string username = usernameInput != null ? usernameInput.text : "";
        string password = passwordInput != null ? passwordInput.text : "";

        // 1. Validación en el CLIENTE: el SDK solo exige que no sean nulos ni vacíos
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            Debug.LogWarning("Validación cliente: El usuario o la contraseña no pueden estar vacíos.");
            return;
        }

        try
        {
            // 2. PASO 5: Vincular la sesión anónima activa usando AddUsernamePasswordAsync
            await AuthenticationService.Instance.AddUsernamePasswordAsync(username, password);
            Debug.Log($"¡Cuenta vinculada con éxito! Player ID actual: {AuthenticationService.Instance.PlayerId}");
        }
        catch (AuthenticationException ex)
        {
            // 3. PASO 4: El SERVIDOR es quien rechaza contraseñas cortas o con mal formato
            Debug.LogError($"[Servidor ErrorCode {ex.ErrorCode}]: {ex.Message}");
        }
        catch (RequestFailedException ex)
        {
            Debug.LogError($"Error de solicitud: {ex.Message}");
        }
    }
}