using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml;
using UnityEditor;
using UnityEditor.Android;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace NFShoot.Unity.Editor
{
    // All Gradle edits affect the generated project, preserving the host's templates.
    public sealed class NFShootAndroidBuild : IPreprocessBuildWithReport, IPostGenerateGradleAndroidProject
    {
        public int callbackOrder { get { return 10000; } }
        private const string MainActivity = "com.example.myapplication.MainActivity";
        private const string ApplicationClass = "com.example.myapplication.MyApplication";
        private const string Marker = "// NFSHOOT SDK MANAGED";
        private const string GradleJdkPreference = "NFShootSDK.GradleJdkHome";
        private static string SdkRoot { get { return Path.Combine(Application.dataPath, "NFShootSDK"); } }

        [MenuItem("Tools/NF Shoot SDK/Configure Android Player")]
        public static void ConfigurePlayer()
        {
            PlayerSettings.Android.minSdkVersion = (AndroidSdkVersions)28;
            PlayerSettings.Android.targetSdkVersion = (AndroidSdkVersions)36;
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARMv7 | AndroidArchitecture.ARM64;
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
            ConfigureObfuscator();
            ApplyEnvironmentTools();
            Debug.Log("NF Shoot SDK: Android API 28/36 and ARMv7/ARM64 configured. " +
                      "Keep Unity's embedded JDK 11; Gradle uses a separate JDK 17. Select external Gradle 8.13; SDK platforms;android-36 " +
                      "and build-tools;36.1.0 must be installed. Package ID, version and signing stay in Player Settings.");
        }

        private static void ConfigureObfuscator()
        {
            bool installed = false;
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                if (assembly.GetType("OPS.Obfuscator.Attribute.DoNotObfuscateClassAttribute") != null) installed = true;
            if (!installed) return;
            string defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android);
            if (!(";" + defines + ";").Contains(";NFSDK_OPS_OBFUSCATOR;"))
                PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android,
                    string.IsNullOrEmpty(defines) ? "NFSDK_OPS_OBFUSCATOR" : defines + ";NFSDK_OPS_OBFUSCATOR");
        }

        private static void ApplyEnvironmentTools()
        {
            string gradle = GetToolOption("NFSDK_GRADLE_HOME", "-nfSdkGradleHome");
            string sdk = GetToolOption("NFSDK_ANDROID_SDK_ROOT", "-nfSdkAndroidSdkRoot");
            if (!string.IsNullOrEmpty(gradle)) AndroidExternalToolsSettings.gradlePath = gradle;
            if (!string.IsNullOrEmpty(sdk)) AndroidExternalToolsSettings.sdkRootPath = sdk;
        }

        // GameCI v4 only forwards a fixed environment allowlist to Docker. Custom
        // parameters reach Unity and must refer to paths inside the container.
        private static string GetToolOption(string environmentName, string argumentName)
        {
            string[] args = Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length; i++)
            {
                if (!string.Equals(args[i], argumentName, StringComparison.OrdinalIgnoreCase)) continue;
                if (i + 1 >= args.Length || args[i + 1].StartsWith("-", StringComparison.Ordinal))
                    throw new BuildFailedException("NF Shoot SDK: " + argumentName + " requires a tool directory path.");
                return args[i + 1];
            }
            return Environment.GetEnvironmentVariable(environmentName);
        }

        private static bool HasArgument(string name)
        {
            foreach (string arg in Environment.GetCommandLineArgs())
                if (string.Equals(arg, name, StringComparison.OrdinalIgnoreCase)) return true;
            return false;
        }

        [MenuItem("Tools/NF Shoot SDK/Select Gradle JDK 17")]
        public static void SelectGradleJdk()
        {
            string path = EditorUtility.OpenFolderPanel("Select JDK 17 home (macOS: Contents/Home)", GetGradleJdk(), "");
            if (!string.IsNullOrEmpty(path)) EditorPrefs.SetString(GradleJdkPreference, path);
        }

        private static string GetGradleJdk()
        {
            string configured = GetToolOption("NFSDK_JAVA_HOME", "-nfSdkJavaHome");
            return string.IsNullOrEmpty(configured) ? EditorPrefs.GetString(GradleJdkPreference, "") : configured;
        }

        public void OnPreprocessBuild(BuildReport report)
        {
            if (report.summary.platform != BuildTarget.Android) return;
            if (HasArgument("-nfSdkCi"))
            {
                PlayerSettings.Android.minSdkVersion = (AndroidSdkVersions)Math.Max(28, (int)PlayerSettings.Android.minSdkVersion);
                PlayerSettings.Android.targetSdkVersion = (AndroidSdkVersions)36;
            }
            ApplyEnvironmentTools();
            ValidateTools();
            if ((int)PlayerSettings.Android.minSdkVersion < 28 ||
                (int)PlayerSettings.Android.targetSdkVersion != 36)
                throw new BuildFailedException("NF Shoot SDK: run Tools > NF Shoot SDK > Configure Android Player " +
                                               "to use the current project's minSdk 28 / targetSdk 36.");
        }

        private static void ValidateTools()
        {
            string javaHome = GetGradleJdk();
            string release = Path.Combine(javaHome, "release");
            var java = File.Exists(release) ? Regex.Match(File.ReadAllText(release), "JAVA_VERSION=\"(\\d+)") : Match.Empty;
            int major;
            if (!java.Success || !int.TryParse(java.Groups[1].Value, out major) || major < 17 || major > 23)
                throw new BuildFailedException("NF Shoot SDK requires a Gradle JDK 17-23 visible to Unity. " +
                    "Configured path: '" + javaHome + "'; release file exists: " + File.Exists(release) + ". " +
                    "For GameCI Docker use customParameters: -nfSdkJavaHome /github/workspace/.ci-tools/jdk. " +
                    "For local builds select Gradle JDK in the SDK menu or set NFSDK_JAVA_HOME. Keep Unity on embedded JDK 11.");
            ValidateTrustStore(javaHome);
            if (!File.Exists(Path.Combine(AndroidExternalToolsSettings.gradlePath, "lib", "gradle-launcher-8.13.jar")))
                throw new BuildFailedException("NF Shoot SDK requires external Gradle 8.13. Set it under Preferences > " +
                    "External Tools, or set NFSDK_GRADLE_HOME before starting Unity.");
            string sdk = AndroidExternalToolsSettings.sdkRootPath;
            if (!File.Exists(Path.Combine(sdk, "platforms", "android-36", "android.jar")) ||
                !Directory.Exists(Path.Combine(sdk, "build-tools", "36.1.0")))
                throw new BuildFailedException("NF Shoot SDK: install platforms;android-36 and build-tools;36.1.0 " +
                    "in the Android SDK selected in External Tools.");
            Debug.Log("NF Shoot SDK toolchain: Unity JDK=" + AndroidExternalToolsSettings.jdkRootPath +
                "; Gradle JDK=" + javaHome + " (Java " + major + "); Gradle=" +
                AndroidExternalToolsSettings.gradlePath + "; Android SDK=" + sdk);
        }

        private static void ValidateTrustStore(string javaHome)
        {
            string cacerts = Path.Combine(javaHome, "lib", "security", "cacerts");
            try
            {
                using (var stream = File.OpenRead(cacerts))
                    if (stream.Length == 0) throw new IOException("The trust store file is empty.");
            }
            catch (Exception error) when (error is IOException || error is UnauthorizedAccessException)
            {
                throw new BuildFailedException("NF Shoot SDK: Gradle JDK trust store is missing, empty or unreadable: " +
                    cacerts + ". Update Editor/CI/stage-gameci-jdk.py and prepare-gameci-tools.sh so host symlinks " +
                    "are copied as actual files visible inside Docker. " + error.Message);
            }
        }

        public void OnPostGenerateGradleAndroidProject(string path)
        {
            string root = Directory.GetParent(path).FullName;
            string resources = Path.Combine(SdkRoot, "Editor", "Android");
            foreach (string name in new[] { "nf-dependencies.gradle", "nf-repositories.gradle", "nf-build.gradle", "nf-validate-manifest.gradle" })
                File.Copy(Path.Combine(resources, name), Path.Combine(root, name), true);

            string baseGradle = Path.Combine(root, "build.gradle");
            string original = File.ReadAllText(baseGradle);
            string upgraded = Regex.Replace(original,
                "(id\\s+['\"]com\\.android\\.(?:application|library)['\"]\\s+version\\s+['\"])[^'\"]+(['\"])",
                "${1}8.13.1$2");
            upgraded = Regex.Replace(upgraded, "com\\.android\\.tools\\.build:gradle:[^'\"\\s]+",
                                     "com.android.tools.build:gradle:8.13.1");
            if (!upgraded.Contains("8.13.1"))
                throw new BuildFailedException("NF Shoot SDK could not locate AGP in the generated root build.gradle.");
            File.WriteAllText(baseGradle, upgraded);
            AppendManaged(baseGradle, "apply from: rootProject.file('nf-build.gradle')");
            AppendManaged(Path.Combine(root, "settings.gradle"), "apply from: new File(settingsDir, 'nf-repositories.gradle')");
            AppendManaged(Path.Combine(path, "build.gradle"), "apply from: rootProject.file('nf-dependencies.gradle')");
            string javaHome = GetGradleJdk();
            if (string.IsNullOrEmpty(javaHome))
                throw new BuildFailedException("NF Shoot SDK: select Gradle JDK 17 or set NFSDK_JAVA_HOME.");
            AppendManaged(Path.Combine(root, "gradle.properties"),
                "android.useAndroidX=true\nandroid.enableJetifier=true\norg.gradle.java.home=" + javaHome.Replace('\\', '/'));
            ConfigureEntryManifest(Path.Combine(path, "src", "main", "AndroidManifest.xml"));
        }

        private static void AppendManaged(string path, string body)
        {
            string marker = path.EndsWith(".properties", StringComparison.OrdinalIgnoreCase) ?
                "# NFSHOOT SDK MANAGED" : Marker;
            string text = File.Exists(path) ? File.ReadAllText(path) : "";
            text = Regex.Replace(text, Regex.Escape(marker) + " BEGIN[\\s\\S]*?" + Regex.Escape(marker) + " END\\r?\\n?", "");
            File.WriteAllText(path, text.TrimEnd() + "\n\n" + marker + " BEGIN\n" + body + "\n" + marker + " END\n");
        }

        private static void ConfigureEntryManifest(string path)
        {
            const string android = "http://schemas.android.com/apk/res/android";
            const string tools = "http://schemas.android.com/tools";
            var document = new XmlDocument();
            document.Load(path);
            XmlElement application = (XmlElement)document.SelectSingleNode("/manifest/application");
            if (application == null) throw new BuildFailedException("NF Shoot SDK: Unity application manifest is missing.");
            string existing = application.GetAttribute("name", android);
            if (!string.IsNullOrEmpty(existing) && existing != "android.app.Application" && existing != ApplicationClass)
                throw new BuildFailedException("NF Shoot SDK needs " + ApplicationClass + " but the host already uses " +
                    existing + ". Merge MyApplication.onCreate initialization into that Application before integrating.");
            application.SetAttribute("name", android, ApplicationClass);
            application.SetAttribute("usesCleartextTraffic", android, "true");
            // Preserve other tools:replace attributes contributed by the host.
            string replace = application.GetAttribute("replace", tools);
            if (!replace.Contains("android:usesCleartextTraffic"))
                application.SetAttribute("replace", tools, string.IsNullOrEmpty(replace) ?
                    "android:usesCleartextTraffic" : replace + ",android:usesCleartextTraffic");
            XmlElement launcher = null;
            var knownActivities = new List<XmlElement>();
            foreach (XmlElement activity in application.SelectNodes("activity"))
            {
                existing = activity.GetAttribute("name", android);
                if (activity.GetAttribute("node", tools) == "remove") continue;
                if (IsKnownUnityActivity(existing)) knownActivities.Add(activity);
                if (!IsLauncherActivity(activity)) continue;
                if (!IsKnownUnityActivity(existing))
                    throw new BuildFailedException("NF Shoot SDK: host launcher Activity is " + existing +
                        ". Integrate its lifecycle into MainActivity first; the package preserves the current Android Activity chain.");
                if (launcher == null || existing == MainActivity) launcher = activity;
            }
            if (launcher == null) throw new BuildFailedException("NF Shoot SDK: no Unity launcher Activity found in the generated manifest.");
            foreach (XmlElement activity in knownActivities)
            {
                if (activity == launcher) continue;
                MergeActivity(document, launcher, activity);
                application.RemoveChild(activity);
            }
            ImportLibraryActivitySettings(path, document, launcher);
            launcher.SetAttribute("name", android, MainActivity);
            launcher.SetAttribute("exported", android, "true");
            launcher.SetAttribute("screenOrientation", android, "portrait");
            launcher.SetAttribute("launchMode", android, "singleTask");
            launcher.SetAttribute("hardwareAccelerated", android, "false");
            launcher.SetAttribute("resizeableActivity", android, "false");
            foreach (XmlElement alias in application.SelectNodes("activity-alias"))
                if (IsKnownUnityActivity(alias.GetAttribute("targetActivity", android)))
                    alias.SetAttribute("targetActivity", android, MainActivity);

            // Suppress old entry points contributed by lower-priority plugin
            // manifests. Otherwise they can reappear after this file is patched.
            foreach (string name in new[] { "com.unity3d.player.UnityPlayerActivity", "com.google.firebase.MessagingUnityPlayerActivity" })
            {
                XmlElement removal = null;
                foreach (XmlElement activity in application.SelectNodes("activity"))
                    if (activity.GetAttribute("name", android) == name) removal = activity;
                if (removal == null)
                {
                    removal = document.CreateElement("activity");
                    removal.SetAttribute("name", android, name);
                    application.AppendChild(removal);
                }
                removal.SetAttribute("node", tools, "remove");
            }
            ConfigureNotificationActivity(document, application);
            document.Save(path);
        }

        private static bool IsKnownUnityActivity(string name)
        {
            return name == MainActivity || name == "com.unity3d.player.UnityPlayerActivity" ||
                   name == "com.google.firebase.MessagingUnityPlayerActivity";
        }

        private static bool IsLauncherActivity(XmlElement activity)
        {
            const string android = "http://schemas.android.com/apk/res/android";
            foreach (XmlElement filter in activity.SelectNodes("intent-filter"))
            {
                bool main = false, launcher = false;
                foreach (XmlElement action in filter.SelectNodes("action"))
                    if (action.GetAttribute("name", android) == "android.intent.action.MAIN") main = true;
                foreach (XmlElement category in filter.SelectNodes("category"))
                    if (category.GetAttribute("name", android) == "android.intent.category.LAUNCHER") launcher = true;
                if (main && launcher) return true;
            }
            return false;
        }

        private static void MergeActivity(XmlDocument document, XmlElement target, XmlElement source)
        {
            const string android = "http://schemas.android.com/apk/res/android";
            const string tools = "http://schemas.android.com/tools";
            foreach (XmlAttribute attribute in source.Attributes)
            {
                // Preserve the selected launcher's settings; copy additional host
                // settings such as theme and deep-link handling from known entries.
                if (attribute.NamespaceURI == tools || attribute.Prefix == "xmlns" ||
                    (attribute.LocalName == "name" && attribute.NamespaceURI == android)) continue;
                if (!target.HasAttribute(attribute.LocalName, attribute.NamespaceURI))
                    target.SetAttribute(attribute.LocalName, attribute.NamespaceURI, attribute.Value);
            }
            foreach (XmlNode child in source.ChildNodes)
            {
                var element = child as XmlElement;
                if (element == null) continue;
                bool exists = false;
                foreach (XmlNode current in target.ChildNodes)
                {
                    if (current.OuterXml == child.OuterXml) exists = true;
                    var metadata = current as XmlElement;
                    if (element.Name == "meta-data" && metadata != null && metadata.Name == "meta-data" &&
                        metadata.GetAttribute("name", android) == element.GetAttribute("name", android)) exists = true;
                }
                if (!exists) target.AppendChild(document.ImportNode(child, true));
            }
        }

        private static void ImportLibraryActivitySettings(string manifestPath, XmlDocument document, XmlElement launcher)
        {
            // Unity exports Android library plugin folders directly under unityLibrary.
            // Copy known entry filters before merger removal markers suppress them.
            var main = new DirectoryInfo(Path.GetDirectoryName(manifestPath));
            if (main.Name != "main" || main.Parent == null || main.Parent.Name != "src") return;
            string libraryRoot = main.Parent.Parent.FullName;
            foreach (string folder in Directory.GetDirectories(libraryRoot, "*.androidlib"))
            {
                string path = Path.Combine(folder, "src", "main", "AndroidManifest.xml");
                if (!File.Exists(path)) path = Path.Combine(folder, "AndroidManifest.xml");
                if (!File.Exists(path)) continue;
                var library = new XmlDocument();
                library.Load(path);
                foreach (XmlElement activity in library.SelectNodes("/manifest/application/activity"))
                    if (IsKnownUnityActivity(activity.GetAttribute("name", "http://schemas.android.com/apk/res/android")) &&
                        activity.GetAttribute("node", "http://schemas.android.com/tools") != "remove")
                        MergeActivity(document, launcher, activity);
            }
        }

        private static void ConfigureNotificationActivity(XmlDocument document, XmlElement application)
        {
            const string android = "http://schemas.android.com/apk/res/android";
            const string tools = "http://schemas.android.com/tools";
            const string key = "custom_notification_android_activity";
            XmlElement metadata = null;
            foreach (XmlElement candidate in application.SelectNodes("meta-data"))
                if (candidate.GetAttribute("name", android) == key)
                {
                    metadata = candidate;
                    break;
                }
            if (metadata == null)
            {
                metadata = document.CreateElement("meta-data");
                metadata.SetAttribute("name", android, key);
                application.AppendChild(metadata);
            }
            // Notifications cannot infer a custom Unity Activity when ad SDKs
            // contribute other Activities. Keep its explicit target in sync with
            // the launcher, overriding any older target in the notification library.
            metadata.RemoveAttribute("resource", android);
            metadata.SetAttribute("value", android, MainActivity);
            string replace = metadata.GetAttribute("replace", tools);
            if (!replace.Contains("android:value"))
                metadata.SetAttribute("replace", tools, string.IsNullOrEmpty(replace) ?
                    "android:value" : replace + ",android:value");
        }
    }
}
