using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting;
using System.Security;
using System.Security.Permissions;
using System.Security.Policy;
using System.Text;

namespace Sandbox_Process.Nequeo.Reflection
{
    /// <summary>
    /// 通用创建AppDomain域 限制权限,当前对象需要管理员权限.
    /// 请不要直接New()当前对象
    /// </summary>

    public static class AppDomainExecutor
    {
        public static void Execute(Action action) {
            AppDomain domain = null;

            try {
                domain = AppDomain.CreateDomain("New App Domain: " + Guid.NewGuid());

                AppDomainDelegate domainDelegate = (AppDomainDelegate)domain.CreateInstanceAndUnwrap(typeof(AppDomainDelegate).Assembly.FullName, typeof(AppDomainDelegate).FullName);
                domainDelegate.Execute(action);
            }
            finally {
                if (domain != null)
                    AppDomain.Unload(domain);
            }
        }

        public static void Execute<T>(T parameter, Action<T> action) {
            AppDomain domain = null;

            try {
                domain = AppDomain.CreateDomain("New App Domain: " + Guid.NewGuid());

                AppDomainDelegate domainDelegate = (AppDomainDelegate)domain.CreateInstanceAndUnwrap(typeof(AppDomainDelegate).Assembly.FullName, typeof(AppDomainDelegate).FullName);
                domainDelegate.Execute(parameter, action);
            }
            finally {
                if (domain != null)
                    AppDomain.Unload(domain);
            }
        }
        public static T Execute<T>(Func<T> action) {
            AppDomain domain = null;

            try {
                domain = AppDomain.CreateDomain("New App Domain: " + Guid.NewGuid());

                AppDomainDelegate domainDelegate = (AppDomainDelegate)domain.CreateInstanceAndUnwrap(typeof(AppDomainDelegate).Assembly.FullName, typeof(AppDomainDelegate).FullName);
                return domainDelegate.Execute(action);
            }
            finally {
                if (domain != null)
                    AppDomain.Unload(domain);
            }
        }

        public static TResult Execute<T, TResult>(T parameter, Func<T, TResult> action) {
            AppDomain domain = null;

            try {
                domain = AppDomain.CreateDomain("New App Domain: " + Guid.NewGuid());

                AppDomainDelegate domainDelegate = (AppDomainDelegate)domain.CreateInstanceAndUnwrap(typeof(AppDomainDelegate).Assembly.FullName, typeof(AppDomainDelegate).FullName);
                return domainDelegate.Execute(parameter, action);
            }
            finally {
                if (domain != null)
                    AppDomain.Unload(domain);
            }
        }

        public static TResult Execute<T, TResult>(T parameter, Func<T, AppDomain, TResult> action, string probingPath = null) {
            AppDomain domain = null;

            try {
                if (probingPath == null) {
                    domain = AppDomain.CreateDomain("New App Domain: " + Guid.NewGuid());
                }
                else {
                    domain = AppDomain.CreateDomain("New App Domain: " + Guid.NewGuid(), null, new AppDomainSetup { ApplicationName = "Mod-Bot Launcher", DynamicBase = new DirectoryInfo(probingPath).Parent.Parent.FullName, PrivateBinPath = probingPath });
                }

                AppDomainDelegate domainDelegate = (AppDomainDelegate)domain.CreateInstanceAndUnwrap(typeof(AppDomainDelegate).Assembly.FullName, typeof(AppDomainDelegate).FullName);
                return domainDelegate.Execute(parameter, domain, action);
            }
            finally {
                if (domain != null)
                    AppDomain.Unload(domain);
            }
        }
    }
    public class AppDomainDelegate : MarshalByRefObject
    {
        public void Execute(Action action) {
            action();
        }

        public void Execute<T>(T parameter, Action<T> action) {
            action(parameter);
        }
        public T Execute<T>(Func<T> action) {
            return action();
        }

        public TResult Execute<T, TResult>(T parameter, Func<T, TResult> action) {
            return action(parameter);
        }

