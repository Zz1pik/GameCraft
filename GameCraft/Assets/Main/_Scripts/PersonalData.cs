using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PersonalData : MonoBehaviour
{
    public List<NPC> npcs;
    public GameObject dataContainer;

    void Start()
    {
        foreach (var npc in npcs)
        {
            GameObject container = Instantiate(dataContainer, transform);
            container.transform.Find("NPCAvatar").GetComponent<Image>().sprite = npc.npcAvatar;
            container.transform.Find("Text").GetComponent<TMP_Text>().text = $"Черта: {npc.trait} \nПол: {npc.gender} \nВозраст: {npc.age} \nРост: {npc.height}";
        }
    }
}
