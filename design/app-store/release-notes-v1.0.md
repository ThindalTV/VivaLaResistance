# Release Notes — v1.0

## iOS App Store Format

### What's New in Version 1.0

🎉 **Welcome to Viva La Resistance!**

Your pocket resistor identifier is here. Point your camera at any resistor and instantly see its value — no charts, no guessing.

**Features:**
• Real-time AR overlays showing resistance + tolerance
• Detect multiple resistors simultaneously
• Works 100% offline — no internet needed
• Fast, focused, and privacy-first

Made with ⚡ by one person. Enjoy!

*(411 characters)*

---

## Google Play Store Format

### What's New

**🎉 Version 1.0 — Initial Release**

Viva La Resistance is here! The fastest way to identify resistors using just your phone's camera.

---

**✨ Core Features**

• **Instant AR identification** — Point your camera at a resistor, see its value floating right next to it. No typing, no charts, no squinting at color bands.

• **Multiple resistors at once** — Sorting through a parts bin? Detect and identify several resistors in a single view.

• **100% offline** — All processing happens on your device using an embedded YOLOv8-nano model. No internet required. No cloud. No waiting.

• **Privacy by design** — We access your camera and nothing else. No photos saved, no data transmitted, no analytics.

• **Fast and focused** — No accounts, no setup, no tutorials. Open the app and go.

---

**📱 Technical Details**

• Built with .NET MAUI for iOS and Android
• On-device machine learning (ONNX Runtime)
• Color band analysis for 4-band and 5-band resistors
• Supports values from 0.1Ω to 99MΩ

---

**💚 Support Development**

This app is fully functional from day one. After a 7-day trial, you'll occasionally see a friendly (easily-dismissed) prompt asking if you'd like to support continued development. No features are ever locked. No ads.

---

**🐛 Known Limitations (v1.0)**

• Best results in good lighting conditions
• Portrait orientation only
• Very small or damaged resistors may not be detected

---

We'd love to hear your feedback! Report issues or suggestions on GitHub.

*(1,487 characters)*

---

## Future Release Notes Template

For subsequent releases, use this structure:

### iOS (keep it short)
```
What's New in Version X.Y

[Emoji] [Feature/Fix headline]
• Bullet point 1
• Bullet point 2

Thanks for using Viva La Resistance!
```

### Google Play (can be detailed)
```
What's New

**Version X.Y — [Theme/Codename]**

**✨ New Features**
• Feature 1 description
• Feature 2 description

**🐛 Bug Fixes**
• Fixed issue with...
• Resolved crash when...

**⚡ Improvements**
• Faster detection in...
• Better handling of...

---

Thanks for your support! Feedback welcome on GitHub.
```

---

## Assumptions & Notes

1. **iOS character limits** — Apple's "What's New" field is displayed prominently but truncates on device. Kept v1.0 notes under 500 characters for clean display.

2. **Google Play flexibility** — Android users expect more detail. Included technical specs and known limitations for transparency.

3. **Tone** — Matches the app's personality: friendly, direct, slightly playful. The ⚡ emoji ties to "Resistance" theme.

4. **Known limitations** — Included upfront to set expectations and reduce 1-star reviews from edge cases. Honest about lighting sensitivity (per design guidelines).

5. **Version numbering** — Assumed semantic versioning (1.0.0 = first public release). Adjust if using different scheme.

6. **Resistor value range** — "0.1Ω to 99MΩ" covers standard through-hole resistors. Verify with Bruce that the model/calculator supports this range.

7. **No changelog file** — Release notes live here for app store submission. If maintaining a user-facing CHANGELOG.md in the repo, sync these notes there too.