        public TResult Execute<T, TResult>(T parameter, AppDomain domain, Func<T, AppDomain, TResult> action) {
            return action(parameter, domain);
        }
    }
    public class PluginSandbox
    {
        public interface IPlugin { 
        
        }
        private const string SandboxDomainName = "PluginSandboxDomain";
        private static AppDomain SandboxDomain;

        static PluginSandbox() {
            CreateAppDomain();
        }

        public static void CreateAppDomain() {
            var permSet = new PermissionSet(PermissionState.None);
            permSet.AddPermission(new SecurityPermission(SecurityPermissionFlag.Execution));
            var ptInfo = new AppDomainSetup {
                ApplicationBase = "."
            };

            var strongName = typeof(IPlugin).Assembly.Evidence.GetHostEvidence<StrongName>();

            SandboxDomain = AppDomain.CreateDomain(
                SandboxDomainName,
                AppDomain.CurrentDomain.Evidence,
                ptInfo,
                permSet,
                strongName);
        }

        public static void DeleteAppDomain() {
            AppDomain.Unload(SandboxDomain);
        }

        public static IPlugin LoadAddIn(byte[] addIn, string password) {
            var assembly = SandboxDomain.Load(addIn);
            foreach (var type in assembly.GetTypes()) {
                if (!type.GetInterfaces().Contains(typeof(IPlugin))) continue;
                return assembly.CreateInstance(type.FullName) as IPlugin;
            }
            return null;
        }
    }
//    public class PluginSandbox2 : IDisposable
//    {
//        AppDomain appDomain;
//        bool createDomain;

//        public PluginSandbox2(/*InstalledVersion installedVersion,*/string applicationPath, bool createDomain) {
//            this.createDomain = createDomain;

//            if (createDomain) {
//                var appDomainSetup = new AppDomainSetup();
//                //if (installedVersion.VsVersion != VsVersion.Vs2010) {
//                //    var configFile = Path.GetTempFileName();
//                //    File.WriteAllText(configFile, GenerateConfigFileContents(installedVersion.VsVersion));
//                //    appDomainSetup.ConfigurationFile = configFile;
//                //}

//                appDomainSetup.ApplicationBase = Path.GetDirectoryName(GetType().Assembly.Location);
//                appDomain = AppDomain.CreateDomain("PluginSandbox2", securityInfo: null, info: appDomainSetup);
//            }
//            else {
//                appDomain = AppDomain.CurrentDomain;
//            }

//            InitProbingPathResolver(appDomain, applicationPath);
//        }

//        public T CreateInstance<T>() {
//            return (T)appDomain.CreateInstanceFromAndUnwrap(typeof(T).Assembly.Location, typeof(T).FullName);
//        }

//        public void Dispose() {
//            if (createDomain) {
//                AppDomain.Unload(appDomain);
//            }
//        }

//        static void InitProbingPathResolver(AppDomain appDomain, string appPath) {
//            var resolverType = typeof(ProbingPathResolver);
//            var resolver = (ProbingPathResolver)appDomain.CreateInstanceFromAndUnwrap(
//                resolverType.Assembly.Location, resolverType.FullName);
//            var appDir = Path.GetDirectoryName(appPath);
//            var probingPaths = ".;PrivateAssemblies;PublicAssemblies".Split(';');
//            resolver.Init(appDir, probingPaths);
//        }

//        class ProbingPathResolver : MarshalByRefObject
//        {
//            private string Dir;
//            private string[] ProbePaths;

//            internal void Init(string dir, string[] probePaths) {
//                Dir = dir;
//                ProbePaths = probePaths;
//                AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
//            }

//            private Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args) {
//                var assemblyName = new AssemblyName(args.Name);
//                foreach (var probePath in ProbePaths) {
//                    var path = Path.Combine(Dir, probePath, assemblyName.Name + ".dll");
//                    if (File.Exists(path)) {
//                        return Assembly.LoadFrom(path);
//                    }
//                }

//                return null;
//            }
//        }

////        private static string GenerateConfigFileContents(VsVersion version) {
////            Debug.Assert(version != VsVersion.Vs2010);
////            const string contentFormat = @"
////<configuration>
////  <runtime>
////    <assemblyBinding xmlns=""urn:schemas-microsoft-com:asm.v1"">
////      <dependentAssembly>
////        <assemblyIdentity name=""Microsoft.VisualStudio.ExtensionManager"" publicKeyToken=""b03f5f7f11d50a3a"" culture=""neutral"" />
////        <bindingRedirect oldVersion=""10.0.0.0-{0}"" newVersion=""{0}"" />
////      </dependentAssembly>
////    </assemblyBinding>
////  </runtime>
////</configuration>
////";
////            return string.Format(contentFormat, GetAssemblyVersionNumber(version));
////        }

////        internal static string GetAssemblyVersionNumber(VsVersion version) {
////            return string.Format("{0}.0.0.0", VsVersionUtil.GetVersionNumber(version));
////        }
//    }

