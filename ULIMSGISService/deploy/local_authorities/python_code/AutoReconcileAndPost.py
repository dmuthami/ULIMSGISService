import os, sys
import logging
import arcpy, time, smtplib
import traceback
from arcpy import env
from datetime import datetime

##Custom module containing functions
import Configurations

#Set-up logging
logger = logging.getLogger('myapp')
Configurations.Configurations_localauthority = "temp"
Configurations.Configurations_cat_logfile = os.path.join(os.path.dirname(__file__), Configurations.Configurations_localauthority+'.log')
hdlr = logging.FileHandler(Configurations.Configurations_cat_logfile)
formatter = logging.Formatter('%(asctime)s %(levelname)s %(message)s')
hdlr.setFormatter(formatter)
logger.addHandler(hdlr)
logger.setLevel(logging.INFO)

#start time
msg ="\n------------------------------------------------------------------\n"+"Start Time : " + datetime.now().strftime("-%y-%m-%d_%H-%M-%S")+ "\n------------------------------------------------------------------\n"
print msg
#Logging
logger.info(msg)

try:
    os.chdir("..")
    currDir = os.getcwd()
    ##Obtain script parameter values
    ##location for configuration file
    ##Acquire it as a parameter either from terminal, console or via application
    configFileLocation=arcpy.GetParameterAsText(0)#Get from console or GUI being user input
    if configFileLocation =='': #Checks if supplied parameter is null
        #Defaults to below hard coded path if the parameter is not supplied. NB. May throw exceptions if it defaults to path below
        # since path below might not  be existing in your system with the said file name required
        configFileLocation=os.path.join(currDir, 'otjiwarongo','Config.ini')

    ##Read from config file
    #If for any reason an exception is thrown here then subsequent code will not be executed
    Configurations.setParameters(configFileLocation)

    #Write to the log file specifying the localauthority
    msg = "\n Local Authority : "+Configurations.Configurations_localauthority
    logger.info(msg)

    #Write to the console specifying the localauthority
    print msg

    #output location of Config file
    logger.info("\n Config File Path : "+configFileLocation)

    ##Proper Code starts here
    #Overwrite output
    env.overwriteOutput = True

    null = ""
    currDirDotNet=arcpy.GetParameterAsText(2)#Get from console or GUI being user input passed from .Net application
    if currDirDotNet is not null: #Checks if supplied parameter is null
        #Get path from system argument for location of reconcile log file
        currDir = currDirDotNet
        #Output path for currDir
        msg=  "\n"+currDirDotNet+"\n"
        logger.info("currDirDotNet path : "+msg)
        #print msg
    else:
        #Output path for currDir
        msg=  "\n"+currDir+"\n"
        logger.info("currDirDotNet path : "+msg)
        #print msg

    # Get the Connection to the geodatabase administrator
    connSDE = os.path.join(currDir,\
    Configurations.Configurations_localauthority,\
    Configurations.Configurations_connectionfilesfolder,\
    Configurations.Configurations_sdeworkspace)

    #Output path for connSDE
    msg=  "\n"+connSDE+"\n"
    logger.info("ConnSDE path : "+msg)
    #print msg

    # Get Connection to the geodatabase as the data owner
    connGISADMIN = os.path.join(currDir,\
    Configurations.Configurations_localauthority,\
    Configurations.Configurations_connectionfilesfolder,\
    Configurations.Configurations_gisadminworkspace)

    # Get path to reconcile log file path
    reconcilelogfile =   os.getcwd() + "\\"+ Configurations.Configurations_localauthority +"\\"+\
     Configurations.Configurations_localauthority+"_" + Configurations.Configurations_reconcilelogfile

    dotnetReconcileLogFile=arcpy.GetParameterAsText(1)#Get from console or GUI being user input passed from .Net application
    if dotnetReconcileLogFile is not null: #Checks if supplied parameter is null
        #Get path from system argument for location of reconcile log file
        reconcilelogfile = dotnetReconcileLogFile

    #output location of reconcile log file
    logger.info("\n Reconcile Log File Path : "+ reconcilelogfile)

    # set the workspace
    env.workspace = connSDE

    #workspace variable
    wrkspc = env.workspace

    ##--------------------------------------------------
    ##-----Allow connections to the geodatabase. Previuos code executions code have
    ##-------executed unsuccessfully leading to the fact that the geodatabase is a state that cannot accept connections
    ##-------Ensures no manual intervention via creationof geodatabase connection as DBA to allow connections is NEEDED
    ##----------------------------------------------------------

    #Accept new connections to the database.
    arcpy.AcceptConnections(connSDE, True)

    ##---
    ##---Find Connected Users
    ##---
    # get a list of connected users
    userList = arcpy.ListUsers(connSDE)

    ##---
    ##---Pass the list of connected users
    ##---

        ##    # get a list of user names from the list of named tuples returned from ListUsers
        ##    userNames = [u.Name for u in userList]
        ##
        ##    # take the userNames list and make email addresses by appending the appropriate suffix.
        ##    emailList = [name +  '@company.com' for name in userNames]


    ##---
    ##---Generate and send an E-Mail
    ##---

        ##    import smtplib
        ##    SERVER = "mailserver.yourcompany.com"
        ##    FROM = "SDE Admin <python@yourcompany.com>"
        ##    TO = emailList
        ##    SUBJECT = "Maintenance is about to be performed"
        ##    MSG = "Auto generated Message.\n\rServer maintenance will be performed in 15 minutes. Please log off."
        ##
        ##    # Prepare actual message
        ##    MESSAGE = """\
        ##    From: %s
        ##    To: %s
        ##    Subject: %s
        ##
        ##    %s
        ##    """ % (FROM, ", ".join(TO), SUBJECT, MSG)
        ##
        ##    # Send the mail
        ##    server = smtplib.SMTP(SERVER)
        ##    server.sendmail(FROM, TO, MESSAGE)
        ##    server.quit()

    ##---
    ##---Block connections to the geodatabase
    ##---

    #block new connections to the database.
    arcpy.AcceptConnections(connSDE, False)

    ##---
    ##---Pause the script for a minute
    ##---
    import time
    time.sleep(10)#time is specified in seconds


    ##---
    ##---Disconnect users
    ##---

    #disconnect all users from the database.
    arcpy.DisconnectUser(connSDE, "ALL")

    ##Batch Reconcile versions and post changes

    # Get a list of versions to pass into the ReconcileVersions tool.
    versionList = arcpy.ListVersions(connSDE)

    # Execute the ReconcileVersions tool.
    arcpy.ReconcileVersions_management(connSDE, "ALL_VERSIONS", "sde.DEFAULT", versionList, "LOCK_ACQUIRED", "NO_ABORT", "BY_OBJECT", "FAVOR_TARGET_VERSION", "POST", "DELETE_VERSION", reconcilelogfile)

    ##-----------------COMPRESS---------------------------------
    ##-----Compress the Geodatabase
    ##----------------------------------------------------------

    # Run the compress tool.
    arcpy.Compress_management(connSDE)

    ##--------------------------------------------------
    ##-----Allow connections to the geodatabase
    ##----------------------------------------------------------

    #Accept new connections to the database.
    arcpy.AcceptConnections(connSDE, True)


    ##--------------------------------------------------------------------------------------
    ##-----Rebuild indexes  and update statistics ------------------------------------------
    ##--------------------------------------------------------------------------------------
    ##--------------------------------------------------------------------------------------

    ## Rebuild indices and update statistics for SDE user

    # set the workspace
    env.workspace = connSDE

    # Get the user name for the workspace
    # this assumes you are using database authentication.
    # OS authentication connection files do not have a 'user' property.
    userName = arcpy.Describe(env.workspace).connectionProperties.user

    # Get a list of all the datasets the user has access to.
    # First, get all the stand alone tables, feature classes and rasters owned by the current user.
    dataList = arcpy.ListTables('*.' + userName + '.*') + arcpy.ListFeatureClasses('*.' + userName + '.*') + arcpy.ListRasters('*.' + userName + '.*')

    # Next, for feature datasets owned by the current user
    # get all of the featureclasses and add them to the master list.
    for dataset in arcpy.ListDatasets('*.' + userName + '.*'):
        dataList += arcpy.ListFeatureClasses(feature_dataset=dataset)


    # Execute rebuild indexes and analyze datasets
    # Note: to use the "SYSTEM" option, the user must be an administrator.

    #workspace = "Database Connections/user1.sde"

    arcpy.RebuildIndexes_management(connSDE, "SYSTEM", dataList, "ALL")

    arcpy.AnalyzeDatasets_management(connSDE, "SYSTEM", dataList, "ANALYZE_BASE", "ANALYZE_DELTA", "ANALYZE_ARCHIVE")

    ###-------------------------------------------------------------
    ### Rebuild indices and update statistics for second data owner gisadmin user
    ###-------------------------------------------------------------

    # set the workspace
    env.workspace = connGISADMIN

    # Get the user name for the workspace
    # this assumes you are using database authentication.
    # OS authentication connection files do not have a 'user' property.
    userName = arcpy.Describe(env.workspace).connectionProperties.user

    # Get a list of all the datasets the user has access to.
    # First, get all the stand alone tables, feature classes and rasters owned by the current user.
    dataList = arcpy.ListTables('*.' + userName + '.*') + arcpy.ListFeatureClasses('*.' + userName + '.*') + arcpy.ListRasters('*.' + userName + '.*')

    # Next, for feature datasets owned by the current user
    # get all of the featureclasses and add them to the master list.
    for dataset in arcpy.ListDatasets('*.' + userName + '.*'):
        dataList += arcpy.ListFeatureClasses(feature_dataset=dataset)


    # Execute rebuild indexes and analyze datasets
    # Note: to use the "SYSTEM" option, the user must be an administrator.

    #workspace = "Database Connections/user1.sde"

    arcpy.RebuildIndexes_management(connGISADMIN, "NO_SYSTEM", dataList, "ALL")

    arcpy.AnalyzeDatasets_management(connGISADMIN, "NO_SYSTEM", dataList, "ANALYZE_BASE", "ANALYZE_DELTA", "ANALYZE_ARCHIVE")

    ##---------END--------------------------------------------------------------------------
    ##---------End of Rebuild indexes  and update statistics -------------------------------
    ##--------------------------------------------------------------------------------------
    ##--------------------------------------------------------------------------------------

    msg ="\nExited with Spectacular Success \n------------------------------------------------------------------\n"+"End Time : " + datetime.now().strftime("-%y-%m-%d_%H-%M-%S")+ "\n------------------------------------------------------------------\n"

    print msg
    #Logging
    logger.info(msg)
