using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SBPGenericISOBridge.Processing
{
    class TransactionDet
    {
    }
    public class InquiryResp
    {
        public string isoBalance { get; set; }
        public string accountNo { get; set; }
    }

    public class FTRequest
    {
        public FT_Request FT_Request { get; set; }
    }

    public class FT_Request
    {
        public string TransactionBranch { get; set; }
        public string TransactionType { get; set; }
        public string DEBITACCTNO { get; set; }
        public string DEBITCURRENCY { get; set; }
        public string CREDITCURRENCY { get; set; }
        public string DEBITAMOUNT { get; set; }
        public string CREDITACCTNO { get; set; }
        public string CommissionCode { get; set; }
        public string VtellerAppID { get; set; }
        public string narrations { get; set; }
    }

    public class FundsTransResp
    {
        public Ftresponse FTResponse { get; set; }
    }

    public class Ftresponse
    {
        public string ReferenceID { get; set; }
        public string ResponseCode { get; set; }
        public string ResponseText { get; set; }
        public string Balance { get; set; }
        public string COMMAMT { get; set; }
        public string CHARGEAMT { get; set; }
        public string FTID { get; set; }
    }
    public class ResponseCode
    {
        public string Response(string code)
        {
            string rsp = string.Empty;
            switch (code)
            {
                case "00":
                    rsp = "00";
                    break;
                case "x1005":
                    rsp = "51";
                    break;
                case "RS_400":
                    rsp = "RS_400";
                    break;
                case "RS_401":
                    rsp = "RS_401";
                    break;
                case "OK":
                    rsp = "00";
                    break;
                case "CB02":
                    rsp = "01";
                    break;
            }
            return rsp;
        }
        public string Currency(string code)
        {
            string cur = string.Empty;
            switch (code)
            {
                case "566":
                    cur = "NGN";
                    break;
                case "840":
                    cur = "USD";
                    break;
                case "826":
                    cur = "GBP";
                    break;
                default:
                    cur = "NGN";
                    break;
            }
            return cur;
        }
    }
}
