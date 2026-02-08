\# Patient Synchronization System (.NET 8)



A .NET 8 solution designed to synchronize patient records between \*\*SQL Server\*\* and \*\*CSV files\*\*. This system features a background \*\*Windows Worker Service\*\* for automated tasks and a \*\*WPF Management Application\*\* for real-time monitoring and configuration.



\## 🚀 Key Technical Features



\*   \*\*Clean Architecture:\*\* Strict separation of concerns across Core, Infrastructure, Persistence, and Presentation layers.

\*   \*\*High-Performance Batching:\*\* Optimized SQL generation using \*\*EF Core 8 JSON-based batching\*\* (`OPENJSON`) for bulk operations.

\*   \*\*Intelligent Sync:\*\* Data is compared at the row level; SQL `UPDATE` statements are only generated for columns with actual changes (Intelligent Update).

\*   \*\*Memory Efficiency:\*\* Implements \*\*streaming iterators\*\* for CSV parsing and \*\*database paging\*\* for exports to handle large datasets with a low RAM footprint.

\*   \*\*Scoped-in-Singleton Pattern:\*\* Uses `IServiceScopeFactory` to safely manage \*\*Scoped database contexts\*\* inside \*\*Singleton AppServices\*\*.



---



\## 🛠 Setup \& Configuration



\### 1. Database Connection

The application uses \*\*EF Core\*\* with a "Code-First" approach. 



1\.  Open `appsettings.json` (located in the \*\*Worker Service\*\* project and linked to the \*\*WPF\*\* project).

2\.  \*\*If you can use LocalDb then you can keep appsettings.json unchanged.\*\*

3\.  Otherwise, update the `ConnectionStrings:AppDbContext` to match your SQL Express instance:

&nbsp;   \*   \*\*SQL Express:\*\* `Server=.\\\\SQLEXPRESS;Database=PatientSyncDB;Trusted\_Connection=True;TrustServerCertificate=True;`



> \*\*Note:\*\* On the first launch, the application will automatically create the database `Database.EnsureCreated()`.



---



\## 🚦 Option A: Running via Visual Studio (Development)



Ideal for testing logic and reviewing code without modifying Windows system settings.



1. Just run both apps (right click on project Debug -> Start New Instance

2\.  \*\*Limitation:\*\* In this mode, the Worker Service runs as a console process. The WPF "Start/Stop" buttons and "Status" text will show \*\*"Not Installed"\*\* because the service is not registered with the Windows Service Control Manager (SCM).

3\.  \*\*Functionality:\*\* Background synchronization remains fully functional. View real-time activity in the \*\*Visual Studio Output Window\*\*.



---



\## 🏗 Option B: Full Service Installation (Production Mode)



Use this method to test the full management lifecycle (Start, Stop, and Pause).



\### 1. Build and Publish

1\.  Set the \*\*Solution Configuration\*\* to \*\*Release\*\* in the top toolbar of Visual Studio.

2\.  Right-click the \*\*Worker Service\*\* project and select \*\*Build\*\*.

3\.  Right-click the \*\*Worker Service\*\* project again and select \*\*Publish\*\*.

4\.  Target a local folder (e.g., `C:\\Services\\PatientSync`) and click \*\*Publish\*\*.



\### 2. Registration

1\.  Open \*\*PowerShell (as Administrator)\*\* and run the following command:

&nbsp;   ```powershell

&nbsp;   sc.exe create "PatientSyncService" binpath= "C:\\Services\\PatientSync\\YourService.exe" start= auto

&nbsp;   ```



\### 3. Permissions (Critical for LocalDB)

Because LocalDB instances are owned by the specific user account, the service must run under your Windows account to access the data:

1\.  Open \*\*Services.msc\*\* > Find \*\*PatientSyncService\*\* > \*\*Properties\*\* > \*\*Log On\*\* tab.

2\.  Select \*\*This account\*\*, enter your Windows credentials your username and password, and click \*\*OK\*\*.

3\.  \*\*Restart\*\* the service.



\### 4. Run the Manager

1\.  Navigate to your WPF project output folder and launch the \*\*WPF Application as Administrator\*\* (required to control Windows Services).

2\.  The dashboard will now show the real-time status (\*\*Running/Stopped/Paused\*\*).



---



\## 📅 Synchronization Schedules

The system uses the standard \*\*5-field Unix Cron format\*\*:

> \*\*Immediate Updates:\*\* The Worker Service polls the database every minute. Changes made in the WPF App (paths, schedules, or Pausing) are picked up \*\*immediately\*\* without a restart.



