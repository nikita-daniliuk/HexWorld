using Zenject;

public class SceneLoaderInstaller : MonoInstaller
{
    [Inject] EventBus EventBus;

    public override void InstallBindings()
    {
        SceneLoader SceneLoader = new SceneLoader(EventBus);
        Container.BindInstance<SceneLoader>(SceneLoader).AsSingle();
    }
}