    //    internal class SandboxDomain : MarshalByRefObject
    //    {
    //        internal static readonly Dictionary<string, Assembly> LoadedLibraries = new Dictionary<string, Assembly>();
    //        internal static readonly List<string> LoadedAddons = new List<string>();
    //        internal static Assembly SDK = null;

    //        static SandboxDomain() {
    //            // Listen to requried events
    //            AppDomain.CurrentDomain.AssemblyResolve += DomainOnAssemblyResolve;
    //        }

    //        [PermissionSet(SecurityAction.Assert, Unrestricted = true)]
    //        internal static SandboxDomain CreateDomain(string domainName) {
    //            SandboxDomain domain = null;

    //            try {
    //                if (string.IsNullOrEmpty(domainName)) {
    //                    domainName = "Sandbox" + Guid.NewGuid().ToString("N") + "Domain";
    //                }

    //                // Initialize app AppDomainSetup
    //                var appDomainSetup = new AppDomainSetup {
    //                    ApplicationName = domainName,
    //                    ApplicationBase = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\"
    //                };

    //                // Initialize all permissions
    //                var permissionSet = new PermissionSet(PermissionState.None);
    //                permissionSet.AddPermission(new EnvironmentPermission(EnvironmentPermissionAccess.Read, "USERNAME"));
    //                permissionSet.AddPermission(new FileIOPermission(FileIOPermissionAccess.AllAccess, Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)));
    //                permissionSet.AddPermission(new FileIOPermission(FileIOPermissionAccess.AllAccess, SandboxConfig.DataDirectory));
    //                permissionSet.AddPermission(new FileIOPermission(FileIOPermissionAccess.AllAccess, SandboxConfig.LibrariesDirectory));
    //                permissionSet.AddPermission(new FileIOPermission(FileIOPermissionAccess.AllAccess, SandboxConfig.WrapperDllPath));
    //                permissionSet.AddPermission(new FileIOPermission(FileIOPermissionAccess.PathDiscovery, Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "..\\..\\..\\..\\..\\..\\"))));
    //                permissionSet.AddPermission(new FileIOPermission(FileIOPermissionAccess.Read, Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "..\\..\\..\\..\\..\\..\\"))));
    //                permissionSet.AddPermission(new ReflectionPermission(PermissionState.Unrestricted));
    //                permissionSet.AddPermission(new SecurityPermission(SecurityPermissionFlag.Execution));
    //                permissionSet.AddPermission(new SecurityPermission(SecurityPermissionFlag.Infrastructure));
    //                permissionSet.AddPermission(new SecurityPermission(SecurityPermissionFlag.RemotingConfiguration));
    //                permissionSet.AddPermission(new SecurityPermission(SecurityPermissionFlag.SerializationFormatter));
    //                permissionSet.AddPermission(new SecurityPermission(SecurityPermissionFlag.UnmanagedCode));
    //                permissionSet.AddPermission(new UIPermission(PermissionState.Unrestricted));
    //                /*permissionSet.AddPermission(new WebPermission(NetworkAccess.Connect, new Regex("https?:\\/\\/(\\w+)\\.lolnexus\\.com\\/.*")));
    //                permissionSet.AddPermission(new WebPermission(NetworkAccess.Connect, new Regex("https?:\\/\\/(\\w+)\\.riotgames\\.com\\/.*")));
    //                permissionSet.AddPermission(new WebPermission(NetworkAccess.Connect, new Regex("https?:\\/\\/(www\\.)?champion\\.gg\\/.*")));
    //                permissionSet.AddPermission(new WebPermission(NetworkAccess.Connect, new Regex("https?:\\/\\/(www\\.)?Agony\\.net\\/.*")));
    //                permissionSet.AddPermission(new WebPermission(NetworkAccess.Connect, new Regex("https?:\\/\\/edge\\.Agony\\.net\\/.*")));
    //                permissionSet.AddPermission(new WebPermission(NetworkAccess.Connect, new Regex("https?:\\/\\/(www\\.)?leaguecraft\\.com\\/.*")));
    //                permissionSet.AddPermission(new WebPermission(NetworkAccess.Connect, new Regex("https?:\\/\\/(www\\.)?lolbuilder\\.net\\/.*")));
    //                permissionSet.AddPermission(new WebPermission(NetworkAccess.Connect, new Regex("https?:\\/\\/(www\\.|raw.)?github(usercontent)?\\.com\\/.*")));
    //                permissionSet.AddPermission(new WebPermission(NetworkAccess.Connect, new Regex("https?:\\/\\/(www|oce|las|ru|br|lan|tr|euw|na|eune|sk2)\\.op\\.gg\\/.*")));
    //                permissionSet.AddPermission(new WebPermission(NetworkAccess.Connect, new Regex("https?:\\/\\/ddragon\\.leagueoflegends\\.com\\/.*")));
    //                permissionSet.AddPermission(new WebPermission(NetworkAccess.Connect, new Regex("http?:\\/\\/strefainformatyka\\.hekko24\\.pl\\/.*")));
    //                permissionSet.AddPermission(new WebPermission(NetworkAccess.Connect, new Regex("https?:\\/\\/strefainformatyka\\.hekko24\\.pl\\/.*")));*/

