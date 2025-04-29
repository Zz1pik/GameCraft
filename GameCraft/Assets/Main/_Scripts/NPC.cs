using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public enum NPCTrait { Truthful, Liar, Random }
public enum NPCAge { Young, Old}
public enum NPCHeight { Low, Normal, High}
public enum NPCGender { Woman, Man}

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
            possibleAnswers = trait switch
            {
                NPCTrait.Truthful => question.thiefTruthfulAnswers,
                NPCTrait.Liar => question.thiefLiarAnswers,
                NPCTrait.Random => Random.value > 0.5f ? question.thiefTruthfulAnswers : question.thiefLiarAnswers,
                _ => new List<string>()
            };
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

        // Если вор не назначен или текущий NPC не назначен, выводим ошибку
        if (thiefNPC == null) return "Ошибка: Вор не назначен в Main.";
        if (currentNPC == null) return "Ошибка: Текущий NPC не назначен в Main.";

        List<NPC> allNPCs = Main.Instance.npcs;

        // Проверка на наличие NPC в списке
        if (allNPCs == null || allNPCs.Count == 0) return "Ошибка: Список NPC пуст.";

        // Поиск NPC с нужными условиями
        var randomNPC = allNPCs.Where(n => n != this && n != currentNPC).OrderBy(n => Random.value).FirstOrDefault();
        var truthfulNPC = allNPCs.Where(n => n != this && n.trait == NPCTrait.Truthful && n != currentNPC).OrderBy(n => Random.value).FirstOrDefault();
        var liarNPC = allNPCs.Where(n => n != this && n.trait == NPCTrait.Liar && n != currentNPC).OrderBy(n => Random.value).FirstOrDefault();
        var randomTraitNPC = allNPCs.Where(n => n != this && n.trait == NPCTrait.Random && n != currentNPC).OrderBy(n => Random.value).FirstOrDefault();

        // Заменяем теги на имена NPC или выводим сообщение, если NPC не найден
        answer = answer.Replace("random_npc", GetFormattedName(randomNPC) ?? "Ошибка: Случайный NPC не найден.");
        answer = answer.Replace("thief_npc", GetFormattedName(thiefNPC) ?? "Ошибка: Вор не найден.");
        answer = answer.Replace("random_truthful_npc", GetFormattedName(truthfulNPC) ?? "Ошибка: Справедливый NPC не найден.");
        answer = answer.Replace("random_liar_npc", GetFormattedName(liarNPC) ?? "Ошибка: Лжец NPC не найден.");
        answer = answer.Replace("random_random_npc", GetFormattedName(randomTraitNPC) ?? "Ошибка: NPC с случайным поведением не найден.");

        return answer;
    }

    private string GetFormattedName(NPC npc)
    {
        return npc != null ? $"<b>{npc.npcName}</b>" : "???";
    }
}
