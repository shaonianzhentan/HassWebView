# HassWebView
适配 Home Assistant 的 MAUI WebView 控件

[![NuGet Version](https://img.shields.io/nuget/v/HassWebView.Core.svg?style=flat-square&logo=nuget)](https://www.nuget.org/packages/HassWebView.Core)

MauiProgram.cs
```cs
using HassWebView.Core;

builder
.UseHassWebView()
.UseImmersiveMode() // 可选
.UseRemoteControl()  // 可选
.UseHassPage((sp, options) => { }) // 可选
.UseHttpServer(8125, (sp, server) => { }) // 可选
```

ToDO
- [x] 输入框注入文本
- [x] 创建 HTTP 服务
- [x] 监听资源加载