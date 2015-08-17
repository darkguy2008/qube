using System;

namespace Qube.Web.UI
{
    public interface IQubeFormField
    {
        String FieldName { get; set; }
        String DataField { get; set; }
        void SetValue(object v);
        T GetValue<T>();
    }
}