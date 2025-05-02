using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class RebindButton : MonoBehaviour
{
    [SerializeField] private string actionName;
    [SerializeField] private int bindingIndex;
    [SerializeField] private TMP_Text bindingText;

    private InputAction action;

    private void Start()
    {
        action = InputManager.Instance.GetAction(actionName);
        UpdateButtonText();
    }

    public void StartRebinding()
    {
        bindingText.text = "Press any key...";
        InputManager.Instance.RebindAction(actionName, bindingIndex, () =>
        {
            UpdateButtonText();
        });
    }

    private void UpdateButtonText()
    {
        bindingText.text = action.bindings[bindingIndex].ToDisplayString();
    }
}