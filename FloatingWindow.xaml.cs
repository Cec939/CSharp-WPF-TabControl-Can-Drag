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
using System.Windows.Shapes;

namespace WpfApp1
{
    /// <summary>
    /// FloatingWindow.xaml 的交互逻辑
    /// </summary>
    public partial class FloatingWindow : Window
    {
        public TabItem OriginalTabItem { get; private set; }
        public object TabContent { get; private set; }
        public object TabHeader { get; private set; }

        public FloatingWindow(TabItem tabItem)
        {
            InitializeComponent();

            OriginalTabItem = tabItem;
            TabContent = tabItem.Content;
            TabHeader = tabItem.Header;

            // 创建内容控件
            var contentControl = new ContentControl();
            contentControl.Content = TabContent;
            Content = contentControl;

            Title = TabHeader?.ToString() ?? "Floating Window";
            Width = 300;
            Height = 200;

            // 设置拖拽行为
            this.MouseMove += FloatingWindow_MouseMove;
            this.Closing += FloatingWindow_Closing;

            // 允许内容区域拖拽
            contentControl.PreviewMouseLeftButtonDown += ContentControl_PreviewMouseLeftButtonDown;
            contentControl.PreviewMouseMove += ContentControl_PreviewMouseMove;
        }

        private Point _dragStartPoint;

        private void ContentControl_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _dragStartPoint = e.GetPosition(this);
        }

        private void ContentControl_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                var currentPoint = e.GetPosition(this);
                if (Math.Abs(currentPoint.X - _dragStartPoint.X) > SystemParameters.MinimumHorizontalDragDistance ||
                    Math.Abs(currentPoint.Y - _dragStartPoint.Y) > SystemParameters.MinimumVerticalDragDistance)
                {
                    // 准备拖拽数据
                    var data = new DataObject();
                    data.SetData("TabContent", TabContent);
                    data.SetData("TabHeader", TabHeader);
                    data.SetData("FromFloatingWindow", true);

                    // 开始拖拽
                    DragDrop.DoDragDrop(this, data, DragDropEffects.Move);
                }
            }
        }

        private void FloatingWindow_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }

        private void FloatingWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // 窗口关闭时，将内容返回给主窗口
            var mainWindow = Application.Current.MainWindow as MainWindow;
            if (mainWindow != null)
            {
                mainWindow.ReturnTabFromFloatingWindow(this);
            }
        }
    }
}
