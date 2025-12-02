# HassWebView
适配 Home Assistant 的 MAUI WebView 控件


MauiProgram.cs
```cs
using HassWebView.Core;

builder
.UseHassWebView()
.UseImmersiveMode() // 可选
.UseRemoteControl();  // 可选
```

ToDO
- [ ] 输入框注入文本
- [ ] 创建 HTTP 服务
- [ ] 监听资源加载