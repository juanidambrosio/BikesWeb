using System;
using System.Collections.Generic;

namespace BikeSharing.Client.Kiosk.UWP.BikeSharingServices
{
    public class CheckoutConfirmation
    {
        public ResultType ResultType { get; private set; }
        public string ErrorCode { get; private set; }
        public DateTime CheckoutTime { get; private set; }
        public int BikeStationId { get; private set; }
        public int UserId { get; private set; }
        /// <summary>
        /// Id's of bikes that are checked out
        /// </summary>
        public List<int> Bikes { get; private set; }
        public decimal TotalCharge { get; private set; }

        public CheckoutConfirmation(
            ResultType resultType,
            string errorCode,
            DateTime checkoutTime,
            int bikeStationId,
            int userId,
            List<int> bikes,
            decimal totalCharge)
        {
            ResultType = resultType;
            ErrorCode = errorCode;
            CheckoutTime = checkoutTime;
            BikeStationId = bikeStationId;
            UserId = userId;
            Bikes = bikes;
            TotalCharge = totalCharge;
        }
    }

    public enum ResultType
    {
        Succeeded,
        Failed
    }
}