    //                // Load extra permissions if existing
    //                if (SandboxConfig.Permissions != null) {
    //                    foreach (IPermission permission in SandboxConfig.Permissions) {
    //                        // disabled due to security concerns
    //                        //permissionSet.SetPermission(permission);
    //                    }
    //                }
    //                /*
    //                #if DEBUG
    //                // TODO: Remove once protected domain works
    //                var appDomain = AppDomain.CreateDomain(domainName);
    //                #else
    //                // Create the AppDomain
    //                var appDomain = AppDomain.CreateDomain(domainName, null, appDomainSetup, permissionSet,
    //                    PublicKeys.AllKeys.Concat(new[] { Assembly.GetExecutingAssembly().Evidence.GetHostEvidence<StrongName>() }).ToArray());
    //                #endif
    //                */
    //                var appDomain = AppDomain.CreateDomain(domainName);

    //                // Create a new Domain instance
    //                domain = (SandboxDomain)Activator.CreateInstanceFrom(appDomain, Assembly.GetExecutingAssembly().Location, typeof(SandboxDomain).FullName).Unwrap();
    //                if (domain != null) {
    //                    domain.DomainHandle = appDomain;
    //                    domain.Initialize();
    //                }
    //            }
    //            catch (Exception e) {
    //                Logs.Log("Sandbox: An exception occurred creating the AppDomain!");
    //                Logs.Log(e.ToString());
    //            }

    //            return domain;
    //        }

    //        [PermissionSet(SecurityAction.Assert, Unrestricted = true)]
    //        internal static bool FindAddon(AssemblyName assemblyName, out string resolvedPath) {
    //            resolvedPath = "";

    //            foreach (var candidate in new[] { SandboxConfig.LibrariesDirectory, Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) }
    //                .Where(directory => directory != null && Directory.Exists(directory)).SelectMany(Directory.EnumerateFiles)) {
    //                try {
    //                    if (AssemblyName.GetAssemblyName(candidate).Name.Equals(assemblyName.Name)) {
    //                        resolvedPath = candidate;
    //                        return true;
    //                    }
    //                }
    //                catch (Exception) {
    //                    // ignored
    //                }
    //            }

    //            Logs.Log("Sandbox: Could not find addon '{0}'", assemblyName.Name);
    //            return false;
    //        }

    //        internal static bool IsSystemAssembly(string path) {
    //            return path.EndsWith(".dll") || Path.GetDirectoryName(path).EndsWith("Libraries");
    //        }

