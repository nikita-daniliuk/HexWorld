using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class ButtonsSignalEmiter : MonoBehaviour
{
    [Inject] EventBus EventBus;
    [SerializeField] Button Button;
    [SerializeField] EnumSignals Signals;

    public void OnClick()
    {
        EventBus.Invoke(Signals);
    }
}