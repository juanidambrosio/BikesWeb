using NUnit.Framework;
using Xamarin.UITest;

namespace BikeSharing.Clients.UITest.iOS
{
    [TestFixture(Platform.iOS)]
    public class Tests
    {
        IApp app;
        Platform platform;

        public Tests(Platform platform)
        {
            this.platform = platform;
        }

        [SetUp]
        public void BeforeEachTest()
        {
            app = ConfigureApp.iOS.StartApp();
            System.Threading.Thread.Sleep(1000);
            app.Screenshot("App Launched");
        }

        [Test]
        public void LoginTest()
        {
            app.Tap("Username");
            System.Threading.Thread.Sleep(1000);
            app.Screenshot("Tap on the 'Username' TextField");

            app.EnterText("Jamesm");
            System.Threading.Thread.Sleep(1000);
            app.Screenshot("Typed in username");

            app.DismissKeyboard();
            System.Threading.Thread.Sleep(1000);
            app.Screenshot("Dismissed Keyboard");

            app.Tap("Password");
            System.Threading.Thread.Sleep(1000);
            app.Screenshot("Tap on the 'Password' TextField");

            app.EnterText("Bikes360");
            System.Threading.Thread.Sleep(1000);
            app.Screenshot("Entered password");

            app.DismissKeyboard();
            System.Threading.Thread.Sleep(1000);
            app.Screenshot("Dismissed Keyboard");

            app.Tap("Sign in");
            System.Threading.Thread.Sleep(10000);
            app.Screenshot("Tap on Sign In button");
        }

        [Test]
        public void CheckBottomTabsTest()
        {
            LoginTest();

            TapOnItem("My Rides");

            TapOnItem("Profile");

            TapOnItem("Home");
        }

        public void TapOnItem(string name)
        {
            app.Tap(name);
            System.Threading.Thread.Sleep(5000);
            app.Screenshot($"Tapped on {name}");
        }

        //[Test]
        //public void BookABikeToAquariumTest()
        //{
        //	LoginTest();

        //	app.Tap(x => x.Marked("home new ride"));
        //	// give the map some time to load and correctly position itself
        //	System.Threading.Thread.Sleep(10000);
        //	app.Screenshot("Tapped Book a New Ride button");

        //	app.Tap(x => x.Marked("3rd Ave & Broad St"));
        //	app.Screenshot("Tapped map marker near Space Needle");

        //	app.Tap(x => x.Marked("Seattle Aquarium / Alaskan Way S & Elliott Bay Trail"));
        //	app.Screenshot("Tapped map marker near Seattle Aquarium");

        //	app.Tap(x => x.Marked("Go"));
        //	app.Screenshot("Tapped GO");

        //	app.Tap(x => x.Text("Book a bike"));
        //	app.Screenshot("Tapped Book a Bike");
        //}

        //[Test]
        //public void BookABikeToPier66Test()
        //{
        //	LoginTest();

        //	app.Tap(x => x.Marked("home new ride"));
        //	// give the map some time to load and correctly position itself
        //	System.Threading.Thread.Sleep(10000);
        //	app.Screenshot("Tapped Book a New Ride button");

        //	app.Tap(x => x.Marked("3rd Ave & Broad St"));
        //	app.Screenshot("Tapped map marker near Space Needle");

        //	app.Tap(x => x.Marked("Pier 66 / Alaskan Way & Bell St"));
        //	app.Screenshot("Tapped map marker near Seattle Aquarium");

        //	app.Tap(x => x.Marked("Go"));
        //	app.Screenshot("Tapped GO");

        //	app.Tap(x => x.Text("Book a bike"));
        //	app.Screenshot("Tapped Book a Bike");
        //}

    }
}
