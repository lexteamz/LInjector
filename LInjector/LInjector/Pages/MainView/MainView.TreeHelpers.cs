using System.Windows;
using System.Windows.Media;

namespace LInjector.Pages
{
    public partial class MainView
    {
        public static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                    if (child != null && child is T)
                    {
                        yield return (T)child;
                    }

                    foreach (T childOfChild in FindVisualChildren<T>(child!))
                    {
                        yield return childOfChild;
                    }
                }
            }
        }

        public static IEnumerable<T> FindLogicalChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                foreach (object childObj in LogicalTreeHelper.GetChildren(depObj))
                {
                    DependencyObject? child = childObj as DependencyObject;

                    if (child != null && child is T)
                        yield return (T)child;

                    if (child != null)
                        foreach (T childOfChild in FindLogicalChildren<T>(child))
                            yield return childOfChild;
                }
            }
        }

        private IEnumerable<T> FindDescendants<T>(DependencyObject parent) where T : DependencyObject
        {
            if (parent == null) yield break;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);

                if (child is T descendant)
                    yield return descendant;

                foreach (var descendantChild in FindDescendants<T>(child))
                    yield return descendantChild;
            }
        }
    }
}