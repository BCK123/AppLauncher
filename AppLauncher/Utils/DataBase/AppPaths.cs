using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppLauncher.Utils.DataBase
{
    public static class AppPaths
    {
        public static string AppDir =>
            Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "AppLauncher");

        public static string DbPath =>
            Path.Combine(AppDir, "data.db");
    }
}
