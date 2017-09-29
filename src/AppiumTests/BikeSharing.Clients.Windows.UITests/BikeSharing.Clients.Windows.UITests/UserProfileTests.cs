using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium.Remote;

namespace BikeSharing.Clients.Windows.UITests
{
    [TestClass]
    public class UserProfileTests
    {
        const string username = "eerli";
        const string password = "anything";
        const string AppiumURL = "http://127.0.0.1:4723/wd/hub";
        const string bikeAppID = "d318bc51-13f4-421d-8fd9-85759a7e7bb7_p8ehn122c0e4r!App";

        protected static RemoteWebDriver BikeAppSession;
        protected static RemoteWebElement CalculatorResult;

        [ClassInitialize]
        public static void Setup(TestContext context)
        {
            DesiredCapabilities appCapabilities = new DesiredCapabilities();
            appCapabilities.SetCapability("app", bikeAppID);
            appCapabilities.SetCapability("platformName", "Windows");
            appCapabilities.SetCapability("deviceName", "PC");
            BikeAppSession = new RemoteWebDriver(new Uri(AppiumURL), appCapabilities);
            Assert.IsNotNull(BikeAppSession);
            BikeAppSession.Manage().Timeouts().ImplicitlyWait(TimeSpan.FromSeconds(2));
        }

        [TestMethod]
        public void SignInScenarios()
        {
            // TODO: Add this once button state starts disabled
            // checkLoginButtonEnabled();

            SignInInvalid();
            SignInValid();
            Logout();
        }

        [ClassCleanup]
        public static void TearDown()
        {
            BikeAppSession.Dispose();
            BikeAppSession = null;
        }

        // login button enabled test
        public void checkLoginButtonEnabled()
        {
            BikeAppSession.FindElementByXPath("//Edit[@AutomationId=\"username\"]").Clear();
            BikeAppSession.FindElementByXPath("//Edit[@AutomationId=\"password\"]").Clear();
            Assert.IsFalse(BikeAppSession.FindElementByXPath("//Button[@AutomationId=\"signin\"]").Enabled);

            BikeAppSession.FindElementByXPath("//Edit[@AutomationId=\"password\"]").SendKeys("a");
            Assert.IsFalse(BikeAppSession.FindElementByXPath("//Button[@AutomationId=\"signin\"]").Enabled);

            BikeAppSession.FindElementByXPath("//Edit[@AutomationId=\"username\"]").SendKeys("a");

            // login button should be enabled
            Assert.IsTrue(BikeAppSession.FindElementByXPath("//Button[@AutomationId=\"signin\"]").Enabled);
        }
        
        public void SignInInvalid()
        {
            BikeAppSession.FindElementByXPath("//Edit[@AutomationId=\"username\"]").Clear();
            BikeAppSession.FindElementByXPath("//Edit[@AutomationId=\"username\"]").SendKeys("testuser");
            BikeAppSession.FindElementByXPath("//Edit[@AutomationId=\"password\"]").Clear();
            BikeAppSession.FindElementByXPath("//Edit[@AutomationId=\"password\"]").SendKeys("badpassword");
            BikeAppSession.FindElementByXPath("//Button[@AutomationId=\"signin\"]").Click();

            // Wait for login failure UI to appear
            System.Threading.Thread.Sleep(3000);
            BikeAppSession.FindElementByName("Try again").Click();

            // Verify we're still not Signed in
            Assert.IsNotNull(BikeAppSession.FindElementByXPath("//Edit[@AutomationId=\"username\"]"));

        }

        public void SignInValid()
        {
            BikeAppSession.FindElementByXPath("//Edit[@AutomationId=\"username\"]").Clear();
            BikeAppSession.FindElementByXPath("//Edit[@AutomationId=\"username\"]").SendKeys(username);
            BikeAppSession.FindElementByXPath("//Edit[@AutomationId=\"password\"]").Clear();
            BikeAppSession.FindElementByXPath("//Edit[@AutomationId=\"password\"]").SendKeys(password);
            BikeAppSession.FindElementByXPath("//Button[@AutomationId=\"signin\"]").Click();

            // Verify we're Signed in by finding the Events UI
            Assert.IsNotNull(BikeAppSession.FindElementByName("Events"));
        }

        public void Logout()
        {
            BikeAppSession.FindElementByXPath("//Button[@AutomationId=\"ContentTogglePane\"]").Click();
            System.Threading.Thread.Sleep(2000);

            BikeAppSession.FindElementByXPath("//Button[@AutomationId=\"logout\"]").Click();
//            BikeAppSession.FindElementByName("Logout").Click();

            // View the logout button worked
            System.Threading.Thread.Sleep(3000);
        }
    }
}
