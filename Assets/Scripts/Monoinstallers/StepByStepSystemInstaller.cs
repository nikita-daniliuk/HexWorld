using Zenject;

public class StepByStepSystemSystemInstaller : MonoInstaller
{   
    public override void InstallBindings()
    {
        Container.BindInterfacesAndSelfTo<StepByStepSystem>().AsSingle().WithArguments(ProjectContext.Instance.Container.Resolve<EventBus>());
    }
}