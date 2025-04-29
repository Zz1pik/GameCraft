using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public enum NPCTrait { Truthful, Liar, Random }
public enum NPCAge { Young, Old }
public enum NPCHeight { Low, Normal, High }
public enum NPCGender { Woman, Man }

public class NPC : MonoBehaviour
{
    public string npcName;
    public NPCTrait trait;
    public NPCAge age;
    public NPCHeight height;
    public NPCGender gender;
    public Sprite npcSprite;
    public Sprite npcAvatar;
    public bool isThief = false;
    public QuestionDatabase questionDatabase;

    public string GetAnswer(int questionID)
    {
        if (questionDatabase == null) return "Ошибка: У NPC нет QuestionDatabase.";

        var question = questionDatabase.questions.Find(q => q.id == questionID);
        if (question == null) return "Неизвестный вопрос.";

        List<string> possibleAnswers;

        bool isActuallyThief = (Main.Instance.thief != null) && (Main.Instance.thief == this);

        if (isActuallyThief)
        {
            possibleAnswers = question.liarAnswers;
        }
        else
        {
            possibleAnswers = trait switch
            {
                NPCTrait.Truthful => question.truthfulAnswers,
                NPCTrait.Liar => question.liarAnswers,
                NPCTrait.Random => Random.value > 0.5f ? question.truthfulAnswers : question.liarAnswers,
                _ => new List<string>()
            };
        }

        string selectedAnswer = GetRandomAnswer(possibleAnswers);
        return ReplaceTags(selectedAnswer);
    }

    private string GetRandomAnswer(List<string> answers)
    {
        if (answers == null || answers.Count == 0)
            return "Нет ответа.";

        return answers[Random.Range(0, answers.Count)];
    }

    private string ReplaceTags(string answer)
    {
        if (string.IsNullOrEmpty(answer)) return answer;

        NPC thiefNPC = Main.Instance.thief;
        NPC currentNPC = Main.Instance.currentNPC;

        if (thiefNPC == null) return "Ошибка: Вор не назначен в Main.";
        if (currentNPC == null) return "Ошибка: Текущий NPC не назначен в Main.";

        List<NPC> allNPCs = Main.Instance.npcs;
        if (allNPCs == null || allNPCs.Count == 0) return "Ошибка: Список NPC пуст.";

        var randomNPC = allNPCs.Where(n => n != this && n != currentNPC).OrderBy(n => Random.value).FirstOrDefault();

        answer = answer.Replace("thief_age", thiefNPC.age.ToString());
        answer = answer.Replace("random_age", randomNPC != null ? randomNPC.age.ToString() : "???");
        answer = answer.Replace("thief_trait", thiefNPC.trait.ToString());
        answer = answer.Replace("random_trait", randomNPC != null ? randomNPC.trait.ToString() : "???");
        answer = answer.Replace("thief_height", thiefNPC.height.ToString());
        answer = answer.Replace("random_height", randomNPC != null ? randomNPC.height.ToString() : "???");
        answer = answer.Replace("thief_gender", thiefNPC.gender.ToString());
        answer = answer.Replace("random_gender", randomNPC != null ? randomNPC.gender.ToString() : "???");

        return answer;
    }
}