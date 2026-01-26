using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppLauncher.Utils.Interface
{
    public interface IYoloInfer
    {
        string InferImage(string onnxModelPath, string srcImagePath);
    }

}
