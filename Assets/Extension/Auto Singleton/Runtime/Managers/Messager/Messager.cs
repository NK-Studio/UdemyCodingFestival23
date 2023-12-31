using System;
using System.Collections.Generic;
using UnityEngine;

namespace NKStudio
{
    public static class Messager
    {
        public delegate void Message();

        private static readonly Dictionary<string, Message> RegisteredMessages;

        static Messager()
        {
            RegisteredMessages = new Dictionary<string, Message>();
        }

        public static void RegisterMessage(string messageName, Message message)
        {
            if (!RegisteredMessages.ContainsKey(messageName))
                RegisteredMessages.Add(messageName, message);
            else
            {
                Debug.LogWarning($"Messager : {messageName} 항목에 이미 메시지에 대한 참조가 포함되어 있습니다.");
            }
        }

        public static void RemoveMessage(string messageName)
        {
            if (RegisteredMessages.ContainsKey(messageName))
                RegisteredMessages.Remove(messageName);
        }

        public static void RemoveAllMessages()
        {
            RegisteredMessages.Clear();
        }

        public static void Send(string eventName)
        {
            if (RegisteredMessages.TryGetValue(eventName, out Message message))
            {
                try
                {
                    //반복할 등록된 메시지의 복사본을 가져옵니다. 이것은 반복하는 동안 메시지 수신기를 등록 취소하는 동안 문제를 방지합니다.
                    message?.Invoke();
                }
                catch (Exception e)
                {
                    Debug.LogError($"Messager: {eventName} 메시지를 보내는 동안 {e.GetType().Name}이(가) 포착되었습니다.");
                    Debug.LogException(e);
                }
            }
            else
            {
                Debug.LogWarning($"Messager : {eventName} 이벤트가 등록되지 않았습니다.");
            }
        }
    }
}