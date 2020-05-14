using com.sun.istack.@internal.logging;
using java.lang;
using log4net;
using Newtonsoft.Json;
using org.jpos.iso;
using SBPGenericISOBridge.Qashless;
using SBPGenericISOBridge.Validation;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SBPGenericISOBridge.Processing
{
    public class ValidationProcess
    {
        private static readonly ILog logger =
            LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public ValidationResp GetValidation(ISOMsg m)
        {
            //do validation
            ProcessISO _processISO = new ProcessISO();
            ValidationResp validationResp = new ValidationResp();
            try
            {
                var iSOResult = _processISO.GetISODetails(m);
                //iSOResult.terminalId = "5733148";
                //iSOResult.debitAcct = "362994";

                string endPoint = ConfigurationManager.AppSettings["validateUrl"];
                var payload = new ValidationReq
                {
                    terminalId = iSOResult.terminalId,
                    pin = iSOResult.debitAcct,
                    transactionData = iSOResult.terminalLocation,
                    dateRequested = DateTime.Now
                };
                logger.Error($"Validation payload sent is: {payload}");
                //call the validation api
                QashlessApis _qashlessApis = new QashlessApis();
                var jsonResponse = _qashlessApis.QashlessPost(payload, endPoint);
                validationResp = string.IsNullOrEmpty(jsonResponse) ? null : JsonConvert.DeserializeObject<ValidationResp>(jsonResponse);
                logger.Error($"Validation response returned is: {validationResp}");
            }
            catch (System.Exception ex)
            {
                logger.Error($"Exception at method GetValidation: {ex}");
                validationResp = null;
            }
            return validationResp;
        }
    }
}
