using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace ABCTempTableGenerator
{
    public class CalculateCommand : ICommand
    {
        TempTableViewModel _vm;
        public CalculateCommand(TempTableViewModel vm)
        {
            _vm = vm;
        }

        public bool CanExecute(object parameter)
        {
            return _vm.CanCalculate();
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public void Execute(object parameter)
        {
            _vm.Calculate();
        }
    }
}
