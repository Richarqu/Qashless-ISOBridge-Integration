using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SBPGenericISOBridge.Validation
{
    public class ValidationReq
    {
        public string terminalId { get; set; }
        public string pin { get; set; }
        public string transactionData { get; set; }
        public DateTime dateRequested { get; set; }
    }
}
