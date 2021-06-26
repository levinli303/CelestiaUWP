
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Windows;
using OpenQA.Selenium.Remote;
using Windows.ApplicationModel.Core;
using Windows.ApplicationModel.DataTransfer;
using Windows.System;
using Windows.UI.Core;

namespace CelestiaUWPTest
{
    [TestClass]
    public class UnitTest1
    {
        protected static string[] URLsToTest = new string[]
        {
            "cel://PhaseLock/Sol:Cassini/Sol:Saturn/2004-06-30T21:36:21.34826Z?x=ccpEjvb//////////////w&y=9+3vjvv//////////////w&z=6m32U////////////////w&ow=-0.5156361&ox=0.5204204&oy=-0.49717823&oz=-0.46486115&select=Sol:Cassini&fov=45.483105&ts=10&ltd=0&p=0&rf=203855107&nrf=255&lm=0&tsrc=0&ver=3",
            "cel://Follow/Sol:Jupiter/1997-11-11T02:57:38.68119Z?x=ADxHP+Q+dND//////////w&y=6OYY9EgpmQ&z=APip8LBZYt7//////////w&ow=0.4596731&ox=0.027018305&oy=0.8876392&oz=-0.008204532&select=Sol:Jupiter&fov=4.70441&ts=2&ltd=1&p=0&rf=134258071&nrf=255&lm=32768&tsrc=0&ver=3",
            "cel://Follow/NGC2237/2021-06-26T08:03:39.23761Z?x=AAAAAGDEJ4krmiAD&y=AAAAAAArivObqvYG&z=AAAAAAAxDtGUAQIV&ow=0.98484325&ox=0.1578347&oy=-0.07176972&oz=-0.0045886277&select=NGC2237&fov=29.498112&ts=1&ltd=0&p=0&rf=136615319&nrf=255&lm=16384&tsrc=0&ver=3",
        };

        [TestMethod]
        public void TestShowRenderSettings()
        {
            AppiumOptions options = new AppiumOptions();
            options.AddAdditionalCapability("app", "40507LinfengLi.Celestia_r7f2qt5a05s84!App");
            WindowsDriver<WindowsElement> session = new WindowsDriver<WindowsElement>(new Uri("http://127.0.0.1:4723"), options);

            Thread.Sleep(TimeSpan.FromSeconds(10));

            session.FindElementByAccessibilityId("Render").Click();
            session.FindElementByAccessibilityId("Settings").Click();
            SaveScreenshot("Info", session);
            session.Quit();
        }

        [TestMethod]
        public void TestURLs()
        {
            AppiumOptions options = new AppiumOptions();
            options.AddAdditionalCapability("app", "40507LinfengLi.Celestia_r7f2qt5a05s84!App");
            WindowsDriver<WindowsElement> session = new WindowsDriver<WindowsElement>(new Uri("http://127.0.0.1:4723"), options);

            Thread.Sleep(TimeSpan.FromSeconds(20));
            for (var i = 0; i < URLsToTest.Length; i++)
            {
                TestURL(URLsToTest[i], string.Format("URL{0}", i), session);
            }
            session.Quit();
        }

        public void TestURL(string url, string filename, WindowsDriver<WindowsElement> session)
        {
            session.FindElementByAccessibilityId("File").Click();
            session.FindElementByAccessibilityId("Input URL").Click();
            session.FindElementByAccessibilityId("InputText").SendKeys(url);
            session.FindElementByAccessibilityId("PrimaryButton").Click();
            Thread.Sleep(TimeSpan.FromSeconds(2));
            SaveScreenshot(filename, session);
        }

        public void SaveScreenshot(string filename, WindowsDriver<WindowsElement> session)
        {
            AppiumOptions options = new AppiumOptions();
            options.AddAdditionalCapability("app", "Root");
            WindowsDriver<WindowsElement> desktopSession = new WindowsDriver<WindowsElement>(new Uri("http://127.0.0.1:4723"), options);

            var path = Windows.Storage.ApplicationData.Current.LocalFolder.Path;
            var screenshotDir = string.Format("{0}\\{1}", path, "Screenshots");
            if (!Directory.Exists(screenshotDir))
                Directory.CreateDirectory(screenshotDir);
            desktopSession.GetScreenshot().SaveAsFile(string.Format("{0}\\{1}.png", screenshotDir, filename));
            System.Diagnostics.Debug.WriteLine(string.Format("Screenshot Directory: {0}", screenshotDir));
        }
    }
}
