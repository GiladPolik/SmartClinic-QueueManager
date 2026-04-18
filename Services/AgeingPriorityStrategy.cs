using SmartQueueOperator.Interfaces;
using SmartQueueOperator.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartQueueOperator.Services
{
    public class AgeingPriorityStrategy : IPriorityStrategy
    {
        private readonly double m_UrgencyPointsPerMinute;

        public AgeingPriorityStrategy(double urgencyPointsPerMinute = 0.1)
        {
            m_UrgencyPointsPerMinute = urgencyPointsPerMinute;
        }

        public double CalculatePriorityKey(Patient patient)
        {
            /*
            Formula: MU - W * AT
            MU = Medical Urgency (1-10)
            W = Weight (Points gained per minute)
            AT = Arrival time in minutes


            Formula is MU - W *(CT-AT)
            means, amount of points a patient is gonna get, is his medical urgency level minus ( weight (points for each minute being waited) )
            times (*) the time he has been waiting in minutes.
            when we recalculate it, it equals to: MU - W * CT - W * AT
            note that W*CT is the same calculation for each patient, so we can skip that and save a lot of time (we wont do any key updates during the wait of a patient)
             */
            double arrivalTimeInMinutes = ((DateTimeOffset)patient.m_ArrivalTime).ToUnixTimeSeconds() / 60.0;
            double staticKey = patient.m_MedicalUrgency - (m_UrgencyPointsPerMinute * arrivalTimeInMinutes);
            return staticKey;
        }

    }
}
