using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using System;

namespace MercuryHealth.AutomatedTest.Pages
{
    public class DeleteRoutePage : BasePage
    {
        public DeleteRoutePage(IWebDriver driver)
        {
            _driver = driver;
        }

        #region Actions
        public ExercisePage ClickDeleteButton()
        {
            try
            {
                var deleteButton = _driver.FindElement(By.ClassName("btn"));
                deleteButton.Click();
            }
            catch(Exception e)
            {
                Assert.Fail("Delete exercise failed: " + e.Message);
            }
            return new ExercisePage(_driver);
        }
        #endregion
        
        #region Verification
        public DeleteRoutePage VerifyDeleteExercisePageReached()
        {
            try
            {
                var deleteHeader = _driver.FindElement(By.XPath("/html/body/div[2]/h2"));
            }
            catch(Exception e)
            {
                Assert.Fail("Delete page is not reached: " + e.Message);
            }
            return this;
        }
        #endregion
    }
}