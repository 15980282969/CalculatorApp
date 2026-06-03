using System;
using System.Windows;

namespace CalcPad.UI
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            
            // 单实例检测
            bool createdNew;
            var mutex = new System.Threading.Mutex(true, "CalcPad_SingleInstance", out createdNew);
            
            if (!createdNew)
            {
                MessageBox.Show("计算稿纸已在运行中!", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                Shutdown();
                return;
            }
        }
    }
}
