using Zenject;

public class PlayFabInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        Container.Bind<PlayFabManager>().AsSingle();
    }
}