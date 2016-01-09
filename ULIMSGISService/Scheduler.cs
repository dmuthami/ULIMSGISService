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
        private Timer timer1 = null;
        public Scheduler()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            try
            {
                timer1 = new Timer();
                this.timer1.Interval = 300000; //5 minutes
                this.timer1.Elapsed += new System.Timers.ElapsedEventHandler(this.timer1_Tick);
                timer1.Enabled = true;
                Library.WriteErrorLog("ULIMS GIS Service started");
            }
            catch (Exception ex)
            {
                
                Library.WriteErrorLog(ex);
            }
        }

        private void timer1_Tick(object sender, ElapsedEventArgs e)
        {
            //throw new NotImplementedException();


            if (Library._executing)
                return;

            Library._executing = true;

            try
            {
                Library.WriteErrorLog(@"Timer ticked and executePythonCode()  method or 
                job has been done fired");

                //Call method that executes python code
                executePythonCode(); 
            }
            catch (Exception ex)
            {
                Library.WriteErrorLog(ex);
            }
            finally
            {
                Library.WriteErrorLog(@"Timer ticked and executePythonCode()  method or 
                job has successfully completed");
                Library._executing = false;
            }
        }

        protected override void OnStop()
        {
            try
            {
                timer1.Enabled = false;
                Library.WriteErrorLog("ULIMS GIS Service stopped");
            }
            catch (Exception ex)
            {
                
                Library.WriteErrorLog(ex);
            }
        }

        private void executePythonCode()
        {
            try
            {
                //Call function to execute python process for otjiwarongo town
                Library.pythonCodeFolder = "python_code";
                //Library.executePythonProcess("otjiwarongo");

                //Call function to execute python process for all towns
                Library.executePythonProcess();

                //Wait for key press
                Console.ReadLine();
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
