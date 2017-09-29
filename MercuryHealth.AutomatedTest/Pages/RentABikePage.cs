using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using System;
using System.Linq;

namespace MercuryHealth.AutomatedTest.Pages
{
    public class RentABikePage : BasePage
    {
        public RentABikePage(IWebDriver driver)
        {
            _driver = driver;
        }

        #region Actions
        public CreatePage ClickCreateNewLink()
        {
            try
            {
                var createNewLink = _driver.FindElement(By.LinkText("Create New"));
                createNewLink.Click();
            }
            catch(Exception e)
            {
                Assert.Fail("Could not find create link: " + e.Message);
            }
            return new CreatePage(_driver);
        }

        public DeleteFoodPage ClickDeleteRide(string routeDescription)
        {
            try
            {
                // get the route table
                var routeTable = _driver.FindElement(By.ClassName("table"));
                // get all the rows in the table
                var rowList = routeTable.FindElements(By.TagName("tr"));
                // find all the rows with the route description
                var routeRow = rowList.Where(x => x.Text.Contains(routeDescription));

                if (routeRow.Count() == 0)
                {
                    Assert.Fail("Could not find the route in the table");
                }
                else
                {
                    // get the first route row and delete it
                    var firstFoodRow = routeRow.ElementAt(0);
                    // get the delete link
                    var deleteLink = firstFoodRow.FindElement(By.LinkText("Delete"));
                    deleteLink.Click();
                }
            }
            catch(Exception e)
            {
                Assert.Fail("Could not delete route: " + e.Message);
            }
            return new DeleteFoodPage(_driver);
        }

        public RentABikePage RemoveAllRoutes(string routeDescription)
        {
            try
            {
                // get the routes table
                var foodTable = _driver.FindElement(By.ClassName("table"));
                // get all the rows in the table
                var rowList = foodTable.FindElements(By.TagName("tr"));
                // find all the rows with the route description
                var foodRow = rowList.Where(x => x.Text.Contains(routeDescription));

                if (foodRow.Count() != 0)
                {
                    foreach(var row in foodRow)
                    {
                        var deleteLink = row.FindElement(By.LinkText("Delete"));
                        deleteLink.Click();
                        var deletePage = new DeleteFoodPage(_driver);
                        deletePage.ClickDeleteButton();
                    }
                }
                    
                    
            }
            catch (Exception e)
            {
                Assert.Fail("Could not delete route: " + e.Message);
            }
            
            return this;
        }

        public EditRoutePage ClickEditRideLink(string routeDescription)
        {
            try
            {
                // get the route table
                var rideTable = _driver.FindElement(By.ClassName("ride"));
                // get all the rows in the table
                var rowList = rideTable.FindElements(By.TagName("tr"));
                // find all the rows with the route description
                var rideRow = rowList.Where(x => x.Text.Contains(routeDescription));

                if (rideRow.Count() == 0)
                {
                    Assert.Fail("Could not find the ride in the table");
                }
                else
                {
                    // get the first route row and delete it
                    var firstFoodRow = rideRow.ElementAt(0);
                    // get the delete link
                    var editLink = firstFoodRow.FindElement(By.LinkText("Edit"));
                    editLink.Click();
                }
            }
            catch (Exception e)
            {
                Assert.Fail("Could not edit ride: " + e.Message);
            }
            return new EditRoutePage(_driver);
        }
        #endregion

        #region Verifications
        public RentABikePage VerifyRentABikePageReached()
        {
            try
            {
                var createNewLink = _driver.FindElement(By.XPath("/html/body/div[2]/p/a"));
                Assert.AreEqual("Create New", createNewLink.Text, "could not find create new link");
            }
            catch(Exception e)
            {
                Assert.Fail("Could not find create new link and verify nutrition page reached: " + e.Message);
            }

            return this;
        }

        public RentABikePage VerifyRideInTable(string description, int numTimes)
        {
            try
            {
                var foodTable = _driver.FindElement(By.ClassName("table"));
                var rowList = foodTable.FindElements(By.TagName("tr"));
                var numTimesFoodInTable = rowList.Where(row => row.Text.Contains(description)).Count();

                Assert.AreEqual(numTimes, numTimesFoodInTable, "The table is in the table the wrong number of times.");
            }
            catch(Exception e)
            {
                Assert.Fail("Verify ride failed: " + e.Message);
            }
            return this;
        }

        public RentABikePage VerifyRideNotInTable(string description)
        {
            try
            {
                var rideTable = _driver.FindElement(By.ClassName("table"));
                var rowList = rideTable.FindElements(By.TagName("tr"));
                var numTimesRideInTable = rowList.Where(row => row.Text.Contains(description)).Count();

                Assert.AreEqual(0, numTimesRideInTable, "Ride found in table, should not be in table");
            }
            catch (Exception e)
            {
                Assert.Fail("Verify ride not in table failed: " + e.Message);
            }
            return this;
        }

        public RentABikePage VerifyDistance(string rideDescription, string carbs)
        {
            try
            {
                var rideTable = _driver.FindElement(By.ClassName("table"));
                var rowList = rideTable.FindElements(By.TagName("tr"));
                var rideRow = rowList.FirstOrDefault(row => row.Text.Contains(rideDescription));
                
                if (rideRow == null)
                {
                    Assert.Fail("Could not find the ride row");
                }
                else
                {
                    var dataElements = rideRow.FindElements(By.TagName("td"));
                    var rideElement = dataElements[7];
                    Assert.AreEqual(carbs, rideElement.Text, "distance value is wrong.");
                    
                }
            }
            catch (Exception e)
            {
                Assert.Fail("Verify distance failed: " + e.Message);
            }
            return this;
        }
        #endregion
    }
}