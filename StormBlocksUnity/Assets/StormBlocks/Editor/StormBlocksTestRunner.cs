using System;
using UnityEditor;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;

namespace StormBlocks.Editor
{
    public static class StormBlocksTestRunner
    {
        private const string ResultsArgument = "-stormBlocksTestResults";
        private const string ResultsPrefsKey = "StormBlocks.BatchTestResultsPath";
        private const string ActivePrefsKey = "StormBlocks.BatchTestRunnerActive";

        [InitializeOnLoadMethod]
        private static void RegisterCallbacksAfterReload()
        {
            if (!Application.isBatchMode || !EditorPrefs.GetBool(ActivePrefsKey, false))
            {
                return;
            }

            string resultsPath = EditorPrefs.GetString(ResultsPrefsKey, string.Empty);
            if (string.IsNullOrEmpty(resultsPath))
            {
                return;
            }

            RegisterResultCallbacks(resultsPath);
        }

        public static void RunEditMode()
        {
            Run(TestMode.EditMode, "StormBlocks.EditMode.Tests");
        }

        public static void RunPlayMode()
        {
            Run(TestMode.PlayMode, "StormBlocks.PlayMode.Tests");
        }

        private static void Run(TestMode mode, string assemblyName)
        {
            string resultsPath = ResolveResultsPath(mode);
            Debug.Log("Storm Blocks invoking " + mode + " tests for " + assemblyName + " -> " + resultsPath);
            EditorPrefs.SetBool(ActivePrefsKey, true);
            EditorPrefs.SetString(ResultsPrefsKey, resultsPath);
            var api = ScriptableObject.CreateInstance<TestRunnerApi>();
            RegisterResultCallbacks(resultsPath);
            api.Execute(new ExecutionSettings(new Filter
            {
                testMode = mode,
                assemblyNames = new[] { assemblyName }
            }));
        }

        private static void RegisterResultCallbacks(string resultsPath)
        {
            var callbacks = ScriptableObject.CreateInstance<BatchCallbacks>();
            callbacks.Initialize(resultsPath);
            callbacks.hideFlags = HideFlags.HideAndDontSave;
            TestRunnerApi.RegisterTestCallback(callbacks);
        }

        private static string ResolveResultsPath(TestMode mode)
        {
            string[] args = Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length - 1; i++)
            {
                if (string.Equals(args[i], ResultsArgument, StringComparison.Ordinal))
                {
                    return args[i + 1];
                }
            }

            return mode == TestMode.PlayMode ? "playmode-results.xml" : "editmode-results.xml";
        }

        private sealed class BatchCallbacks : ScriptableObject, ICallbacks
        {
            [SerializeField] private string resultsPath;

            public void Initialize(string outputPath)
            {
                resultsPath = outputPath;
            }

            public void RunStarted(ITestAdaptor testsToRun)
            {
                Debug.Log("Storm Blocks test run started: " + testsToRun.FullName);
            }

            public void RunFinished(ITestResultAdaptor result)
            {
                TestRunnerApi.SaveResultToFile(result, resultsPath);
                Debug.Log("Storm Blocks test run finished: passed=" + result.PassCount + " failed=" + result.FailCount + " skipped=" + result.SkipCount);
                EditorPrefs.DeleteKey(ActivePrefsKey);
                EditorPrefs.DeleteKey(ResultsPrefsKey);
                if (Application.isBatchMode)
                {
                    int exitCode = result.FailCount == 0 ? 0 : 1;
                    EditorApplication.delayCall += delegate { EditorApplication.Exit(exitCode); };
                }
            }

            public void TestStarted(ITestAdaptor test)
            {
            }

            public void TestFinished(ITestResultAdaptor result)
            {
            }
        }
    }
}
