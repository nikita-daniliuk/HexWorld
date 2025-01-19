using UnityEngine;
using Zenject;
using Unity.Services.Authentication;
using Unity.Services.Core;

#if!UNITY_EDITOR
using System.Diagnostics;
#endif

public class PlayFabAutorization : MonoBehaviour
{
    [Inject] EventBus EventBus;
    [Inject] GameData GameData;
    [Inject] PlayFabManager PlayFabManager;

    // void Start()
    // {
    //     #if !UNITY_EDITOR
    //     Process currentProcess = Process.GetCurrentProcess();
    //     Process[] processes = Process.GetProcessesByName(currentProcess.ProcessName);

    //     if (processes.Length > 1)
    //     {
    //         UnityEngine.Debug.LogError("Another instance of the game is already running.");
    //         Canvas.SetActive(true);
    //         return;
    //     }
    //     #endif
    //     PlayFabManager.Login();
    //     EventBus.Subscribe<EnumPlayFabSignals>(SignalBox);
    // }

    public void Quit()
    {
        Application.Quit();
    }

    void SignalBox<T>(T Obj)
    {
        switch (Obj)
        {
            case EnumPlayFabSignals.LoginSucces :
                Registration();
                #if !UNITY_EDITOR
                GameData.PlayerName = SystemInfo.deviceUniqueIdentifier;
                #else
                GameData.PlayerName = "Test";
                #endif
                break;
            default: break;
        }
    }

    async void Registration()
    {
        await UnityServices.InitializeAsync();
        await AuthenticationService.Instance.SignInAnonymouslyAsync();    
        EventBus.Invoke(EnumSignals.LoadingScene);     
    }

    void OnDestroy() => EventBus.UnsubscribeFromAll<object>(SignalBox);
}