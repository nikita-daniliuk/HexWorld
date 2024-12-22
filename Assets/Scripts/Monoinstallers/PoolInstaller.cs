using Zenject;

public class PoolInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        Container.BindInterfacesAndSelfTo<Pool>().AsSingle();
    }
}