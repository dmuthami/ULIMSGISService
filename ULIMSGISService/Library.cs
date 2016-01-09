using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;//Process

namespace ULIMSGISService
{
    class Library
    {

        /// <summary>
        /// Create a log method (WriteErrorLog) to log the exceptions
        /// </summary>
        /// <param name="ex"></param>
        public static void WriteErrorLog(Exception ex)
        {
            StreamWriter sw = null;
            try
            {
                sw = new StreamWriter(AppDomain.CurrentDomain.BaseDirectory + "logfile.txt", true);
                sw.WriteLine(DateTime.Now.ToString() + ": " + ex.Source.ToString().Trim() +
                    ex.Message.ToString().Trim());
                sw.Flush();
                sw.Close();
            }
            catch
            {

            }

        }

        /// <summary>
        /// Create a log method (WriteErrorLog) to log the custom messages
        /// </summary>
        /// <param name="Message"></param>
        public static void WriteErrorLog(string Message)
        {
            StreamWriter sw = null;
            try
            {
                sw = new StreamWriter(AppDomain.CurrentDomain.BaseDirectory + "logfile.txt", true);
                sw.WriteLine(DateTime.Now.ToString() + ": " + Message);
                sw.Flush();
                sw.Close();
            }
            catch
            {

            }

        }

        public static void executePythonProcess()
        {
            try
            {
                //Create a dictionary object with all the 10 piloting sites
                Dictionary<string, string> dictionary = new Dictionary<string, string>();

                dictionary.Add("otjiwarongo", "otjiwarongo");
                dictionary.Add("outapi", "outapi");
                dictionary.Add("keetmanshoop", "keetmanshoop");
                dictionary.Add("oshakati", "oshakati");

                dictionary.Add("walvis_bay", "walvis_bay");
                dictionary.Add("helao_nafidi", "helao_nafidi");
                dictionary.Add("katima_mulilo", "katima_mulilo");
                dictionary.Add("tsumeb", "tsumeb");

                dictionary.Add("rundu", "rundu");
                dictionary.Add("okahandja", "okahandja");

                //Loop over pairs with foreach loop
                foreach (KeyValuePair<string, string> townpair in dictionary)
                {
                    //Call function to execute python process          
                    executePythonProcess((String)townpair.Value);

                }
            }
            catch (Exception)
            {
                throw;
            }

        }
        public static void executePythonProcess(String townName)
        {
            try
            {
                //Get path of the exe and its directory path
                GetPaths();

                //Get path of config file
                String configFilePath = "\"" + mExecutableRootDirectory + String.Format("\\local_authorities\\{0}\\Config.ini", townName) + "\"";

                //Get path of main python file
                String pathToPythonMainFile = mExecutableRootDirectory + String.Format("\\local_authorities\\{0}\\AutoReconcileAndPost.py", Library.pythonCodeFolder);
                pathToPythonMainFile = "\"" + pathToPythonMainFile + "\"";

                //Get path of reconcile log file
                String reconcileLogFilePath = mExecutableRootDirectory + String.Format("\\local_authorities\\{0}\\{0}_reconcile.log", townName);
                reconcileLogFilePath = "\"" + reconcileLogFilePath + "\"";

                //Set path for current directory or strictly speaking directory of interest that you want to make current
                String currDirPath = mExecutableRootDirectory + String.Format("\\local_authorities", "");
                currDirPath = "\"" + currDirPath + "\"";

                //Create an instance of Python Process class
                Process p = new Process();

                //We execute python code
                p.StartInfo.FileName = "python.exe"; // Ensure python path is referenced in the Environment variables


                p.StartInfo.UseShellExecute = false;//make sure we can read output from stdout

                // Redirect the standard output of the sort command.  
                // This stream is read asynchronously using an event handler.
                p.StartInfo.RedirectStandardOutput = true;
                sortOutput = new StringBuilder("");


                // Set our event handler to asynchronously read the sort output.
                p.OutputDataReceived += new DataReceivedEventHandler(SortOutputHandler);

                //startInfo.Arguments = "\"" + fileName + "\"";

                /*
                 * Start the program with 4 parameters.NB Use of escape characters to escape spaces in file paths
                 * 
                 * First argument: Tells pyhton which python script to execute
                 * Second argument: Supplys the path to the config file
                 * Third argument: Supplys the path to the reconcile log file
                 * Fourth argument: Supplies the current directory. NB. Different from current working directory
                 */
                p.StartInfo.Arguments = pathToPythonMainFile + " " + configFilePath + " " + reconcileLogFilePath + " " + currDirPath;
                p.Start(); //Start the process (the python program)


                // To avoid deadlocks, use asynchronous read operations on at least one of the streams.
                // Do not perform a synchronous read to the end of both redirected streams.
                p.BeginOutputReadLine();


                //Wait for the python process
                p.WaitForExit();

                Library.WriteErrorLog(sortOutput.ToString());//Write to dotnet log file

                Console.WriteLine(sortOutput); //Write output to console


                //Releases all resources by the component
                p.Close();
            }
            catch (Exception)
            {

                throw;
            }

        }
        private static void SortOutputHandler(object sendingProcess, DataReceivedEventArgs outLine)
        {
            try
            {
                // Collect the sort command output.
                if (!String.IsNullOrEmpty(outLine.Data))
                {
                    numOutputLines++;

                    // Add the text to the collected output.
                    sortOutput.Append(Environment.NewLine +
                        "[" + numOutputLines.ToString() + "] - " + outLine.Data);
                }
            }
            catch (Exception)
            {

                throw;
            }
        }
        private static void GetPaths()
        {
            try
            {
                mExecutablePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
                mExecutableRootDirectory = System.IO.Path.GetDirectoryName(mExecutablePath);
            }
            catch (Exception)
            {

                throw;
            }
        }
        public static string mExecutablePath { get; set; }
        public static string mExecutableRootDirectory { get; set; }
        public static StringBuilder sortOutput { get; set; }
        public static int numOutputLines { get; set; }
        public static string pythonCodeFolder { get; set; }
        public static bool _executing { get; set; }
    }
}
