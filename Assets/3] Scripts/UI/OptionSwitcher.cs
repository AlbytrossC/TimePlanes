using UnityEngine;
using TMPro;

public class OptionSwitcher : MonoBehaviour
{
    public GameObject[] options; // Array of option GameObjects
    public int currentOptionIndex = 0; // Index of the currently active option
    public GameObject ActiveOptionDisplay; // Reference to the currently active option display
    public GameObject ActiveOptionIndicatorPanel; // Reference to the panel that indicates the active and available options

    private void Start()
    {
        // Fill the options array with the child GameObjects of ActiveOptionIndicatorPanel
        options = new GameObject[ActiveOptionIndicatorPanel.transform.childCount];
        for (int i = 0; i < options.Length; i++)
        {
            options[i] = ActiveOptionIndicatorPanel.transform.GetChild(i).gameObject;
        }

        UpdateActiveOptionDisplay();
    }

    public void SwitchOption(int direction)
    {
        // Update the current option index based on the direction (1 for next, -1 for previous)
        currentOptionIndex += direction;

        // Wrap around if the index goes out of bounds
        if (currentOptionIndex < 0)
        {
            currentOptionIndex = options.Length - 1;
        }
        else if (currentOptionIndex >= options.Length)
        {
            currentOptionIndex = 0;
        }

        // Update the active option display and indicator panel
        UpdateActiveOptionDisplay();
    }

    void UpdateActiveOptionDisplay()
    {

        // change the colour of the indicator of the currently active option to green by setting the color of the active indicator to green and the rest to white
        for (int i = 0; i < options.Length; i++)
        {
            if (i == currentOptionIndex)
            {
                options[i].GetComponent<UnityEngine.UI.Image>().color = Color.green;
            }
            else
            {
                options[i].GetComponent<UnityEngine.UI.Image>().color = Color.white;
            }
        }
        // Update the active option display reference
        ActiveOptionDisplay.GetComponent<TMP_Text>().text = options[currentOptionIndex].gameObject.name;
    }


}
