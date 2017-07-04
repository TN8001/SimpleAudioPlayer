using System;
using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace SimpleAudioPlayer
{
    //雑いです 複数選択時の挙動や、スクロールしない等
    //良いライブラリがたくさんあるのでそっちを使いましょう
    public class DragAndDropBehavior
    {
        public static Type GetType(DependencyObject obj) => (Type)obj.GetValue(TypeProperty);
        public static void SetType(DependencyObject obj, Type value) => obj.SetValue(TypeProperty, value);
        public static readonly DependencyProperty TypeProperty
            = DependencyProperty.RegisterAttached("Type", typeof(Type), typeof(DragAndDropBehavior), new PropertyMetadata(null));

        public static bool GetIsEnabled(DependencyObject obj) => (bool)obj.GetValue(IsEnabledProperty);
        public static void SetIsEnabled(DependencyObject obj, bool value) => obj.SetValue(IsEnabledProperty, value);
        public static readonly DependencyProperty IsEnabledProperty
            = DependencyProperty.RegisterAttached("IsEnabled", typeof(bool), typeof(DragAndDropBehavior),
                new PropertyMetadata(false, OnIsEnabledChanged));
        private static void OnIsEnabledChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var lv = obj as ListView;
            if(lv == null) return;

            if(!(e.NewValue is bool isEnabled)) return;

            if(isEnabled)
            {
                lv.PreviewMouseLeftButtonDown += OnMouseLeftButtonDown;
                lv.PreviewMouseMove += OnMouseMove;
                lv.PreviewMouseLeftButtonUp += OnMouseLeftButtonUp;
                lv.Drop += OnDrop;
            }
            else
            {
                lv.PreviewMouseLeftButtonDown -= OnMouseLeftButtonDown;
                lv.PreviewMouseMove -= OnMouseMove;
                lv.PreviewMouseLeftButtonUp -= OnMouseLeftButtonUp;
                lv.Drop -= OnDrop;
            }
        }

        private static Point origin;
        private static bool isButtonDown;

        private static void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var lv = sender as ListView;
            var item = VisualTreeHelper.HitTest(lv, e.GetPosition(lv)).VisualHit;
            while(item != null)
            {
                if(item is ListViewItem) break;
                item = VisualTreeHelper.GetParent(item);
            }
            if(item == null) return;

            origin = e.GetPosition(lv);
            isButtonDown = true;
            return;
        }
        private static void OnMouseMove(object sender, MouseEventArgs e)
        {
            if(e.LeftButton != MouseButtonState.Pressed || !isButtonDown) return;

            var lv = sender as ListView;
            var point = e.GetPosition(lv);
            if(!CheckDistance(point, origin)) return;

            DragDrop.DoDragDrop(lv, lv.SelectedItem, DragDropEffects.Copy);
            isButtonDown = false;
            e.Handled = true;
        }
        private static void OnDrop(object sender, DragEventArgs e)
        {
            var lv = sender as ListView;

            var newIndex = GetItemIndex(lv, e.GetPosition(lv));
            var source = e.Data.GetData(GetType(lv));
            var list = lv.ItemsSource as IList;

            if(newIndex != -1 && source != null)
            {
                list?.Remove(source);
                list?.Insert(newIndex, source);
            }
        }
        private static void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
            => isButtonDown = false;
        private static bool CheckDistance(Point x, Point y)
            => Math.Abs(x.X - y.X) >= SystemParameters.MinimumHorizontalDragDistance
            || Math.Abs(x.Y - y.Y) >= SystemParameters.MinimumVerticalDragDistance;
        private static int GetItemIndex(ListView listView, Point pos)
        {
            var item = VisualTreeHelper.HitTest(listView, pos).VisualHit;

            while(item != null)
            {
                if(item is ListViewItem) break;
                item = VisualTreeHelper.GetParent(item);
            }

            if(item != null)
                return listView.Items.IndexOf(((ListViewItem)item).Content);

            return -1;
        }
    }
}
