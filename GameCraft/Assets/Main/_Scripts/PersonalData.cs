using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PersonalData : MonoBehaviour
{
    public GameObject dataContainer;
    
    [Header("Локализация")]
    public string traitText = "Черта";
    public string genderText = "Пол";
    public string ageText = "Возраст";
    public string heightText = "Рост";

    void Start()
    {
        foreach (var npc in Main.Instance.npcs)
        {
            GameObject container = Instantiate(dataContainer, transform);
            container.transform.Find("NPCAvatar").GetComponent<Image>().sprite = npc.npcAvatar;
            
            // Получаем локализованные названия характеристик
            string localizedTrait = GetLocalizedTrait(npc.trait);
            string localizedGender = GetLocalizedGender(npc.gender);
            string localizedAge = GetLocalizedAge(npc.age);
            string localizedHeight = GetLocalizedHeight(npc.height);
            
            // Формируем текст с переведенными характеристиками
            string infoText = $"{traitText}: {localizedTrait}\n" +
                             $"{genderText}: {localizedGender}\n" +
                             $"{ageText}: {localizedAge}\n" +
                             $"{heightText}: {localizedHeight}";
            
            container.transform.Find("Text").GetComponent<TMP_Text>().text = infoText;
        }
    }

    private string GetLocalizedTrait(NPCTrait trait)
    {
        switch(trait)
        {
            case NPCTrait.Truthful: return "Правдивый";
            case NPCTrait.Liar: return "Лжец";
            case NPCTrait.Random: return "Случайный";
            default: return trait.ToString();
        }
    }

    private string GetLocalizedGender(NPCGender gender)
    {
        switch(gender)
        {
            case NPCGender.Woman: return "Женщина";
            case NPCGender.Man: return "Мужчина";
            default: return gender.ToString();
        }
    }

    private string GetLocalizedAge(NPCAge age)
    {
        switch(age)
        {
            case NPCAge.Young: return "Молодой";
            case NPCAge.Old: return "Старый";
            default: return age.ToString();
        }
    }

    private string GetLocalizedHeight(NPCHeight height)
    {
        switch(height)
        {
            case NPCHeight.Low: return "Низкий";
            case NPCHeight.Normal: return "Средний";
            case NPCHeight.High: return "Высокий";
            default: return height.ToString();
        }
    }
}