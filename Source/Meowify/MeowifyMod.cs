using System;
using UnityEngine;
using Verse;

namespace Meowify
{
    /// <summary>
    /// 模组入口：Mod 构造时（早于 def 加载与语言注入）装上 Harmony 补丁，并载入设置。
    /// </summary>
    public class MeowifyMod : Mod
    {
        /// <summary>与 About\About.xml 的 modVersion、csproj 的 Version 保持一致。</summary>
        public const string Version = "1.0.0";

        public static MeowSettings Settings;

        public MeowifyMod(ModContentPack content) : base(content)
        {
            Settings = GetSettings<MeowSettings>();
            ApplySettings();
            HarmonyPatches.Initialize();
            Log.Message("[Meowify] v" + Version + " 已加载，全场开始喵化。喵。");

            if (Prefs.DevMode)
            {
                Log.Message("[Meowify] 样例: " + Meowifier.Meowify("你的殖民者发现了一片富含钢铁的矿脉。"));
            }

        }

        public static void ApplySettings()
        {
            Meowifier.Options = Settings.ToOptions();
            Meowifier.ClearCache();
        }

        public override string SettingsCategory()
        {
            return "喵化世界 Meowify";
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            bool changed = false;
            Listing_Standard listing = new Listing_Standard();
            listing.Begin(inRect);

            listing.Label("语气：每句话末尾都要带「喵」。");
            listing.Gap(6f);

            bool sentenceEnd = Settings.SentenceEnd;
            listing.CheckboxLabeled("句尾加「喵」", ref sentenceEnd, "按中英文标点断句，在句末标点前插入「喵」；没有标点时补在行尾。");
            changed |= sentenceEnd != Settings.SentenceEnd;
            Settings.SentenceEnd = sentenceEnd;

            bool pronounSwap = Settings.PronounSwap;
            listing.CheckboxLabeled("人称替换", ref pronounSwap, "我 → 本喵，你 → 主人（我们/你们 一并替换）。");
            changed |= pronounSwap != Settings.PronounSwap;
            Settings.PronounSwap = pronounSwap;

            bool kaomoji = Settings.Kaomoji;
            listing.CheckboxLabeled("句末颜文字", ref kaomoji, "较长且带句读的文本，末尾随机追加一个猫咪颜文字。");
            changed |= kaomoji != Settings.Kaomoji;
            Settings.Kaomoji = kaomoji;

            if (Settings.Kaomoji)
            {
                float minChars = listing.SliderLabeled(
                    "颜文字最短字数：" + Settings.KaomojiMinChars,
                    Settings.KaomojiMinChars, 4f, 30f, 0.6f,
                    "全文可见字符数达到这个长度才会追加颜文字，避免把按钮/信息卡撑破。");
                int rounded = Mathf.RoundToInt(minChars);
                if (rounded != Settings.KaomojiMinChars)
                {
                    Settings.KaomojiMinChars = rounded;
                    changed = true;
                }

                listing.Label("颜文字候选（每行一个，留空就不加）：");
                string raw = listing.TextEntry(Settings.KaomojiListRaw, 6);
                if (!string.Equals(raw, Settings.KaomojiListRaw, StringComparison.Ordinal))
                {
                    Settings.KaomojiListRaw = raw;
                    changed = true;
                }
            }

            listing.Gap(10f);
            if (listing.ButtonText("恢复默认"))
            {
                Settings.ResetToDefaults();
                changed = true;
            }

            listing.End();

            if (changed)
            {
                ApplySettings();
                Settings.Write();
            }
        }
    }
}
