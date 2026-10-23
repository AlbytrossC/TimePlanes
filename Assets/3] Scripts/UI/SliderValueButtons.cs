using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SliderValueButtons : MonoBehaviour
{
    public Slider slider;
    private TMP_Text label;
    
    void Start()
    {
        if (slider == null)
        {
            slider = GetComponent<Slider>();
        }
        label = GetComponentInChildren<TMP_Text>();
        label.text = slider.value.ToString();
    }

    public void AdjustValue(int direction)
    {
        slider.value += direction;
        slider.value = Mathf.Clamp(slider.value, slider.minValue, slider.maxValue); // Ensure the value stays within bounds
        slider.onValueChanged.Invoke(slider.value); // Trigger the slider's value changed event
        label.text = slider.value.ToString();
    }
}
