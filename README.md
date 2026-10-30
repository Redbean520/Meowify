# Meowify（喵化世界）

RimWorld **1.7** 的「全员喵化」模组：不改动任何原始翻译文件，运行时把全部文本换成「QQ喵喵助手」那种猫娘腔——句尾带「喵」、人称替换、长句再加一个猫咪颜文字。需要 Harmony。

效果示例：

- 主菜单 `新建殖民地` → `新建殖民地喵`
- 物品信息 `高草（已生长60%）` → `高草喵（已生长60%喵）`
- `你的殖民者受到了伤害，需要治疗。` → `主人的殖民者受到了伤害，需要治疗喵。（｡･ω･｡）`
- `我的殖民地` → `本喵的殖民地喵`
- 事件/描述由语法规则生成，同样在输出处喵化
- 小人 / 动物 / 武器等**名字默认不喵**（`裴雨泽` 还是 `裴雨泽`），可以在设置里打开「给名字也加喵」
- `几句喵喵。` → `几句喵~。`（句末连续句尾词自动折叠成带 `~` 的一个）

## 目录

    About\             模组元数据（About.xml）
    1.7\Assemblies\    1.7（net10.0）编译产物 Meowify.dll（Harmony 由 Harmony 模组提供）
    Source\Meowify\    C# 工程（测试版才有，发布包不含）
    LoadFolders.xml
    tools\             打包脚本

## 截图

![主视觉](docs/screenshots/00-poster.jpg)

| ① 句尾喵 | ② 人称替换 |
|---|---|
| ![](docs/screenshots/06-feature-tail.jpg) | ![](docs/screenshots/07-feature-pronoun.jpg) |

![③ 随机颜文字](docs/screenshots/08-feature-kaomoji.jpg)

实机效果：

![实机效果](docs/screenshots/09-onscreen.jpg)

| 主菜单 | 殖民地界面 | 社交日志 |
|---|---|---|
| ![](docs/screenshots/01-main-menu.jpg) | ![](docs/screenshots/02-ingame.jpg) | ![](docs/screenshots/03-social-log.jpg) |

全部图在 `docs/screenshots/`（1920×1080 主视觉与特性图 + 1920×1080 实机截图），可直接用于工坊页面。

> v1.3 把设置界面从单页长滚动改成了「预设 / 句尾 / 人称 / 颜文字 / 范围 / 高级」六个标签页。`04-settings-presets.jpg` 与 `05-settings-preview.jpg` 仍是 1.2 版单页布局的旧图，六标签页真机截图待补（见 `docs/workshop.md`）。

## 安装

- 需要 **Harmony**（brrainz.harmony 的 1.7 版本，Harmony 3）
- 发布包解压后整个文件夹放进 `Mods/Meowify/`
- 游戏内模组列表勾选，重启

## 功能与设置

游戏内 **选项 → Mod 设置 → 喵化世界 Meowify**。

v1.3 起设置界面分成六个标签页，顶部固定标题与版本，底部固定「恢复默认」：

| 标签页 | 内容 |
|---|---|
| 预设 | 小喵一声 / 猫娘日常 / 喵力全开三个一键预设 + 三条实时预览 |
| 句尾 | 句尾开关、最短喵化字数、加喵概率、中文句尾词候选、英文句尾词候选、折叠连续句尾词与后缀 |
| 人称 | 替换开关 + 十个人称槽位（我/我们/你/你们/他/他们/她/她们/它/它们）+ 自定义替换表 + 保护词说明 |
| 颜文字 | 开关、概率、最短字数、按语境挑、语境池说明、额外颜文字候选 |
| 范围 | 四个适用范围开关、给名字也加喵、关键词白名单 / 黑名单 |
| 高级 | 额外保护词、排除片段、版本号 |

