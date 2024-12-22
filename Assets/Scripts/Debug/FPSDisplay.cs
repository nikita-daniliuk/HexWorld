using UnityEngine;

public class FPSDisplay : MonoBehaviour
{
    private float _fps;
    private float _deltaTime;

    [SerializeField] private GUIStyle _guiStyle;

    [SerializeField] private float offsetX = 10f;
    [SerializeField] private float offsetY = 10f;

    private void Update()
    {
        _deltaTime += (Time.unscaledDeltaTime - _deltaTime) * 0.1f;
        _fps = 1.0f / _deltaTime;
    }

    private void OnGUI()
    {
        string fpsText = $"FPS: {Mathf.RoundToInt(_fps)}";

        Vector2 position = CalculateAlignedPosition();

        GUI.Label(new Rect(position.x + offsetX, position.y + offsetY, 100, 30), fpsText, _guiStyle);
    }
    private Vector2 CalculateAlignedPosition()
    {
        float posX = 0;
        float posY = 0;

        switch (_guiStyle.alignment)
        {
            case TextAnchor.UpperLeft:
                posX = 0;
                posY = 0;
                break;
            case TextAnchor.UpperCenter:
                posX = Screen.width / 2;
                posY = 0;
                break;
            case TextAnchor.UpperRight:
                posX = Screen.width;
                posY = 0;
                break;
            case TextAnchor.MiddleLeft:
                posX = 0;
                posY = Screen.height / 2;
                break;
            case TextAnchor.MiddleCenter:
                posX = Screen.width / 2;
                posY = Screen.height / 2;
                break;
            case TextAnchor.MiddleRight:
                posX = Screen.width;
                posY = Screen.height / 2;
                break;
            case TextAnchor.LowerLeft:
                posX = 0;
                posY = Screen.height;
                break;
            case TextAnchor.LowerCenter:
                posX = Screen.width / 2;
                posY = Screen.height;
                break;
            case TextAnchor.LowerRight:
                posX = Screen.width;
                posY = Screen.height;
                break;
        }

        return new Vector2(posX, posY);
    }
}
