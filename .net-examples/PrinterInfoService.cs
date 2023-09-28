using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Fargo.PrinterSDK;
using System.Threading;
using CardPrintService.Model;
using CodeExamples.Lib;
using System.Management;
using System.Collections;
using CardPrintService.Printing.Matica;
using CardPrintService.Printing.JVC;
using System.Configuration;
using CardPrintService.Printing.Matidesk;


namespace CardPrintService.Printing {

    /**
     * This class retrieves the Fargo printer properties.  It runs in a separate thread so that if the printer is not reachable,
     * it will timeout in 8-20 seconds.
     */ 
    public class PrinterInfoService {

        volatile bool isComplete = false;
        private PrinterInfoStatus result = null;
        private string printerName = "";
        private int printerModelType = 14;//Matica by default
        int getDetail = 1;

    
        /**
         * Get the printer info for the given printer.  If getDetail is true, return the complete set of properties.
         * If getDetail is false, only the printer activity and error info is returned.
         * 
         * This method times out with a "printer is unavailable" error if the data is not returned within 8-20 seconds.
         */ 
        public PrinterInfoStatus getPrinterStatus(string printerName, int printerModelType,int getDetail) {
            this.getDetail = getDetail;
            this.printerName = printerName;
            this.printerModelType = printerModelType;

            ThreadedExecuter<PrinterInfoStatus> executer = null;

            PrintCardFactory.initSettings();
            if (isMaticaPrinter(printerModelType) && (PrintCardFactory.useDirectJVCApi || PrintCardFactory.isIPAddress(printerName))) {
                executer = new ThreadedExecuter<PrinterInfoStatus>(getJVCPrinterStatusThread, getPrinterStatusComplete);
            } else {
                if (isFargoPrinter(printerModelType)) {
                    executer = new ThreadedExecuter<PrinterInfoStatus>(getFargoPrinterStatusThread, getPrinterStatusComplete);
                } else if (isMatideskPrinter(printerModelType))  {
                    executer = new ThreadedExecuter<PrinterInfoStatus>(getMatideskPrinterStatusThread, getPrinterStatusComplete);
                } else {
                    executer = new ThreadedExecuter<PrinterInfoStatus>(getMaticaPrinterStatusThread, getPrinterStatusComplete);
                }
            }

            executer.Start();

            int timeoutCount = 0;
            //FIXME - set to 100 for production
            int maxTimeout = getDetail==1 ? 90 : 60;

            while (!isComplete && timeoutCount++ < maxTimeout) {//60/30 second timeout
                //Logging.Log("sleeping - " + timeoutCount,LoggingLevelEnum.DEBUG);
                Thread.Sleep(1000);
            }

            if (!isComplete) {
                Logging.Log("aborting printer status thread - operation timed out - returning printer is unavailable");
                executer.Abort();
                PrinterInfoStatus pStatus = new PrinterInfoStatus();
                pStatus.printerError = "printer is unavailable";
                pStatus.printerStatus = "printer may be offline or unplugged";
       
                JVCAPI jvc = JVCPrinterCollection.getJVC(printerName, printerModelType);
                bool alreadyConnected = jvc.isConnected || jvc.isPrinting;
                if (!alreadyConnected)
                    jvc.disconnect();    

                result = pStatus;
            }
            return result;
        }

        /**
         * read the app.config settings and look for the printer name - ip setting.
         */
        public static string getJVCDLLPrinterIP(string printerName) {

            string printerList = PrintCardFactory.jvcPrinters;

            if (printerList == null)
                 return null;
            
            string[] pList = printerList.Split(';');
            
            foreach (string prntr in pList) {
                string[] hostAndPrinter = prntr.Split('/');
                if (hostAndPrinter.Length==2 && printerName.Equals(hostAndPrinter[1])) {
                    return hostAndPrinter[0];
                }
            }

           return null;
        }

        /**
         * This is the thread callback, which sets the isComplete flag used by getPrinterStatus().
         */
        public void getPrinterStatusComplete(PrinterInfoStatus returnValue) {
            result = returnValue;
            isComplete = true;
        }


        public static bool isFargoPrinter(int deviceTypeId) {
            return (deviceTypeId == 1 || deviceTypeId == 2);
        }

        public static bool isMaticaPrinter(int deviceTypeId) {
            return (deviceTypeId == 14 || deviceTypeId == 15);
        }

        public static bool isMatideskPrinter(int deviceTypeId)
        {
            return (deviceTypeId == 17);
        }

