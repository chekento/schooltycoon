using System;
using System.Linq;
using System.Reflection;

/// <summary>Runs the SAME NUnit domain tests without the Unity editor. Requires an NUnit assembly.</summary>
internal static class HeadlessTestRunner
{
    public static int Main()
    {
        Type fixture = typeof(KoSch.SchoolTycoon.Tests.SchoolSimulationTests);
        MethodInfo setup = fixture.GetMethods().Single(m => m.GetCustomAttributes(typeof(NUnit.Framework.SetUpAttribute), true).Any());
        var tests = fixture.GetMethods().Where(m => m.GetCustomAttributes(typeof(NUnit.Framework.TestAttribute), true).Any()).OrderBy(m => m.Name);
        int passed = 0, failed = 0;
        foreach (MethodInfo test in tests)
        {
            object instance = Activator.CreateInstance(fixture);
            try
            {
                setup.Invoke(instance, null); test.Invoke(instance, null);
                Console.WriteLine("PASS " + test.Name); passed++;
            }
            catch (Exception e)
            {
                Console.WriteLine("FAIL " + test.Name + ": " + (e.InnerException ?? e).Message); failed++;
            }
        }
        Console.WriteLine("\nDomain tests: " + passed + " passed, " + failed + " failed. Unity renderer/UI not exercised by this runner.");
        return failed == 0 ? 0 : 1;
    }
}
