using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Microsoft.Win32;

namespace SharpLocateAdmin
{
    class Program
    {
        static void Main(string[] args)
        {
            //Console.WriteLine("OS Version is:{0}", Environment.OSVersion.Version);
            Console.WriteLine("System Language is:{0}", System.Threading.Thread.CurrentThread.CurrentCulture.Name);
            int timezone = TimeZone.CurrentTimeZone.GetUtcOffset(System.DateTime.Now).Hours;
            string os_version = Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion", "ProductName", "").ToString();
            Console.WriteLine("OS Version is:{0}", os_version);

            if (timezone > 0) 
            {
                Console.WriteLine("Machine Time Zone is:UTC+{0}", timezone);
            }
            else
            {
                Console.WriteLine("Machine Time Zone is:UTC{0}", timezone);
            }
            
            //string os_version = Environment.OSVersion.Version.Major.ToString() + "." + Environment.OSVersion.Version.Minor.ToString();
            string system_language = System.Threading.Thread.CurrentThread.CurrentCulture.Name;

            if (args.Length == 0)
            {
                Console.WriteLine("Usage: SharpLocateAdmin.exe -4624");
                Console.WriteLine("       SharpLocateAdmin.exe -4625");
            }
            if (args.Length == 1 && (args[0] == "-4624"))
            {
                EventLog_4624(os_version, system_language);
            }
            if (args.Length == 1 && (args[0] == "-4625"))
            {
                EventLog_4625(system_language);
            }
        }

