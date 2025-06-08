using UnityEngine;
using UnityEngine.UI;

public class OptionsMenu : MonoBehaviour
{
    public Slider MouseXSensitivitySlider;
    public Slider MouseYSensitivitySlider;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        MouseXSensitivitySlider.value = GameManager.Instance.mouseSensitivity.x;
        MouseYSensitivitySlider.value = GameManager.Instance.mouseSensitivity.y;

        MouseXSensitivitySlider.onValueChanged.AddListener((value) =>
        {
            GameManager.Instance.mouseSensitivity.x = value;
            ApplySettings();
        });
        MouseYSensitivitySlider.onValueChanged.AddListener((value) =>
        {
            GameManager.Instance.mouseSensitivity.y = value;
            ApplySettings();
        });
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ResetMouseSensitivity()
    {
        MouseXSensitivitySlider.value = GameManager.Instance.originalMouseSensitivity.x;
        MouseYSensitivitySlider.value = GameManager.Instance.originalMouseSensitivity.y;
    }

    void ApplySettings()
    {
        //
    }
}
