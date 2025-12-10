using System;
using System.Runtime.InteropServices;

namespace AppLauncher.Services
{
    public static class ShortcutResolver
    {
        // 尝试解析 .lnk 到真实 Target（使用 WScript.Shell COM）
        public static (string targetPath, string? arguments) ResolveLink(string linkPath)
        {
            try
            {
                var shellType = Type.GetTypeFromProgID("WScript.Shell");
                if (shellType == null) return (linkPath, null);
                var shell = Activator.CreateInstance(shellType);
                var shortcut = shell!.GetType().InvokeMember("CreateShortcut", System.Reflection.BindingFlags.InvokeMethod, null, shell, new object[] { linkPath });
                var targetPath = (string?)shortcut!.GetType().InvokeMember("TargetPath", System.Reflection.BindingFlags.GetProperty, null, shortcut, null);
                var arguments = (string?)shortcut.GetType().InvokeMember("Arguments", System.Reflection.BindingFlags.GetProperty, null, shortcut, null);
                return (targetPath ?? linkPath, arguments);
            }
            catch
            {
                // 回退
                return (linkPath, null);
            }
        }
    }
}
