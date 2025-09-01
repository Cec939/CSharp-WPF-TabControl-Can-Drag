using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace WpfApp1
{
    public class DockableTabControl : TabControl
    {
        static DockableTabControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DockableTabControl),
                new FrameworkPropertyMetadata(typeof(TabControl)));
        }

        public DockableTabControl()
        {
            AllowDrop = true;
            DragOver += OnDragOver;
            Drop += OnDrop;
        }

        private void OnDragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent("TabItem"))
            {
                e.Effects = DragDropEffects.Move;
                e.Handled = true;
            }
        }

        private void OnDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent("TabItem"))
            {
                var tabItem = e.Data.GetData("TabItem") as TabItem;
                var sourceTabControl = FindParentTabControl(tabItem);

                if (sourceTabControl != this)
                {
                    // 从源TabControl移除
                    var content = tabItem.Content;
                    sourceTabControl.Items.Remove(tabItem);

                    // 添加到当前TabControl
                    var newTabItem = new TabItem
                    {
                        Header = tabItem.Header,
                        Content = content
                    };
                    Items.Add(newTabItem);
                    SelectedItem = newTabItem;
                }
                e.Handled = true;
            }
        }

        private TabControl FindParentTabControl(DependencyObject child)
        {
            while (child != null)
            {
                if (child is TabControl tabControl) return tabControl;
                child = VisualTreeHelper.GetParent(child);
            }
            return null;
        }
    }
}
