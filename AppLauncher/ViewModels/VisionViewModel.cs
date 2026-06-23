using AppLauncher.Models;
using AppLauncher.Services;
using AppLauncher.Core.Log;
using AppLauncher.Utils;
using AppLauncher.Utils.Utils;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using OpenCvSharp;
using Prism.Commands;
using Prism.Mvvm;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
namespace AppLauncher.ViewModels
{
   public  class VisionViewModel : BindableBase
    {


        // ================== 1️⃣ 数据集路径 ==================
        private string _datasetPath;
        public string DatasetPath
        {
            get => _datasetPath;
            set => SetProperty(ref _datasetPath, value);
        }

        // ================== 2️⃣ 是否预处理 ==================
        private bool _inspection;
        public bool Inspection
        {
            get => _inspection;
            set => SetProperty(ref _inspection, value);
        }

        // ================== 3️⃣ 命令 ==================
        public DelegateCommand BrowseDatasetCommand { get; }
        public DelegateCommand OpenDatasetCommand { get; }

        public DelegateCommand BrowseScriptCommand { get; }
        
        public DelegateCommand BrowseImageCommand { get; }

        public DelegateCommand ProcessImageCommand { get; }

        // 1. GPU开关属性（绑定RadioButton的IsChecked） SetProperty来通知属性变更   
        private bool _isUseGpu;
        public bool IsUseGpu
        {
            get => _isUseGpu;
            set => SetProperty(ref _isUseGpu, value);

        }


        private string _scriptPath;
        public string ScriptPath
        {
            get => _scriptPath;
            set => SetProperty(ref _scriptPath, value);
        }

        private Double _processTime;
        public Double ProcessTime
        {
            get => _processTime;
            set => SetProperty(ref _processTime, value);
        }

        private String _processResult;
        public String ProcessResult
        {
            get => _processResult;
            set => SetProperty(ref _processResult, value);
        }

        private readonly ILoggerService _logger;

        public AppSettings Settings { get; private set; }

        
   

        public VisionViewModel(ILoggerService loggerService)
        {
            _logger = loggerService;
            _logger.Info("VisionModel 初始化");
            BrowseDatasetCommand = new DelegateCommand(OnBrowseDataset);
            OpenDatasetCommand = new DelegateCommand(OnOpenDataset);
            BrowseScriptCommand = new DelegateCommand(OnBrowseScript);
            BrowseImageCommand = new DelegateCommand(OnBrowseImage);
            ProcessImageCommand = new DelegateCommand(OnProcessImage);

            IsUseGpu = false;
            // 初始化 Settings（如果需要加载默认值，可以在这里调用 SettingsService.Load()）
            Settings = new AppSettings();

        }
        // ================== 4️⃣ 命令实现 ==================
        private void OnBrowseDataset()
        {
            using var dialog = new OpenFileDialog();
            dialog.Title = "请选择模型";
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                DatasetPath = dialog.FileName;
                _logger.Info("模型加载成功" + DatasetPath);
                Settings.modelpath = DatasetPath;
            }
        }

        private void OnOpenDataset()
        {
            // 这里只是示例，后面会拆出去
            if (string.IsNullOrWhiteSpace(DatasetPath))
            {
                System.Windows.MessageBox.Show("请先选择数据集路径");
                return;
            }

            System.Windows.MessageBox.Show(
                $"数据集路径：{DatasetPath}\n是否预处理：{Inspection}"
            );
        }

