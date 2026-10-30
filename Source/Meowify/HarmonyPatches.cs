using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.Grammar;

namespace Meowify
{
    /// <summary>
    /// 全部 Harmony 补丁。装在哪一环见各补丁注释。
    /// </summary>
    public static class HarmonyPatches
    {
        public const string HarmonyId = "redbean.meowify";

        private static readonly Harmony Harmony = new Harmony(HarmonyId);
        private static bool initialized;

        public static void Initialize()
        {
            if (initialized)
            {
                return;
            }
            initialized = true;

            // 1) 所有 Translate / TranslateSimple 最终都会落到这里（包括英文回退与 DefInjected 读回）。
            TryPatch(
                "LoadedLanguage.TryGetTextFromKey",
                AccessTools.Method(typeof(LoadedLanguage), "TryGetTextFromKey", new Type[] { typeof(string), typeof(TaggedString).MakeByRefType() }),
                postfix: new HarmonyMethod(typeof(HarmonyPatches), "TryGetTextFromKey_Postfix"));

            // 2) 语言注入写入 def 之前，直接改注入值，这样 label/description/规则串等都会带喵。
            TryPatch(
                "DefInjectionPackage.InjectIntoDefs",
                AccessTools.Method(typeof(DefInjectionPackage), "InjectIntoDefs", new Type[] { typeof(bool) }),
                prefix: new HarmonyMethod(typeof(HarmonyPatches), "InjectIntoDefs_Prefix"));

            // 3) 语法拼出来的句子（名字、事件描述、传闻等）在出口处加喵。
            TryPatch(
                "GrammarResolver.Resolve",
                AccessTools.Method(typeof(GrammarResolver), "Resolve", new Type[]
                {
                    typeof(string), typeof(GrammarRequest), typeof(string), typeof(bool),
                    typeof(string), typeof(List<string>), typeof(List<string>), typeof(bool)
                }),
                postfix: new HarmonyMethod(typeof(HarmonyPatches), "Resolve_Postfix"));

            // 4) 全部 def 注入完成后兜底：没被翻译的、代码生成/隐含的 def 的 label/description 也补喵。
            TryPatch(
                "LoadedLanguage.InjectIntoData_AfterImpliedDefs",
                AccessTools.Method(typeof(LoadedLanguage), "InjectIntoData_AfterImpliedDefs", Type.EmptyTypes),
                postfix: new HarmonyMethod(typeof(HarmonyPatches), "AfterImpliedDefs_Postfix"));

            Log.Message("[Meowify] Harmony patches installed: " + PatchedCount + "/4. 喵。");

        }

        private static int PatchedCount;

        private static void TryPatch(string name, MethodBase target, HarmonyMethod prefix = null, HarmonyMethod postfix = null)
        {
            if (target == null)
            {
                Log.Error("[Meowify] 找不到目标方法，跳过补丁: " + name);
                return;
            }
            try
            {
                Harmony.Patch(target, prefix: prefix, postfix: postfix);
                PatchedCount++;
            }
            catch (Exception e)
            {
                Log.Error("[Meowify] 补丁失败 " + name + ": " + e);
            }
        }

        // ---------------------------------------------------------------- 补丁实现

        internal static void TryGetTextFromKey_Postfix(bool __result, ref TaggedString translated)
        {
            if (!__result || !Meowifier.Options.ChannelKeyed)
            {
                return;
            }
            string raw = translated.RawText;
            if (string.IsNullOrEmpty(raw))
            {
                return;
            }
            string meowed = Meowifier.Meowify(raw);
            if (!string.Equals(meowed, raw, StringComparison.Ordinal))
            {
                translated = new TaggedString(meowed);
            }
        }

        internal static void InjectIntoDefs_Prefix(DefInjectionPackage __instance)
        {
            if (!Meowifier.Options.ChannelDefInjected)
            {
                return;
            }
            MeowInjections.MeowAll(__instance);
        }

        internal static void Resolve_Postfix(string rootKeyword, ref string __result)
        {
            if (string.IsNullOrEmpty(__result) || !Meowifier.Options.ChannelGrammar)
            {
                return;
            }
            // 小人/动物/武器/派系/任务的名字是语法生成的，默认不喵（名字带喵会很怪）
            if (!Meowifier.Options.IncludeNames && IsNameKeyword(rootKeyword))
            {
                return;
            }
            __result = Meowifier.Meowify(__result);
        }

