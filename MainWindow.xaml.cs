using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfApp1
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {


            InitializeComponent();
            MainTabControl.AllowDrop = true;

            // 注册事件
            MainTabControl.DragOver += MainTabControl_DragOver;
            MainTabControl.Drop += MainTabControl_Drop;
        }

        private void Window_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent("IsFromTabControl"))
            {
                // 显示拖拽提示
                DragHint.Visibility = Visibility.Visible;
                e.Effects = DragDropEffects.Copy;
                e.Handled = true;
            }
            else
            {
                DragHint.Visibility = Visibility.Collapsed;
            }
        }

        private void Window_Drop(object sender, DragEventArgs e)
        {
            DragHint.Visibility = Visibility.Collapsed;

            if (e.Data.GetDataPresent("IsFromTabControl"))
            {
                var tabItem = e.Data.GetData("TabItem") as TabItem;
                var sourceTabControl = e.Data.GetData("SourceTabControl") as TabControl;
                var content = e.Data.GetData("TabContent");
                var header = e.Data.GetData("TabHeader");

                if (sourceTabControl != null && tabItem != null)
                {
                    // 创建浮动窗口
                    CreateFloatingWindow(tabItem, e.GetPosition(this));

                    // 设置拖拽结果为Copy，表示创建了浮动窗口
                    e.Effects = DragDropEffects.Copy;
                    StatusText.Text = "已创建浮动窗口";
                }

                e.Handled = true;
            }
        }

        private void CreateFloatingWindow(TabItem tabItem, Point position)
        {
            var floatingWindow = new FloatingWindow(tabItem)
            {
                Left = position.X - 50, // 稍微偏移，避免完全遮挡
                Top = position.Y - 50,
                Owner = this
            };
            floatingWindow.Show();
        }

        public void ReturnTabFromFloatingWindow(FloatingWindow floatingWindow)
        {
            var newTabItem = new TabItem
            {
                Header = floatingWindow.TabHeader,
                Content = floatingWindow.TabContent
            };

            MainTabControl.Items.Add(newTabItem);
            MainTabControl.SelectedItem = newTabItem;

            StatusText.Text = "已从浮动窗口恢复选项卡";
        }

        // 当拖拽离开窗口时隐藏提示
        private void Window_DragLeave(object sender, DragEventArgs e)
        {
            DragHint.Visibility = Visibility.Collapsed;
        }

        // 在主窗口类中添加以下方法
        private void MainTabControl_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent("FromFloatingWindow"))
            {
                e.Effects = DragDropEffects.Move;
                e.Handled = true;
            }
        }

        private void MainTabControl_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent("FromFloatingWindow"))
            {
                var content = e.Data.GetData("TabContent");
                var header = e.Data.GetData("TabHeader");

                var newTabItem = new TabItem
                {
                    Header = header,
                    Content = content
                };

                MainTabControl.Items.Add(newTabItem);
                MainTabControl.SelectedItem = newTabItem;

                // 关闭源浮动窗口
                var floatingWindow = FindFloatingWindowWithContent(content);
                floatingWindow?.Close();

                StatusText.Text = "已从浮动窗口拖回选项卡";
                e.Handled = true;
            }
        }

        private FloatingWindow FindFloatingWindowWithContent(object content)
        {
            foreach (Window window in Application.Current.Windows)
            {
                if (window is FloatingWindow floatingWindow && floatingWindow.TabContent == content)
                {
                    return floatingWindow;
                }
            }
            return null;
        }
    }
}