| 设置 | 默认 | 说明 |
|---|---|---|
| 一键预设 | 猫娘日常 | 小喵一声 = 只加句尾喵；猫娘日常 = QQ 喵喵助手风格；喵力全开 = 句尾词更多、颜文字更密 |
| 句尾加语气词 | 开 | 按中英文标点断句，在句末标点前插入句尾词；没有标点时补在行尾 |
| 最短喵化字数 | 2 | 少于这个可见字数的文本不加句尾词，避免按钮、图标说明被撑破 |
| 加喵概率 % | 100 | 100 = 每一句都加（默认）；调低则只给一部分句子加喵，适合嫌太密的玩家。按原文哈希决定，同一句结果固定 |
| 句尾词候选 | `喵` | 每行一个，随机挑一个；留空 = 不加句尾词（全文原样） |
| 英文句尾词 | `meow` / `nya` / `mrrp` | 只在英文界面生效，插入时自动补前导空格（`Hello.` → `Hello meow.`） |
| 替换人称代词 | 开 | 默认 `我`→`本喵`、`你`→`主人`（`我们`/`你们` 一并变复数）；第三人称默认留空，也就是 `他 / 她 / 它` 保持原样，想要猫娘腔可以在「人称」页填 |
| 第一 / 第二 / 第三人称 | `本喵` / `主人` / 空 | 十个槽位：我、我们、你、你们、他、他们、她、她们、它、它们。复数留空 = 自动「单数 + 们」；任意槽位留空 = 那个词不替换 |
| 自定义替换表 | 空 | 每行 `原词=替换词`，和内置人称一起参与替换，长词优先（例：`俺=人家`、`俺们=咱们家`） |
| 长句追加颜文字 | 开 | 可见字数达到阈值、且含句读的文本，末尾追加一个猫咪颜文字 |
| 触发概率 % | 100 | 100 = 每句都加，0 = 不加；按原文哈希决定，同一句结果固定 |
| 颜文字最短字数 | 10 | 全文可见字符数达到这个长度才追加，避免把信息卡撑破 |
| 额外颜文字候选 | 空 | 每行一个；留空就只用内置的 90 个社区颜文字 |
| 额外保护词 | 空 | 含「我 / 你」但不是人称代词的词，替换前整体跳过；内置 迷你 / 自我 / 忘我 / 无我 / 敌我 / 你我 / 你死我活 / 你争我夺 / 其他 / 其它 / 吉他 / 他乡 / 他人 |
| 排除片段 | 空 | 每行一个片段；文本里出现任一片段就整段原样返回（不喵化、不换人称），给个别模组的特殊文本留后门 |
| 给名字也加喵 | 关 | 小人 / 动物 / 武器 / 艺术品等由语法生成的名字默认保持原样；想要连名字一起喵（`裴雨泽` → `裴雨泽喵`）才打开 |
| 适用范围：界面文本 | 开 | 对应补丁 1（Keyed 文本）。关掉后按钮、提示、标签保持原样 |
| 适用范围：定义注入文本 | 开 | 对应补丁 2（DefInjected）。关掉后物品/特性/研究等 def 字段保持原样 |
| 适用范围：语法生成句子 | 开 | 对应补丁 3（GrammarResolver）。关掉后事件日志、社交描述等生成文本保持原样 |
| 适用范围：定义字段兜底 | 开 | 对应补丁 4（def 扫尾）。关掉后未被翻译的 def 文本保持原样 |
| 折叠连续句尾词 | 开 | 句末连续重复的句尾词折叠成一个，并接上「折叠后的后缀」 |
| 折叠后的后缀 | `~` | 折叠后接的尾巴，默认 `~`（所以是 `喵~`）；留空就只留一个「喵」 |
| 只喵这些词 | 空 | 白名单，每行一个；填了就只喵化包含这些词的文本（例如只填「殖民者」），其余保持原样 |
| 不要喵这些词 | 空 | 黑名单，每行一个；命中就整段原样返回，优先级高于白名单 |
| 按语境挑颜文字 | 开 | 内置 8 类语境池（可爱 / 战斗 / 社交 / 工作 / 食物 / 动物 / 难过 / 恋爱，约 90 个来自社区合集的全角颜文字），按原文关键词挑应景的，再叠自定义候选 |
| 额外颜文字候选 | 空 | 每行一个，和语境池一起参与挑选；语境池关掉时用这里的 + 内置可爱池 |

