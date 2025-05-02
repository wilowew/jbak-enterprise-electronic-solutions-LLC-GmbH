using JBAK;
using System;
using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance;

    private PlayerControls controls;
    private string savePath;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            savePath = Path.Combine(Application.persistentDataPath, "controls.json");
            InitializeControls();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeControls()
    {
        controls = new PlayerControls();
        LoadBindings();
    }

    public void RebindAction(string actionName, int bindingIndex, Action callback)
    {
        InputAction action = controls.asset.FindAction(actionName);
        if (action == null)
        {
            Debug.LogError($"Action {actionName} not found!");
            return;
        }

        action.PerformInteractiveRebinding(bindingIndex)
            .OnComplete(operation =>
            {
                callback?.Invoke();
                operation.Dispose();
                SaveBindings();
            })
            .Start();
    }

    public InputAction GetAction(string actionName) => controls.asset.FindAction(actionName);

    private void SaveBindings()
    {
        string overrides = controls.asset.SaveBindingOverridesAsJson();
        File.WriteAllText(savePath, overrides);
    }

    private void LoadBindings()
    {
        if (File.Exists(savePath))
        {
            string overrides = File.ReadAllText(savePath);
            controls.asset.LoadBindingOverridesFromJson(overrides);
        }
        controls.Enable();
    }

    public void ResetToDefaults()
    {
        controls.asset.RemoveAllBindingOverrides();
        SaveBindings();
        LoadBindings();
    }
}