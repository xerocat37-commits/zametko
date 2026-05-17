using ZAMETKI.Editor.Models;
using MauiEditor = Microsoft.Maui.Controls.Editor;

namespace ZAMETKI.Editor.Behaviors;

public class FocusOnRequestBehavior : Behavior<MauiEditor>
{
    private MauiEditor? _editor;
    private BlockBase? _block;

    protected override void OnAttachedTo(MauiEditor bindable)
    {
        base.OnAttachedTo(bindable);
        _editor = bindable;
        bindable.BindingContextChanged += OnContextChanged;
        Hook(bindable.BindingContext as BlockBase);
    }

    protected override void OnDetachingFrom(MauiEditor bindable)
    {
        bindable.BindingContextChanged -= OnContextChanged;
        Hook(null);
        _editor = null;
        base.OnDetachingFrom(bindable);
    }

    private void OnContextChanged(object? sender, EventArgs e)
        => Hook(_editor?.BindingContext as BlockBase);

    private void Hook(BlockBase? block)
    {
        if (_block is not null) _block.FocusRequested -= OnFocusRequested;
        _block = block;
        if (_block is not null) _block.FocusRequested += OnFocusRequested;
    }

    private void OnFocusRequested(object? sender, EventArgs e)
    {
        var editor = _editor;
        if (editor is null) return;
        MainThread.BeginInvokeOnMainThread(() => editor.Focus());
    }
}
