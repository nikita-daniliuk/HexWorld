using UnityEngine;

public class FPSClamper : MonoBehaviour
{
    [SerializeField] int FPS;

    void Start()
    {
        Application.targetFrameRate = FPS;
    }
}