        /// <summary>语法根关键字是不是在造名字（r_name / r_weapon_name / r_art_name / r_deityName…）。</summary>
        internal static bool IsNameKeyword(string rootKeyword)
        {
            return !string.IsNullOrEmpty(rootKeyword) &&
                   rootKeyword.IndexOf("name", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        internal static void AfterImpliedDefs_Postfix()
        {
            if (!Meowifier.Options.ChannelDefSweep)
            {
                return;
            }
            DefSweeper.SweepAll();
        }
    }

    /// <summary>
    /// 在 DefInjectionPackage.InjectIntoDefs 之前，把翻译注入值喵化。
    /// 只喵化「看起来是给人看的文字」的注入值：语言包也可能往非文本字段（颜色通道名、
    /// 贴图路径等）里注入值，例如绮罗族把 HAR 的 colorChannels.0.name 注入成 skin/hair，
    /// 一旦被改成「skin喵」，HAR 里用硬编码 "skin"/"hair" 取色的地方就会失配而让贴图变透明。
    /// </summary>
    internal static class MeowInjections
    {
        /// <summary>
        /// 这些路径片段指向的是机器标识符（颜色通道名、贴图路径、着色器名等），一律不喵化。
        /// </summary>
        private static readonly string[] NonTextPathTokens =
        {
            "colorchannel",
            "texpath",
            "graphicpath",
            "graphicdata",
            "maskpath",
            "shadertype",
            "shader",
            "texturepath",
            "pathprefix",
            "rulesstring",
            "generalrules",
            "grammar",
        };

        /// <summary>
        /// 这些路径后缀确定字段是给人看的文字，即使值是单个英文词也照常喵化。
        /// </summary>
        private static readonly string[] TextualPathSuffixes =
        {
            ".label",
            ".labelcap",
            ".description",
            ".title",
            ".helptext",
            ".text",
            ".reportstring",
            ".verb",
            ".gerund",
            ".jobstring",
        };

        private static readonly FieldInfo DefTypeField =
            AccessTools.Field(typeof(DefInjectionPackage), "defType");
        private static readonly FieldInfo InjectionsField =
            AccessTools.Field(typeof(DefInjectionPackage), "injections");

        private static FieldInfo injectionField;
        private static FieldInfo fullListField;
        private static FieldInfo placeholderField;
        private static FieldInfo pathField;
        private static bool fieldsReady;
        private static bool fieldsFailed;

        public static void MeowAll(DefInjectionPackage pkg)
        {
            if (pkg == null || !EnsureFields())
            {
                return;
            }

            try
            {
                // 语法规则（rulesStrings）是词块，不能在词中间加喵，交给 GrammarResolver 出口处理。
                if (DefTypeField != null && DefTypeField.GetValue(pkg) is Type defType && defType == typeof(RulePackDef))
                {
                    return;
                }

                IDictionary dict = InjectionsField.GetValue(pkg) as IDictionary;
                if (dict == null)
                {
                    return;
                }

                foreach (DictionaryEntry de in dict)
                {
                    object entry = de.Value;
                    if (entry == null)
                    {
                        continue;
                    }
                    try
                    {
                        if (placeholderField != null && placeholderField.GetValue(entry) is bool isPlaceholder && isPlaceholder)
                        {
                            continue;
                        }

                        string path = (pathField != null ? pathField.GetValue(entry) as string : null) ?? (de.Key as string);
                        string injection = injectionField.GetValue(entry) as string;
                        if (!IsTextPath(path))
                        {
                            continue;
                        }

                        if (ShouldMeow(path, injection))
                        {
                            string meowed = Meowifier.Meowify(injection);
                            if (!string.Equals(meowed, injection, StringComparison.Ordinal))
                            {
                                injectionField.SetValue(entry, meowed);
                            }
                        }

                        IList list = fullListField.GetValue(entry) as IList;
                        if (list != null)
                        {
                            for (int i = 0; i < list.Count; i++)
                            {
                                string item = list[i] as string;
                                if (!ShouldMeow(path, item))
                                {
                                    continue;
                                }
                                string meowed = Meowifier.Meowify(item);
                                if (!string.Equals(meowed, item, StringComparison.Ordinal))
                                {
                                    list[i] = meowed;
                                }
                            }
                        }
                    }
                    catch (Exception)
                    {
                        // 单条注入失败不影响其它
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error("[Meowify] 注入值喵化失败: " + e);
            }
        }

        /// <summary>
        /// 路径是否指向可显示文本字段。命中机器字段黑名单的一律不喵。
        /// </summary>
        private static bool IsTextPath(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return true;
            }

            string lower = path.ToLowerInvariant();
            for (int i = 0; i < NonTextPathTokens.Length; i++)
            {
                if (lower.IndexOf(NonTextPathTokens[i], StringComparison.Ordinal) >= 0)
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// 判断这条注入值要不要喵：
        /// 非文本路径一律不喵；含中日韩字符的当文本；label/description 这类字段就算只有单个英文词也当文本；
        /// 其余纯 ASCII 且没有空白的当标识符（skin / Blank / Hair_Def 等）跳过；含 / 或 \ 的当资源路径跳过。
        /// </summary>
        internal static bool ShouldMeow(string path, string value)
        {
            if (string.IsNullOrEmpty(value) || !IsTextPath(path))
            {
                return false;
            }

            bool hasAsian = false;
            bool hasSpace = false;
            for (int i = 0; i < value.Length; i++)
            {
                char c = value[i];
                if (c >= '\u2E80')
                {
                    hasAsian = true;
                }
                else if (char.IsWhiteSpace(c))
                {
                    hasSpace = true;
                }
            }

            if (hasAsian)
            {
                return true;
            }
            if (HasTextualPathSuffix(path))
            {
                return true;
            }
            if (!hasSpace)
            {
                return false;
            }
            if (value.IndexOf('/') >= 0 || value.IndexOf('\\') >= 0)
            {
                return false;
            }
            return true;
        }

        private static bool HasTextualPathSuffix(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return false;
            }
            string lower = path.ToLowerInvariant();
            for (int i = 0; i < TextualPathSuffixes.Length; i++)
            {
                if (lower.EndsWith(TextualPathSuffixes[i], StringComparison.Ordinal))
                {
                    return true;
                }
            }
            return false;
        }

        private static bool EnsureFields()
        {
            if (fieldsReady)
            {
                return true;
            }
            if (fieldsFailed)
            {
                return false;
            }

            try
            {
                if (InjectionsField == null)
                {
                    throw new Exception("DefInjectionPackage.injections 字段不存在");
                }
                Type[] genericArgs = InjectionsField.FieldType.GetGenericArguments();
                if (genericArgs.Length < 2)
                {
                    throw new Exception("DefInjectionPackage.injections 不是 Dictionary<string, DefInjection>");
                }
                Type entryType = genericArgs[1];
                injectionField = AccessTools.Field(entryType, "injection");
                fullListField = AccessTools.Field(entryType, "fullListInjection");
                placeholderField = AccessTools.Field(entryType, "isPlaceholder");
                pathField = AccessTools.Field(entryType, "path");
                if (injectionField == null)
                {
                    throw new Exception("DefInjection.injection 字段不存在");
                }
                fieldsReady = true;
                return true;
            }
            catch (Exception e)
            {
                fieldsFailed = true;
                Log.Error("[Meowify] 反射 DefInjection 字段失败: " + e);
                return false;
            }
        }
    }

    /// <summary>
    /// 全部 def 注入完之后的兜底扫尾：喵化 label/description 以及背景故事标题，并清掉 LabelCap 缓存。
    /// </summary>
    internal static class DefSweeper
    {
        private static readonly Dictionary<Type, FieldInfo[]> CacheFieldCache = new Dictionary<Type, FieldInfo[]>();

        public static void SweepAll()
        {
            try
            {
                HashSet<Def> seen = new HashSet<Def>();
                foreach (Type defType in typeof(Def).AllSubclasses())
                {
                    if (defType == null || defType.IsAbstract)
                    {
                        continue;
                    }

                    IEnumerable<Def> defs;
                    try
                    {
                        defs = GenDefDatabase.GetAllDefsInDatabaseForDef(defType);
                    }
                    catch (Exception)
                    {
                        continue;
                    }
                    if (defs == null)
                    {
                        continue;
                    }

                    try
                    {
                        foreach (Def def in defs)
                        {
                            if (def == null || !seen.Add(def))
                            {
                                continue;
                            }
                            ScrubDef(def);
                        }
                    }
                    catch (Exception)
                    {
                        // 某个 def 库枚举失败就跳过该库
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error("[Meowify] def 扫尾失败: " + e);
            }

            // 语言数据重载后旧缓存不再适用
            Meowifier.ClearCache();
        }

        private static void ScrubDef(Def def)
        {
            bool changed = false;
            changed |= MeowField(ref def.label);
            changed |= MeowField(ref def.description);

            BackstoryDef backstory = def as BackstoryDef;
            if (backstory != null)
            {
                changed |= MeowField(ref backstory.title);
                changed |= MeowField(ref backstory.titleFemale);
                changed |= MeowField(ref backstory.titleShort);
                changed |= MeowField(ref backstory.titleShortFemale);
                changed |= MeowField(ref backstory.baseDesc);
            }

            if (changed)
            {
                ClearLabelCaches(def);
            }
        }

        private static bool MeowField(ref string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return false;
            }
            string meowed = Meowifier.Meowify(value);
            if (string.Equals(meowed, value, StringComparison.Ordinal))
            {
                return false;
            }
            value = meowed;
            return true;
        }

        /// <summary>Def 基类与各子类里可能缓存了 LabelCap（TaggedString / string / float）。</summary>
        private static void ClearLabelCaches(Def def)
        {
            Type type = def.GetType();
            FieldInfo[] fields;
            if (!CacheFieldCache.TryGetValue(type, out fields))
            {
                List<FieldInfo> list = new List<FieldInfo>();
                for (Type cur = type; cur != null && cur != typeof(object); cur = cur.BaseType)
                {
                    foreach (FieldInfo f in cur.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
                    {
                        if (f.Name.IndexOf("cachedLabelCap", StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            list.Add(f);
                        }
                    }
                    if (cur == typeof(Def))
                    {
                        break;
                    }
                }
                fields = list.ToArray();
                CacheFieldCache[type] = fields;
            }

            foreach (FieldInfo f in fields)
            {
                try
                {
                    if (f.FieldType == typeof(TaggedString))
                    {
                        f.SetValue(def, default(TaggedString));
                    }
                    else if (f.FieldType == typeof(string))
                    {
                        f.SetValue(def, null);
                    }
                    else if (f.FieldType == typeof(float))
                    {
                        f.SetValue(def, -1f);
                    }
                }
                catch (Exception)
                {
                    // 缓存清理失败不影响主要功能
                }
            }
        }
    }
}