        public static void EventLog_4624(string OSVersion, string SystemLanguage)
        {
            EventLog log = new EventLog("Security");
            Console.WriteLine("\r\n========== SharpLocateAdmin -> 4624 ==========\r\n");
            var entries = log.Entries.Cast<EventLogEntry>().Where(x => x.InstanceId == 4624);
            string win7 = "Windows 7";
            string win8 = "Windows 8";
            string win10 = "Windows 10";
            string server08 = "Windows Server 2008";
            string server12 = "Windows Server 2012";
            string server16 = "Windows Server 2016";
            entries.Select(x => new
            {
                x.MachineName,
                x.Site,
                x.Source,
                x.Message,
                x.TimeGenerated
            }).ToList();

            if (SystemLanguage == "zh-CN") 
            {
                foreach (EventLogEntry elog in entries)
                {
                    string text = elog.Message;
                    string ipaddress = MidStrProcess(text, "源网络地址:", "源端口:");
                    if (ipaddress.Length < 7 || ipaddress.Length > 15 || ipaddress == "127.0.0.1")
                    { continue; }
                    string username_temp = MidStrProcess(text, "新登录:", "进程信息:");
                    string domain = MidStrProcess(username_temp, "帐户域:", "登录ID");
                    string username = "";
                    string logontype = "";
                    string hostname = "";
                    
                    if (OSVersion.Contains(win7) || OSVersion.Contains(server08))
                    {
                        hostname = MidStrProcess(text, "工作站名:", "源网络地址:");
                        username = MidStrProcess(username_temp, "帐户名:", "帐户域:");
                        logontype = MidStrProcess(text, "登录类型:", "新登录:");
                    }
                    else if (OSVersion.Contains(win8) || OSVersion.Contains(server12))
                    {
                        hostname = MidStrProcess(text, "工作站名:", "源网络地址:");
                        username = MidStrProcess(username_temp, "帐户名:", "帐户域:");
                        logontype = MidStrProcess(text, "登录类型:", "模拟级别:");
                    }
                    else if (OSVersion.Contains(win10) || OSVersion.Contains(server16))
                    {
                        hostname = MidStrProcess(text, "工作站名称:", "源网络地址:");
                        username = MidStrProcess(username_temp, "帐户名称:", "帐户域:");
                        logontype = MidStrProcess(text, "登录类型:", "受限制的管理员模式:");
                    }
                    else { Console.WriteLine("Windows Version Not Recognize!"); return; }

                    string logontypename;
                    switch (logontype)
                    {
                        case "2":
                            logontypename = "Interactive (logon at keyboard and screen of system)"; break;
                        case "3":
                            logontypename = "Network"; break;
                        case "4":
                            logontypename = "Batch"; break;
                        case "5":
                            logontypename = "Service"; break;
                        case "7":
                            logontypename = "Unlock"; break;
                        case "8":
                            logontypename = "NetworkCleartext"; break;
                        case "9":
                            logontypename = "NewCredentials(RunAs,Network Drive)"; break;
                        case "10":
                            logontypename = "RemoteInteractive (Terminal Services, Remote Desktop or Remote Assistance)"; break;
                        case "11":
                            logontypename = "CachedInteractive (logon with cached domain credentials such as when logging on to a laptop when away from the network)"; break;
                        default:
                            logontypename = "Logon Type Not Found!"; break;
                    }
                    DateTime Time = elog.TimeGenerated;
                    if (ipaddress.Length >= 7 && ipaddress.Length <= 16 && ipaddress != "127.0.0.1")
                    {
                        Console.WriteLine("\r\n-----------------------------------");
                        Console.WriteLine("Time: " + Time);
                        Console.WriteLine("LogonType:{0}:{1}", logontype, logontypename);
                        Console.WriteLine("Username:{0}\\{1}", domain, username);
                        Console.WriteLine("Hostname: " + hostname);
                        Console.WriteLine("ip: " + ipaddress);
                    }
                }
            }
            else if (SystemLanguage == "en-US")
            {
                foreach (EventLogEntry elog in entries)
                {
                    string text = elog.Message;
                    string ipaddress = MidStrProcess(text, "Source Network Address:", "Source Port:");
                    if (ipaddress.Length < 7 || ipaddress.Length > 15 || ipaddress == "127.0.0.1")
                    { continue; }
                    string username_temp = MidStrProcess(text, "New Logon:", "Process Information:");
                    string domain = MidStrProcess(username_temp, "AccountDomain:", "LogonID");
                    string username = "";
                    string logontype = "";
                    string hostname = "";

                    if (OSVersion.Contains(win7) || OSVersion.Contains(server08))
                    {
                        hostname = MidStrProcess(text, "Workstation Name:", "Source Network Address:");
                        username = MidStrProcess(username_temp, "AccountName:", "AccountDomain");
                        logontype = MidStrProcess(text, "Logon Type:", "New Logon:");
                    }
                    else if (OSVersion.Contains(win8) || OSVersion.Contains(server12))
                    {
                        hostname = MidStrProcess(text, "Workstation Name:", "Source Network Address:");
                        username = MidStrProcess(username_temp, "AccountName:", "AccountDomain");
                        logontype = MidStrProcess(text, "Logon Type:", "Impersonation Level:");
                    }
                    else if (OSVersion.Contains(win10) || OSVersion.Contains(server16))
                    {
                        hostname = MidStrProcess(text, "Workstation Name:", "Source Network Address:");
                        username = MidStrProcess(username_temp, "AccountName:", "AccountDomain:");
                        logontype = MidStrProcess(text, "Logon Type:", "Restricted Admin Mode:");
                    }

                    else { Console.WriteLine("Windows Version Not Recognize!"); return; }

                    string logontypename;
                    switch (logontype)
                    {
                        case "2":
                            logontypename = "Interactive (logon at keyboard and screen of system)"; break;
                        case "3":
                            logontypename = "Network"; break;
                        case "4":
                            logontypename = "Batch"; break;
                        case "5":
                            logontypename = "Service"; break;
                        case "7":
                            logontypename = "Unlock"; break;
                        case "8":
                            logontypename = "NetworkCleartext"; break;
                        case "9":
                            logontypename = "NewCredentials(RunAs,Network Drive)"; break;
                        case "10":
                            logontypename = "RemoteInteractive (Terminal Services, Remote Desktop or Remote Assistance)"; break;
                        case "11":
                            logontypename = "CachedInteractive (logon with cached domain credentials such as when logging on to a laptop when away from the network)"; break;
                        default:
                            logontypename = "Logon Type Not Found!"; break;
                    }
                    DateTime Time = elog.TimeGenerated;
                    if (ipaddress.Length >= 7 && ipaddress.Length <= 16 && ipaddress != "127.0.0.1")
                    {
                        Console.WriteLine("\r\n-----------------------------------");
                        Console.WriteLine("Time: " + Time);
                        Console.WriteLine("LogonType:{0}:{1}", logontype, logontypename);
                        Console.WriteLine("Username:{0}\\{1}", domain, username);
                        Console.WriteLine("Hostname: " + hostname);
                        Console.WriteLine("ip: " + ipaddress);
                    }
                }
            }
            else
            {
                Console.WriteLine("System Language Not Support:" + SystemLanguage);
            }
        }

