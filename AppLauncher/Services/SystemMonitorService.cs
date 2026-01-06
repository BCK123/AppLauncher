using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Timers;
using System.Windows;

namespace AppLauncher.Services
{
    public class SystemMonitorService : INotifyPropertyChanged, IDisposable
    {
        private PerformanceCounter? _cpuCounter;
        private Timer? _timer;

        private double _cpuUsage;
        public double CpuUsage
        {
            get => _cpuUsage;
            private set
            {
                if (Math.Abs(_cpuUsage - value) > 0.1)
                {
                    _cpuUsage = value;
                    OnPropertyChanged(nameof(CpuUsage));
                }
             }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// 构造函数不做重活
        /// </summary>
        public SystemMonitorService() { }

        /// <summary>
        /// 显式启动监控（后台）
        /// </summary>
        public void Start()
        {
            // 后台初始化（避免启动卡顿）
            System.Threading.Tasks.Task.Run(() =>
            {
                _cpuCounter = new PerformanceCounter(
                    "Processor Information",
                    "% Processor Utility",
                    "_Total");

                _cpuCounter.NextValue(); // 预热

                _timer = new Timer(1000); // 1 秒刷新
                _timer.Elapsed += (_, __) => UpdateCpu();
                _timer.Start();
            });
        }

        private void UpdateCpu()
        {
            if (_cpuCounter == null) return;

            double value = _cpuCounter.NextValue();

            // ⚠ 回到 UI 线程更新属性
            Application.Current.Dispatcher.Invoke(() =>
            {
                CpuUsage = value;
            });
        }

        protected void OnPropertyChanged(string name)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        public void Dispose()
        {
            _timer?.Stop();
            _timer?.Dispose();
            _cpuCounter?.Dispose();
        }
    }
}
