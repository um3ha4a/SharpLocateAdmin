# SharpLocateAdmin
```
优化了知道创宇404的SharpEventLog，用于通过windows event4624，event4625识别登录方，SharpEventLog没做过滤，会回显出很多噪音事件，故此做了优化，并且加入了自动识别windows日志版本，windows语言版本。支持多个.NET版本，自行编译即可。
```

## 使用方法：
```
 SharpLocateAdmin.exe -4624
 SharpLocateAdmin.exe -4625
```
 
## CobaltStrike:
```
 execute-assembly /path/to/SharpLocateAdmin.exe -4624
```
