using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace WpfApp1
{
    public static class TabDragDropBehavior
    {
        public static readonly DependencyProperty IsEnabledProperty =
            DependencyProperty.RegisterAttached("IsEnabled", typeof(bool),
            typeof(TabDragDropBehavior), new PropertyMetadata(false, OnIsEnabledChanged));

        public static bool GetIsEnabled(DependencyObject obj) => (bool)obj.GetValue(IsEnabledProperty);
        public static void SetIsEnabled(DependencyObject obj, bool value) => obj.SetValue(IsEnabledProperty, value);

        private static void OnIsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TabControl tabControl)
            {
                if ((bool)e.NewValue)
                {
                    tabControl.PreviewMouseMove += TabControl_PreviewMouseMove;
                    tabControl.PreviewMouseLeftButtonDown += TabControl_PreviewMouseLeftButtonDown;
                    tabControl.Drop += TabControl_Drop;
                    tabControl.DragOver += TabControl_DragOver;
                }
                else
                {
                    tabControl.PreviewMouseMove -= TabControl_PreviewMouseMove;
                    tabControl.PreviewMouseLeftButtonDown -= TabControl_PreviewMouseLeftButtonDown;
                    tabControl.Drop -= TabControl_Drop;
                    tabControl.DragOver -= TabControl_DragOver;
                }
            }
        }

        private static Point _startPoint;
        private static TabItem _draggedTabItem;

        private static void TabControl_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var tabControl = sender as TabControl;
            if (tabControl != null)
            {
                _startPoint = e.GetPosition(tabControl);
                _draggedTabItem = FindAncestor<TabItem>((DependencyObject)e.OriginalSource);
            }
        }

        private static void TabControl_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && _draggedTabItem != null)
            {
                var tabControl = sender as TabControl;
                var currentPoint = e.GetPosition(tabControl);

                if (Math.Abs(currentPoint.X - _startPoint.X) > SystemParameters.MinimumHorizontalDragDistance ||
                    Math.Abs(currentPoint.Y - _startPoint.Y) > SystemParameters.MinimumVerticalDragDistance)
                {
                    // 创建拖拽数据包
                    var dataPackage = new DataObject();
                    dataPackage.SetData("TabItem", _draggedTabItem);
                    dataPackage.SetData("SourceTabControl", tabControl);
                    dataPackage.SetData("TabContent", _draggedTabItem.Content);
                    dataPackage.SetData("TabHeader", _draggedTabItem.Header);
                    dataPackage.SetData("IsFromTabControl", true);

                    // 设置拖拽效果
                    var dragResult = DragDrop.DoDragDrop(_draggedTabItem, dataPackage,
                        DragDropEffects.Move | DragDropEffects.Copy);

                    // 如果拖拽到窗口外创建了浮动窗口，从源TabControl移除
                    if (dragResult == DragDropEffects.Copy && _draggedTabItem.Parent != null)
                    {
                        tabControl.Items.Remove(_draggedTabItem);
                    }

                    // 重置状态
                    _draggedTabItem = null;
                }
            }
        }

        private static void TabControl_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent("TabItem"))
            {
                e.Effects = DragDropEffects.Move;
                e.Handled = true;
            }
        }

        private static void TabControl_Drop(object sender, DragEventArgs e)
        {
            var targetTabControl = sender as TabControl;

            if (e.Data.GetDataPresent("TabItem"))
            {
                var sourceTabItem = e.Data.GetData("TabItem") as TabItem;
                var sourceTabControl = e.Data.GetData("SourceTabControl") as TabControl;
                var content = e.Data.GetData("TabContent");
                var header = e.Data.GetData("TabHeader");

                // 如果是从同一个TabControl拖拽，只是重新排序
                if (sourceTabControl == targetTabControl)
                {
                    // 计算插入位置
                    int sourceIndex = sourceTabControl.Items.IndexOf(sourceTabItem);
                    var hitTest = VisualTreeHelper.HitTest(targetTabControl, e.GetPosition(targetTabControl));
                    if (hitTest != null)
                    {
                        var targetTabItem = FindAncestor<TabItem>(hitTest.VisualHit);
                        if (targetTabItem != null)
                        {
                            int targetIndex = targetTabControl.Items.IndexOf(targetTabItem);
                            if (targetIndex != -1 && sourceIndex != targetIndex)
                            {
                                targetTabControl.Items.RemoveAt(sourceIndex);
                                targetTabControl.Items.Insert(targetIndex, sourceTabItem);
                                targetTabControl.SelectedItem = sourceTabItem;
                            }
                        }
                    }
                }
                else
                {
                    // 从其他TabControl或窗口拖拽过来
                    sourceTabControl.Items.Remove(sourceTabItem);

                    var newTabItem = new TabItem
                    {
                        Header = header,
                        Content = content,
                        Tag = sourceTabItem.Tag // 保留其他属性
                    };

                    targetTabControl.Items.Add(newTabItem);
                    targetTabControl.SelectedItem = newTabItem;
                }

                e.Handled = true;
            }
        }

        private static T FindAncestor<T>(DependencyObject current) where T : DependencyObject
        {
            while (current != null)
            {
                if (current is T ancestor) return ancestor;
                current = VisualTreeHelper.GetParent(current);
            }
            return null;
        }
    }
}