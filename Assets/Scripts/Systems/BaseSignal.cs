using UnityEngine;

public abstract class BaseSignal : MonoBehaviour
{
    public delegate void Signal(object Obj);

    public event Signal MySignal;

    public void Subscribe(Signal listener)
    {
        MySignal += listener;
    }

    public void Unsubscribe(Signal listener)
    {
        MySignal -= listener;
    }

    public void EmitSignal(object Obj)
    {
        #if UNITY_EDITOR
        if (Obj is Message)
        {
            Message message = (Message)Obj;
            string senderText = message.Sender != null ? message.Sender.name : "Sender not specified";
            string dataTypeText;
            string dataText;

            switch (message.Data)
            {
                case string stringData:
                    dataTypeText = "System.String";
                    dataText = stringData;
                    break;
                case int intData:
                    dataTypeText = "System.Int32";
                    dataText = $"Integer value: {intData}";
                    break;
                case float floatData:
                    dataTypeText = "System.Single";
                    dataText = $"Float value: {floatData:F2}";
                    break;
                case GameObject gameObjectData:
                    dataTypeText = "UnityEngine.GameObject";
                    dataText = $"GameObject: {gameObjectData.name}";
                    break;
                case null:
                    dataTypeText = "null";
                    dataText = "Data is null";
                    break;
                default:
                    dataTypeText = message.Data?.GetType().ToString() ?? "Unknown type";
                    dataText = message.Data?.ToString() ?? "Data not specified";
                    break;
            }

            Debug.Log($"<color=yellow><b>Sender:</b></color> {senderText} <color=#ADD8E6><b>Signal:</b></color> {message.Signal} <color=green><b>DataType:</b></color> {dataTypeText} <color=orange><b>Data:</b></color> {dataText}");
        }
        #endif

        MySignal?.Invoke(Obj);
    }
}