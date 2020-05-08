using org.jpos.iso;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SBPGenericISOBridge
{
    public class ISODetails
    {
        public string procCode { get; set; }
        public string debitAcct { get; set; }
        public string creditAcct { get; set; }
        public string amt { get; set; }
        public string isoCharge { get; set; }
        public string terminalId { get; set; }
        public string terminalLocation { get; set; }
        public string rrn { get; set; }
        public string stan { get; set; }
        public string debitCurrency { get; set; }
    }
    public class ProcessISO
    {
        public ISODetails GetISODetails (ISOMsg m)
        {
            ISODetails _iSODetails = new ISODetails();
            _iSODetails = new ISODetails
            {
                procCode = m.getString(3),
                debitAcct = m.getString(102),
                creditAcct = m.getString(103),
                amt = m.getString(4),
                isoCharge = m.getString(28),
                terminalId = m.getString(41),
                rrn = m.getString(37),
                stan = m.getString(11),
                terminalLocation = m.getString(43),
                debitCurrency = m.getString(49)
            };
            return _iSODetails;
        }
    }
}
