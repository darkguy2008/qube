using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;

namespace Qube.Extensions
{
    // http://stackoverflow.com/a/8760569
    public class ElementWithContext<T>
    {
        public T Previous { get; private set; }
        public T Next { get; private set; }
        public T Current { get; private set; }

        public ElementWithContext(T current, T previous, T next)
        {
            Current = current;
            Previous = previous;
            Next = next;
        }
    }

    public static class LinqExtensions
    {
        public static IEnumerable<ElementWithContext<T>>
            WithContext<T>(this IEnumerable<T> source)
        {
            T previous = default(T);
            T current = source.FirstOrDefault();

            foreach (T next in source.Union(new[] { default(T) }).Skip(1))
            {
                yield return new ElementWithContext<T>(current, previous, next);
                previous = current;
                current = next;
            }
        }

        // http://developmentpassion.blogspot.com/2015/03/how-to-convert-list-to-datatable-in-c.html
        public static DataTable ToDataTable<T>(this List<T> iList)
        {
            DataTable dataTable = new DataTable();
            PropertyDescriptorCollection propertyDescriptorCollection = TypeDescriptor.GetProperties(typeof(T));
            for (int i = 0; i < propertyDescriptorCollection.Count; i++)
            {
                PropertyDescriptor propertyDescriptor = propertyDescriptorCollection[i];
                Type type = propertyDescriptor.PropertyType;

                if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
                    type = Nullable.GetUnderlyingType(type);

                dataTable.Columns.Add(propertyDescriptor.Name, type);
            }
            object[] values = new object[propertyDescriptorCollection.Count];
            foreach (T iListItem in iList)
            {
                for (int i = 0; i < values.Length; i++)
                    values[i] = propertyDescriptorCollection[i].GetValue(iListItem);
                dataTable.Rows.Add(values);
            }
            return dataTable;
        }
    }
}
