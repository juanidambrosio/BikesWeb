using NUnit.Framework;
using Xamarin.UITest;

namespace BikeSharing.Clients.UITest.Android
{
    [TestFixture(Platform.Android)]
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
			app = ConfigureApp.Android.StartApp();

			app.Screenshot("App Launched");
		}

		[Test]
		public void LoginTest()
		{
            
			app.Tap("Username");
			app.Screenshot("Tap on the 'Username' TextField");
			app.EnterText("Jamesm");
			app.Screenshot("Typed in username 'Jamesm");
			app.DismissKeyboard();

			app.Screenshot("Dismissed Keyboard");

			app.Tap("Password");
			app.Screenshot("Tap on the 'Password' TextField");

			app.EnterText("Bikes360");
			app.Screenshot("Entered password");

			app.DismissKeyboard();
			app.Screenshot("Dismissed Keyboard");

			app.Tap("Sign in");
			app.Screenshot("Tap on 'Login'");
		}

		[Test]
		public void CheckBottomTabsTest()
		{
			LoginTest();

			TapOnItem("My Rides");
			TapOnItem("Upcoming ride");
			TapOnItem("Report");
			TapOnItem("Profile");
		}

		public void TapOnItem(string name)
		{
			app.Screenshot($"Tapped on {name}");

			app.Tap("OK");

			app.Tap(name);

			System.Threading.Thread.Sleep(1000);
			app.Back();
		}

		//[Test]
		//public void BookABikeToAquariumTest()
		//{
		//	LoginTest();

		//	app.Tap(x => x.Marked("home new ride"));
		//	// give the map some time to load and correctly position itself
		//	System.Threading.Thread.Sleep(5000);
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
		//	System.Threading.Thread.Sleep(5000);
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