        public static void EventLog_4625(string SystemLanguage)
        {
            EventLog log = new EventLog("Security");
            Console.WriteLine("\r\n========== SharpLocateAdmin -> 4625 ==========\r\n");
            var entries = log.Entries.Cast<EventLogEntry>().Where(x => x.InstanceId == 4625);
            entries.Select(x => new
            {
                x.MachineName,
                x.Site,
                x.Source,
                x.Message,
                x.TimeGenerated
            }).ToList();

            if (SystemLanguage == "zh-CN") 
            {
                foreach (EventLogEntry log1 in entries)
                {
                    string text = log1.Message;
                    string ipaddress = MidStrProcess(text, "源网络地址:", "源端口:");
                    string username_temp = MidStrProcess(text, "登录失败的帐户:", "失败原因:");
                    string domain = MidStrProcess(username_temp, "帐户域:", "失败信息:");
                    string hostname = MidStrProcess(text, "工作站名:", "源网络地址:");
                    string username = MidStrProcess(username_temp, "帐户名:", "帐户域:");
                    string logontype = MidStrProcess(text, "登录类型:", "登录失败的帐户:");

                    string logontypename;
                    switch (logontype)
                    {
                        case "2":
                            logontypename = "Interactive (logon at keyboard and screen of system)"; break;
                        case "3":
                            logontypename = "Network"; break;
                        case "4":
                            logontypename = "Batch"; break;
                        case "5":
                            logontypename = "Service"; break;
                        case "7":
                            logontypename = "Unlock"; break;
                        case "8":
                            logontypename = "NetworkCleartext"; break;
                        case "9":
                            logontypename = "NewCredentials(RunAs,Network Drive)"; break;
                        case "10":
                            logontypename = "RemoteInteractive (Terminal Services, Remote Desktop or Remote Assistance)"; break;
                        case "11":
                            logontypename = "CachedInteractive (logon with cached domain credentials such as when logging on to a laptop when away from the network)"; break;
                        default:
                            logontypename = "Logon Type Not Found!"; break;
                    }
                    DateTime Time = log1.TimeGenerated;

                    Console.WriteLine("\r\n-----------------------------------");
                    Console.WriteLine("Time: " + Time);
                    Console.WriteLine("LogonType:{0}:{1}", logontype, logontypename);
                    Console.WriteLine("Username:{0}\\{1}", domain, username);
                    Console.WriteLine("Hostname: " + hostname);
                    Console.WriteLine("ip: " + ipaddress);
                }
            }
            else if (SystemLanguage == "en-US") 
            {
                foreach (EventLogEntry log1 in entries)
                {
                    string text = log1.Message;
                    string ipaddress = MidStrProcess(text, "Source Network Address:", "Source Port:");
                    string username_temp = MidStrProcess(text, "Account For Which Logon Failed:", "Failure Reason:");
                    string domain = MidStrProcess(username_temp, "AccountDomain:", "FailureInformation:");
                    string hostname = MidStrProcess(text, "Workstation Name:", "Source Network Address:");
                    string username = MidStrProcess(username_temp, "AccountName:", "AccountDomain:");
                    string logontype = MidStrProcess(text, "Logon Type:", "Account For Which Logon Failed:");

                    string logontypename;
                    switch (logontype)
                    {
                        case "2":
                            logontypename = "Interactive (logon at keyboard and screen of system)"; break;
                        case "3":
                            logontypename = "Network"; break;
                        case "4":
                            logontypename = "Batch"; break;
                        case "5":
                            logontypename = "Service"; break;
                        case "7":
                            logontypename = "Unlock"; break;
                        case "8":
                            logontypename = "NetworkCleartext"; break;
                        case "9":
                            logontypename = "NewCredentials(RunAs,Network Drive)"; break;
                        case "10":
                            logontypename = "RemoteInteractive (Terminal Services, Remote Desktop or Remote Assistance)"; break;
                        case "11":
                            logontypename = "CachedInteractive (logon with cached domain credentials such as when logging on to a laptop when away from the network)"; break;
                        default:
                            logontypename = "Logon Type Not Found!"; break;
                    }
                    DateTime Time = log1.TimeGenerated;

                    Console.WriteLine("\r\n-----------------------------------");
                    Console.WriteLine("Time: " + Time);
                    Console.WriteLine("LogonType:{0}:{1}", logontype, logontypename);
                    Console.WriteLine("Username:{0}\\{1}", domain, username);
                    Console.WriteLine("Hostname: " + hostname);
                    Console.WriteLine("ip: " + ipaddress);
                }
            }
            else 
            {
                Console.WriteLine("System Language Not Support:" + SystemLanguage); 
            }
        }

        public static string MidStrProcess(string Source, string StartStr, string EndStr)
        {
            string result = string.Empty;
            int startindex, endindex;
            startindex = Source.IndexOf(StartStr);

            if (startindex == -1)
                return result;

            string tmpstr = Source.Substring(startindex + StartStr.Length);
            endindex = tmpstr.IndexOf(EndStr);

            if (endindex == -1)
                return result;

            result = tmpstr.Remove(endindex);
            string strResult = result.Replace("\n", "").Replace(" ", "").Replace("\t", "").Replace("\r", "");

            return strResult;
        }
    }
}