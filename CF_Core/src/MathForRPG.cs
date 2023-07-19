using System;
public class MathForRPG
{
    public static double GetMultiplierEx(int attackerLevel, int victimLevel, double max = 3.0, double maxM = 100.0, double tolerance = 0.1, double exponent = 1.25)
    {
        double aLvL = victimLevel * (1.0 - tolerance);

        double output = Math.Min(Math.Pow(attackerLevel / aLvL, exponent) * (attackerLevel / (maxM * max)), max);

        return output;
    }
    public static double CalculateExpectedWinProbability(int ratingA, int ratingV, double kFactor = 1.0)
    {
        // Calculate the difference in ratings between the two players
        double ratingDiff = ratingA - ratingV;

        // Calculate the expected win probability for player 1
        double expectedWinProbability = 1 / (1 + Math.Pow(10, ratingDiff / 400.0));

        // Adjust the expected win probability based on the kFactor value
        expectedWinProbability *= kFactor;

        // Return the adjusted win probability
        return expectedWinProbability;
    }
}
