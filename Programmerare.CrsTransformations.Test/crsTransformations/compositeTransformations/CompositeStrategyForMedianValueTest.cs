using System;
using System.Linq;
using System.Collections.Generic;
using NUnit.Framework;
using Programmerare.CrsTransformations.Coordinate;
using Programmerare.CrsConstants.ConstantsByAreaNameNumber.v9_5_4;

namespace Programmerare.CrsTransformations.CompositeTransformations 
{
    public class CompositeStrategyForMedianValueTest : CompositeStrategyTestBase {

    private const double delta = 0.00001;

    [Test]
    public void transform_shouldReturnMedianResult_whenUsingMedianCompositeAdapter() {
        CrsCoordinate expectedCoordinateWithMedianLatitudeAndLongitude = calculateMedianCoordinate(base.allCoordinateResultsForTheDifferentImplementations);

        ICrsTransformationAdapter medianCompositeAdapter = CrsTransformationAdapterCompositeFactory.CreateCrsTransformationMedian(
            allAdapters
        );
        CrsTransformationResult medianResult = medianCompositeAdapter.Transform(wgs84coordinate, EpsgNumber.SWEDEN__SWEREF99_TM__3006);
        Assert.IsNotNull(medianResult);
        Assert.IsTrue(medianResult.IsSuccess);
        Assert.AreEqual(base.allCoordinateResultsForTheDifferentImplementations.Count, medianResult.GetTransformationResultChildren().Count);

        CrsCoordinate coordinateReturnedByMedianAdapter = medianResult.OutputCoordinate;

        // The same transformation as above has been done in the base class for the individual adapters
        Assert.AreEqual(
            expectedCoordinateWithMedianLatitudeAndLongitude.XEastingLongitude, 
            coordinateReturnedByMedianAdapter.XEastingLongitude, 
            delta
        );
        Assert.AreEqual(
            expectedCoordinateWithMedianLatitudeAndLongitude.YNorthingLatitude, 
            coordinateReturnedByMedianAdapter.YNorthingLatitude, 
            delta
        );
    }

    private CrsCoordinate calculateMedianCoordinate(List<CrsCoordinate> coordinateResultsForTheDifferentImplementations) {
        var latitudes = coordinateResultsForTheDifferentImplementations.Select(c => c.Latitude).ToList();
        var longitudes = coordinateResultsForTheDifferentImplementations.Select(c => c.Longitude).ToList();
        // TODO maybe use the below library for finding the median
        // https://github.com/mathnet/mathnet-numerics
        double medianLongitude = MedianValueUtility.Median(longitudes);
        double medianLatitude = MedianValueUtility.Median(latitudes);
        return CrsCoordinateFactory.CreateFromXEastingLongitudeAndYNorthingLatitude(medianLongitude, medianLatitude, EpsgNumber.SWEDEN__SWEREF99_TM__3006);
    }

}
public static class MedianValueUtility
{
    // https://stackoverflow.com/questions/4140719/calculate-median-in-c-sharp
    // The below methods are copy/pasted from the above URL
//----------------------------------------------------------------
    /// <summary>
    /// Partitions the given list around a pivot element such that all elements on left of pivot are <= pivot
    /// and the ones at thr right are > pivot. This method can be used for sorting, N-order statistics such as
    /// as median finding algorithms.
    /// Pivot is selected ranodmly if random number generator is supplied else its selected as last element in the list.
    /// Reference: Introduction to Algorithms 3rd Edition, Corman et al, pp 171
    /// </summary>
    private static int Partition<T>(this IList<T> list, int start, int end, Random rnd = null) where T : IComparable<T>
    {
        if (rnd != null)
            list.Swap(end, rnd.Next(start, end+1));

        var pivot = list[end];
        var lastLow = start - 1;
        for (var i = start; i < end; i++)
        {
            if (list[i].CompareTo(pivot) <= 0)
                list.Swap(i, ++lastLow);
        }
        list.Swap(end, ++lastLow);
        return lastLow;
    }

    /// <summary>
    /// Returns Nth smallest element from the list. Here n starts from 0 so that n=0 returns minimum, n=1 returns 2nd smallest element etc.
    /// Note: specified list would be mutated in the process.
    /// Reference: Introduction to Algorithms 3rd Edition, Corman et al, pp 216
    /// </summary>
    public static T NthOrderStatistic<T>(this IList<T> list, int n, Random rnd = null) where T : IComparable<T>
    {
        return NthOrderStatistic(list, n, 0, list.Count - 1, rnd);
    }
    private static T NthOrderStatistic<T>(this IList<T> list, int n, int start, int end, Random rnd) where T : IComparable<T>
    {
        while (true)
        {
            var pivotIndex = list.Partition(start, end, rnd);
            if (pivotIndex == n)
                return list[pivotIndex];

            if (n < pivotIndex)
                end = pivotIndex - 1;
            else
                start = pivotIndex + 1;
        }
    }

    public static void Swap<T>(this IList<T> list, int i, int j)
    {
        if (i==j)   //This check is not required but Partition function may make many calls so its for perf reason
            return;
        var temp = list[i];
        list[i] = list[j];
        list[j] = temp;
    }

    /// <summary>
    /// Note: specified list would be mutated in the process.
    /// </summary>
    public static T Median<T>(this IList<T> list) where T : IComparable<T>
    {
        return list.NthOrderStatistic((list.Count - 1)/2);
    }

    public static double Median<T>(this IEnumerable<T> sequence, Func<T, double> getValue)
    {
        var list = sequence.Select(getValue).ToList();
        var mid = (list.Count - 1) / 2;
        return list.NthOrderStatistic(mid);
    }
//----------------------------------------------------------------
}
}