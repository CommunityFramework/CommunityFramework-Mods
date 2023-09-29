using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
public static class Access
{
    public static T GetPrivateField<T>(this object obj, string name)
    {
        BindingFlags bindingAttr = BindingFlags.Instance | BindingFlags.NonPublic;
        Type type = obj.GetType();
        FieldInfo field = type.GetField(name, bindingAttr);
        return (T)(field.GetValue(obj));
    }
    public static void SetPrivateField(this object obj, string name, object value)
    {
        BindingFlags bindingAttr = BindingFlags.Instance | BindingFlags.NonPublic;
        Type type = obj.GetType();
        FieldInfo field = type.GetField(name, bindingAttr);
        field.SetValue(obj, value);
    }
    public static void SetPublicField(this object obj, string name, object value)
    {
        BindingFlags bindingAttr = BindingFlags.Instance | BindingFlags.Public;
        Type type = obj.GetType();
        FieldInfo field = type.GetField(name, bindingAttr);
        field.SetValue(obj, value);
    }
    public static void CallPrivateMethod(this object obj, string name, params object[] param)
    {
        BindingFlags bindingAttr = BindingFlags.Instance | BindingFlags.NonPublic;
        Type type = obj.GetType();
        MethodInfo method = type.GetMethod(name, bindingAttr);
        method.Invoke(obj, param);
    }
}
