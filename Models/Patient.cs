using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartQueueOperator.Models
{
    public class Patient
    {
        public DateTime m_ArrivalTime { get; set; }
        public int m_Id { get; set; }
        public int m_MedicalUrgency { get; set; }
        public ResourceType m_ResourceType { get; set; }
        public bool m_IsDowngradable { get; set; }
        public int m_Age { get; set; }
    }
}
