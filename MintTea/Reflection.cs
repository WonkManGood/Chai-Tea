using BepInEx.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MintTea {
    class Reflection {
        public static ManualLogSource Logger { get; set; }

        public static T Access<T>(object o, string field) {
            FieldInfo fieldInfo = o.GetType().GetField(field, BindingFlags.NonPublic | BindingFlags.GetField | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
            if (fieldInfo == null)
                Logger?.LogError("Unrecognized field " + field + " in class " + o.GetType().FullName);
            T value = (T)fieldInfo.GetValue(o);
            if (value == null) {
                Logger?.LogWarning("Value was null for field " + field + " in class " + o.GetType().FullName);
            }
            return value;
        }
        public static T AccessProperty<T>(object o, Type type, string field) {
            PropertyInfo propertyInfo = type.GetProperty(field, BindingFlags.NonPublic | BindingFlags.GetProperty | BindingFlags.Instance);
            if (propertyInfo == null)
                Logger?.LogError("Unrecognized property " + field + " in class " + type.FullName);
            T value = (T)propertyInfo.GetValue(o);
            if (value == null) {
                Logger?.LogWarning("Value was null for property " + field + " in class " + type.FullName);
            }
            return value;
        }

        public static void Set(object o, string field, object value) {
            FieldInfo fieldInfo = o.GetType().GetField(field, BindingFlags.NonPublic | BindingFlags.GetField | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
            fieldInfo.SetValue(o, value);
        }
    }
}