细节：

- 设置改完立即生效并清缓存；关闭设置窗口时自动保存
- 「预览」区用当前设置实时喵化三句示例，不用进游戏试
- 颜文字按原文 FNV-1a 哈希确定性选取：同一句话永远同一个颜文字，不会每帧闪烁
- 句末连续重复的句尾词会自动折叠成一个并接上后缀（`几句喵喵。` → `几句喵~。`），句中正常的「喵喵叫」不受影响
- 名字（语法关键字 `r_name` / `r_weapon_name` / `r_art_name` 等，含 `name`）默认跳过；「给名字也加喵」打开后名字也加喵
- 白名单 / 黑名单在最终判定处生效：黑名单命中整段原样返回，优先级高于白名单
- 语境颜文字：文本命中「食物 / 战斗 / 社交 / 工作 / 太空 / 天气」关键词时从对应池里按哈希挑，否则用通用池
- 四个适用范围开关分别对应四个补丁，关掉哪一层哪一层完全原样，方便和别的模组共存
- 人称替换是单次从左到右扫描、长词优先：`他`→`他喵` 之后不会被再次匹配成「他喵喵」，所以重复处理同一句也稳定
- 内置颜文字来自 [kaomoji-collection](https://github.com/kaomojiya-collection/kaomoji-collection)（MIT）等社区合集，筛掉了可能缺字形的泰文/兼容字形，只留常见全角颜文字
- 幂等：已经是喵化结果的文本不会被二次处理

## 原理

全部在运行时完成，四个 Harmony 补丁（`Source\Meowify\HarmonyPatches.cs`）：

| 补丁 | 目标 | 作用 |
|---|---|---|
| postfix | `Verse.LoadedLanguage.TryGetTextFromKey` | 界面 Keyed 文本（按钮、提示、标签） |
| prefix | `Verse.DefInjectionPackage.InjectIntoDefs` | DefInjected 注入（物品/特性/研究/事件等 def 字段），只喵可见文本，跳过 RulePackDef、占位符与机器字段 |
| postfix | `Verse.Grammar.GrammarResolver.Resolve` | 语法规则生成的句子（事件日志、描述），避免把规则词块拆散；名字关键字（含 `name`）默认跳过 |
| postfix | `Verse.LoadedLanguage.InjectIntoData_AfterImpliedDefs` | 全部 def 注入完成后扫尾（label/description/背景故事），覆盖未被翻译的 def |

为什么不在 `PlayDataLoader.DoPlayLoad` 上收尾：Mod 构造函数在 DoPlayLoad 帧内部执行，Harmony 不会回溯已进入的帧；`InjectIntoData_AfterImpliedDefs` 是 mod 构造之后才进入的帧，能可靠挂上。

名字的判定不靠反射：所有名字都走 `Verse.NameGenerator.GenerateName` → `GrammarResolver.Resolve(rootKeyword, …)`，关键字是 `r_name` / `r_weapon_name` / `r_art_name` / `r_deityName` 这类含 `name` 的字符串，所以在语法出口按关键字跳过即可。

## 喵化规则（`Meowifier.cs`）

- 逐行处理（`\n` 分段），多行文本每行都喵
- 中文句末标点 `。！？…` 之前插入「喵」；没有句末标点就在行尾补（避开行尾闭合标签、右引号/括号）
- 人称替换只作用在可见文本段：`<color=…>` 等标签、`{0}`/`{PROJECT_label}` 占位符原样保留；单次从左到右扫描，长词优先，替换结果不参与再次匹配（`他`→`他喵` 不会变 `他喵喵`）
- 句尾词从候选里随机挑（按原文字符串的 FNV-1a 哈希取模），同一句稳定不变；文本已带任一候选词即视为已处理
- 人称替换前先用私有区字符（U+E000/U+E001）把保护词整体遮住，替换完再还原，避免「迷你」「自我」「敌我」被拆坏
- 颜文字只加给「足够长 + 含句读」的文本，按原文哈希决定是否触发、选哪一个，插在行尾空白之前
- 句尾词是否插入、颜文字是否触发都支持按原文哈希的概率控制（默认 100% 全中）
- 句末/行尾若已连续出现同一个句尾词，会折叠成一个并接上 `CollapseSuffix`（默认 `~`，即 `喵喵。`→`喵~。`；句中不动）
- 「冒号 + 占位符」结尾的模板（`Possible unwanted relation: {0}`、`版本：{0}`）不补喵，避免代入名字后看着像名字被喵化；`粗糙的{0}`、`{0}或{1}` 这类仍照常补
- 名字类语法（rootKeyword 含 `name`）默认直接返回原文，除非打开「给名字也加喵」
- 白名单（只喵这些词）非空时，文本必须含其中任一词才喵化；黑名单（不要喵这些词）命中则整段原样返回，优先级更高
- 语境颜文字：命中战斗 / 难过 / 食物 / 动物 / 恋爱 / 社交 / 工作关键词时从对应池挑选，否则用可爱池；自定义候选一起参与
- 文本命中「排除片段」时整段原样返回，连人称都不换
- 跳过：纯数字/纯空白/纯标点、单字符行
- 幂等：已经以「喵」结尾、或已经带颜文字结尾的文本不重复处理；相同字符串缓存结果（上限 5 万条）

## 英文界面（English UI）

游戏语言是英文时自动切到英文模式（检测 `LanguageDatabase.activeLanguage.folderName` 是否以 `English` 开头；换语言需要重启，游戏本身也是这样要求的）：

- 句尾词换成 `meow` / `nya` / `mrrp`（可在「句尾」页改），插入时自动带前导空格：`Hello.` → `Hello meow.`
- 英文模式把 `.` 也算句末标点，但夹在两个数字之间的点不算：`Version 1.7 is out.` → `Version 1.7 is out meow.`，不会拆坏版本号
- 设置界面与提示文案走 `MeowifyText.cs` 的双语表（键是中文原文，缺词条回退中文），英文界面下显示英文
- 不做英文人称替换：`I / you` 换成猫娘腔会破坏语法，人称表只对中文代词生效
- 颜文字池中英共用，按语境挑选对英文关键词同样生效（`enemy` / `food` / `cat`……）

中文界面行为完全不变。

## 构建

    dotnet build Source\Meowify\Meowify.csproj -c Release

产物自动复制到 `1.7\Assemblies\Meowify.dll`。

参考程序集：Krafs.Rimworld.Ref 1.7.4985-unstable + Lib.Harmony 3.0.0-rc.3（`ExcludeAssets="runtime"`，运行时不带 0Harmony）。

## 测试建议

- 主菜单、选项、Mod 列表（Keyed 文本）
- 物品/地块/植物信息卡（DefInjected）
- 事件日志与描述（GrammarResolver）
- 小人 / 动物 / 武器名字默认不带喵；在设置里打开「给名字也加喵」后重启，名字应也带喵
- 殖民者背景故事（BackstoryDef 扫尾）
- 异种族模组（HAR / 绮罗族等）的小人贴图是否正常

## 兼容性

- 补丁 2 只改「可见文本」字段：路径命中 `.label`/`.description`/`.title`/`.text` 等白名单，或值里含中文时才喵；颜色通道名、贴图路径这类纯 ASCII 机器字段（`colorChannels.N.name`、`texPath`、`graphicPath`、`shaderType`……）一律原样保留。
- 这条规则是为修绮罗族（Kiiro + HAR）贴图 bug 加的：HAR 在代码里用字面量 `"skin"`/`"hair"` 取颜色通道，Kiiro 的中文语言包把这几个通道名当字符串注入，旧版会把它们喵成 `skin喵`/`hair喵`，导致脸和头发取不到颜色而不绘制（看起来就是透明）。
- 语法规则串（RulePackDef）仍在 `GrammarResolver.Resolve` 出口统一处理，不在词块层面拆散。

## 版本号与发版

- 日常改动（修 bug、调文本、小重构）**不升版本号**，只提交测试版仓库；版本号保持当前发布版本（1.3.0）不动，也不重新打包。
- 当前发布版本：**1.3.0**（2026-10-30 打包，`release\Meowify\Meowify-1.3.0` 与同名 zip）。后续改动继续在测试版迭代，确认发版时再升号。
- 只有三种情况才升版本，且升之前先跟 Redbean 确认：
  1. 功能阶段完成、要交付或上传工坊；
  2. Redbean 明确说「发版」；
  3. 有不兼容改动（存档 / 配置 / 接口），必须升并说明影响。
- 升版本动作固定：三处同步 → 打包 → 打 tag
  1. `About\About.xml` 的 `<modVersion>`
  2. `Source\Meowify\MeowifyMod.cs` 的 `const string Version`
  3. `Source\Meowify\Meowify.csproj` 的 `<Version>`
  4. `powershell -NoProfile -ExecutionPolicy Bypass -File tools\make-release.ps1`，产出到 `release\Meowify\`：`Meowify-<版本>\`（可直接放进 `Mods\`）与同名 zip
  5. `git tag v<版本>` 并推送
- 号段语义：`1.0.x` 修 bug / 文本微调；`1.x.0` 新功能；`2.0.0` 不兼容或大改。

## 发布物料

- 工坊发布文案：`docs/workshop.md`（v1.3.0 可直接复制的标题 / 摘要 / BBCode 正文 / 标签 / 截图顺序 / 发布清单）
- 预览图：`About/Preview.png`（1280×720 插画封面，工坊首图；`docs/screenshots/00-poster.jpg` 是它的 1920×1080 版本）
- 截图：`docs/screenshots/`（00 主视觉 + 06/07/08 特性图 + 09 实机效果 + 01–05 真机截图）
- 上传流程：按「版本号与发版」三处同步 → `powershell -NoProfile -ExecutionPolicy Bypass -File tools\make-release.ps1` → 上传 zip → 工坊生成 PublishedFileId 后写进 `About/PublishedFileId.txt`（已被 .gitignore 忽略）。
- ⚠️ **更新已有工坊条目时不要丢 `About/PublishedFileId.txt`**：`make-release.ps1` 现在会把它一并带进发布包，游戏靠它更新原条目；缺了会被当成新 mod，新建第二个工坊条目（订阅、评论、合集分家）。首次发布时该文件不存在，上传成功后由游戏写入，之后一直保留在 mod 根目录。

## 参考 / 相关工作

- [qq790795623-sudo/qq-meow-assistant](https://github.com/qq790795623-sudo/qq-meow-assistant)（QQ喵喵助手）：本模组的风格来源——`我`→`本喵`、`你`→`主人` 人称替换、按标点断句句尾加`喵`、消息末尾随机猫咪颜文字（可自定义）、编辑不重复处理。它是 Android 无障碍应用，这里只借它的规则与默认词表，实现完全重写为 Harmony 运行时补丁。
- [csh1668/AutoTranslation](https://github.com/csh1668/AutoTranslation)：运行时机器翻译未翻译文本。它同样把注入点放在 `LoadedLanguage.InjectIntoData_BeforeImpliedDefs` / `InjectIntoData_AfterImpliedDefs`（与本模组的时机判断一致），区别是它把译文写回 keyed / DefInjected 字典；本模组在输出处做变换，不改游戏数据。
- [RimWorld-zh/RimTrans](https://github.com/RimWorld-zh/RimTrans)：成熟的离线翻译工具（导出 / 合并 / 校验），如果以后想把喵化做成静态语言包，可以用它批处理。
- [sgoudham/uwuifyy](https://github.com/sgoudham/uwuifyy)：通用 uwu 文本变形实现，标点与大小写处理可参考。
- [kaomojiya-collection/kaomoji-collection](https://github.com/kaomojiya-collection/kaomoji-collection)：4 万多条、500 多个分类的颜文字合集（MIT）。内置的 90 个颜文字从它的分类 JSON 里筛出来，按语境分成 8 个池。
- 官方机制：Dev 模式「生成翻译报告」+ 未翻译文本列表，可用来核对喵化覆盖率。

## 许可

- 作者：Redbean
- 需要 RimWorld 1.7 与 Harmony（brrainz.harmony）
