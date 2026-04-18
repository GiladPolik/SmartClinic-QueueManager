using SmartQueueOperator.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartQueueOperator.Interfaces
{
    public interface IClinicManager
    {
        void AddPatient(Patient patient);

        Patient GetNextPatient(ResourceType resource);
    }
}
