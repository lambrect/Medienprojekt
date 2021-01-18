using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "New StoryNode", menuName = "Nodes/StoryNode")]
public class StoryNode : ScriptableObject
{
    // Liste von Videos die im Knoten abgespielt werden sollen
    public List<VideoClip> mainvideos;
    //Liste von Knoten die am aktuellen Knoten hängen
    public List<StoryNode> nextNodes;
    //Antwort des aktuellen Knoten
    public string Decision;
    
    //Speichert die Antworten in einem Dictionary und gibt dieses zurück
    public Dictionary<string, int> GetDecisions()
    {
        Dictionary<string, int> result = new Dictionary<string, int>();

        for (int i = 0; i < nextNodes.Count; i++)
        {
             result.Add(nextNodes[i].Decision, i);
        }
        return result;
    }

    // Gibt den spezifischen Knoten mit dem Index "index" aus der nextNodes Liste 
    public StoryNode NextNode(int index)
    {
        return nextNodes[index];
    }
}

