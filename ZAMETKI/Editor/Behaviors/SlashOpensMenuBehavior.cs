using System.Windows.Input;
using MauiEditor = Microsoft.Maui.Controls.Editor;

namespace ZAMETKI.Editor.Behaviors;

public class SlashOpensMenuBehavior : Behavior<MauiEditor>
{
    public static readonly BindableProperty CommandProperty =
        BindableProperty.Create(nameof(Command), typeof(ICommand), typeof(SlashOpensMenuBehavior));

    public static readonly BindableProperty CommandParameterProperty =
        BindableProperty.Create(nameof(CommandParameter), typeof(object), typeof(SlashOpensMenuBehavior));

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

    protected override void OnAttachedTo(MauiEditor bindable)
    {
        base.OnAttachedTo(bindable);
        bindable.TextChanged += OnTextChanged;
    }

    protected override void OnDetachingFrom(MauiEditor bindable)
    {
        bindable.TextChanged -= OnTextChanged;
        base.OnDetachingFrom(bindable);
    }

    private void OnTextChanged(object? sender, TextChangedEventArgs e)
    {
        if (e.NewTextValue == "/" && string.IsNullOrEmpty(e.OldTextValue))
        {
            if (Command?.CanExecute(CommandParameter) == true)
                Command.Execute(CommandParameter);
        }
    }
}
