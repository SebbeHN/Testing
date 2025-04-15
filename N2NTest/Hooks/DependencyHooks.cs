using System;
using System.Threading.Tasks;
using N2NTest.Helpers;
using TechTalk.SpecFlow;
using Xunit;

public class DependencyHooks
{
    private static bool _adminCreateCompleted = false;
    
    [AfterScenario("adminCreate")]
    public void MarkAdminCreateComplete()
    {
        _adminCreateCompleted = true;
    }

    [BeforeScenario("dependsOn:adminCreate")]
    public void EnsureAdminCreateComplete()
    {
        if (!_adminCreateCompleted)
            throw new Exception("Skipping test: required adminCreate feature hasn't run yet");
    }
}