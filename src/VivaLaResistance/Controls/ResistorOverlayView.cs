using System.Collections.ObjectModel;
using System.Collections.Specialized;
using VivaLaResistance.Core.Models;

namespace VivaLaResistance.Controls;

/// <summary>
/// Full-screen, input-transparent GraphicsView that renders AR badge overlays
/// for detected resistors.  Bind <see cref="ResistorReadings"/> to
/// <c>MainViewModel.DetectedResistors</c> to drive the overlay.
///
/// The overlay re-renders automatically whenever the bound collection changes.
/// Hysteresis is handled internally by <see cref="ResistorOverlayDrawable"/>:
/// badges appear at confidence ≥ 0.65 and disappear below 0.60.
/// </summary>
public sealed class ResistorOverlayView : ContentView
{
    // ── Bindable property ─────────────────────────────────────────────────────

    /// <summary>
    /// The collection of resistor readings to overlay.  Bind this to
    /// <c>MainViewModel.DetectedResistors</c>.
    /// </summary>
    public static readonly BindableProperty ResistorReadingsProperty =
        BindableProperty.Create(
            nameof(ResistorReadings),
            typeof(ObservableCollection<ResistorReading>),
            typeof(ResistorOverlayView),
            defaultValue: null,
            propertyChanged: OnReadingsPropertyChanged);

    public ObservableCollection<ResistorReading>? ResistorReadings
    {
        get => (ObservableCollection<ResistorReading>?)GetValue(ResistorReadingsProperty);
        set => SetValue(ResistorReadingsProperty, value);
    }

    // ── Private state ─────────────────────────────────────────────────────────

    private readonly GraphicsView _graphicsView;
    private readonly ResistorOverlayDrawable _drawable;

    // ── Constructor ───────────────────────────────────────────────────────────

    public ResistorOverlayView()
    {
        _drawable = new ResistorOverlayDrawable();

        _graphicsView = new GraphicsView
        {
            Drawable         = _drawable,
            HorizontalOptions = LayoutOptions.Fill,
            VerticalOptions  = LayoutOptions.Fill,
            InputTransparent = true,
            BackgroundColor  = Colors.Transparent,
        };

        // The ContentView itself is also input-transparent so touch events
        // pass through to the camera surface below.
        InputTransparent = true;
        Content = _graphicsView;
    }

    // ── MAUI handler lifecycle — subscribe/unsubscribe safely ─────────────────

    /// <summary>
    /// Subscribe to <see cref="CollectionChanged"/> when the view attaches to
    /// a handler (enters the visual tree), and unsubscribe when it detaches, to
    /// prevent memory leaks.
    /// </summary>
    protected override void OnHandlerChanged()
    {
        base.OnHandlerChanged();

        if (Handler is not null)
        {
            // Attaching — subscribe if a collection is already bound.
            if (ResistorReadings is { } col)
                col.CollectionChanged += OnCollectionChanged;
        }
        else
        {
            // Detaching — always unsubscribe to avoid a dangling reference.
            if (ResistorReadings is { } col)
                col.CollectionChanged -= OnCollectionChanged;
        }
    }

    // ── Binding change handler ────────────────────────────────────────────────

    private static void OnReadingsPropertyChanged(
        BindableObject bindable, object oldValue, object newValue)
    {
        var view = (ResistorOverlayView)bindable;

        if (oldValue is ObservableCollection<ResistorReading> old)
            old.CollectionChanged -= view.OnCollectionChanged;

        if (newValue is ObservableCollection<ResistorReading> fresh)
        {
            fresh.CollectionChanged += view.OnCollectionChanged;
            view.Refresh(fresh);
        }
        else
        {
            view.Refresh(Enumerable.Empty<ResistorReading>());
        }
    }

    private void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (sender is ObservableCollection<ResistorReading> collection)
            Refresh(collection);
    }

    // ── Rendering ─────────────────────────────────────────────────────────────

    private void Refresh(IEnumerable<ResistorReading> readings)
    {
        _drawable.UpdateReadings(readings);
        _graphicsView.Invalidate();
    }
}
