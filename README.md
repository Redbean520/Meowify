# Meowify（喵化世界）

RimWorld **1.7** 的「全员喵化」模组：不改动任何原始翻译文件，运行时给所有文本加上句尾「喵」。需要 Harmony。

效果示例：

- 主菜单 `新建殖民地` → `新建殖民地喵`
- 物品信息 `高草（已生长60%）` → `高草喵（已生长60%喵）`
- 事件/名字/描述由语法规则生成，同样在句尾喵化

## 目录

    About\             模组元数据（About.xml）
    1.7\Assemblies\    1.7（net10.0）编译产物 Meowify.dll（Harmony 由 Harmony 模组提供）
    Source\Meowify\    C# 工程（测试版才有，发布包不含）
    LoadFolders.xml
    tools\             打包脚本

## 安装

- 需要 **Harmony**（brrainz.harmony 的 1.7 版本，Harmony 3）
- 发布包解压后整个文件夹放进 `Mods/Meowify/`
- 游戏内模组列表勾选，重启

## 原理

全部在运行时完成，四个 Harmony 补丁（`Source\Meowify\HarmonyPatches.cs`）：

| 补丁 | 目标 | 作用 |
|---|---|---|
| postfix | `Verse.LoadedLanguage.TryGetTextFromKey` | 界面 Keyed 文本（按钮、提示、标签） |
| prefix | `Verse.DefInjectionPackage.InjectIntoDefs` | DefInjected 注入（物品/特性/研究/事件等 def 字段），跳过 RulePackDef 与占位符 |
| postfix | `Verse.Grammar.GrammarResolver.Resolve` | 语法规则生成的句子（事件日志、名称、描述），避免把规则词块拆散 |
| postfix | `Verse.LoadedLanguage.InjectIntoData_AfterImpliedDefs` | 全部 def 注入完成后扫尾（label/description/背景故事），覆盖未被翻译的 def |

为什么不在 `PlayDataLoader.DoPlayLoad` 上收尾：Mod 构造函数在 DoPlayLoad 帧内部执行，Harmony 不会回溯已进入的帧；`InjectIntoData_AfterImpliedDefs` 是 mod 构造之后才进入的帧，能可靠挂上。

## 喵化规则（`Meowifier.cs`）

- 逐行处理（`\n` 分段），多行文本每行都喵
- 中文句末标点 `。！？…` 之前插入「喵」；没有句末标点就在行尾补（避开行尾闭合标签、右引号/括号）
- 跳过：`<color=…>` 等标签内部、`{0}`/`{PROJECT_label}` 占位符、纯数字/纯空白/纯标点、单字符行
- 幂等：已经以「喵」结尾的不重复加；相同字符串缓存结果

## 构建

    dotnet build Source\Meowify\Meowify.csproj -c Release

产物自动复制到 `1.7\Assemblies\Meowify.dll`。

参考程序集：Krafs.Rimworld.Ref 1.7.4985-unstable + Lib.Harmony 3.0.0-rc.3（`ExcludeAssets="runtime"`，运行时不带 0Harmony）。

## 测试建议

- 主菜单、选项、Mod 列表（Keyed 文本）
- 物品/地块/植物信息卡（DefInjected）
- 事件日志与小人名字（GrammarResolver）
- 殖民者背景故事（BackstoryDef 扫尾）

## 版本号（三处一致）

1. `About\About.xml` 的 `<modVersion>`
2. `Source\Meowify\MeowifyMod.cs` 的 `const string Version`
3. `Source\Meowify\Meowify.csproj` 的 `<Version>`

## 发版

    powershell -NoProfile -ExecutionPolicy Bypass -File tools\make-release.ps1

产出到工作区的 `release\Meowify\`：`Meowify-<版本>\`（可直接放进 `Mods\`）与同名 zip。

## 许可

- 作者：Redbean
- 需要 RimWorld 1.7 与 Harmony（brrainz.harmony）
