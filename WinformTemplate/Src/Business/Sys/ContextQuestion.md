﻿#  ContextQuestion
Context创建思路有2种
- 单一DbContext（包含所有模型）
- 多个DbContext（每个模型一个） 
~~~
代码参考：
	Context\Full: 就是单一Context创建思路模式
	Context：是按照每一个Model创建的多Context模式
~~~
## 单一DbContext
优点:

简化依赖注入 - 只需要注入一个DbContext
事务管理更简单 - 单一DbContext可以在一个事务中处理多个实体的变更
关联查询更简便 - 跨实体的查询不需要额外的上下文关联
减少代码重复 - 连接字符串、日志配置等只需要设置一次
资源利用更高效 - 减少了数据库连接数量和内存占用

缺点:

DbContext可能变得很大 - 随着模型增加，可能会变得臃肿
变更追踪开销增加 - 当有很多实体时，EF的变更追踪会有性能损耗
并发操作时可能出现问题 - 所有操作共享同一个DbContext可能导致并发问题

## 多个DbContext（每个模型一个）
优点:

关注点分离 - 每个DbContext关注特定的领域模型
减少变更追踪开销 - 每个上下文只追踪少量实体
更好的并发处理 - 不同的业务功能使用不同的上下文，减少冲突
更细粒度的控制 - 可以为不同的模型配置不同的连接字符串、重试策略等

缺点:

跨实体关系处理困难 - 处理多个DbContext间的关系需要额外工作
事务管理复杂 - 跨DbContext事务需要额外的分布式事务机制
代码冗余 - 需要重复DbContext的配置代码
可能导致更多的数据库连接 - 增加服务器负担

## 选择策略
- 如果应用规模较小，个人开发项目，不存在过多的同时写入，建议使用单一DbContext
- 如果应用规模较大，开发人员为复数，需要很多并行处理，需要进行权责分离，建议使用多个DbContext

本项目提供了单个Context和多个Context案例，但是后续关于Sys的功能编写是基于单一Context处理的。