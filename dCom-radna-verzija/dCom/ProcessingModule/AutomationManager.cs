using Common;
using System;
using System.Collections.Generic;
using System.Threading;

namespace ProcessingModule
{
    /// <summary>
    /// Class containing logic for automated work.
    /// </summary>
    public class AutomationManager : IAutomationManager, IDisposable
    {
        private Thread digitalWorker;
        private Thread analogWorker;
        private AutoResetEvent automationTrigger;
        private IStorage storage;
        private IProcessingManager processingManager;
        private int delayBetweenCommands;
        private IConfiguration configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="AutomationManager"/> class.
        /// </summary>
        /// <param name="storage">The storage.</param>
        /// <param name="processingManager">The processing manager.</param>
        /// <param name="automationTrigger">The automation trigger.</param>
        /// <param name="configuration">The configuration.</param>
        public AutomationManager(IStorage storage, IProcessingManager processingManager, AutoResetEvent automationTrigger, IConfiguration configuration)
        {
            this.storage = storage;
            this.processingManager = processingManager;
            this.configuration = configuration;
            this.automationTrigger = automationTrigger;
        }

        /// <summary>
        /// Initializes and starts the threads.
        /// </summary>
        private void InitializeAndStartThreads()
        {
            InitializeAutomationWorkerThread();
            StartAutomationWorkerThread();
        }

        /// <summary>
        /// Initializes the automation worker thread.
        /// </summary>
        private void InitializeAutomationWorkerThread()
        {
            digitalWorker = new Thread(DigitalWorker_DoWork);
            digitalWorker.Name = "Digital Automation Thread";

            analogWorker = new Thread(AnalogWorker_DoWork);
            analogWorker.Name = "Analog Automation Thread";
        }

        /// <summary>
        /// Starts the automation worker thread.
        /// </summary>
        private void StartAutomationWorkerThread()
        {
            digitalWorker.Start();
            analogWorker.Start();
        }

        private void DigitalWorker_DoWork()
        {
            PointIdentifier tm1 = new PointIdentifier(PointType.DIGITAL_OUTPUT, 2000);
            PointIdentifier tm2 = new PointIdentifier(PointType.DIGITAL_OUTPUT, 2001);
            PointIdentifier wm1 = new PointIdentifier(PointType.DIGITAL_OUTPUT, 2002);
            PointIdentifier wm2 = new PointIdentifier(PointType.DIGITAL_OUTPUT, 2003);

            List<PointIdentifier> digitalList = new List<PointIdentifier> { tm1, tm2, wm1, wm2 };

            while (!disposedValue)
            {
                // Očitavanje digitalnih tačaka
                List<IPoint> digitalPoints = storage.GetPoints(digitalList);

                foreach (var point in digitalPoints)
                {
                    processingManager.ExecuteReadCommand(point.ConfigItem, configuration.GetTransactionId(), configuration.UnitAddress, point.ConfigItem.StartAddress, 1);
                }

                // Čekanje 2 sekunde
                Thread.Sleep(2000);
            }
        }

        private void AnalogWorker_DoWork()
        {
            PointIdentifier sc = new PointIdentifier(PointType.ANALOG_INPUT, 3400);
            PointIdentifier lc = new PointIdentifier(PointType.ANALOG_INPUT, 3401);
            PointIdentifier hrm1 = new PointIdentifier(PointType.ANALOG_INPUT, 3402);
            PointIdentifier hrm2 = new PointIdentifier(PointType.ANALOG_INPUT, 3403);

            List<PointIdentifier> analogList = new List<PointIdentifier> { sc, lc, hrm1, hrm2 };

            while (!disposedValue)
            {
                // Očitavanje analognih tačaka
                List<IPoint> analogPoints = storage.GetPoints(analogList);

                foreach (var point in analogPoints)
                {
                    processingManager.ExecuteReadCommand(point.ConfigItem, configuration.GetTransactionId(), configuration.UnitAddress, point.ConfigItem.StartAddress, 1);
                }

                // Čekanje 4 sekunde
                Thread.Sleep(4000);
            }
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        /// <summary>
        /// Disposes the object.
        /// </summary>
        /// <param name="disposing">Indication if managed objects should be disposed.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                }
                disposedValue = true;
            }
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // GC.SuppressFinalize(this);
        }

        /// <inheritdoc />
        public void Start(int delayBetweenCommands)
        {
            this.delayBetweenCommands = delayBetweenCommands * 1000;
            InitializeAndStartThreads();
        }

        /// <inheritdoc />
        public void Stop()
        {
            Dispose();
        }
        #endregion
    }
}
