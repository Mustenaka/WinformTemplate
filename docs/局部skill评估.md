# 项目局部 Skill 评估

结论：可以，而且已经创建 WinformTemplate 项目局部 skill。当前项目已经形成稳定的模块边界、重复文件结构和守卫测试，适合把“按业务要求新增或修改模块”的流程固化成 skill。

当前 skill 位置：

```text
D:\Work\Code\CSharp\WinformTemplate\.codex\skills\winformtemplate-business-modifier\SKILL.md
```

## 适合制作的原因

- 架构约束明确：`UI -> ViewModel -> Service -> Repository` 是主路径。
- 新业务模块有可复制样板：Product 适合常规模块，DemoNote 适合三数据源对照。
- 数据源实现有固定三件套：`EfCore`、`WebApi`、`Local`。
- 菜单和权限同步点固定：`SysDatabaseInitializer`、`MockData`、`PageRegistryDefaultPages`。
- 测试入口清晰：启动集成、导航守卫、Repository、ViewModel。
- 服务端契约集中在 `docs/api-contract.md`，适合让 skill 强制先改契约再改客户端/服务端代码。

## Skill 应覆盖的任务

建议把 skill 设计成“WinformTemplate 业务修改助手”，覆盖这些场景：

- 根据业务描述新增一个常规模块。
- 给现有模块加字段、筛选条件、排序或批量操作。
- 给模块补 EF / WebAPI / Local 仓储实现。
- 同步菜单、权限 seed 和页面注册。
- 更新 `api-contract.md` 并在服务端补 Minimal API 端点。
- 补充或更新对应测试。
- 在修改完成后给出必须运行的验证命令。

## Skill 不应承诺的范围

- 不应承诺完全自动设计复杂 UI。AntdUI WinForms 页面仍需要结合现有控件和布局手工检查。
- 不应在没有需求细节时推断权限模型、字段约束或业务状态语义。
- 不应绕过现有架构去引入新的 MVVM 框架、ORM 模式或前端技术栈。
- 不应替代测试运行。skill 可以规定测试清单，但修改后仍要执行命令验证。

## 推荐触发方式

后续可以这样使用：

```text
按 WinformTemplate 项目规则，新增一个订单管理模块：
- 字段：订单号、客户名、金额、状态、创建时间
- 支持搜索、状态筛选、分页、排序
- 支持 EF 和 Local，WebAPI 先补契约和客户端仓储
- 菜单放在业务管理下，仅 admin 可见
```

理想情况下，不再需要每次附带大段架构 prompt；skill 负责提醒和执行固定步骤。

## 建议 Skill 内容

建议创建一个 `SKILL.md`，核心指令包括：

```text
name: winformtemplate-business-modifier
description: Modify the WinformTemplate WinForms client and optional WinformTemplateServer backend following the repository architecture.
```

主要规则：

- 先阅读 `README.md`、`docs/项目架构与文件结构.md`、`docs/二开指南.md`、`docs/api-contract.md`。
- 若涉及服务端，同时阅读 `D:\Work\Code\CSharp\WinformTemplateServer\README.md` 和 `src/WinformTemplateServer/Program.cs`。
- 新常规模块优先复制 Product 模式。
- 三数据源演示或数据源对照优先复制 DemoNote 模式。
- 所有查询条件下推到 Repository。
- 菜单 key 必须同步 EF seed、Local seed、PageRegistry。
- WebAPI 变更先更新契约，再改客户端仓储和服务端端点。
- 修改完成后运行相关测试，至少说明未运行的测试及原因。

## 使用方式

在 Codex 中建议显式触发：

```text
Use $winformtemplate-business-modifier，根据下面业务要求修改项目：
...
```

如果当前环境没有自动发现仓库局部 skill，可以直接引用文件路径：

```text
使用 D:\Work\Code\CSharp\WinformTemplate\.codex\skills\winformtemplate-business-modifier\SKILL.md 的规则。
```

## 落地位置

当前已放在：

```text
D:\Work\Code\CSharp\WinformTemplate\.codex\skills\winformtemplate-business-modifier\SKILL.md
```

如果 Codex 只扫描用户级 skill，则把同样内容安装到 `$CODEX_HOME/skills` 下，并在 skill 中写死本项目路径和服务端路径。

## 预期收益

- 减少后续需求 prompt 中重复描述架构、数据源和菜单同步规则。
- 降低新增模块时漏改 seed、漏注册页面、漏补测试的概率。
- 让“业务要求 -> 代码修改 -> 测试验证 -> 文档更新”的流程稳定下来。

## 评估建议

建议制作。这个项目的重复改动模式已经足够稳定，局部 skill 会有实际收益。第一版不需要追求覆盖全部场景，先覆盖“新增常规模块”和“修改现有模块字段/筛选/菜单/API”即可。等后续真实修改中发现高频遗漏，再把规则补进 skill。
