# Owin.ServiceStack [![Build status](https://ci.appveyor.com/api/projects/status/12p77gegunmqlw5k?svg=true)](https://ci.appveyor.com/project/thedillonb/owin-servicestack)

Host ServiceStack (v3) as OWIN middleware. This library provides the necessary adapters to support any V3 ServiceStack application in an OWIN pipeline. The adapters convert the OWIN request/response objects into ServiceStack request/response objects and pass them to a special IAppHost implementation which does the rest!
