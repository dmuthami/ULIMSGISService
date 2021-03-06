﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;//Process

namespace ULIMSGISService
{
    class PythonLibrary : ULIMSGISService.IPythonLibrary
    {
        /// <summary>
        /// Create a log method (WriteErrorLog) to log the exceptions
        /// </summary>
        /// <param name="ex"></param>
        public  void WriteErrorLog(Exception ex)
        {
            StreamWriter streamWriter = null;
            try
            {
                //initializes a new instance of the StreamWriter class for the specified file in the location of the *.exe. Allows create or append to the file.
                streamWriter = new StreamWriter(AppDomain.CurrentDomain.BaseDirectory + "logfile.txt", true);

                //Write string followed by line terminator. Components of string is time and source & message of the exception object
                streamWriter.WriteLine(DateTime.Now.ToString() + ": " + ex.Source.ToString().Trim() +
                    ex.Message.ToString().Trim());

                //Clears all buffers for the current writer and causes any buffered data to be written to the underlying stream.
                streamWriter.Flush();

                //Closes the current StreamWriter object and the underlying stream
                streamWriter.Close();
            }
            catch
            {
                //Nothing goes here
            }
        }
        /// <summary>
        /// Create a log method (WriteErrorLog) to log the custom messages
        /// </summary>
        /// <param name="Message"></param>
        public  void WriteErrorLog(string Message)
        {
            StreamWriter streamWriter = null;
            try
            {
                //initializes a new instance of the StreamWriter class for the specified file in the location of the *.exe. Allows create or append to the file.
                streamWriter = new StreamWriter(AppDomain.CurrentDomain.BaseDirectory + "logfile.txt", true);

                //Write string followed by line terminator. Components of string is time and custom message
                streamWriter.WriteLine(DateTime.Now.ToString() + ": " + Message);

                //Clears all buffers for the current writer and causes any buffered data to be written to the underlying stream.
                streamWriter.Flush();

                //Closes the current StreamWriter object and the underlying stream
                streamWriter.Close();
            }
            catch
            {
                //Nothing goes here
            }
        }
        /// <summary>
        /// Method : executePythonProcess()
        /// Loops through a dictionary object listing the 10 piloting towns
        ///     For each town launches a process to perfom automatic reconcile and post
        /// </summary>
        public  void executePythonProcess()
        {
            try
            {
                //Create a dictionary object with all the 10 piloting sites
                Dictionary<string, string> dictionary = new Dictionary<string, string>();

                //Read towns from config file
                IConfigReader iConfigReader = new ConfigReader(this);

                //Method returns listof towns as a dictionary
                dictionary = iConfigReader.readNamibiaLocalAuthoritiesSection();

                //Loop over pairs with foreach loop
                foreach (KeyValuePair<string, string> townpair in dictionary)
                {
                    //Call function to execute python process for computing stand number for each town    
                    executePythonProcess((String)townpair.Value, "InvokeComputeStandNo.py");
                    
                    //Call function to execute python process for auto reconcile for each town   
                    executePythonProcess((String)townpair.Value, "AutoReconcileAndPost.py");
                    
                }
            }
            catch (Exception)
            {
                throw; //In case of an error then throws it up the stack trace
            }

        }
        /// <summary>
        /// Method : executePythonProcess(String townName)
        /// Creates a python process, passess it parameers and waits for completion. 
        /// Stdout from the python script is read asynchronously and captured into the .net log file
        /// </summary>
        /// <param name="townName"></param>
        /// <param name="pythonFileExecute"></param>  
        public void executePythonProcess(String townName, String pythonFileToExecute)
        {
            try
            {
                //Get path of the exe and its directory path
                getPaths();

                //Get path of config file
                String configFilePath = "\"" + mExecutableRootDirectory + String.Format("\\local_authorities\\{0}\\Config.ini", townName) + "\"";

                //Get path of main python file
                String pathToPythonMainFile = "\"" + mExecutableRootDirectory + String.Format("\\local_authorities\\{0}\\{1}", mPythonCodeFolder, pythonFileToExecute) + "\"";

                //Get path of reconcile log file
                String reconcileLogFilePath = "\"" + mExecutableRootDirectory + String.Format("\\local_authorities\\{0}\\{0}_reconcile.log", townName) + "\"";

                //Set path for current directory or strictly speaking directory of interest that you want to make current
                String currDirPath = "\"" + mExecutableRootDirectory + String.Format("\\local_authorities", "") + "\"";

                //Create an instance of Python Process class
                Process process = new Process();

                /*
                 * We execute python code
                 * Ensure python path is referenced in the Environment variables
                 */
                process.StartInfo.FileName = "python.exe"; 

                //make sure we can read output from stdout
                process.StartInfo.UseShellExecute = false;

                // Redirect the standard output of the sort command.  
                // This stream is read asynchronously using an event handler.
                process.StartInfo.RedirectStandardOutput = true;

                //intialize pointer to memory location storing a Stringbuilder object
                mSortOutput = new StringBuilder("");

                // Set our event handler to asynchronously read the sort output.
                process.OutputDataReceived += new DataReceivedEventHandler(sortOutputHandler);

                /*
                 * Start the program with 4 parameters.NB Use of escape characters to escape spaces in file paths
                 * 
                 * First argument: Tells pyhton which python script to execute
                 * Second argument: Supplys the path to the config file
                 * Third argument: Supplys the path to the reconcile log file
                 * Fourth argument: Supplies the current directory. NB. Different from current working directory
                 */
                process.StartInfo.Arguments = pathToPythonMainFile + " " + configFilePath + " " + reconcileLogFilePath + " " + currDirPath;

                //Start the process (i.e the python program)
                process.Start();

                // To avoid deadlocks, use asynchronous read operations on at least one of the streams.
                // Do not perform a synchronous read to the end of both redirected streams.
                process.BeginOutputReadLine();

                //Wait for the python process
                process.WaitForExit();

                WriteErrorLog(mSortOutput.ToString());//Write to dotnet log file

                //Releases all resources by the component
                process.Close();

            }
            catch (Exception)
            {

                throw;//In case of an error then throws it up the stack trace
            }

        }
        /// <summary>
        /// event Handler : sortOutputHandler
        /// Asynchronously captures ouptput writen to console by python
        /// </summary>
        /// <param name="sendingProcess"></param>
        /// <param name="outLine"></param> 
        private  void sortOutputHandler(object sendingProcess, DataReceivedEventArgs outLine)
        {
            try
            {
                // Collect the sort command output.
                if (!String.IsNullOrEmpty(outLine.Data))
                {
                    mNumOutputLines++; //concatenate

                    // Add the text to the collected output.
                    mSortOutput.Append(Environment.NewLine +
                        "[" + mNumOutputLines.ToString() + "] - " + outLine.Data);
                }
            }
            catch (Exception)
            {

                throw;//In case of an error then throws it up the stack trace
            }
        }
        /// <summary>
        /// Method : getPaths()
        /// Retuns path and directory of the exutable file and stores in member variables
        /// </summary>        
        private  void getPaths()
        {
            try
            {
                mExecutablePath = System.Reflection.Assembly.GetExecutingAssembly().Location;//get path to *.exe
                mExecutableRootDirectory = System.IO.Path.GetDirectoryName(mExecutablePath); // get directory containing the *.exe
            }
            catch (Exception)
            {

                throw;//In case of an error then throws it up the stack trace
            }
        }
        /// <summary>
        /// Property : mExecutablePath
        /// Wrapped up in a getter and setter
        /// </summary>
        public  string mExecutablePath { get; set; }
        /// <summary>
        /// Property : mExecutableRootDirectory
        /// Wrapped up in a getter and setter
        /// </summary>
        public  string mExecutableRootDirectory { get; set; }
        /// <summary>
        /// Property : mSortOutput
        /// Wrapped up in a getter and setter
        /// </summary>
        public  StringBuilder mSortOutput { get; set; }
        /// <summary>
        /// Property : mNumOutputLines
        /// Wrapped up in a getter and setter
        /// </summary>
        public  int mNumOutputLines { get; set; }
        /// <summary>
        /// Property : mPythonCodeFolder
        /// Wrapped up in a getter and setter
        /// </summary>
        public  string mPythonCodeFolder { get; set; }
        /// <summary>
        /// Property : mEexecuting
        /// Wrapped up in a getter and setter
        /// </summary>       
        public  bool mEexecuting { get; set; }
    }
}
