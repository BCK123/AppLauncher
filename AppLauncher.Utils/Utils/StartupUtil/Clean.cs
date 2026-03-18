
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppLauncher.Utils.Utils.StartupUtil
{
    public class Clean
    {
     
        public static void DeleteImageCache()
        { 
          
            //  删除temp 下图片 包含 baidetect字符的图像
            try
            {
                

                Directory.EnumerateFiles(Path.GetTempPath())
                   .Where(f => Path.GetFileName(f).Contains("bai_detect", StringComparison.OrdinalIgnoreCase))
                   .ToList()
                   .ForEach(file => { try { File.Delete(file); } catch { } });

            }
            catch {
            
            }
        }
    }
}
