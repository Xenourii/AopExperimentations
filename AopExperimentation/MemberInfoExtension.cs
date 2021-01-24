using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AopExperimentation
{

    public static class MemberInfoExtension
    {
        public static string TrimName(this MemberInfo member)
            => member.Name.Split('_').Last();
        public static bool IsPropertyGetter(this MemberInfo member)
            => member.Name.StartsWith("get_", StringComparison.Ordinal);
        public static bool IsPropertySetter(this MemberInfo member)
            => member.Name.StartsWith("set_", StringComparison.Ordinal);
        public static bool IsProperty(this MemberInfo member)
            => member.IsPropertyGetter() || member.IsPropertySetter();

        public static bool IsSubscribeEvent(this MemberInfo member)
            => member.Name.StartsWith("add_", StringComparison.Ordinal);
        public static bool IsUnsubscribeEvent(this MemberInfo member)
            => member.Name.StartsWith("remove_", StringComparison.Ordinal);
        public static bool IsEvent(this MemberInfo member)
            => member.IsSubscribeEvent() || member.IsUnsubscribeEvent();
    }
}
