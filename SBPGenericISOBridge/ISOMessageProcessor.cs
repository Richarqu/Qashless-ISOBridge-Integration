using System;
using org.jpos.iso;
using ThreadPool = org.jpos.util.ThreadPool;
using System.Net.Sockets;
using log4net;
using System.Configuration;
using SBPGenericISOBridge.Validation;
using SBPGenericISOBridge.Qashless;
using Newtonsoft.Json;
using SBPGenericISOBridge.Processing;

namespace SBPGenericISOBridge
{
    class ISOMessageProcessor : ISORequestListener
    {
        //private static TcpListener _myListener;
        //private int _port;
        //private ServerChannel _channel;
        ISOMisc u = new ISOMisc();
        //private ThreadPool _pool;
        private static readonly ILog logger =
               LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public ISOMessageProcessor() : base()
        {

        }

        public bool process(ISOSource isos, ISOMsg m)
        {
            ResponseCode respCode = new ResponseCode();
            ProcessISO _processISO = new ProcessISO();
            try
            {
                string isotext = ISOUtil.dumpString(m.pack());
                Console.WriteLine("Aboout to process message: ");
                Console.WriteLine(isotext);
                var brk = u.BreakMsg(m);
                Console.WriteLine(brk);

                var rsp = @ConfigurationManager.AppSettings["DefaultRspCode"];

                
                var availBal = string.Empty;
                var ledBal = string.Empty;
                var trxnType = string.Empty;
                var charge = string.Empty;
                var trans_ref = string.Empty; 
                var isoBal = string.Empty;
                //get mti
                string mti = m.getMTI();

                switch (mti)
                {
                    case "0800":
                        rsp = "00";
                        break;
                    case "0200":
                        var iSOResult = _processISO.GetISODetails(m);
                        ValidationProcess valProcess = new ValidationProcess();
                        TransactionProcess tranProcess = new TransactionProcess();

                        trxnType = iSOResult.procCode.Substring(0, 2);
                        trans_ref = trxnType + iSOResult.stan + iSOResult.rrn + iSOResult.terminalId; ;
                        if (iSOResult.isoCharge != "")
                        {
                            if (iSOResult.isoCharge.Substring(0, 1) == "D")
                            {
                                //Add Charges to Total Transaction Amount
                                charge = iSOResult.isoCharge.Substring(1,iSOResult.isoCharge.Length - 1);
                            }
                            else
                            {
                                //Deduct subcharge from Total Transaction Amount
                                charge = "-"+iSOResult.isoCharge.Substring(1, iSOResult.isoCharge.Length - 1);
                            }
                        }
                        var acc_Currency = string.Empty;
                        var appRsp = string.Empty;
                        //var iSOResult = _processISO.GetISODetails(m);
                        //Process the various transaction types
                        switch (trxnType)
                        {
                            case "00":
                                //pass the debit Account,transaction amount, transaction currency,uniqueRef,charge_Amt to the Purchase Method

                                //Method should return the response,currency code, available balance and ledger balance
                                acc_Currency = string.Empty;
                                appRsp = string.Empty;
                                rsp = u.ISO_Response(appRsp);
                                isoBal = u.GetISOBalanceFormat(acc_Currency, availBal, ledBal);
                                break;
                            case "01":
                                //pass the debit Account,transaction amount, transaction currency,uniqueRef,charge_Amt to the Cash_Wihdrawal Method
                                var valResp = valProcess.GetValidation(m);
                                if (valResp.responseCode == "05")
                                {
                                    rsp = "05";
                                }
                                else if (valResp.responseCode == "00")
                                {
                                    var wdrResult = tranProcess.DoTransfer(m, valResp.data.ToString());
                                    if (wdrResult != null)
                                    {
                                        rsp = respCode.Response(wdrResult.FTResponse.ResponseCode);
                                        if (rsp == "00")
                                        {
                                            //get available bal and ledger bal and compute the isobal.
                                            availBal = (Convert.ToDouble(wdrResult.FTResponse.Balance) * 100).ToString();
                                            ledBal = (Convert.ToDouble(wdrResult.FTResponse.Balance) * 100).ToString();
                                            isoBal = u.GetISOBalanceFormat(respCode.Currency(iSOResult.debitCurrency), availBal, ledBal);
                                            m.set("44", wdrResult.FTResponse.FTID);
                                        }
                                    }
                                    else { rsp = "96"; logger.Error($"Withdrawal failed, wdrResult returns null"); }

                                    //call the isobalance method
                                    string isoBalResp = tranProcess.GetAccountBal(valResp.data.ToString());
                                    //check iso balance is not null or empty
                                    if (!string.IsNullOrEmpty(isoBalResp))
                                    {
                                        isoBal = isoBalResp;
                                        m.set("102", valResp.data.ToString());
                                        rsp = "00";
                                    }
                                    else { rsp = "01"; }

                                }
                                else { rsp = "96"; }
                                //Method should return the response,currency code, available balance and ledger balance
                                acc_Currency = string.Empty;
                                appRsp = string.Empty;
                                rsp = u.ISO_Response(appRsp);
                                isoBal = u.GetISOBalanceFormat(acc_Currency, availBal, ledBal);
                                break;
                            case "21":
                                break;
                            case "31":
                                var valResponse = valProcess.GetValidation(m);
                                if (valResponse.responseCode == "05")
                                {
                                    rsp = "05";
                                }
                                else if (valResponse.responseCode == "00")
                                {
                                    //call the isobalance method
                                    string isoBalResp = tranProcess.GetAccountBal(valResponse.data.ToString());
                                    //check iso balance is not null or empty
                                    if (!string.IsNullOrEmpty(isoBalResp))
                                    {
                                        isoBal = isoBalResp;
                                        m.set("102", valResponse.data.ToString());
                                        rsp = "00";
                                    }
                                    else { rsp = "01"; }
                                    
                                }
                                else { rsp = "96"; }
                                break;
                            case "38":
                                break;
                            case "37":
                                break;
                            case "40":
                                //pass the debit Account,credit account,transaction amount, transaction currency,uniqueRef,charge_Amt to the Cash_Wihdrawal Method

                                //Method should return the response,currency code, available balance and ledger balance
                                acc_Currency = string.Empty;
                                appRsp = string.Empty;
                                rsp = u.ISO_Response(appRsp);
                                isoBal = u.GetISOBalanceFormat(acc_Currency, availBal, ledBal);
                                break;
                            case "50":
                                //pass the debit Account,credit account,transaction amount, transaction currency,uniqueRef,charge_Amt to the Cash_Wihdrawal Method

                                //Method should return the response,currency code, available balance and ledger balance
                                acc_Currency = string.Empty;
                                appRsp = string.Empty;
                                rsp = u.ISO_Response(appRsp);
                                isoBal = u.GetISOBalanceFormat(acc_Currency, availBal, ledBal);
                                break;
                        }
                        break;
                    case "0420":
                        var iSOResult2 = _processISO.GetISODetails(m);
                        trxnType = iSOResult2.procCode.Substring(0, 2);
                        var reversal_Field = m.getString(90);
                        var rev_Stan = reversal_Field.Substring(0, 0);
                        var rev_RRN = reversal_Field.Substring(0, 0);
                        trans_ref = trxnType + rev_Stan + rev_RRN + iSOResult2.terminalId; 
                        if (iSOResult2.isoCharge != "")
                        {
                            if (iSOResult2.isoCharge.Substring(0, 1) == "D")
                            {
                                //Add Charges to Total Transaction Amount
                                charge = iSOResult2.isoCharge.Substring(1, iSOResult2.isoCharge.Length - 1);
                            }
                            else
                            {
                                //Deduct subcharge from Total Transaction Amount
                                charge = "-" + iSOResult2.isoCharge.Substring(1, iSOResult2.isoCharge.Length - 1);
                            }
                        }
                        //Process each transaction type
                        switch (trxnType)
                        {
                            case "00":
                                break;
                            case "01":
                                break;
                            case "21":
                                break;
                            case "40":
                                break;
                            case "50":
                                break;
                        }
                        break;
                    case "0100":
                        var iSOResult3 = _processISO.GetISODetails(m);
                        trxnType = iSOResult3.procCode.Substring(0, 2);
                        trans_ref = trxnType + iSOResult3.stan + iSOResult3.rrn + iSOResult3.terminalId;
                        rsp = "00";
                        //m.set(59, "0535473595");
                        //m.set(123, "511201513344002");
                        //m.set("127.3", "SWTSBPsrc   ESBNAMENQsnk008664008664SBPMCDebit  ");
                        m.set("127.6", "11");
                        m.set("127.9", "NXXXXXXXXXXXXXXXXX");
                        break;
                    case "0220":
                        var iSOResult4 = _processISO.GetISODetails(m);
                        trxnType = iSOResult4.procCode.Substring(0, 2);
                        trans_ref = trxnType + iSOResult4.stan + iSOResult4.rrn + iSOResult4.terminalId;
                        break;
                }

                //Set the Response Code
                m.setResponseMTI();
                m.set(39, rsp);
                if (isoBal != "")
                {
                    m.set(54, isoBal);
                }
                isos.send(m);


                //Display the breakdown
                isotext = ISOUtil.dumpString(m.pack());
                Console.WriteLine("Response Message: ");
                Console.WriteLine(isotext);
                brk = u.BreakMsg(m);
                Console.ForegroundColor = ConsoleColor.DarkCyan;
                Console.WriteLine(brk);
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.WriteLine("Message sent ==>");
                Console.ForegroundColor = ConsoleColor.White;

            }
            catch (ISOException e)
            {
                logger.Error(e);
            }
            return true;
        }
    }
}
