# Winform Template (Winform 模板程序)

自用的WInform C# .NET 模板程序，包含常用的、配置文件加载（JSON\XML\INI）、四级别的日志（利用log4net）、NPOI Excel交互工具、csvHelper交互工具、DB（Mysql）链接，单元测试组件、计算辅助库、MVVM设计模式、命令设计模式、发布版本管理等、项目按照标准C# .NET规范项目构建。

### 引用Package

| 名称                         | 介绍                                   | 版本    |
| ---------------------------- | -------------------------------------- | ------- |
| AntdUI                       | UI控件库                               | 1.6.10  |
| CsvHelper                    | CSV导出模块                            | 33.0.1  |
| LiveCharts.WinForms.NetCore3 | 图表UI控件库                           | 0.9.8   |
| log4net                      | 高性能日志库                           | 3.0.1   |
| Microsoft.NET.Test.Sdk       | 测试SDK                                | 17.10.0 |
| MySql.Data                   | Mysql数据支持库                        | 9.1.0   |
| MySqlConnector               | Mysql连接器                            | 2.3.7   |
| Newtonsoft.Json              | Json交互模块                           | 13.0.3  |
| NPOI                         | Excel交互模块（功能丰富）              | 2.7.1   |
| NUnit                        | 单元测试库（这个别改版本）             | 3.14.0  |
| NUnit3TestAdapter            | NUnit第三版的测试适配器（链接测试SDK） | 4.6.0   |
| System.Management            | 管理信息和管理事件器                   | 8.0.0   |



#### 快速chatGPT沟通

为了可以使chatGPT更好的理解问题，你使用这个项目进行自己的项目的二次开发时，可以使用以下的提示词从而提升回答的准确性

> 接下来我有一系列c# Winform程序的开发问题将会问你，为了使你的回复更加准确，我将使用如下技术栈：
> 1. NPOI，进行Excel交互的开发
> 2. csvHelper用于进行csv交互处理
> 3. AntdUI https://github.com/AntdUI/AntdUI 界面UI库
> 4. log4net 用于日志处理
> 5. Newtonsoft.Json 用于json的交互处理
> 6. NUnit、Nunit3TestAdapter、Microsoft.NET.Test.Sdk 用于进行单元测试
> 7. Microsoft.EntityFrameworkCore; EF框架，通过ORM操作MySQL数据库 
> 8. MySqlConnector 用于链接Mysql
> 9. LiveCharts2 https://livecharts.dev/docs/winforms/2.0.0-rc2/gallery 图表UI库
> 在你理解我使用的技术栈之后，我将会有很多相关的开发问题需要咨询，请你直接且准确的回复我