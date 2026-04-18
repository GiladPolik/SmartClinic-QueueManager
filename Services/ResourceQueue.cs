using System.Collections;
using System.Collections.Generic;
using SmartQueueOperator.Interfaces;
using SmartQueueOperator.Models;

namespace SmartQueueOperator.Services
{
    public class ResourceQueue
    {
        private readonly PriorityQueue<Patient, (double Score, long ArrivalTicks, int Age)> m_Queue;
        private readonly IPriorityStrategy m_PriorityStrategy;

        public ResourceType RoomType { get; }

        public ResourceQueue(ResourceType roomType, IPriorityStrategy priorityStrategy)
        {
            RoomType = roomType;
            m_PriorityStrategy = priorityStrategy;
            IComparer<(double Score, long ArrivalTicks, int Age)> advancedComparer = Comparer<(double Score, long ArrivalTicks, int Age)>.Create((a, b) =>
            {
                // stage 1: highest score gets priority (max heap)
                int scoreComparison = b.Score.CompareTo(a.Score);

                if (scoreComparison != 0)
                {
                    return scoreComparison;
                }

                // stage 2: earliest arrival gets priority (min heap based on exact ticks)
                int timeComparison = a.ArrivalTicks.CompareTo(b.ArrivalTicks);

                if (timeComparison != 0)
                {
                    return timeComparison;
                }

                // stage 3: younger patient gets priority (min heap based on Age)
                return a.Age.CompareTo(b.Age);
            });

            m_Queue = new PriorityQueue<Patient, (double Score, long ArrivalTicks, int Age)>(advancedComparer);
        }

        public void Enqueue(Patient patient)
        {
            double priorityKey = m_PriorityStrategy.CalculatePriorityKey(patient);

            m_Queue.Enqueue(patient, (priorityKey, patient.m_ArrivalTime.Ticks, patient.m_Age));
        }

        public Patient Dequeue()
        {
            if (m_IsEmpty)
            {
                throw new InvalidOperationException($"Cannot dequeue. The queue for {RoomType} is currently empty.");
            }

            return m_Queue.Dequeue();
        }

        public Patient Peek()
        {
            if (m_IsEmpty)
            {
                throw new InvalidOperationException($"Cannot peek. The queue for {RoomType} is currently empty.");
            }

            return m_Queue.Peek();
        }

        public bool m_IsEmpty => m_Queue.Count == 0;

        public int m_Count => m_Queue.Count;
    }
}