    //        internal static Assembly AddonLoadFrom(string path) {
    //            if (IsSystemAssembly(path)) {
    //                return Assembly.LoadFrom(path);
    //            }

    //            var buffer = File.ReadAllBytes(path);

    //            if (!buffer.IsDosExecutable()) {
    //                try {
    //                    buffer = SignedAddon.VerifyAndDecrypt(buffer);
    //                }
    //                catch (Exception e) {
    //                    Logs.Log("Sandbox: Unexpected exception when loading signed addon: {0}, Exception:", path);
    //                    Logs.Log(e.ToString());
    //                }
    //            }

    //            return buffer == null ? null : Assembly.Load(buffer);
    //        }

    //        [PermissionSet(SecurityAction.Assert, Unrestricted = true)]
    //        // ReSharper disable once InconsistentNaming
    //        private static void InitSDKBootstrap(Assembly sdk) {
    //            SDK = sdk;
    //        }

    //        [PermissionSet(SecurityAction.Assert, Unrestricted = true)]
    //        internal static void UnloadDomain(SandboxDomain domain) {
    //            AppDomain.Unload(domain.DomainHandle);
    //        }

    //        [PermissionSet(SecurityAction.Assert, Unrestricted = true)]
    //        internal static Assembly DomainOnAssemblyResolve(object sender, ResolveEventArgs args) {
    //#if DEBUG
    //            Logs.Log("Sandbox: Resolving '{0}'", args.Name);
    //#endif
    //            Assembly resolvedAssembly = null;

    //            try {
    //                // Don't handle resources
    //                if (args.Name.Contains(".resources")) {
    //                    return null;
    //                }

    //                // Get AssemblyName and special token
    //                var assemblyName = new AssemblyName(args.Name);
    //                var assemblyToken = assemblyName.GenerateToken();

    //                if (Assembly.GetExecutingAssembly().FullName.Equals(args.Name)) {
    //                    // Executing assembly
    //                    resolvedAssembly = Assembly.GetExecutingAssembly();
    //                }
    //                //else if (Sandbox.EqualsPublicToken(assemblyName, "7339047cb10f6e86"))
    //                else if (assemblyName.Name == "Agony") {
    //                    Console.WriteLine("Agony Token: " + assemblyName.GetPublicKeyToken().Select(o => o.ToString("x2")).Concat(new[] { string.Empty }).Aggregate(string.Concat));
    //                    // Agony.dll
    //                    resolvedAssembly = Assembly.LoadFrom(SandboxConfig.WrapperDllPath);
    //                }
    //                else {
    //                    string resolvedPath;
    //                    if (FindAddon(assemblyName, out resolvedPath)) {
    //#if DEBUG
    //                        Logs.Log("Sandbox: Successfully resolved '{0}'", assemblyName.Name);
    //#endif
    //                        if (LoadedLibraries.ContainsKey(assemblyToken)) {
    //                            resolvedAssembly = LoadedLibraries[assemblyToken];
    //                        }
    //                        else {
    //#if DEBUG
    //                            Logs.Log("Sandbox: Creating new instance '{0}'", assemblyName.Name);
    //#endif
    //                            // Load the addon into the app domain
    //                            //resolvedAssembly = Assembly.LoadFrom(resolvedPath); //AddonLoadFrom(resolvedPath);
    //                            resolvedAssembly = AddonLoadFrom(resolvedPath);

    //                            // Add the addon to the loaded addons dictionary
    //                            LoadedLibraries.Add(assemblyToken, resolvedAssembly);

    //                            if (resolvedAssembly.IsFullyTrusted) {
    //                                // Check if the DLL is the SDK
    //                                //if(assemblyName.Name == "Agony.SDK")
    //                                if (Sandbox.EqualsPublicToken(assemblyName, "a99070253df2afda")) {
    //                                    // Call bootstrap
    //                                    InitSDKBootstrap(resolvedAssembly);
    //                                }
    //                            }
    //                        }
    //                    }
    //                }
    //            }
    //            catch (Exception e) {
    //                Logs.Log("Sandbox: Failed to resolve addon:");
    //                Logs.Log(e.ToString());
    //            }