except:
    ## Return any Python specific errors and any error returned by the geoprocessor
    ##
    tb = sys.exc_info()[2]
    tbinfo = traceback.format_tb(tb)[0]
    pymsg = "PYTHON ERRORS:\nTraceback Info:\n" + tbinfo + "\nError Info:\n    " + \
            str(sys.exc_type)+ ": " + str(sys.exc_value) + "\n"
    msgs = "Geoprocessing  Errors :\n" + arcpy.GetMessages(2) + "\n"

    ##Add custom informative message to the Python script tool
    arcpy.AddError(pymsg) #Add error message to the Python script tool(Progress dialog box, Results windows and Python Window).
    arcpy.AddError(msgs)  #Add error message to the Python script tool(Progress dialog box, Results windows and Python Window).

    ##For debugging purposes only
    ##To be commented on python script scheduling in Windows _log
    print pymsg
    print "\n" +msgs
    logger.info("autoReconcileAndPost.py "+pymsg)
    logger.info("autoReconcileAndPost.py "+msgs)
    msg ="\nFailed \n------------------------------------------------------------------\n"+"End Time : " + datetime.now().strftime("-%y-%m-%d_%H-%M-%S")+ "\n------------------------------------------------------------------\n"
    print msg

    #Logging
    logger.info(msg)
