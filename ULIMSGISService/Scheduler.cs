﻿using System;
using System.Configuration;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Timers;


namespace ULIMSGISService
{
    public partial class Scheduler : ServiceBase
    {
        //Create a pointer to a timer as a member variable named mTimer
        private Timer mTimer = null;
        private Library library = null;

        /// <summary>
        /// Get and Setter Methods
        /// </summary>
        internal Library MLibrary
        {
            get { return library; }
            set { library = value; }
        }
        /// <summary>
        /// Constructor
        /// </summary>
        public Scheduler()
        {
            InitializeComponent();
        }
        /// <summary>
        /// Method: OnStart
        /// Triggers when the windows service starts
        /// </summary>
        /// <param name="args"></param>
        protected override void OnStart(string[] args)
        {
            try
            {
                //Initialize anew instance of the timer class
                mTimer = new Timer();

                //Get timer interval from app.config
                double timerInterval;
                timerInterval = Convert.ToDouble(ConfigurationManager.AppSettings["timer_interval"].ToString());
                this.mTimer.Interval = timerInterval;
                /*
                 * Wire timer elapsed event to the timer tick handler
                 * Occurs when the interval elapses
                 */
                this.mTimer.Elapsed += new System.Timers.ElapsedEventHandler(this.mTimer_Tick);

                //Enable the timer to whether to raise the elapsed event
                mTimer.Enabled = true;

                /*
                 * Get instance of Library class
                 * Contains propoerties and methods to help with execution
                 */
                MLibrary = new Library();

                //Write to log file indicating GIS service has started successfully.
                library.WriteErrorLog("ULIMS GIS Synchonization Service started");

            }
            catch (Exception ex)
            {

                library.WriteErrorLog(ex);//Write error to log file
            }
        }
        private void mTimer_Tick(object sender, ElapsedEventArgs e)
        {
            //Check that elapsed event raises this event handler and executes code in it 
            //  if and only if the previous execution has completed
            if (library.mEexecuting)
                return;

            //Set library is executing as true
            library.mEexecuting = true;

            try
            {
                //Indicate and write to log file shouting that execution of python code is firing from all cylinders
                library.WriteErrorLog(@"Timer ticked and executePythonCode()  method or 
                job has been done fired");

                //Call method that executes python code
                executePythonCode();
            }
            catch (Exception ex)
            {
                library.WriteErrorLog(ex);//Write error to log file
            }
            finally
            {
                library.WriteErrorLog(@"Timer ticked and executePythonCode()  method or 
                job has successfully completed(But with a pinch of salt-There could be errors)");
                library.mEexecuting = false;
            }
        }
        /// <summary>
        /// Method: OnStop()
        /// Triggers when the windows service starts
        /// </summary>
        protected override void OnStop()
        {
            try
            {
                //Desist/falsify the timer from raising the elapsed event
                mTimer.Enabled = false;

                //Write to log file indicating that service could have stopped for whatever reasons
                // Possible reason could be user action on services.msc stopping this particular service
                library.WriteErrorLog("ULIMS GIS Synchronize Service Stopped");
            }
            catch (Exception ex)
            {

                library.WriteErrorLog(ex);//Write error to log file
            }
        }
        /// <summary>
        /// executePythonCode Method: Calls other functions to execute python code
        /// </summary>
        private void executePythonCode()
        {
            try
            {
                //Load config settings from app.config file               
                library.mPythonCodeFolder = ConfigurationManager.AppSettings["python_folder"];

                //Call function to execute python process for all towns
                library.executePythonProcess();

                /*
                 * Add code to call .Net Synch Service that performs write to SharePoint Lists
                 * Add new Erfs to SharePoint Lists
                 * Update to SharePoint lists
                 * Flag deleted erfs in SharePoint List
                 */

            }
            catch (Exception)
            {
                throw; //In case of an error then throws it up the stack trace
            }
        }
    }
}
