# Decision: Responsive Design & Permission Scope

**Proposed by:** Hope (UX Designer)  
**Date:** 2026-02-25  
**Status:** For Review

---

## Summary

ThindalTV has directed two key constraints that affect the entire app:
1. **Responsive design** across phones, Samsung Galaxy Fold, and tablets
2. **Camera-only permissions** — no gallery access

---

## Device Targets

| Device Class | Screen Width | Notes |
|--------------|--------------|-------|
| **Standard Phone** | 360-420dp | Primary target, portrait only |
| **Samsung Galaxy Fold (outer)** | ~360dp | Compact/cramped layout, 24:9 aspect |
| **Samsung Galaxy Fold (inner)** | ~720dp | Near-square, rich layout |
| **Tablet** | 600dp+ | Large canvas, potential landscape |

### Breakpoints for MAUI

| Breakpoint | Width Range | Badge Format |
|------------|-------------|--------------|
| **Compact** | < 400dp | Value + tolerance only (e.g., "10kΩ ±1%") |
| **Standard** | 400-599dp | Full format (e.g., "5-band · 10kΩ ±1%") |
| **Expanded** | ≥ 600dp | Full format, larger text (18sp) |

---

## Fold Handling Approach

1. **Detect screen width** via `DeviceDisplay.Current.MainDisplayInfo` and `Window.SizeChanged`
2. **Apply breakpoint rules** based on current width
3. **Animate transitions** when crossing 400dp threshold (200ms fade between formats)
4. **Limit visible badges** on compact screens (max 2)
5. **Support dialog** becomes bottom sheet (<600dp) or centered modal (≥600dp)

---

## Permission Scope

### Required

| Platform | Permission | Purpose |
|----------|------------|---------|
| iOS | `NSCameraUsageDescription` | Live resistor detection |
| Android | `android.permission.CAMERA` | Live resistor detection |

### Explicitly Excluded (Confirmed by ThindalTV)

| Permission | Reason |
|------------|--------|
| `READ_MEDIA_IMAGES` (Android) | No gallery access |
| `READ_EXTERNAL_STORAGE` (Android) | No gallery access |
| `NSPhotoLibraryUsageDescription` (iOS) | No gallery access |
| `RECORD_AUDIO` (Android) | Audio not needed |
| `NSMicrophoneUsageDescription` (iOS) | Audio not needed |

### UX Implications

- **No "pick from gallery" button** anywhere in the app
- **No photo import** flow or suggestion
- **Camera permission prompt** text: "VivaLaResistance needs camera access to detect resistors."

---

## Impact

- **Shuri:** Must implement responsive breakpoint handling in MAUI XAML
- **Bruce:** No impact (ML detection is screen-agnostic)
- **Natasha:** Must test on Fold device (or emulator) and tablet

---

## References

- Updated: `design/design-guidelines.md` (v1.1) — Sections 2, 3, 12
