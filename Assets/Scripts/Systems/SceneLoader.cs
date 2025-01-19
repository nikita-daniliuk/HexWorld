using UnityEngine;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;

public class SceneLoader : ISystems
{
    EventBus EventBus;

    public SceneLoader(EventBus EventBus)
    {
        this.EventBus = EventBus;
        this.EventBus.Subscribe<EnumSignals>(SignalBox);
        LoadGame();
    }
    
    protected void SignalBox<T>(T Obj)
    {
        switch (Obj)
        {
            case EnumSignals.LoadingScene:
                LoadGame();
                break;
            default: break;
        }
    }

    async void LoadGame()
    {
        if (IsUISceneActive()) 
        {
            await UnloadSceneAsync(2);
        }

        await LoadSceneAsync(1);

        await LoadSceneAsync(2, LoadSceneMode.Additive);
    }

    private bool IsUISceneActive()
    {
        Scene activeScene = SceneManager.GetActiveScene();
        return activeScene.buildIndex == 2;
    }

    private async Task LoadSceneAsync(int sceneIndex, LoadSceneMode mode = LoadSceneMode.Single)
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneIndex, mode);

        while (!asyncLoad.isDone)
        {
            await Task.Yield();
        }
    }

    private async Task UnloadSceneAsync(int sceneIndex)
    {
        AsyncOperation asyncUnload = SceneManager.UnloadSceneAsync(sceneIndex);

        while (!asyncUnload.isDone)
        {
            await Task.Yield();
        }
    }
}