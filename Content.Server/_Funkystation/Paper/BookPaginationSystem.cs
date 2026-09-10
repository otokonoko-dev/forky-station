using Content.Shared._Funkystation.Paper;
using Content.Shared.Paper;
using static Content.Shared.Paper.PaperComponent;

namespace Content.Server._Funkystation.Paper;

public sealed class BookPaginationSystem : EntitySystem
{
    /// Smallest number of characters we assume can fit on a page. Only used to bound how far
    /// a bookmark may point into a book, so a malformed message can't store a silly page number.
    private const int MinCharsPerPage = 20;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BookPaginationComponent, BookPageChangeMessage>(OnPageChange);
    }

    private void OnPageChange(EntityUid uid, BookPaginationComponent component, BookPageChangeMessage args)
    {
        if (args.NewPage < 0)
            return;

        if (!TryComp<PaperComponent>(uid, out var paper))
            return;

        // The client lays pages out using its own font metrics and window size, so the page count
        // isn't reproducible here. CurrentPage is only a bookmark, so accept what the reader
        // reports and just guard against absurd values.
        if (args.NewPage > MaxPage(paper.Content))
            return;

        component.CurrentPage = args.NewPage;
        Dirty(uid, component);
    }

    private static int MaxPage(string content)
    {
        if (string.IsNullOrEmpty(content))
            return 0;

        return Math.Max(1, content.Length / MinCharsPerPage);
    }
}
