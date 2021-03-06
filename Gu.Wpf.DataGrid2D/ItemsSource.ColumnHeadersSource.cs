﻿namespace Gu.Wpf.DataGrid2D
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;

    public static partial class ItemsSource
    {
        public static readonly DependencyProperty ColumnHeadersSourceProperty = DependencyProperty.RegisterAttached(
            "ColumnHeadersSource",
            typeof(IEnumerable),
            typeof(ItemsSource),
            new PropertyMetadata(null, OnColumnHeadersSourceChanged),
#pragma warning disable WPF0007 // Name of ValidateValueCallback should match registered name.
            HeadersSourceValidateValue);
#pragma warning restore WPF0007 // Name of ValidateValueCallback should match registered name.

        private static readonly RoutedEventHandler OnColumnsChangedHandler = OnColumnsChanged;

        private static readonly DependencyProperty ColumnHeaderListenerProperty = DependencyProperty.RegisterAttached(
            "ColumnHeaderListener",
            typeof(ColumnHeaderListener),
            typeof(ItemsSource),
            new PropertyMetadata(default(ColumnHeaderListener)));

        /// <summary>
        /// Helper for setting ColumnHeadersSource property on a DataGrid.
        /// </summary>
        /// <param name="element">DataGrid to set ColumnHeadersSource property on.</param>
        /// <param name="value">ColumnHeadersSource property value.</param>
        public static void SetColumnHeadersSource(this DataGrid element, IEnumerable value)
        {
            element.SetValue(ColumnHeadersSourceProperty, value);
        }

        /// <summary>
        /// Helper for reading ColumnHeadersSource property from a DataGrid.
        /// </summary>
        /// <param name="element">DataGrid to read ColumnHeadersSource property from.</param>
        /// <returns>ColumnHeadersSource property value.</returns>
        [AttachedPropertyBrowsableForChildren(IncludeDescendants = false)]
        [AttachedPropertyBrowsableForType(typeof(DataGrid))]
        public static IEnumerable GetColumnHeadersSource(this DataGrid element)
        {
            return (IEnumerable)element.GetValue(ColumnHeadersSourceProperty);
        }

        private static void OnColumnHeadersSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var dataGrid = (DataGrid)d;
            var headers = (IEnumerable)e.NewValue;
            (dataGrid.GetValue(ColumnHeaderListenerProperty) as IDisposable)?.Dispose();
            dataGrid.ClearValue(ColumnHeaderListenerProperty);

            if (headers == null)
            {
                foreach (var column in dataGrid.Columns)
                {
                    BindingOperations.ClearBinding(column, DataGridColumn.HeaderProperty);
                }

                dataGrid.RemoveHandler(Events.ColumnsChanged, OnColumnsChangedHandler);
                return;
            }

            dataGrid.SetCurrentValue(ColumnHeaderListenerProperty, new ColumnHeaderListener(dataGrid));
            dataGrid.UpdateHandler(Events.ColumnsChanged, OnColumnsChangedHandler);
            OnColumnsChanged(dataGrid, null);
        }

        private static void OnColumnsChanged(object sender, RoutedEventArgs routedEventArgs)
        {
            var dataGrid = (DataGrid)sender;
            var headers = dataGrid.GetColumnHeadersSource();
            var count = headers.Count();
            for (int i = 0; i < Math.Min(count, dataGrid.Columns.Count); i++)
            {
                var column = dataGrid.Columns[i];
                column.Bind(DataGridColumn.HeaderProperty)
                      .OneWayTo(headers, i);
            }
        }

        private static bool HeadersSourceValidateValue(object value)
        {
            if (value == null ||
                value is IList ||
                value is IReadOnlyList<object>)
            {
                return true;
            }

            return false;
        }
    }
}
