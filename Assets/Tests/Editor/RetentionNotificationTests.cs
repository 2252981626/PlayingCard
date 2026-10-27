using System;
using System.Reflection;
using NUnit.Framework;
using Tiger.Retention;
using UnityEngine;

public class RetentionNotificationTests
{
    private static object Invoke(string method, params object[] arguments)
    {
        return typeof(RetentionNotification).GetMethod(method,
            BindingFlags.NonPublic | BindingFlags.Static).Invoke(null, arguments);
    }

    [SetUp]
    public void SetUp()
    {
        Invoke("ResetState");
        RetentionNotification.Setup(new RetentionConfig());
    }

    [TearDown]
    public void TearDown()
    {
        Invoke("ResetState");
    }

    [TestCase(22, 59, 0, 22, 59)]
    [TestCase(23, 0, 1, 10, 0)]
    [TestCase(0, 0, 0, 10, 0)]
    [TestCase(8, 59, 0, 10, 0)]
    [TestCase(9, 0, 0, 9, 0)]
    public void QuietHoursMoveOnlyNightNotificationsForward(int hour, int minute,
        int daysLater, int expectedHour, int expectedMinute)
    {
        var input = new DateTime(2026, 10, 27, hour, minute, 0);
        var expected = input.Date.AddDays(daysLater).AddHours(expectedHour).AddMinutes(expectedMinute);
        Assert.That((DateTime)Invoke("SafeFireTime", input), Is.EqualTo(expected));
    }

    [Test]
    public void ClickSourceIsConsumedOnceAcrossRepeatedResumes()
    {
        Assert.That(Invoke("ConsumeLaunchSource", 101, "retention:d1"), Is.EqualTo("d1"));
        Assert.That(Invoke("ConsumeLaunchSource", 101, "retention:d1"), Is.Null);
        Assert.That(Invoke("ConsumeLaunchSource", 102, "retention:d3"), Is.EqualTo("d3"));
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase("other:d1")]
    [TestCase("retention:")]
    public void InvalidClickPayloadDoesNotConsumeNotificationId(string payload)
    {
        Assert.That(Invoke("ConsumeLaunchSource", 101, payload), Is.Null);
        Assert.That(Invoke("ConsumeLaunchSource", 101, "retention:d7"), Is.EqualTo("d7"));
    }

    [TestCase(SystemLanguage.English)]
    [TestCase(SystemLanguage.Portuguese)]
    [TestCase(SystemLanguage.Indonesian)]
    public void GameMessagesWorkWithAndWithoutSavedProgress(SystemLanguage language)
    {
        var setup = Type.GetType("RetentionSetup, Assembly-CSharp", throwOnError: true);
        var config = (RetentionConfig)setup.GetMethod("CreateConfig",
            BindingFlags.NonPublic | BindingFlags.Static).Invoke(null, new object[] { language });

        Assert.That(config.AutoLaunchOnStart, Is.False, "The game owns permission timing and startup.");
        foreach (var message in config.Messages)
        {
            foreach (int progress in new[] { 0, 15 })
            {
                string title = (string)Invoke("Format", message.Title, message.TitleNoProgress, progress);
                string body = (string)Invoke("Format", message.Body, message.BodyNoProgress, progress);
                Assert.That(title, Is.Not.Empty);
                Assert.That(body, Is.Not.Empty);
                Assert.That(title + body, Does.Not.Contain("{progress}"));
            }
        }
    }

    [Test]
    public void EnteringGameRequestsPermissionWithoutWaitingForLevelCompletion()
    {
        var setupType = Type.GetType("RetentionSetup, Assembly-CSharp", throwOnError: true);
        var host = new GameObject("RetentionPermissionTest");
        try
        {
            var setup = host.AddComponent(setupType);
            setupType.GetMethod("Start", BindingFlags.NonPublic | BindingFlags.Instance)
                .Invoke(setup, null);
            Assert.That(setupType.GetField("_permissionRequested", BindingFlags.NonPublic | BindingFlags.Instance)
                .GetValue(setup), Is.True);
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(host);
        }
    }
}
