using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AopExperimentation
{
    public class SpyLogger<T> : DispatchProxy where T : class
    {
        private T? _decorated;
        private List<string> _methodsToExcludeFromProfiling = new List<string>();
        private bool _useJsonSerializer;
        private string? _customMessage;

        public static T Create(T decorated, SpyLoggerConfiguration? config = null)
        {
            var proxy = Create<T, SpyLogger<T>>();
            (proxy as SpyLogger<T>)?.SetParameters(decorated, config);
            return proxy;
        }

        private void SetParameters(T decorated, SpyLoggerConfiguration? configuration)
        {
            _decorated = decorated;
            _methodsToExcludeFromProfiling = configuration?.MethodNamesToExclude ?? new List<string>();
            _useJsonSerializer = configuration?.UseJsonSerializer ?? true;
            _customMessage = configuration?.CustomMessage;
        }

        protected override object? Invoke(MethodInfo targetMethod, object?[] args)
        {
            try
            {
                var result = targetMethod.Invoke(_decorated, args);

                if (ShouldProfile(targetMethod))
                    LogInvokedMethod(targetMethod, args, result);

                return result;
            }
            catch (Exception ex) when (ex is TargetInvocationException)
            {
                LogException(ex.InnerException ?? ex, targetMethod);
                throw ex.InnerException ?? ex;
            }
        }

        private bool ShouldProfile(MemberInfo method)
            => !_methodsToExcludeFromProfiling.Contains(method.Name);

        private void LogInvokedMethod(MethodInfo targetMethod, object?[] args, object? result)
        {
            if (result is Task resultAsTask)
            {
                resultAsTask.ContinueWith(task =>
                {
                    var property = task.GetType()
                        .GetTypeInfo()
                        .GetProperties()
                        .FirstOrDefault(p => p.Name == "Result");

                    if (property is null)
                        return;

                    result = property.GetValue(task);
                    Log(targetMethod, args, result);
                });
            }
            else
            {
                Log(targetMethod, args, result);
            }
        }

        private void Log(MethodBase method, IReadOnlyList<object?> args, object? result = null)
        {
            if (!string.IsNullOrEmpty(_customMessage))
                Console.WriteLine(_customMessage);

            if (method.IsPropertyGetter())
                LogPropertyGet(method, result);
            else if (method.IsPropertySetter())
                LogPropertySet(method, args);
            else if (method.IsSubscribeEvent())
                LogSubscribeEvent(method, args);
            else if (method.IsUnsubscribeEvent())
                LogUnsubscribeEvent(method, args);
            else
                LogMethod(method, GetMethodArguments(method, args), result);

            Console.Write(Environment.NewLine);
        }

        private void LogPropertyGet(MemberInfo member, object? result)
            => Console.WriteLine($"Get {_decorated?.GetType().FullName}:{member.TrimName()} (returned: {FormatObject(result)})");

        private void LogPropertySet(MemberInfo member, IEnumerable<object?> args)
            => Console.WriteLine($"Set {_decorated?.GetType().FullName}:{member.TrimName()} with value: {FormatObject(args.FirstOrDefault())}");

        private void LogSubscribeEvent(MemberInfo member, IEnumerable<object?> args)
            => Console.WriteLine($"Subscribe event {_decorated?.GetType().FullName}:{member.TrimName()} with {FormatObject(args.FirstOrDefault())}");

        private void LogUnsubscribeEvent(MemberInfo member, IEnumerable<object?> args)
            => Console.WriteLine($"Unsubscribe event {_decorated?.GetType().FullName}:{member.TrimName()} with {FormatObject(args.FirstOrDefault())}");

        private void LogMethod(MemberInfo member, string arguments, object? result = null)
        {
            var formattedResult = result is null ? string.Empty : $"with result {FormatObject(result)}";
            Console.WriteLine($"Call {_decorated?.GetType().FullName}:{member.Name}{arguments} {formattedResult}");
        }

        private void LogException(Exception exception, MemberInfo methodInfo)
            => Console.WriteLine($"{_decorated?.GetType().FullName}:{methodInfo.Name} threw exception:{Environment.NewLine}{exception}");

        private string GetMethodArguments(MethodBase method, IReadOnlyList<object?> args)
        {
            var argsLength = args.Count;
            if (argsLength == 0)
                return string.Empty;

            var arguments = new StringBuilder();
            arguments.Append('(');
            var parameters = method.GetParameters();

            for (var i = 0; i < argsLength; i++)
            {
                arguments.Append(parameters[i].Name).Append(": ").Append(FormatObject(args[i]));

                if (i < argsLength - 1)
                    arguments.Append(", ");
            }

            arguments.Append(')');

            return arguments.ToString();
        }

        private string FormatObject(object? obj)
        {
            switch (obj)
            {
                case null:
                    return "null";
                case EventHandler evt:
                    return $"EventHandler(methodName={evt.Method.Name})";
            }

            if (_useJsonSerializer)
                return ConvertToJsonWithIgnoredLooping(obj);

            return obj.ToString() ?? "???";
        }

        private static string ConvertToJsonWithIgnoredLooping(object obj) =>
            JsonConvert.SerializeObject(obj, Formatting.None,
                new JsonSerializerSettings {ReferenceLoopHandling = ReferenceLoopHandling.Ignore, MaxDepth = 1});
    }
}
