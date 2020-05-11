using log4net;
using Newtonsoft.Json;
using org.jpos.iso;
using SBPGenericISOBridge.Qashless;
using sun.net.www.protocol.ftp;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;

namespace SBPGenericISOBridge.Processing
{
    public class TransactionProcess
    {
        private static readonly ILog logger =
            LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public FundsTransResp DoTransfer(ISOMsg m, string account)
        {
            ResponseCode respCode = new ResponseCode();
            FundsTransResp fundsTransResp = new FundsTransResp();
            string commCode = ConfigurationManager.AppSettings["commissionCode"];
            string creditAcct = ConfigurationManager.AppSettings["creditAcct"];
            string creditCur = ConfigurationManager.AppSettings["creditCur"];
            string vTellerAppID = ConfigurationManager.AppSettings["vTellerAppID"];
            string tranBranch = ConfigurationManager.AppSettings["transactionBranch"];
            string tranType = ConfigurationManager.AppSettings["transactionType"];

            ProcessISO _processISO = new ProcessISO();
            var iSOResult = _processISO.GetISODetails(m);
            //build the transfer payload
            var load = new FT_Request() { CommissionCode = commCode, CREDITACCTNO = creditAcct, CREDITCURRENCY = creditCur, DEBITACCTNO = account, DEBITAMOUNT = iSOResult.amt, DEBITCURRENCY = respCode.Currency(iSOResult.debitCurrency), VtellerAppID = vTellerAppID, narrations = iSOResult.terminalLocation, TransactionBranch = tranBranch, TransactionType = tranType };

            var payload = new FTRequest
            {
                FT_Request = load
            };
            //call the getaccountfullinfo endpoint
            QashlessApis qashlessApis = new QashlessApis();
            string isoBalance = string.Empty;
            try
            {
                string fTUrl = ConfigurationManager.AppSettings["ftUrl"];
                /////
                string fTResponse = qashlessApis.DoTransferPost(payload, fTUrl);

                logger.Error($"Raw FT Response from DoTransferPost method for account: {account} is: {fTResponse}");
                fundsTransResp = string.IsNullOrEmpty(fTResponse) ? null : JsonConvert.DeserializeObject<FundsTransResp>(fTResponse);
                logger.Error($"Deserialized FT Response from DoTransferPost method for account: {account} is: {fundsTransResp}");
            }
            catch (Exception ex)
            {
                logger.Error($"Exception at method GetAccountBal: {ex}");
                isoBalance = null;
            }
            return fundsTransResp;
        }
        public string GetAccountBal(string account)
        {
            //call the getaccountfullinfo endpoint
            QashlessApis qashlessApis = new QashlessApis(); 
            string isoBalance = string.Empty;
            try
            {
                string balUrl = ConfigurationManager.AppSettings["balanceUrl"];
                string balance = qashlessApis.GetBalance(balUrl, account);
                logger.Error($"Raw balance from GetBalance method for account: {account} is: {balance}");
                var accountResp = string.IsNullOrEmpty(balance) ? null : JsonConvert.DeserializeObject<AccountFullInfo>(balance);
                logger.Error($"Deserialized balance from GetBalance method for account: {account} is: {accountResp}");
                //form the isobalance
                string useableBal = (Convert.ToDouble(accountResp.BankAccountFullInfo.UsableBal) * 100).ToString();
                string cleBal = (Convert.ToDouble(accountResp.BankAccountFullInfo.CLE_BAL) * 100).ToString();
                isoBalance = accountResp == null ? isoBalance : GetISOBalanceFormat(accountResp.BankAccountFullInfo.T24_CUR_CODE, useableBal, cleBal);

                logger.Error($"isoBalance returned for account: {account} is: {isoBalance}");
            }
            catch(Exception ex)
            {
                logger.Error($"Exception at method GetAccountBal: {ex}");
                isoBalance = null;
            }
            return isoBalance;
        }
        private string GetISOBalanceFormat(string currency, string availBalance, string ledgerBalance)
        {
            var isobal = string.Empty;
            try
            {
                var bal = new StringBuilder("1002" + currency);
                if (availBalance.Substring(0, 1) == "-")
                {
                    bal.Append("D" + availBalance.PadRight(12, '0'));
                }
                else
                {
                    bal.Append("C" + availBalance.PadRight(12, '0'));
                }

                bal.Append("1001" + currency);

                if (ledgerBalance.Substring(0, 1) == "-")
                {
                    bal.Append("D" + ledgerBalance.PadRight(12, '0'));
                }
                else
                {
                    bal.Append("C" + ledgerBalance.PadRight(12, '0'));
                }
                isobal = bal.ToString();
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
            return isobal;
        }
    }

    public class AccountFullInfo
    {
        public Bankaccountfullinfo BankAccountFullInfo { get; set; }
    }

    public class Bankaccountfullinfo
    {
        public string NUBAN { get; set; }
        public string BRA_CODE { get; set; }
        public string DES_ENG { get; set; }
        public string CUS_NUM { get; set; }
        public string CUR_CODE { get; set; }
        public string LED_CODE { get; set; }
        public string CUS_SHO_NAME { get; set; }
        public string AccountGroup { get; set; }
        public string CustomerStatus { get; set; }
        public string ADD_LINE1 { get; set; }
        public string ADD_LINE2 { get; set; }
        public string MOB_NUM { get; set; }
        public string email { get; set; }
        public string ACCT_NO { get; set; }
        public string MAP_ACC_NO { get; set; }
        public string ACCT_TYPE { get; set; }
        public string ISO_ACCT_TYPE { get; set; }
        public string TEL_NUM { get; set; }
        public string DATE_OPEN { get; set; }
        public string STA_CODE { get; set; }
        public string CLE_BAL { get; set; }
        public string CRNT_BAL { get; set; }
        public string TOT_BLO_FUND { get; set; }
        public object INTRODUCER { get; set; }
        public string DATE_BAL_CHA { get; set; }
        public string NAME_LINE1 { get; set; }
        public string NAME_LINE2 { get; set; }
        public string BVN { get; set; }
        public string REST_FLAG { get; set; }
        public RESTRICTION[] RESTRICTION { get; set; }
        public string IsSMSSubscriber { get; set; }
        public string Alt_Currency { get; set; }
        public string Currency_Code { get; set; }
        public string T24_BRA_CODE { get; set; }
        public string T24_CUS_NUM { get; set; }
        public string T24_CUR_CODE { get; set; }
        public string T24_LED_CODE { get; set; }
        public string OnlineActualBalance { get; set; }
        public string OnlineClearedBalance { get; set; }
        public string OpenActualBalance { get; set; }
        public string OpenClearedBalance { get; set; }
        public string WorkingBalance { get; set; }
        public string CustomerStatusCode { get; set; }
        public string CustomerStatusDeecp { get; set; }
        public object LimitID { get; set; }
        public string LimitAmt { get; set; }
        public string MinimumBal { get; set; }
        public string UsableBal { get; set; }
        public string AccountDescp { get; set; }
        public string CourtesyTitle { get; set; }
        public string AccountTitle { get; set; }
    }

    public class RESTRICTION
    {
        public object RestrictionCode { get; set; }
        public object RestrictionDescription { get; set; }
    }

}
