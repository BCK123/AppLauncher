using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace AppLauncher.Utils
{

    //ICommand是 C# 内置接口（位于System.Windows.Input），所有「可绑定命令」都必须实现它
    public class RelayCommand : ICommand
    {

        //        这两个是「委托字段」（C# 特有的，类比 Java 的函数式接口），用来存储外部传入的逻辑：
        //Action<object?> _execute：
        //Action<T> 是 C# 内置委托，代表「无返回值、参数为 T 类型」的方法（类比 Java 的Consumer<T>）；
        //object? 表示参数可以是任意类型，也可以是 null（C# 8 + 的可空引用类型）；
        //readonly表示字段初始化后不能修改，保证线程安全。
        //Func<object?, bool>? _canExecute：
        //Func<T, TResult> 是 C# 内置委托，代表「参数为 T、返回值为 TResult」的方法（类比 Java 的Predicate<T>）；
        //? 表示这个委托可以为 null（即可选）；
        //作用是判断「命令是否可以执行」（比如按钮是否禁用）。
        private readonly Action<object?> _execute;
        private readonly Func<object?, bool>? _canExecute;


    
        //第一个参数execute是「命令要执行的核心逻辑」（必须传，否则抛异常）；
        //第二个参数canExecute是「判断命令能否执行的逻辑」（可选，默认 null）；
        //execute ?? throw new ArgumentNullException(...)：C# 的空合并运算符（类比 Java 的Objects.requireNonNull(execute)），如果execute为 null，直接抛参数空异常。
        public RelayCommand(Action<object?> execute, Func<object?, bool>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        //        event是 C# 的事件机制（类比 Java 的监听器），属于ICommand接口的要求；
        //作用：当「命令能否执行的条件」变化时（比如按钮从禁用变可用），触发这个事件，UI 层会自动更新控件状态（比如按钮启用 / 禁用）；
        //EventHandler? 是 C# 内置的事件委托（无返回值，参数是object和EventArgs），?表示事件可以没有订阅者。
        public event EventHandler? CanExecuteChanged;
        public bool CanExecute(object? parameter) => _canExecute?.Invoke(parameter) ?? true;
        public void Execute(object? parameter) => _execute(parameter);

        public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);


        //这段代码的本质是：用委托（函数式编程）把「业务逻辑方法」包装成「符合 ICommand 接口的命令对象」
        //，实现 UI 层（按钮）和业务逻辑（打开记事本）的解耦，这是 MVVM 模式的核心思想之一。
    }
}