        private void OnBrowseScript()
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Title = "选择 HALCON 脚本",
                Filter = "HALCON Script (*.hdev;*.hproc)|*.hdev;*.hproc"
            };

            if (dialog.ShowDialog() == true)
            {
                ScriptPath = dialog.FileName;
            }
        }


        // ✅ 修正：ImagePath改为Prism规范的可绑定属性（和其他属性保持一致） prism框架下 BindableBase 提供了 SetProperty 方法 用于简化属性更改通知的实现
        private string _imagePath = "";
        public string ImagePath
        {
            get => _imagePath;  
            set => SetProperty(ref _imagePath, value);
        }
        // ✅ 新增：缩放后的图片显示宽度（绑定到XAML Image.Width）
        private double _imageShowWidth;
        public double ImageShowWidth
        {
            get => _imageShowWidth;
            set => SetProperty(ref _imageShowWidth, value);
        }

        // ✅ 新增：缩放后的图片显示高度（绑定到XAML Image.Height）
        private double _imageShowHeight;
        private Bitmap imageDetected;

        public double ImageShowHeight
        {
            get => _imageShowHeight;
            set => SetProperty(ref _imageShowHeight, value);
        }

        // ✅ 新增：图片缩放最大限制（常量，方便后期修改）
        private const double MaxImageWidth = 380;  // 最大宽度
        private const double MaxImageHeight = 250; // 最大高度
                                                   // ✅ 完整修正：OnBrowseImage方法（修复路径获取+适配Prism属性+补充图片筛选）
                                                   // ✅ 完整修改：OnBrowseImage + 图片尺寸获取 + 等比缩放算法
        private void OnBrowseImage()
        {
            using var dialog = new OpenFileDialog();
            dialog.Title = "请选择图像";
            dialog.Filter = "图像文件 (*.png;*.jpg;*.jpeg;*.bmp;*.gif)|*.png;*.jpg;*.jpeg;*.bmp;*.gif|所有文件 (*.*)|*.*";
            dialog.Multiselect = false;

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    // 1. 获取图片路径并赋值
                    ImagePath = dialog.FileName;
                    _logger.Info("选择图像成功，路径：" + ImagePath);
                    double originalWidth ;
                    double originalHeight ;
                    // 2. 读取原图尺寸（使用using保证资源释放）
                    // 替换 using var originalImage = Image.FromFile(ImagePath);
                    // 使用 SixLabors.ImageSharp.Image.Load 读取图片
                    using (var originalImage = SixLabors.ImageSharp.Image.Load(ImagePath))
                    {
                         originalWidth = originalImage.Width;
                         originalHeight = originalImage.Height;
                        _logger.Info($"原图尺寸：宽{originalWidth} × 高{originalHeight}");

                        // 3. 核心：等比缩放算法（保证宽高不超380×250，不变形）
                        CalculateScaledSize(originalWidth, originalHeight);
                    }
                   
                    _logger.Info($"原图尺寸：宽{originalWidth} × 高{originalHeight}");

                    // 3. 核心：等比缩放算法（保证宽高不超380×250，不变形）
                    CalculateScaledSize(originalWidth, originalHeight);

                  
                }
                catch (Exception ex)
                {
                    // 异常处理：防止图片损坏/格式错误导致崩溃
                    _logger.Error("图片读取/缩放失败：" + ex.Message);
                    System.Windows.MessageBox.Show("图片格式错误或已损坏，请重新选择！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                    ImagePath = ""; // 清空错误路径
                }
            }
        }

        // ✅ 新增：等比缩放计算方法（独立封装，方便复用）
        private void CalculateScaledSize(double originalWidth, double originalHeight)
        {
            // 缩放比例初始值为1（原图尺寸≤限制时，不缩放）
            double scaleRatio = 1.0;

            // 4.1 计算宽度缩放比例（如果原图宽>380，需要缩放）
            if (originalWidth > MaxImageWidth)
            {
                scaleRatio = MaxImageWidth / originalWidth;
            }

            // 4.2 计算高度缩放比例（如果原图高>250，取更小的比例，保证高度也不超）
            if (originalHeight > MaxImageHeight)
            {
                double heightRatio = MaxImageHeight / originalHeight;
                scaleRatio = Math.Min(scaleRatio, heightRatio); // 取最小比例，等比不变形
            }

            // 5. 计算缩放后的最终尺寸（保留小数，避免整数截断导致变形）
            ImageShowWidth = originalWidth * scaleRatio;
            ImageShowHeight = originalHeight * scaleRatio;

            _logger.Info($"缩放后尺寸：宽{ImageShowWidth:F2} × 高{ImageShowHeight:F2}，缩放比例：{scaleRatio:F2}");
        }

        private string _lastDetectedImagePath;
        private void DeleteLastDetectedImage()
        {
            try
            {
                if (!string.IsNullOrEmpty(_lastDetectedImagePath) &&
                    File.Exists(_lastDetectedImagePath))
                {
                    File.Delete(_lastDetectedImagePath);
                    _logger.Info("已删除临时检测图：" + _lastDetectedImagePath);
                }
            }
            catch (Exception ex)
            {
                _logger.Warn("删除临时检测图失败：" + ex.Message);
            }
        }



        // ================== 检测图片核心方法 ==================
        public void OnProcessImage()
        {
            // 初始化 YOLO
            using (var yolo = new YoloOnnxDetector(DatasetPath))
            {
                if (ImagePath == "" || ImagePath == null)
                {
                    HandyControl.Controls.MessageBox.Show("图片路径不能为空");
                    return;
                }
                // 读取原图
                using (var image = new Mat(ImagePath))
                {
                    DateTime dt1 = DateTime.Now;

                    // 推理
                    List<Prediction> predictions = yolo.Predict(image);

                    // 画框 输出检测项
                    foreach (var pred in predictions)
                    {
                        Cv2.Rectangle(image, pred.Box, Scalar.Red, 10);
                        string label = $"{pred.Label} ({pred.Confidence:P2})";
                      
                  
                        Cv2.PutText(
                            image,
                            label,
                            new OpenCvSharp.Point(pred.Box.X, pred.Box.Y - 5),
                            HersheyFonts.HersheySimplex,
                           5,
                            Scalar.Red,
                           5);
                    }

                    // 先删旧的
                    //DeleteLastDetectedImage();

                    // ✅ 保存为新图片路径（关键）
                    string detectedPath = Path.Combine(
                        Path.GetTempPath(),
                        $"bai_detect_{Guid.NewGuid():N}.png");

                    Cv2.ImWrite(detectedPath, image);

                    // 记录
                    _lastDetectedImagePath = detectedPath;
                    // ✅ 更新绑定属性，UI 自动刷新
                    ImagePath = detectedPath;

                    // 可选：日志
                    ProcessTime = (DateTime.Now - dt1).TotalMilliseconds;


                    ProcessResult = "";
                    foreach(var i in predictions)
                    {
                        // 保留三位小数拼接：Label + ： + 三位小数的置信度
                        ProcessResult += $"{i.Label}：{i.Confidence:F3}  ";
                    }
                  
                    _logger.Info($"共检测出{predictions.Count}个结果，耗时:{ProcessTime}ms") ;
                }
            }
        }

    }
}
