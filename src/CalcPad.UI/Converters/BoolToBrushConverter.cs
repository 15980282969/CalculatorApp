using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace CalcPad.UI.Converters
{
    /// <summary>
    /// 布尔值到画笔转换器
    /// 用于错误结果标红显示
    /// </summary>
    public class BoolToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isError && isError)
            {
                return Brushes.IndianRed; // 错误显示红色
            }
            return Brushes.White; // 正常显示白色
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
