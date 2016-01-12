================================================================================
================================================================================
Author:    David Warui Muthami
Date:      2016-01-12
Copyright: TBL 2015
Module:    ULIMS GIS Synch Service Code
Website:   https://www.technobraingroup.com
================================================================================
README.txt
================================================================================

Location in code base
================================================================================

    Code/ULIMSGISService/README.txt


Dependencies for Windows Operating System
================================================================================

    * You need the "Visual Studio 2013 or later installed" 
	
	* You need the "SQL Server 2012 Standard Edition or higher" 
	
    * You need to have added the directory where "python" resides to your
      PATH environmental variable. 
	  
	  e.g. C:\Python27\ArcGIS10.3
	  
	*	Test python by launching command prompt and executing below  statement
	  
		python
		
	  * You should see the below
	  
		  ActivePython 2.7.10.12 (ActiveState Software Inc.) based on
			Python 2.7.10 (default, Aug 21 2015, 14:42:12) [MSC v.1500 32 bit (Intel)] on wi
			n32
			Type "help", "copyright", "credits" or "license" for more information.
			>>>
	  
	*	Test arcpy after testing python above by running the below statemets in the console
	
			Microsoft Windows [Version 10.0.10240]
			(c) 2015 Microsoft Corporation. All rights reserved.

			C:\Users\dmuthami>python
			ActivePython 2.7.10.12 (ActiveState Software Inc.) based on
			Python 2.7.10 (default, Aug 21 2015, 14:42:12) [MSC v.1500 32 bit (Intel)] on wi
			n32
			Type "help", "copyright", "credits" or "license" for more information.
			>>> import arcpy
			>>>

    * You need a valid license for ArcGIS for Desktop 10.3.*
    
    * The 10 Databases for the 10 towns need to be installed and configured
 

Windows Operating System Instructions for compiling and running application
================================================================================

To run the code launch Visual stusio 2013 and open the solution file in the directory :

    Code/ULIMSGISService/ULIMSGISService.sln
    
Clean, build and rebuild both debug and release. Check for any errors and resolve. Upon successful build then 	

    
Copy the rellease folder into a location of your choice and rename it to deploy

  e.g. ../Code/ULIMSGISService/ULIMSGISService/deploy

Launch Developer Command prompt for Visual studio 2012 in administrator mode

Change directory to the location of above "deploy" folder

 e.g C:\Program Files (x86)\Microsoft Visual Studio 12.0>cd /D E:\GIS Data\Namibia ULIMS\Code\ULIMSGISService\ULIMSGISService\deploy

Create windows service by executing below line statement

InstallUtil.exe "ULIMSGISService.exe"


 Observe the following log files in the "deploy" folder:
	a) .Net log file named "logfile.txt"
	b) Python log files
		* temp.log
		* <local authority name>_reconcile.log
		