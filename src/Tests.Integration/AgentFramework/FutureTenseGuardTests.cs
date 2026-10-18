using Goodtocode.AgentFramework.Infrastructure.AgentFramework.Intents;

namespace Goodtocode.AgentFramework.Tests.Integration.AgentFramework;

[TestClass]
public sealed class FutureTenseGuardTests
{
    [TestMethod]
    [DataRow("I will perform a web search to find the latest information on .NET.")]
    [DataRow("I'll look that up for you.")]
    [DataRow("I’ll check now.")]
    [DataRow("I am going to search for that.")]
    [DataRow("Let me get that for you.")]
    [DataRow("One moment please.")]
    [DataRow("Please wait while the data is retrieved.")]
    [DataRow("We will review the results.")]
    [DataRow("I plan to query the actors.")]
    [DataRow("Next, I query the sessions.")]
    [DataRow("The tool will now search the index.")]
    public void ContainsFutureTenseDetectsAnnouncements(string reply)
    {
        Assert.IsTrue(FutureTenseGuard.ContainsFutureTense(reply));
        Assert.IsFalse(FutureTenseGuard.ContainsFutureTense(FutureTenseGuard.Enforce(reply)));
    }

    [TestMethod]
    [DataRow("Web search is not available.")]
    [DataRow("You have no chat sessions yet.")]
    [DataRow("I searched the actors and found 3 matches.")]
    [DataRow("The request was not completed.")]
    public void ContainsFutureTenseAllowsPastAndPresentTense(string reply)
    {
        Assert.IsFalse(FutureTenseGuard.ContainsFutureTense(reply));
        Assert.AreEqual(reply, FutureTenseGuard.Enforce(reply));
    }

    [TestMethod]
    public void EnforceKeepsCompliantSentencesAndTableRows()
    {
        var reply = "Found 1 actor. I will fetch more.\n| # | Name |\n| 1 | Ada |";

        var result = FutureTenseGuard.Enforce(reply);

        Assert.IsFalse(FutureTenseGuard.ContainsFutureTense(result));
        StringAssert.Contains(result, "Found 1 actor.");
        StringAssert.Contains(result, "| 1 | Ada |");
    }

    [TestMethod]
    public void EnforceReturnsFallbackWhenNothingCompliantRemains()
    {
        Assert.AreEqual(FutureTenseGuard.FallbackReply, FutureTenseGuard.Enforce("I will look into it. Let me check."));
    }
}