        /**
         * get the printer status from the Fargo printer for this instance, and return it.
         */
        public PrinterInfoStatus getFargoPrinterStatusThread() {
            PrinterInfoStatus pStatus = new PrinterInfoStatus();

            try {


                Fargo.PrinterSDK.PrinterInfo pInfo = new PrinterInfo(printerName);

                // pInfo.SendAPDU();

                //pInfo.SetLCDMessage("Getting Printer Info");
                /*
                Logging.Log("COM SDK Version:   " + pInfo.ToString());
                Logging.Log("Current Activity: " + pInfo.CurrentActivity().ToString());
                Logging.Log("Error Status:   " + pInfo.PrinterError().ToString());
                Logging.Log("Printer Status:   " + pInfo.PrinterStatus().ToString());
                Logging.Log("Sensor Status:   " + pInfo.SensorStatus);

                Logging.Log("LCD Display:      " + pInfo.LCDInfo.ToString());
                Logging.Log("Card Count:       " + pInfo.CardCount.ToString());
                Logging.Log("Film Remaining:   " + pInfo.FilmPercentRemaining.ToString());
                Logging.Log("Ribbon Remaining: " + pInfo.RibbonPercentRemaining.ToString());
                Logging.Log("has laminate: " + pInfo.HasLamination);                
                Logging.Log("Laminate Remaining: " + pInfo.LaminatePercentRemaining(0).ToString());


                Logging.Log("Magnetic Station Status:   " + pInfo.StationStatus(Station.Magnetic).ToString());
                Logging.Log("Reject Station Status:   " + pInfo.StationStatus(Station.Reject).ToString());
                */

                pStatus.printerError = pInfo.PrinterError().ToString();
                pStatus.printerErrorEx = pInfo.PrinterErrorEx().ToString();
                pStatus.currentActivity = pInfo.CurrentActivity().ToString();
                //pStatus.deviceError = pInfo.DeviceError().ToString();
                //pStatus.printerStatus = pInfo.PrinterStatus().ToString();
                pStatus.printerStatus = pInfo.CurrentActivity().ToString();

                if (getDetail==1) {
                    pStatus.comsdkVersion = pInfo.SDKVersion;
                    pStatus.sdkVersion = pInfo.SDKVersion;
                    pStatus.serialNumber = pInfo.SerialNumber;
                    pStatus.ribbonPartNumber = pInfo.RibbonOEMPartNumber;
                    pStatus.ribbonPercentRemaining = pInfo.RibbonPercentRemaining;
                    pStatus.filmPartNumber = pInfo.FilmPartNumber;
                    pStatus.filmPercentRemaining = pInfo.FilmPercentRemaining;
                    pStatus.firmwareVersion = pInfo.FirmwareVersion;
                    pStatus.ribbonOEMPartNumber = pInfo.RibbonOEMPartNumber;
                    pStatus.filmOEMPartNumber = pInfo.FilmOEMPartNumber;
                    pStatus.lcdInfo = pInfo.LCDInfo;
                    pStatus.sensorStatus = pInfo.SensorStatus;
                    pStatus.hasFlipper = pInfo.HasFlipper;
                    pStatus.hasLamination = pInfo.HasLamination;
                    pStatus.cardCount = pInfo.CardCount;
                } else if (getDetail == 2) {
                    pStatus.ribbonPercentRemaining = pInfo.RibbonPercentRemaining;
                    pStatus.filmPercentRemaining = pInfo.FilmPercentRemaining;
                }

            } catch (Exception error) {
                Logging.Log("cannot retrieve printer info {0}", LoggingLevelEnum.ERROR, error.ToString());
            }

            return pStatus;
        }


        public PrinterInfoStatus getMaticaPrinterStatusThread() {
            PrinterInfoStatus pStatus = new PrinterInfoStatus();

            try {
                MaticaAPI matica = new MaticaAPI(printerName);
                return matica.getPrinterStatus(getDetail==1 || getDetail==2);
            } catch (Exception error) {
                Logging.Log("cannot retrieve printer info {0}", LoggingLevelEnum.ERROR, error.ToString());
            }

            return pStatus;
        }

        public PrinterInfoStatus getJVCPrinterStatusThread() {
            PrinterInfoStatus pStatus = new PrinterInfoStatus();
            JVCAPI jvc = JVCPrinterCollection.getJVC(printerName, printerModelType);
            bool alreadyConnected = jvc.isConnected || jvc.isPrinting;
            try {
                
                if (jvc.isPrinting) {
                    Logging.Log("printing status - returning busy status");
                    pStatus.printerStatus = jvc.lastPrintStatus;
                    pStatus.currentActivity = jvc.lastCurrentActivity;
                   // pStatus.printerStatus = CardPrintService.Printing.Matica.PrinterStatusEnum.Busy.ToString();
                   // pStatus.currentActivity = CardPrintService.Printing.Matica.PrinterStatusEnum.Busy.ToString();
                } else {
                
                    Logging.Log("printing status - getting status from printer");

                    Logging.Log("info service - connected? " + alreadyConnected);

                    if (!alreadyConnected)
                        jvc.connect();
                    pStatus = jvc.getPrinterStatus(getDetail);

                }

                

            } catch (Exception error) {
                Logging.Log("cannot retrieve printer info {0}", LoggingLevelEnum.ERROR, error.ToString());
            } finally {
                if (!alreadyConnected)
                    jvc.disconnect();          
            }

            return pStatus;
        }

        public PrinterInfoStatus getMatideskPrinterStatusThread()
        {
            PrinterInfoStatus pStatus = new PrinterInfoStatus();
            MatideskAPI matidesk = MatideskPrinterCollection.getMatidesk(printerName, printerModelType);
            bool alreadyConnected = matidesk.isConnected || matidesk.isPrinting;
            try
            {

                if (matidesk.isPrinting)
                {
                    Logging.Log("printing status - returning busy status");
                    pStatus.printerStatus = matidesk.lastPrintStatus;
                    pStatus.currentActivity = matidesk.lastPrintStatus;
                }
                else
                {
                    Logging.Log("printing status - getting status from printer");

                    Logging.Log("info service - connected? " + alreadyConnected);

                    if (!alreadyConnected)
                        matidesk.connect();
                    pStatus = matidesk.getPrinterStatus(getDetail);
                }
            }
            catch (Exception error)
            {
                Logging.Log("cannot retrieve printer info {0}", LoggingLevelEnum.ERROR, error.ToString());
            }
            finally
            {
                if (!alreadyConnected)
                    matidesk.disconnect();
            }

            return pStatus;
        }
    }
}
