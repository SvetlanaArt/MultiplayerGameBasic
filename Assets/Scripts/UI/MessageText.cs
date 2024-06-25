using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct Message{
    public GameMessageType type;
    public String text;
}

[CreateAssetMenu(fileName = "MessageText", menuName = "Scriptable Objects/MessageText")]

public class MessageText : ScriptableObject
{
    [SerializeField] List<Message> messages = new List<Message>();
    
    public string GetMessage(GameMessageType type)
    {
        foreach(Message message in messages)
        {
            if (type == message.type)
            {
                return message.text;
            }
        }
        return "";
    }
}
