using GhostInTheShell.Modules.InfraStructure;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GhostInTheShell.Modules.MvvmInfra
{
    public interface IGhostIdentifier
    {
        string Identifier { get; }
        event GhostIdentifierChangedEventHandler IdentifierChanged;
    }
    public abstract class GhostViewModelBase : BindableBase, IDialogAware, IGhostIdentifier
    {
        string _Identifier;
        double _Left = 400, _Top;
        double _Width, _Height;

        public string Identifier => _Identifier;

        public double Left
        {
            get => _Left;
            set => SetProperty(ref _Left, value);
        }
        public double Top
        {
            get => _Top;
            set => SetProperty(ref _Top, value);
        }
        public double Width
        {
            get => _Width;
            set => SetProperty(ref _Width, value);
        }
        public double Height
        {
            get => _Height;
            set => SetProperty(ref _Height, value);
        }


        public string Title => _Identifier;

        public event Action<IDialogResult> RequestClose;
        public event GhostIdentifierChangedEventHandler? IdentifierChanged;

        public bool CanCloseDialog() => true;

        public void OnDialogClosed()
        {
            //throw new NotImplementedException();
        }

        public void OnDialogOpened(IDialogParameters parameters) => OnDialogOpenedBase(parameters);

        protected virtual void OnDialogOpenedBase(IDialogParameters parameters)
        {
            if (!parameters.ContainsKey(nameof(GhostViewModelBase.Identifier)))
                throw new KeyNotFoundException();

            _Identifier = parameters.GetValue<string>(nameof(GhostViewModelBase.Identifier));
            IdentifierChanged?.Invoke(this, new GhostIdentifierChangedEventArgs(Identifier));   // ForView
        }

        protected bool IsTarget(string identifier)
        {
            if(String.IsNullOrEmpty(identifier))
                throw new ArgumentNullException(nameof(identifier));

            return String.Equals(identifier, _Identifier, StringComparison.OrdinalIgnoreCase);
        }
    }
}
