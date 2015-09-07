using System;

namespace Qube.Web.UI
{
    public interface IQubeFormField
    {
        String FieldName { get; set; }
        String DataField { get; set; }
        String DataFormatString { get; set; }
        void SetValue(object v);
        T GetValue<T>();
        String GetFormattedValue();
    }
}