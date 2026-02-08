# Patient Synchronization

The solution was created using **VisualStudio 2026** and **.NET 8**.
This system features a background **Windows Worker Service** (WorkerService project) and a **WPF Application** (WpfApp project).


## 🛠 Setup & Configuration

### 1. Database Connection
The application uses **EF Core** with a "Code-First" approach. 

* **Tested & Recommended:** The app was tested with **SQL LocalDB**, and as such, it is recommended to use SQL LocalDB for the best experience. 
* **Configuration:** You can check or edit the `ConnectionString` in the `appsettings.json` file (located in the **WorkerService** project and also linked to the **WpfApp** project).
> [!NOTE]  
> If you choose to use **LocalDB**, you can typically keep the default `appsettings.json` unchanged.


---

## 🚀 How to Run the Application

You can run this solution in two ways depending on whether you are developing/debugging or testing the full Windows Service integration.

---

### Option A: Running via Visual Studio (Development)
*Ideal for testing logic and reviewing code without modifying Windows system settings.*

#### Run both apps in VisualStudio
1. In Visual Studio, start the **WorkerService** project first, then start the **WpfApp** project (right-click each project and select Debug → Start New Instance)

**Limitation:** In this mode, only the Pause operation is available. Start and Stop will not work because the service is not registered. All other functionality works as expected.
**Note:** View real-time logs in the VisualStudio Output window and WorkerService Console app.

---

### Option B: Full Service Installation (Production Mode)
*Use this method to test the full management lifecycle (Start, Stop, and Pause).*

#### 1. Build and Publish
1. Build **WorkerService** project in Release mode.
2. Right-click the **WorkerService** project and select **Publish**.
3. Target a local folder (e.g., `C:\Services\PatientSync`) and click **Publish**.

#### 2. Registration
1. Open **PowerShell (as Administrator)** and run:
   ```powershell
   sc.exe create "PatientSyncService" binpath= "C:\Services\PatientSync\YourService.exe" start= auto

#### 3. Permissions (Critical for LocalDB)
If you use SQL LocalDB instances as recommended, the service must run under your user credentials:
1. Open **Services.msc** > Find **PatientSyncService** > **Properties** > **Log On** tab.
2. Select **This account**, enter your Windows username and password, and click **OK**.
3. **Restart** the service to apply changes.

#### 4. Run the WpfApp 
1. Navigate to your WpfApp project output folder and launch the **WpfApp.exe as Administrator** (required to control Windows Services).
2. The WpfApp will now show the real-time status and allow you to (**Start/Stop/Pause**) service.

---

## 📅 Synchronization Schedules

The system uses the standard **5-field Unix Cron format**:

> **Immediate Updates:** The Worker Service polls the database every minute. Changes made in the WpfApp (paths, schedules, or pausing) are picked up service without a restart.
