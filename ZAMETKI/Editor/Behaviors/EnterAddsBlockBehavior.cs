using System.Windows.Input;
using MauiEditor = Microsoft.Maui.Controls.Editor;

namespace ZAMETKI.Editor.Behaviors;

public class EnterAddsBlockBehavior : Behavior<MauiEditor>
{
    public static readonly BindableProperty CommandProperty =
        BindableProperty.Create(nameof(Command), typeof(ICommand), typeof(EnterAddsBlockBehavior));

    public static readonly BindableProperty CommandParameterProperty =
        BindableProperty.Create(nameof(CommandParameter), typeof(object), typeof(EnterAddsBlockBehavior));

    public ICommand? Command
    {
        get => (ICommand?)GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    public object? CommandParameter
    {
        get => GetValue(CommandParameterProperty);
        set => SetValue(CommandParameterProperty, value);
    }

    private MauiEditor? _editor;

#if WINDOWS
    private Microsoft.UI.Xaml.Controls.TextBox? _platform;
    private Microsoft.UI.Xaml.Input.KeyEventHandler? _handler;
#endif

    protected override void OnAttachedTo(MauiEditor bindable)
    {
        base.OnAttachedTo(bindable);
        _editor = bindable;
        bindable.HandlerChanged += OnHandlerChanged;
    }

    protected override void OnDetachingFrom(MauiEditor bindable)
    {
        bindable.HandlerChanged -= OnHandlerChanged;
        Unhook();
        _editor = null;
        base.OnDetachingFrom(bindable);
    }

    private void OnHandlerChanged(object? sender, EventArgs e)
    {
        if (_editor?.Handler is null)
        {
            Unhook();
            return;
        }

#if WINDOWS
        if (_editor.Handler.PlatformView is Microsoft.UI.Xaml.Controls.TextBox tb)
        {
            Unhook();
            _platform = tb;
            tb.AcceptsReturn = true;
            _handler = (s, args) =>
            {
                if (args.Key != Windows.System.VirtualKey.Enter) return;

                var shiftState = Microsoft.UI.Input.InputKeyboardSource
                    .GetKeyStateForCurrentThread(Windows.System.VirtualKey.Shift);
                var shiftDown = shiftState.HasFlag(Windows.UI.Core.CoreVirtualKeyStates.Down);
                if (shiftDown) return;

                args.Handled = true;
                if (Command?.CanExecute(CommandParameter) == true)
                    Command.Execute(CommandParameter);
            };
            tb.KeyDown += _handler;
        }
#endif
    }

    private void Unhook()
    {
#if WINDOWS
        if (_platform is not null && _handler is not null)
            _platform.KeyDown -= _handler;
        _platform = null;
        _handler = null;
#endif
    }
}
