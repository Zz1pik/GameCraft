using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class QuestionEntry
{
    public int id;
    public string text;

    public List<string> truthfulAnswers;
    public List<string> liarAnswers;
}

[CreateAssetMenu(fileName = "QuestionDatabase", menuName = "Game/QuestionDatabase")]
public class QuestionDatabase : ScriptableObject
{
    public List<QuestionEntry> questions;
}
