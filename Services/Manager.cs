using SmartQueueOperator.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartQueueOperator.Interfaces;
using SmartQueueOperator.Models;
using System.Runtime.CompilerServices;

namespace SmartQueueOperator.Services
{
    public class Manager : IOptimizedClinicManager
    {
        private readonly Dictionary<ResourceType, ResourceQueue> m_Queues;
        private readonly object m_Lock = new object();

        public Manager(IPriorityStrategy priorityStrategy)
        {
            m_Queues = new Dictionary<ResourceType, ResourceQueue>
            {
                { ResourceType.Doctor, new ResourceQueue(ResourceType.Doctor, priorityStrategy) },
                { ResourceType.Nurse, new ResourceQueue(ResourceType.Nurse, priorityStrategy) },
                { ResourceType.XRayRoom, new ResourceQueue(ResourceType.XRayRoom, priorityStrategy) }
            };
        }

        public void AddPatient(Patient patient)
        {
            (bool isValid, string message) validatePatientResult = ValidatePatientDetails(patient);

            if(!validatePatientResult.isValid)
            {
                throw new ArgumentException($"Invalid patient data: {validatePatientResult.message}");
            }

            lock (m_Lock)
            {
                if (m_Queues.ContainsKey(patient.m_ResourceType))
                {
                    m_Queues[patient.m_ResourceType].Enqueue(patient);
                }
                else
                {
                    throw new ArgumentException($"No queue registered for resource type: {patient.m_ResourceType}");
                }
            }
        }

        public Patient GetNextPatient(ResourceType resource)
        {
            lock (m_Lock)
            {
                return GetFromQueue(resource);
            }
        }

        public Patient TryOptimizeDoctorQueue()
        {
            lock (m_Lock)
            {
                // Check if doctor queue has patients and if the top one is downgradable
                if (!m_Queues[ResourceType.Doctor].m_IsEmpty)
                {
                    Patient topPatient = m_Queues[ResourceType.Doctor].Peek();

                    if (topPatient != null && topPatient.m_IsDowngradable)
                    {
                        // We "intercept" the patient from the doctor's queue 
                        // to be handled by an available nurse
                        return m_Queues[ResourceType.Doctor].Dequeue();
                    }
                }

                return null; // No suitable patient found for optimization
            }
        }
        private Patient GetFromQueue(ResourceType resource)
        {
            if (!m_Queues.ContainsKey(resource))
            {
                throw new ArgumentException("Invalid resource type.");
            }

            ResourceQueue targetQueue = m_Queues[resource];

            if (targetQueue.m_IsEmpty)
            {
                throw new InvalidOperationException($"The queue for {resource} is currently empty.");
            }

            return targetQueue.Dequeue();
        }

        private (bool IsValid, string ErrorMessage) ValidatePatientDetails(Patient patient)
        {
            if (patient == null)
                return (false, "Patient object is null.");

            if (patient.m_Age < 0)
                return (false, "Age cannot be negative.");

            if (patient.m_MedicalUrgency < 1 || patient.m_MedicalUrgency > 10)
                return (false, "Medical urgency must be between 1 and 10.");

            if (patient.m_ArrivalTime > DateTime.Now)
                return (false, "Arrival time cannot be in the future.");

            return (true, string.Empty);
        }
    }
}
