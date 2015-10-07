using System;

namespace Qube.Web.UI
{
    public interface IQubeFormField
    {
        string DisplayName { get; set; }
        string DisplayFormat { get; set; }
        string DataField { get; set; }
        string DataFormatString { get; set; }
        string OnClientValueChanged { get; set; }
        bool RenderLabel { get; set; }
        void SetValue(object v);
        T GetValue<T>();
        string GetFormattedValue();
    }
}