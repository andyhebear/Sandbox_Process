using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;

namespace Sandbox_Process
{
    /// <summary>
    /// 根据Windows用户账号实现启动进程的权限控制，主要是文件访问
    /// </summary>
    public class AppWindowsAccountSandbox
    {
        /// <summary>
        /// 配置信息
        /// </summary>
        public class Option
        {
            /// <summary>
            /// 要启动的进程
            /// </summary>
            public string Programma;
            public string ProgrammaArgs = "";
            /// <summary>
            /// windows域
            /// </summary>
            public string WinDominion = "";            
            public string WINAccount = "";
            public string WINPassword = "";
#if DEBUG
            internal Option CreateTest() {          
                Option op = new Option() {
                     Programma="B:\\Temp\\ConsoleApp1.exe",
                     ProgrammaArgs="this is arg",
                     WinDominion="",
                     WINAccount= "sandbox_oj",
                     WINPassword= "sandbox_oj"
                };
                return op;
            }
#endif
        }
               
        private static readonly string[] VARS = new string[] {"OS", "PATHEXT", "PROCESSOR_ARCHITECTURE", "PROCESSOR_IDENTIFIER", "PROCESSOR_LEVEL",
                            "LOCALAPPDATA", "NUMBER_OF_PROCESSORS", "LOGONSERVER", "COMPUTERNAME","APPDATA", "HOMEDRIVE", "PUBLIC", "HOMEPATH",
                            "SystemDrive","SystemRoot","USERNAME", "USERPROFILE", "USERDOMAIN", "USERDOMAIN_ROAMINGPROFILE", "TMP", "TEMP"  };
        private  readonly string SandBoxFolder = @"B:\Temp\";
        public Option OpConfig {
            get;
            private set;
        }
        public void RunProramWithAccessControl(Option op,string sandboxDir) {
            this.OpConfig = op;
            if (!Directory.Exists(sandboxDir)) {
                try {
                    Directory.CreateDirectory(sandboxDir);
                }
                catch (Exception e) {
                    Console.WriteLine("Unable to create directory during installation. Error:" + e.ToString(), EventLogEntryType.Error);
                }
            }
            try {
                SetDirectoryAccessControl(SandBoxFolder, op.WINAccount);
                //SetDirectoryOwner(SandBoxFolder, UTENTE);
                //var errout = Console.Error;
                if (File.Exists(op.Programma)) {
                    //errout.WriteLine(Resources.ver);
                    SecureString SS = new SecureString();
                    if (op.WINPassword.Length > 0) {
                        foreach (char CARA in op.WINPassword.ToCharArray()) {
                            SS.AppendChar(CARA);
                        }
                    }
                    //errout.WriteLine(Resources.set);
                    ProcessStartInfo PSI;
                    PSI = new ProcessStartInfo(op.Programma, op.ProgrammaArgs);
                    PSI.WindowStyle = ProcessWindowStyle.Maximized;
                    PSI.WorkingDirectory = SandBoxFolder;
                    PSI.UseShellExecute = false;
                    PSI.UserName = op.WINAccount;
                    //PSI.Domain = DOMINIO;
                    PSI.Password = SS;
                    foreach (string VAR in VARS) {
                        //PSI.Environment.Remove(VAR);
                        PSI.EnvironmentVariables.Remove(VAR);
                    }
                    //PSI.Environment.Add("PATHEXT", ".EXE");
                    PSI.EnvironmentVariables.Add("PATHEXT", ".EXE");
                    Process.Start(PSI);
                    //errout.WriteLine(Resources.done);

                }
            }
            catch (Exception ex) { 
            
            }
        }

        #region 文件夹权限
        static void SetDirectoryOwner(string dirPath, string prisonUsername) {
            DirectorySecurity ds = new DirectorySecurity(dirPath, AccessControlSections.All);
            ds.SetOwner(new NTAccount(prisonUsername));
            var fs_rule = new FileSystemAccessRule(
                    prisonUsername, FileSystemRights.FullControl, InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit,
                    PropagationFlags.None, AccessControlType.Allow);
            ds.SetAccessRule(fs_rule);
        }
        static bool SetDirectoryAccessControl(string path, string userName) {
            try {
                var ds = new DirectorySecurity();//WindowsIdentity.GetCurrent().Name
                //var fs_rule = new FileSystemAccessRule(userName, FileSystemRights.FullControl,
                //                                       AccessControlType.Allow);
                var fs_rule = new FileSystemAccessRule(
                    userName, FileSystemRights.FullControl, InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit,
                    PropagationFlags.None, AccessControlType.Allow);
                ds.SetAccessRule(fs_rule);

                Directory.SetAccessControl(path, ds);

                return true;
            }
            catch {
                return false;
            }
        }
        /// <summary>
        /// 创建公开目录，谁都有权限访问
        /// </summary>
        /// <param name="dirPath"></param>
        static void CreateExposedDirectory(string dirPath) {
            if (!Directory.Exists(dirPath)) {
                try {
                    Directory.CreateDirectory(dirPath);
                }
                catch (Exception e) {
                    Console.WriteLine("Unable to create directory during installation. Error:" + e.ToString(), EventLogEntryType.Error);
                }
            }

            DirectoryInfo dirInfo = new DirectoryInfo(dirPath);

            SecurityIdentifier sid = new System.Security.Principal.SecurityIdentifier(System.Security.Principal.WellKnownSidType.WorldSid, null);
            NTAccount acct = sid.Translate(typeof(System.Security.Principal.NTAccount)) as System.Security.Principal.NTAccount;

            FileSystemAccessRule rule = new FileSystemAccessRule(acct.ToString(), FileSystemRights.FullControl, InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit, PropagationFlags.None, AccessControlType.Allow);
            if (!dirInfo.Exists) {
                DirectorySecurity security = new DirectorySecurity();
                security.SetAccessRule(rule);
                dirInfo.Create(security);
            }
            else {
                DirectorySecurity security = dirInfo.GetAccessControl();
                security.AddAccessRule(rule);
                dirInfo.SetAccessControl(security);
            }
        }
        #endregion
    }
}
