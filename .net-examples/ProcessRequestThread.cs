using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using CardPrintService.Model;
using CardPrintService.Printing;
using CodeExamples.Lib;
 
namespace CardPrintService.Printing {

    /**
     * This class is a long-running monitor thread, that monitors the
     * job collection for new jobs, and processes the jobs, in order of the request.
     * 
     * this thread also cleans up jobs that have completed
     * 
     * Run one of these thread methods, per printer...
     */
    public class ProcessRequestThread {

        private PrinterCollection jobCollection;
        private Thread mainProcessThread = null;
        public ProcessRequestThread(PrinterCollection jobCollection) {
            this.jobCollection = jobCollection;
            mainProcessThread = new Thread(processNext);
            mainProcessThread.Start();
        }

        /**
         * call when starting the job monior process
         */
        public void startProcess() {
            mainProcessThread.Start();
        }

        /*
         * call to stop the job monitor process
         */
        public void stopProcess() {
            mainProcessThread.Abort();
        }

        /*
         * iterate through all of the printers and look for a print job that needs to be executed.
         * The printer is then locked for processing, and the JobProcessingService class is then called to process the next available job.
         */
        public void processNext() {

            while (true) {
                Thread.Sleep(1000);
                Thread.Yield();


                //iterate through all the printers, and process the next job that's waiting on a new thread. 
                
                foreach (string printer in jobCollection.getPrinterList()) {
                    PrinterJobCollection pj = jobCollection.getPrinter(printer);
                     if (pj.getPrintJobList().Count > 0) {
                        //Logging.Log("job count..printer:{0} jobcount:{1}  isprocessing: {2}",LoggingLevelEnum.INFO,printer, pj.getPrintJobList().Count(),pj.isProcessing);
               
                        if (pj.compareAndSetProcessing(false, true)) {
                            //mark thread as processing and begin thread iterating through print job queue
                            JobProcessingService procService = new JobProcessingService(pj);
                            new Thread(procService.processNextJob).Start();
                        }
                    }
                }
            }
        }
        
    }
}
