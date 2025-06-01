using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class NPCDeathOnDialogueEnd : MonoBehaviour
{
    [Header("Ссылка на диалог")]
    [SerializeField] private Dialogue targetDialogue;

    [Header("Настройки замены")]
    [SerializeField] private GameObject[] npcsToReplace;
    [SerializeField] private GameObject deathPrefab;
    [SerializeField] private float delayAfterDialogue = 3f;

    [Header("Настройки перехода на сцену")]
    [SerializeField] private bool transitionAfterReplace = true;
    [SerializeField] private string nextSceneName;
    [SerializeField] private float cameraMoveHeight = 5f;
    [SerializeField] private float cameraMoveHorizontal = 0f;
    [SerializeField] private float sceneTransitionDelay = 1f;

    private SceneTransistor2_cutscene sceneTransistor;

    private void Start()
    {
        SubscribeToDialogueEvent();
        sceneTransistor = FindObjectOfType<SceneTransistor2_cutscene>();
    }

    private void OnEnable()
    {
        SubscribeToDialogueEvent();
    }

    private void OnDisable()
    {
        UnsubscribeFromDialogueEvent();
    }

    private void SubscribeToDialogueEvent()
    {
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.OnDialogueEnd += HandleDialogueEnd;
        }
    }

    private void UnsubscribeFromDialogueEvent()
    {
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.OnDialogueEnd -= HandleDialogueEnd;
        }
    }

    private void HandleDialogueEnd(Dialogue endedDialogue)
    {
        if (endedDialogue == targetDialogue)
        {
            StartCoroutine(ReplaceAndTransition());
        }
    }

    private IEnumerator ReplaceAndTransition()
    {
        yield return new WaitForSeconds(delayAfterDialogue);

        foreach (GameObject npc in npcsToReplace)
        {
            if (npc != null)
            {
                Instantiate(deathPrefab, npc.transform.position, npc.transform.rotation);
                Destroy(npc);
            }
        }

        if (transitionAfterReplace)
        {
            yield return new WaitForSeconds(sceneTransitionDelay);

            if (sceneTransistor != null)
            {
                sceneTransistor.SetSceneParameters(
                    nextSceneName,
                    cameraMoveHeight,
                    cameraMoveHorizontal
                );

                sceneTransistor.StartTransition();
            }
            else
            {
                Debug.LogWarning("SceneTransistor not found. Loading scene directly.");
                SceneManager.LoadScene(nextSceneName);
            }
        }

        enabled = false;
    }
}