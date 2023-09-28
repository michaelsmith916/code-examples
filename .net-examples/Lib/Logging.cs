using System;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Timers;

namespace CodeExamples.Lib {
    /*
     * This class uses a log4j-like logging method that redirects all output to the printService.log file.
     */
    public class Logging {

        private static FileStream ostrm = null;
        private static StreamWriter logWriter = null;
        private static TextWriter oldOut = null;
        public static LoggingLevelEnum logLevelThreshold = LoggingLevelEnum.INFO;
        public static String currentLogFileName;
        public static string DATEFORMAT = "yyyyMMdd";
        private static String logBaseName = "log";
        static long needsUpdate = -1;
        static long hasUpdated = 0;
        const double updateInterval = 30 * 60 * 1000; // every half hour
        static System.Timers.Timer checkForTime = null;

        public static void enableFileLogging(String logFileName) {
            logBaseName = logFileName != null ? logFileName : logBaseName;
            enableFileLogging();
        }

        /**
         * calling this method redirects console output to the printService.log file.
         */
        private static void enableFileLogging() {
            try {
                oldOut = Console.Out;
                Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);
                deleteOldFiles();
                checkForUpdate(true);
               

                checkForTime = new System.Timers.Timer(updateInterval);
                checkForTime.Elapsed += new ElapsedEventHandler(scheduleUpdateEvent);
                checkForTime.Enabled = true;
                checkForTime.Start();
 
            } catch (Exception e) {
                Console.WriteLine("Error: Unable to write to log file");
                Console.WriteLine(e.ToString());
            }
        }

        /**
         * stops logging to file, and returns the system output to the console.
         */
        public static void disableFileLogging() {
            Console.SetOut(oldOut);
            if (logWriter != null)
                logWriter.Close();
            if (ostrm != null)
                ostrm.Close();

        }

        public static void Log(string logMessage,LoggingLevelEnum logLevel) {
            Log(logMessage, logLevel,null);
        }

        /*
         * log the given logmessage.
         */
        public static void Log(string logMessage) {
            Log(logMessage, null);
        }

        public static void Log(string logMessage, params object[] objs) {
            Log(logMessage,LoggingLevelEnum.INFO,objs);
        }

        /**
         * log the given logmessage, and optionally include any parameters provided by objs.
         */
        public static void Log(string logMessage, LoggingLevelEnum logLevel, params object[] objs) {

            try {
                if (printForLogLevel(logLevel,logLevelThreshold)) {
                    Console.Write("{0} --", DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.fff"));
                    Console.Write("{0}--  ", logLevel);

                    if (objs != null)
                        Console.WriteLine(logMessage, objs);
                    else
                        Console.WriteLine(logMessage);

                    logWriter.Flush();  
                }
            } catch (Exception e) {
                Console.WriteLine("unable to log error to file.  Error: {0} Log Msg: {1}", e.ToString(), logMessage);
            }

        }

        /**
         * return true if the given logLevel should be printed for the given log threshold.  Return false, otherwise.
         */
        public static bool printForLogLevel(LoggingLevelEnum logLevel, LoggingLevelEnum threshold) {
            if (logLevel.Equals(threshold) || 
                (threshold.Equals(LoggingLevelEnum.WARN) && !logLevel.Equals(LoggingLevelEnum.DEBUG) && !logLevel.Equals(LoggingLevelEnum.TRACE)) ||
                (threshold.Equals(LoggingLevelEnum.DEBUG) && !logLevel.Equals(LoggingLevelEnum.TRACE)) || 
                threshold.Equals(LoggingLevelEnum.TRACE) || logLevel.Equals(LoggingLevelEnum.ERROR))
                return true;

           return false;
        }

        /**
         * get file name for right now...
         */
        private static string getFileName() {
            string curDate = DateTime.Now.ToString(DATEFORMAT);

            return logBaseName + curDate + ".log" ;
        }

        private static void scheduleUpdateEvent(object sender, ElapsedEventArgs e) {
            //need to rollover and delete old files based on current time
            deleteOldFiles();

            checkForUpdate(false);


        }

        /**
         * delete log files older than x days
         */ 
        private static void deleteOldFiles() {
            try {
                int rolloverPeriod;
                if (!int.TryParse(ConfigurationManager.AppSettings["loggingRolloverInterval"], out rolloverPeriod))
                    rolloverPeriod = 30;

                string folder = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
                string filter = "*.log";
                string[] files = Directory.GetFiles(folder, filter);

                foreach (string file in files) {
                    string fileName = file.Substring(file.LastIndexOf('\\') + 1);

                    if (fileName.Length >= logBaseName.Length + 8) {
                        string datepart = fileName.Substring(logBaseName.Length, 8);


                        DateTime dateResult;
                        if (DateTime.TryParseExact(datepart, DATEFORMAT, CultureInfo.InvariantCulture, DateTimeStyles.None, out dateResult)) {
                            DateTime rolloverDate = DateTime.Now.AddDays(-1 * rolloverPeriod);
                            if (dateResult.CompareTo(rolloverDate) <= 0) {
                                //delete the file
                                try {
                                    File.Delete(file);
                                } catch (Exception e) {
                                    Logging.Log("Warn: unable to delete log file: {0} . Error: {1} ", LoggingLevelEnum.WARN, file, e.Message);
                                }
                            }
                        }
                    }
                }
            } catch (Exception e) {
                Logging.Log("Error deleting old log files. {0}",LoggingLevelEnum.ERROR,e.Message);
            }
        }
        
        /**
         * this should fire on a thread every 30 minutes.
         */
        private static void checkForUpdate(bool forceUpdate) {
       
            DateTime curTime =  DateTime.Now;
            
            
            if (curTime.Hour==0 && hasUpdated==0 || forceUpdate) {
                hasUpdated = 1;
                updateStream();
            } else if (curTime.Hour>0){
                hasUpdated = 0; 
            }
        }

        private static void updateStream() {
         
            if (0 == Interlocked.Exchange(ref needsUpdate, 1)) {                
                ostrm = new FileStream("./" + getFileName(), FileMode.Append, FileAccess.Write);
                logWriter = new StreamWriter(ostrm);
                logWriter.AutoFlush = true;

                logLevelThreshold = EnumUtil.ParseEnum<LoggingLevelEnum>(ConfigurationManager.AppSettings["logginglevel"], LoggingLevelEnum.INFO);

                Console.SetOut(logWriter);

                Interlocked.Exchange(ref needsUpdate, 0))
            }
            
        }

    }

 

    public enum LoggingLevelEnum {
        WARN,
        INFO,
        DEBUG,
        TRACE,
        ERROR
    }
}
