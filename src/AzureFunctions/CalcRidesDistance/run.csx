#r "System.Data"
#r "System.Configuration"

using System;
using System.Data.SqlClient;
using System.Configuration;


public static async Task Run(TimerInfo calcRidesDistanceTimer, TraceWriter log)
{
    log.Verbose("+++ begin function CalcRidesDistance  +++");
    var constr = ConfigurationManager.ConnectionStrings["RidesDb"].ConnectionString;
    using (var con = new SqlConnection(constr)) {
        con.Open();
        var sql = @"UPDATE R
                    SET R.GeoDistance = D.GeoDistance
                    FROM rides R
                    INNER JOIN 
                    (
                        SELECT RideId, SUM(GeoDistance) AS GeoDistance
                        FROM
                        (
                                SELECT RideId, 
                                CAST(LAG(GeoPosition, 1, GeoPosition) OVER (PARTITION BY RideId ORDER BY TS).STDistance(GeoPosition) AS INT) AS GeoDistance
                                FROM
                                (
                                        SELECT RP.*, 
                                            geography::STPointFromText('POINT(' + CAST(Longitude AS VARCHAR(20)) + ' ' + CAST(Latitude AS VARCHAR(20)) + ')', 4326) AS GeoPosition 
                                        FROM ridePositions RP 
                                        INNER JOIN rides R
                                        ON RP.RideId=R.Id
                                        WHERE R.GeoDistance IS NULL AND R.[Stop] IS NOT NULL
                                ) positions
                        ) distances
                        GROUP BY RideId
                    ) D
                    ON R.Id = D.RideId";
        var cmd = new SqlCommand(sql, con);
        var rows = await cmd.ExecuteNonQueryAsync();
        log.Info($"Routes distance calculated. {rows} affected");
        log.Verbose("+++ end function CalcRidesDistance +++");
    }  
}