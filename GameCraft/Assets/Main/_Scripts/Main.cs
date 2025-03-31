using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Main : MonoBehaviour
{
    public static Main Instance { get; private set; } // Синглтон
    public QuestionDatabase questionDatabase;
    public List<Button> questionButtons;
    public TMP_Text responseText;

    public GameObject mapCanvas;
    public GameObject dialogueCanvas;
    public Button backButton;
    public Button accuseButton; 

    private List<QuestionEntry> selectedQuestions = new List<QuestionEntry>();
    private Dictionary<Button, QuestionEntry> buttonQuestionMap = new Dictionary<Button, QuestionEntry>();

    public List<NPC> npcs;

    public NPC currentNPC { get; private set; }
    public NPC thief { get; private set; }
    private bool isAccusing = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject); // Если `Main` уже есть, уничтожаем дубликат
            return;
        }
    }

    void Start()
    {
        ChooseThief();
        SelectQuestionsOnce();
        dialogueCanvas.SetActive(false);
        backButton.onClick.AddListener(ExitDialogue);
        accuseButton.onClick.AddListener(EnterAccusationMode);
    }

    void ChooseThief()
    {
        if (npcs.Count == 0)
        {
            Debug.LogError("Нет NPC для выбора вора!");
            return;
        }

        int index = Random.Range(0, npcs.Count);
        thief = npcs[index];
        thief.isThief = true;
        Debug.Log($"Вором стал: {thief.npcName}");
    }

    void SelectQuestionsOnce()
    {
        selectedQuestions.Clear();
        List<QuestionEntry> allQuestions = new List<QuestionEntry>(questionDatabase.questions);

        if (allQuestions.Count < 4)
        {
            Debug.LogError("Недостаточно вопросов в базе!");
            return;
        }

        for (int i = 0; i < 4; i++)
        {
            int index = Random.Range(0, allQuestions.Count);
            selectedQuestions.Add(allQuestions[index]);
            allQuestions.RemoveAt(index);
        }

        AssignQuestionsToButtons();
    }

    void AssignQuestionsToButtons()
    {
        buttonQuestionMap.Clear();

        for (int i = 0; i < questionButtons.Count; i++)
        {
            if (i < selectedQuestions.Count)
            {
                Button button = questionButtons[i];
                QuestionEntry question = selectedQuestions[i];

                buttonQuestionMap[button] = question;
                button.GetComponentInChildren<TMP_Text>().text = question.text;
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => AskQuestion(button));
                button.gameObject.SetActive(true);
            }
            else
            {
                questionButtons[i].gameObject.SetActive(false);
            }
        }
    }

    public void StartDialogue(NPC npc)
    {
        if (isAccusing)
        {
            AccuseNPC(npc);
            return;
        }

        currentNPC = npc;
        mapCanvas.SetActive(false);
        dialogueCanvas.SetActive(true);
        responseText.text = $"Разговор с {npc.npcName}";
    }

    void ExitDialogue()
    {
        currentNPC = null;
        dialogueCanvas.SetActive(false);
        mapCanvas.SetActive(true);
    }

    void AskQuestion(Button button)
    {
        if (currentNPC == null || !buttonQuestionMap.ContainsKey(button)) return;

        QuestionEntry question = buttonQuestionMap[button];
        string answer = currentNPC.GetAnswer(question.id);
        responseText.text = $"{currentNPC.npcName}: {answer}";

        button.gameObject.SetActive(false);
        buttonQuestionMap.Remove(button);
    }

    void EnterAccusationMode()
    {
        if (isAccusing){
            isAccusing = false;
            Debug.Log("Режим выбора вора деактивирован!");
        }
        else{
            isAccusing = true;
            Debug.Log("Режим выбора вора активирован! Выберите NPC.");
        }
        
    }

    void AccuseNPC(NPC npc)
    {
        if (npc == null) return;

        isAccusing = false; // Выключаем режим обвинения после выбора

        if (npc == thief)
        {
            Debug.Log("Поздравляем! Вы нашли вора!");
            responseText.text = "Вы нашли вора! Победа!";
        }
        else
        {
            Debug.Log("Ошибка! Это не вор. Вы проиграли.");
            responseText.text = "Ошибка! Это не вор. Вы проиграли.";
        }
        
        EndGame();
    }

    void EndGame()
    {
        accuseButton.gameObject.SetActive(false);
    }
}
