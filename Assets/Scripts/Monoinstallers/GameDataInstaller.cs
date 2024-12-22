using Zenject;

public class GameDataInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        Container.Bind<GameData>().AsSingle();
    }
}