using System;
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

                //Set timer to 20 minutes or three hundred thousand milliseconds . The rate at which to raise the elapsed event
                this.mTimer.Interval = 1200000;
                /*
                 * Wire timer elapsed event to the timer tick handler
                 * Occurs when the interval elapses
                 */
                this.mTimer.Elapsed += new System.Timers.ElapsedEventHandler(this.mTimer_Tick);

                //Enable the timer to whether to raise the elapsed event
                mTimer.Enabled = true;

                //Write to log file indicating GIS service has started successfully.
                Library.WriteErrorLog("ULIMS GIS Synchonization Service started");

            }
            catch (Exception ex)
            {

                Library.WriteErrorLog(ex);//Write error to log file
            }
        }

        private void mTimer_Tick(object sender, ElapsedEventArgs e)
        {
            //Check that elapsed event raises this event handler and executes code in it 
            //  if and only if the previous execution has completed
            if (Library.mEexecuting)
                return;

            //Set library is executing as true
            Library.mEexecuting = true;

            try
            {
                //Indicate and write to log file shouting that execution of python code is firing from all cylinders
                Library.WriteErrorLog(@"Timer ticked and executePythonCode()  method or 
                job has been done fired");

                //Call method that executes python code
                executePythonCode(); 
            }
            catch (Exception ex)
            {
                Library.WriteErrorLog(ex);//Write error to log file
            }
            finally
            {
                Library.WriteErrorLog(@"Timer ticked and executePythonCode()  method or 
                job has successfully completed(But with a pinch of salt-There could be errors)");
                Library.mEexecuting = false;
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
                Library.WriteErrorLog("ULIMS GIS synchronize Service stopped");
            }
            catch (Exception ex)
            {

                Library.WriteErrorLog(ex);//Write error to log file
            }
        }
        /// <summary>
        /// executePythonCode Method: Calls other functions to execute python code
        /// </summary>
        private void executePythonCode()
        {
            try
            {
                //Call function to execute python process for otjiwarongo town
                Library.mPythonCodeFolder = "python_code";

                //Call function to execute python process for all towns
                Library.executePythonProcess();

                //Wait for key press
                Console.ReadLine(); //Comment this line for the release version
            }
            catch (Exception)
            {
                throw; //In case of an error then throws it up the stack trace
            }
        }
    }
}
