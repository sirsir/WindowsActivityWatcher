# Development datails

## Environment & Dependency

### Language :: C#.NET

### IDE :: Visual Studio

### External Library ::

1. (Accord) FFMPEG :: Capture VDO

2. log4net :: main logging system

3. ini-parser :: read INI file

4. NewtonSoft Json.net :: deal with json file/string

4. ScapLIB :: Replace with pure FFMPEG

5. RabbitMQ :: Message server

6. AngularJs (maybe replace with Vue.js) :: Render HTML for notification

### Related Tools :: for test/debug
1. Ruby
   1. send message to rabbitMq server
   2. 
2. Php ::
   1. let server receive/serve VDO
   2. medium between database and main program
3. HTML5/CSS/Javascript ::
   1. View for Notification
   2. View for captured VDO
4. MySQL ::
   1. Database for Computer/Windows Activity

5 nginx-rtmp-docker :: View for captured VDO 
(request to php to get VDOs based-on intervals by shell script 
browser--vdo start/stop-->
php-->bash script
[ list VDOs >> Join to one VDO >> stream ]
--stream path-->browser



### Application Type :: Systray Application

-   Application that dont have main windows but with systray icon
-   **Note:** Service app can not has systray or any GUI interface

# Functions

## Desktop Activity Monitor

### Logon/Logoff monitor

1.  When clients turn on/off their PC, PC details will be submitted/recorded on the server

2.  PC details ::

    -   IP
    -   Login Name
    -   Mac address
    -   Computer Name
    -   Domain Name
    -   OS info

### Window Active Monitor

1.  Record client's openning window details and submit it to the server

2.  Window Details ::

    -   window<sub>title</sub>
    -   process.exe
    -   process name
    -   times: Activated time and duration

## Desktop Screen Record

### Capture the client desktop, record it and submit/record in the server

## Notification
1. Receive the message from RabbitMQ server, then popup the toast notification

2. Notification behavior is based on its type:
   - notice
   - question
   - warning
   - error
   - success
   - custom :: can customize background color etc.

3. It is customizable and it can be simulate via Configuration window




# Developer Notes

## Programming Flows

### Check if program already run or not, if not then start program.

### Initialize and show icon on notification (tray) bar

-   **Menu:** -   Open log file
    -   Open configuration file
    -   Open log configuration file
    -   Version dialog (+Computer Info)
	-   Open Notification Configuration Windows

### Load configuration files

1.  sources (priority from highest to lowest order)

    -   configuration.txt from server (url from registry)
    -   configuration.txt in Users\\{{username}}\AppData\Local\AmivoiceWatcher load from the first one
    -   AmivoiceWatcher.ini in same folder as AmivoiceWatcher.exe
    -   Hardcode in Globals.cs

### Intitialize and run threads/for each subfunctions

1. Submit computer info on program start event

2. Submit computer info on program close event (include sitution when PC is logoff or turnoff)

3. Start threads to monitor client windows activity

4. Start threads to capture client screen

5. Start thread to deal with notification

   1) firstly it will, create a queue in RabbitMQ. 
!!! if the running PC with specific user never run the program, there will be no queue and no message.


## References
### https://docs.google.com/presentation/d/1oPrsfv3cKnfPctTa17u_IUvGMjvGTx7z5cYHBQyw4VQ/edit?ts=57e8f231#slide=id.p

 
