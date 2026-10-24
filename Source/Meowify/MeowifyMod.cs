using Verse;

namespace Meowify
{
    /// <summary>
    /// 模组入口：Mod 构造时（早于 def 加载与语言注入）装上 Harmony 补丁。
    /// </summary>
    public class MeowifyMod : Mod
    {
        /// <summary>与 About\About.xml 的 modVersion、csproj 的 Version 保持一致。</summary>
        public const string Version = "1.0.0";

        public MeowifyMod(ModContentPack content) : base(content)
        {
            HarmonyPatches.Initialize();
            Log.Message("[Meowify] v" + Version + " 已加载，全场开始喵化。喵。");
        }
    }
}
