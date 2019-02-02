namespace AlphaMosaik.Logger.WindowsService
{
    partial class ProjectInstaller
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.serviceProcessInstaller1 = new System.ServiceProcess.ServiceProcessInstaller();
            this.serviceInstaller1 = new System.ServiceProcess.ServiceInstaller();
            this.eventLogInstaller1 = new System.Diagnostics.EventLogInstaller();
            this.performanceCounterInstaller1 = new System.Diagnostics.PerformanceCounterInstaller();
            // 
            // serviceProcessInstaller1
            // 
            this.serviceProcessInstaller1.Password = null;
            this.serviceProcessInstaller1.Username = null;
            // 
            // serviceInstaller1
            // 
            this.serviceInstaller1.DisplayName = "Alphamosaik Logger Service";
            this.serviceInstaller1.ServiceName = "AlphaMosaikLogger";
            // 
            // eventLogInstaller1
            // 
            this.eventLogInstaller1.CategoryCount = 0;
            this.eventLogInstaller1.CategoryResourceFile = null;
            this.eventLogInstaller1.Log = "AlphaMosaik";
            this.eventLogInstaller1.MessageResourceFile = null;
            this.eventLogInstaller1.ParameterResourceFile = null;
            this.eventLogInstaller1.Source = "AlphaMosaik Logger";
            // 
            // performanceCounterInstaller1
            // 
            this.performanceCounterInstaller1.CategoryHelp = "Counters of AlphaMosaik Logger";
            this.performanceCounterInstaller1.CategoryName = "AlphaMosaik Logger";
            this.performanceCounterInstaller1.CategoryType = System.Diagnostics.PerformanceCounterCategoryType.SingleInstance;
            this.performanceCounterInstaller1.Counters.AddRange(new System.Diagnostics.CounterCreationData[] {
            new System.Diagnostics.CounterCreationData("QueueSize", "Number of items waiting to be treated", System.Diagnostics.PerformanceCounterType.NumberOfItems32)});
            // 
            // ProjectInstaller
            // 
            this.Installers.AddRange(new System.Configuration.Install.Installer[] {
            this.serviceProcessInstaller1,
            this.serviceInstaller1,
            this.eventLogInstaller1,
            this.performanceCounterInstaller1});

        }

        #endregion

        private System.ServiceProcess.ServiceProcessInstaller serviceProcessInstaller1;
        private System.ServiceProcess.ServiceInstaller serviceInstaller1;
        private System.Diagnostics.EventLogInstaller eventLogInstaller1;
        private System.Diagnostics.PerformanceCounterInstaller performanceCounterInstaller1;
    }
}