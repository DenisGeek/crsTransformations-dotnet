namespace Programmerare.CrsTransformations.CompositeTransformations {

using System;
using Programmerare.CrsTransformations;
using Programmerare.CrsTransformations.Coordinate;
using NUnit.Framework;
using Programmerare.CrsConstants.ConstantsByAreaNameNumber.v9_5_4;
using Programmerare.CrsTransformations.Adapter.DotSpatial;
using Programmerare.CrsTransformations.Adapter.MightyLittleGeodesy;
using System.Collections.Generic;

[TestFixture]
public class CrsTransformationAdapterCompositeTest {

    private const int EXPECTED_NUMBER_OF_ADAPTER_LEAF_IMPLEMENTATIONS = CrsTransformationAdapterTest.EXPECTED_NUMBER_OF_ADAPTER_LEAF_IMPLEMENTATIONS;

    [Test]
    public void isReliableTest() {
        var crsTransformationAdapterCompositeFactory = CrsTransformationAdapterCompositeFactory.Create();
        CrsTransformationAdapterComposite crsTransformationComposite = crsTransformationAdapterCompositeFactory.CreateCrsTransformationAverage();
        var children = crsTransformationComposite.GetTransformationAdapterChildren();
        Assert.AreEqual(3, children.Count);

        CrsCoordinate wgs84coordinateInSweden = CrsCoordinateFactory.LatLon(59.31,18.04);
        CrsTransformationResult resultWhenTransformingToSwedishCRS = crsTransformationComposite.Transform(wgs84coordinateInSweden, EpsgNumber.SWEDEN__SWEREF99_TM__3006);
        Assert.IsNotNull(resultWhenTransformingToSwedishCRS);
        Assert.IsTrue(resultWhenTransformingToSwedishCRS.IsSuccess);
        CrsTransformationResultStatistic crsTransformationResultStatistic = resultWhenTransformingToSwedishCRS.CrsTransformationResultStatistic;
        Assert.IsNotNull(crsTransformationResultStatistic);
        Assert.IsTrue(crsTransformationResultStatistic.IsStatisticsAvailable);

        int actualNumberOfResults = crsTransformationResultStatistic.NumberOfPotentiallySuccesfulResults;
        Assert.AreEqual(
            EXPECTED_NUMBER_OF_ADAPTER_LEAF_IMPLEMENTATIONS,
            actualNumberOfResults
        );
        double actualMaxDiffXLongitude = crsTransformationResultStatistic.MaxDifferenceForXEastingLongitude;
        double actualMaxDiffYLatitude = crsTransformationResultStatistic.MaxDifferenceForYNorthingLatitude;
        double actualMaxDiffXorY = Math.Max(actualMaxDiffXLongitude, actualMaxDiffYLatitude);
        Assert.That(actualMaxDiffXorY, Is.LessThan(0.01));

        Assert.IsTrue(resultWhenTransformingToSwedishCRS.IsReliable(actualNumberOfResults, actualMaxDiffXorY));

        // assertFalse below since trying to require one more result than available
        Assert.IsFalse(resultWhenTransformingToSwedishCRS.IsReliable(actualNumberOfResults + 1, actualMaxDiffXorY));

        // assertFalse below since trying to require too small maxdiff
        Assert.IsFalse(resultWhenTransformingToSwedishCRS.IsReliable(actualNumberOfResults, actualMaxDiffXorY - 0.00000000001));
    }

    [Test]
    public void TransformToCoordinateWithComposite_ShouldAggregateAsExpected_WhenTheLeafsAreAlsoCompositesAndNestedAtManyLevels() {
        // The method first creates two composites (average and success)
        // and then uses those two composites as leafs for a 
        // weighted average composite, which in 
        // turn is then used as a leaf within 
        // the final median composite (together with a "normal leaf" i.e. DotSpatial implementation)
        var crsTransformationAdapterCompositeFactory = CrsTransformationAdapterCompositeFactory.Create();
        var compositeAverage = crsTransformationAdapterCompositeFactory.CreateCrsTransformationAverage();
        var compositeFirstSuccess = crsTransformationAdapterCompositeFactory.CreateCrsTransformationFirstSuccess();
        var weightsForCompositeLeafs = new List<CrsTransformationAdapterWeight>{
            CrsTransformationAdapterWeight.CreateFromInstance(
                compositeAverage,
                1.0 // weight
            ),
            CrsTransformationAdapterWeight.CreateFromInstance(
                compositeFirstSuccess,
                2.0 // weight
            )
        };
        CrsTransformationAdapterComposite weightedCompositeAdapterWithOtherCompositesAsLeafs = crsTransformationAdapterCompositeFactory.CreateCrsTransformationWeightedAverage(
            weightsForCompositeLeafs
        );
        var normalLeafDotSpatialAdapter = new CrsTransformationAdapterDotSpatial();        
        var adaptersForMedian = new List<ICrsTransformationAdapter>{
            weightedCompositeAdapterWithOtherCompositesAsLeafs,
            normalLeafDotSpatialAdapter
        };
        var compositeMedian = crsTransformationAdapterCompositeFactory.CreateCrsTransformationMedian(
            adaptersForMedian
        );
        // Now the "complex" composite (nested at two lelvels, with composites as leafs)
        // has been conctructed. 
        // Now create a coordinate:
        var inputCoordinate = CrsCoordinateFactory.LatLon(60.0, 20.0);
        // Now use the above "complex" composite to transform the coordinate:
        var coordinateResultMedianWithNestedComposite = compositeMedian.TransformToCoordinate(
            inputCoordinate, EpsgNumber.SWEDEN__SWEREF99_TM__3006
        );

        // Now use some leaf (not using DotSpatial as above)        
        // to also make the same Transform, to use it 
        // in the result comparison
        var mightyLittleGeodesyAdapter = new CrsTransformationAdapterMightyLittleGeodesy();        
        var coordinateResultMightyLittleGeodesyAdapter = mightyLittleGeodesyAdapter.TransformToCoordinate(
            inputCoordinate, EpsgNumber.SWEDEN__SWEREF99_TM__3006
        );
        // The difference should not be very large, and the delta
        // value below is one decimeter
        const double deltaValueForAssertions = 0.1;

        Assert.AreEqual(
            coordinateResultMightyLittleGeodesyAdapter.X, 
            coordinateResultMedianWithNestedComposite.X,
            deltaValueForAssertions
        );
        Assert.AreEqual(
            coordinateResultMightyLittleGeodesyAdapter.Y, 
            coordinateResultMedianWithNestedComposite.Y,
            deltaValueForAssertions
        );
    }
}
}