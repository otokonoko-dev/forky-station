using JetBrains.Annotations;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Utility;
using Content.Shared.Paper;
using Content.Shared._Funkystation.Paper;
using static Content.Shared.Paper.PaperComponent;

namespace Content.Client.Paper.UI;

[UsedImplicitly]
public sealed class PaperBoundUserInterface : BoundUserInterface
{
    [ViewVariables]
    private PaperWindow? _window;

    public PaperBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
    }

    protected override void Open()
    {
        base.Open();

        _window = this.CreateWindow<PaperWindow>();
        _window.OnSaved += InputOnTextEntered;
        _window.OnSignatureRequested += OnSignatureRequested;

        if (EntMan.TryGetComponent<PaperComponent>(Owner, out var paper))
        {
            _window.MaxInputLength = paper.ContentSize;
        }
        if (EntMan.TryGetComponent<PaperVisualsComponent>(Owner, out var visuals))
        {
            _window.InitVisuals(Owner, visuals);
        }

        // Funky Station - Book Pagination
        if (EntMan.TryGetComponent<BookPaginationComponent>(Owner, out var pagination))
        {
            _window.EnablePagination(pagination.CurrentPage, pagination.LinesPerPage);
            _window.OnPageChanged += OnPageChanged;
        }
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);

        if (state is not PaperBoundUserInterfaceState paperState)
            return;

        // Funky Station - Book Pagination: pick up the page the server confirmed for us
        if (EntMan.TryGetComponent<BookPaginationComponent>(Owner, out var pagination))
            _window?.SetPage(pagination.CurrentPage);

        _window?.Populate(paperState);
    }

    private void InputOnTextEntered(string text)
    {
        SendMessage(new PaperInputTextMessage(text));

        if (_window != null)
        {
            _window.Input.TextRope = Rope.Leaf.Empty;
            _window.Input.CursorPosition = new TextEdit.CursorPos(0, TextEdit.LineBreakBias.Top);
        }
    }

    private void OnSignatureRequested(int signatureIndex)
    {
        SendMessage(new PaperSignatureRequestMessage(signatureIndex));
    }

    // Funky Station - Book Pagination
    private void OnPageChanged(int newPage)
    {
        SendMessage(new BookPageChangeMessage(newPage));
    }
}
