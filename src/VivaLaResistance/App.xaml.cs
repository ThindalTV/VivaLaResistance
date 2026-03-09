namespace VivaLaResistance;

/// <summary>
/// MAUI Application class.
/// Handles app-level lifecycle including window size changes for
/// Samsung Galaxy Fold fold/unfold transitions and orientation changes.
/// </summary>
public partial class App : Application
{
    public App()
    {
        InitializeComponent();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        var window = new Window(new AppShell());

        // Subscribe to window size changes.
        // On Samsung Galaxy Fold, fold/unfold fires this event, changing the reported
        // window width from ~300 dp (folded) to ~880 dp (unfolded landscape).
        // AdaptiveTrigger states on MainPage.xaml re-evaluate automatically when the
        // window size changes -- no manual layout update is required here.
        window.SizeChanged += OnWindowSizeChanged;

        return window;
    }

    private void OnWindowSizeChanged(object? sender, EventArgs e)
    {
        // ── Samsung Galaxy Fold physical test notes ─────────────────────────
        // These tests must be performed on real hardware; emulators do not
        // accurately simulate the fold hinge or the resize timing.
        //
        // 1. FOLDED state (~280-320 dp width):
        //    - Launch app while folded.
        //    - Verify: HUD bar (Layer 3) does not clip status text or the detection badge.
        //    - Verify: "Starting Camera..." / "Camera Access Required" overlays fit without
        //              horizontal overflow (Padding="24,0" and FontSize=18 on SmallPhone state).
        //    - Verify: LightingIndicatorView (Layer 2) does not obscure HUD bar.
        //
        // 2. UNFOLD while app is running:
        //    - Open app while folded, then unfold to ~880 dp width.
        //    - Verify: AdaptiveTrigger transitions to LargePhone state (Padding="20,14",
        //              StatusText FontSize=16, badge FontSize=15).
        //    - Verify: Camera preview ContentView (Layer 0) fills the new wider viewport
        //              edge-to-edge with no letterboxing.
        //    - Verify: AR overlay badges (AbsoluteLayout Layer 1) reposition to match the
        //              wider viewport -- detection coordinates must be normalised (0..1).
        //
        // 3. FOLD while app is running:
        //    - Unfold app, then fold back.
        //    - Verify: AdaptiveTrigger transitions back to SmallPhone state without visual
        //              glitching or clipping.
        //
        // 4. ORIENTATION CHANGE (folded portrait -> landscape):
        //    - On Android, if android:configChanges does NOT include screenSize/orientation
        //      in the MainActivity declaration, the Activity is recreated, which triggers
        //      OnDisappearing + OnAppearing in MainPage -- camera pipeline restarts cleanly.
        //    - If android:configChanges IS set to suppress recreation, verify that the
        //      camera preview surface reattaches correctly via MainPage.xaml.cs code-behind.
        //    - On iOS, rotation fires UIViewController.ViewWillTransitionToSize; MAUI
        //      handles this transparently -- the ContentView fills the new bounds.
        //
        // 5. RESUME AFTER BACKGROUND (App.OnSleep -> OnAppearing):
        //    - Background the app while folded, unfold, then foreground.
        //    - Verify: OnAppearing fires, camera restarts, correct AdaptiveTrigger state
        //              applies for the current (unfolded) window width.
        // ───────────────────────────────────────────────────────────────────
    }
}