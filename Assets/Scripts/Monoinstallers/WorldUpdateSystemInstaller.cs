using Zenject;

public class WorldUpdateSystemInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        Container.BindInterfacesAndSelfTo<WorldUpdateSystem>().AsSingle();
    }
}