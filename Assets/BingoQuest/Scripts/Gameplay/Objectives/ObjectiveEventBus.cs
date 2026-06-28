using System;
using System.Collections.Generic;
using UnityEngine;

namespace BingoQuest.Gameplay.Objectives
{
    /// <summary>
    /// Central event dispatcher for objective-relevant combat events.
    /// Combat systems emit events here; objectives subscribe via handlers.
    /// </summary>
    public class ObjectiveEventBus : MonoBehaviour
    {
        private List<Action<ObjectiveEvent>> subscribers = new();
        private Queue<ObjectiveEvent> eventQueue = new();
        private bool isProcessing;

        public static ObjectiveEventBus Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void Subscribe(Action<ObjectiveEvent> handler)
        {
            if (!subscribers.Contains(handler))
                subscribers.Add(handler);
        }

        public void Unsubscribe(Action<ObjectiveEvent> handler)
        {
            subscribers.Remove(handler);
        }

        public void Emit(ObjectiveEvent evt)
        {
            eventQueue.Enqueue(evt);
            
            if (!isProcessing)
            {
                isProcessing = true;
                ProcessQueue();
                isProcessing = false;
            }
        }

        public void EmitBatch(params ObjectiveEvent[] events)
        {
            foreach (var evt in events)
                eventQueue.Enqueue(evt);

            if (!isProcessing)
            {
                isProcessing = true;
                ProcessQueue();
                isProcessing = false;
            }
        }

        private void ProcessQueue()
        {
            while (eventQueue.Count > 0)
            {
                var evt = eventQueue.Dequeue();
                foreach (var subscriber in subscribers)
                {
                    try
                    {
                        subscriber.Invoke(evt);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"Error processing objective event {evt.Type}: {ex.Message}");
                    }
                }
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }
    }
}
