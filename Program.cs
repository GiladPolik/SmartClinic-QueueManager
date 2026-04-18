using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading;
using SmartQueueOperator.Models;
using SmartQueueOperator.Services;
using SmartQueueOperator.Interfaces;

namespace SmartQueueOperator
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== Smart Clinic Real-Time Event Simulation ===\n");

            IOptimizedClinicManager clinicManager = new Manager(new AgeingPriorityStrategy(0.1));
            string jsonPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "patients.json");

            if (!File.Exists(jsonPath))
            {
                Console.WriteLine($"[Error] JSON file not found at: {jsonPath}");
                return;
            }

            try
            {
                string jsonString = File.ReadAllText(jsonPath);
                JsonSerializerOptions options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                List<Patient> patientList = JsonSerializer.Deserialize<List<Patient>>(jsonString, options);

                Console.WriteLine($"[System] Loaded {patientList.Count} patients. Registering...\n");

                foreach (Patient p in patientList)
                {
                    try
                    {
                        clinicManager.AddPatient(p);
                    }
                    catch (ArgumentException ex)
                    {
                        Console.WriteLine($"[Validation] Patient {p.m_Id} Rejected: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Fatal] Error processing data: {ex.Message}");
                return;
            }

            Console.WriteLine("\n=== Starting Real-Time Triage Processing ===\n");

            // the active rooms
            List<ResourceType> activeRooms = new List<ResourceType>
            {
                ResourceType.Doctor,
                ResourceType.Nurse,
                ResourceType.XRayRoom
            };

            Random eventGenerator = new Random();

            // run until all queues are completely empty
            while (activeRooms.Count > 0)
            {
                // randomly pick which room just finished a treatment and wants a new patient
                int roomIndex = eventGenerator.Next(activeRooms.Count);
                ResourceType requestingRoom = activeRooms[roomIndex];

                try
                {
                    Patient nextPatient = null;

                    // REAL-WORLD WORKFLOW: Option A (Optimization)
                    // if a Nurse is free, she checks the doctor's queue FIRST.
                    if (requestingRoom == ResourceType.Nurse)
                    {
                        nextPatient = clinicManager.TryOptimizeDoctorQueue();

                        if (nextPatient != null)
                        {
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine($"[!] OPTIMIZATION: Nurse intercepted Patient {nextPatient.m_Id} from Doctor queue! (Age: {nextPatient.m_Age}, Urgency: {nextPatient.m_MedicalUrgency})");
                            Console.ResetColor();
                        }
                    }

                    // if Option A didn't apply (or if it's not a nurse), get patient normally
                    if (nextPatient == null)
                    {
                        nextPatient = clinicManager.GetNextPatient(requestingRoom);
                        Console.WriteLine($"[{requestingRoom}] Treating ID: {nextPatient.m_Id,-4} | Urgency: {nextPatient.m_MedicalUrgency,-2} | Arrival: {nextPatient.m_ArrivalTime:HH:mm}");
                    }

                    // sleep for a short time to simulate real-time processing in the console
                    Thread.Sleep(150);
                }
                catch (InvalidOperationException)
                {
                    // the specific queue is empty. remove this room type from the active polling.
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.WriteLine($"--- Queue for {requestingRoom} is now fully empty. ---");
                    Console.ResetColor();

                    activeRooms.RemoveAt(roomIndex);
                }
            }

            Console.WriteLine("\n=== Clinic is Empty. Simulation Finished Successfully. ===");
            Console.ReadLine();
        }
    }
}