    //            if (resolvedAssembly != null && resolvedAssembly.IsFullyTrusted) {
    //#if DEBUG
    //                Logs.Log("Sandbox: Resolved assembly '{0}' is fully trusted!", resolvedAssembly.GetName().Name);
    //#endif
    //            }

    //            return resolvedAssembly;
    //        }

    //        [PermissionSet(SecurityAction.Assert, Unrestricted = true)]
    //        private static void OnUnhandledException(object sender, UnhandledExceptionEventArgs unhandledExceptionEventArgs) {
    //            Logs.Log("Sandbox: Unhandled addon exception:");
    //#if DEBUG
    //            var securityException = unhandledExceptionEventArgs.ExceptionObject as SecurityException;
    //            if (securityException != null) {
    //                Logs.Log(unhandledExceptionEventArgs.ExceptionObject.ToString());
    //            }
    //#endif
    //            Logs.PrintException(unhandledExceptionEventArgs.ExceptionObject);
    //        }

    //        // ==========================================================================================================
    //        // SandboxDomain Instance
    //        // ==========================================================================================================

    //        internal static SandboxDomain Instance { get; set; }

    //        internal AppDomain DomainHandle { get; private set; }

    //        internal void Initialize() {
    //            // Listen to unhandled exceptions
    //            DomainHandle.UnhandledException += OnUnhandledException;
    //        }

    //        internal void Start() {
    //            //SDK.GetType("Agony.SDK.Loading").GetMethod("Initialize", BindingFlags.Public | BindingFlags.Static).Invoke(null, new object[] {});
    //            // Call bootstrap
    //            SDK.GetType("Agony.SDK.Bootstrap").GetMethod("Init", BindingFlags.Public | BindingFlags.Static).Invoke(null, new object[] { SandboxConfig.PluginConfigs, SandboxConfig.CurrentProfile });
    //        }

    //        internal bool LoadAddon(string path, string[] args) {
    //            AssemblyName assemblyName = null;
    //            try {
    //                if (File.Exists(path)) {
    //                    // Get the AssemblyName of the addon by the path
    //                    assemblyName = AssemblyName.GetAssemblyName(path);
    //                    // Try to execute the addon
    //                    DomainHandle.ExecuteAssemblyByName(assemblyName, args);
    //                    if (!LoadedAddons.Contains(assemblyName.Name)) {
    //                        LoadedAddons.Add(assemblyName.Name);
    //                    }
    //                    return true;
    //                }
    //                else {
    //                    Logs.Log("Sandbox: Failed to load addon: File does not exists (" + path + ")");
    //                }
    //            }
    //            catch (MissingMethodException) {
    //                // The addon is a dll
    //                if (assemblyName != null && !LoadedLibraries.ContainsKey(assemblyName.GenerateToken())) {
    //                    try {
    //                        // Load the DLL
    //                        var assembly = DomainHandle.Load(assemblyName);
    //                        Console.WriteLine("Loaded " + assembly.FullName);
    //                        // Store the DLL into loaded addons
    //                        LoadedLibraries[assemblyName.GenerateToken()] = assembly;
    //                        if (assembly.IsFullyTrusted) {
    //                            // Verify that the DLL is the SDK
    //                            //if(assemblyName.Name == "Agony.SDK")
    //                            if (Sandbox.EqualsPublicToken(assemblyName, "a99070253df2afda")) {
    //                                // Call bootstrap
    //                                InitSDKBootstrap(assembly);
    //                            }
    //                        }
    //                        return true;
    //                    }
    //                    catch (Exception e) {
    //                        Logs.Log("Sandbox: Failed to call Bootstrap for Agony.SDK");
    //                        Logs.Log(e.ToString());
    //                    }
    //                }
    //            }
    //            catch (Exception e) {
    //                Logs.Log("Sandbox: Failed to load addon");
    //                Logs.Log(e.ToString());
    //            }
    //            return false;
    //        }

    //        [PermissionSet(SecurityAction.Assert, Unrestricted = true)]
    //        public override object InitializeLifetimeService() {
    //            return null;
    //        }
    //    }
}
