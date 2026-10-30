# Steam Workshop description (English)

> Companion to `workshop.md`. Paste the BBCode below into the Workshop description (Steam localizes per-language pages; this file is the reference text for an English listing/announcement).
> Version: test build with English UI support (not yet packaged) | RimWorld 1.7 | Requires Harmony.

## Title

```text
Meowify - every line in RimWorld now says meow
```

## Short description

```text
Adds "meow" to every line of RimWorld 1.7: sentence-ending meow, I -> this cat / you -> master, random cat kaomoji on longer text. Runtime only, no translation files touched. Three one-click presets plus fine-tuning and live preview. Requires Harmony.
```

## BBCode body

```bbcode
[b]Every line in the rim now talks like a cat.[/b]

A pure text-layer mod: it does not edit translation files, defs or saves. It only rewrites text at display time, giving everything that "sentence ends with meow" voice. Three presets, everything adjustable.

[h2]Features[/h2]
- Sentence-ending word: "New colony" -> "New colony, meow"
- Pronoun swap: I -> this cat, you -> master (toggleable, any words allowed)
- Random cat kaomoji at the end of longer text: (=^w^=) (^v^) ...
- Covers the main menu, buttons, tooltips, item / terrain / plant info cards, event logs, pawn backstories and grammar-generated sentences
- Pawn, animal and weapon names stay untouched by default (a meow in a name looks odd); a setting can meowify names too
- About 90 community cat kaomoji, picked by context: cute / combat / social / work / food / animal / sad / love
- English UI: sentence-ending words become meow / nya / mrrp, and the mod settings window is in English

[h2]Three presets[/h2]
- [b]A Tiny Meow[/b]: sentence-ending word only, smallest change
- [b]Catgirl Daily[/b]: sentence-ending meow + I -> this cat / you -> master + kaomoji on longer text (default)
- [b]Meow Overload[/b]: meow / mrrp / nya~ endings, 1-character minimum, kaomoji on every long line

[h2]Fine-tuning[/h2]
Path: Options -> Mod settings -> Meowify
- Sentence-ending word: toggle, minimum visible characters, chance to apply (some sentences only), word list
- Pronouns: ten slots (I / we / you / you-all / he / they / she / they / it / they) plus a custom "word=replacement" table
- Kaomoji: toggle, chance, minimum length, custom list, context-aware pools
- Protected words: words containing 我 / 你 / 他 that are not pronouns are skipped whole
- Excluded phrases: any text containing one of these is returned untouched, an escape hatch for other mods
- Four scope toggles: interface text / def-injected text / grammar sentences / def field fallback
- Collapse repeated endings with a custom suffix: "meow meow." becomes "meow~."
- Keyword whitelist / blacklist (blacklist wins)
- Six tabs (Presets / Sentence / Pronouns / Kaomoji / Scope / Advanced) with live preview; saves on close

[h2]Compatibility[/h2]
- Text is transformed at runtime; nothing is written back to defs or saves, so it can be removed at any time
- Compatible with alien race mods such as HAR / Kiiro: only visible text is meowified - colour channel names, texture paths and other machine fields are kept intact
- Names are not meowified by default
- Chinese UI keeps its original behaviour; only the English UI switches to English endings
- Requires Harmony

[h2]Install[/h2]
- Subscribe, enable Harmony and Meowify in the mod list, restart
- Load order: Harmony before Meowify (handled automatically)

[h2]Feedback[/h2]
Bugs, weird sentences or ideas for new kaomoji are welcome in the comments.
```

## Tags

```text
Mod, 1.7, Harmony, English, Text/UI, Fun
```

## Notes

- The Chinese listing is `docs/workshop.md`; keep both descriptions in sync when features change.
- Screenshot order is the same as the Chinese listing; English UI screenshots can be added after the English settings window is captured.
