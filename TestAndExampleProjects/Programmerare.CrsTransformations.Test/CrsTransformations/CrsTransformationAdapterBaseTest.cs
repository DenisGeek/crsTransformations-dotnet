﻿using System.Collections.Generic;
using NUnit.Framework;
using Programmerare.CrsTransformations.Adapter.MightyLittleGeodesy;
using Programmerare.CrsTransformations.Adapter.DotSpatial;
using Programmerare.CrsTransformations.Adapter.ProjNet4GeoAPI;
using Programmerare.CrsTransformations.CompositeTransformations;

namespace Programmerare.CrsTransformations.Test.CrsTransformation {

    // Test class for CrsTransformationAdapterBase

    [TestFixture]
    class CrsTransformationAdapterBaseTest {
        private CrsTransformationAdapterCompositeFactory crsTransformationAdapterCompositeFactory, crsTransformationAdapterCompositeFactoryWithLeafsInReversedOrder, crsTransformationAdapterCompositeFactoryWithOneLeafDifferentlyConfigured;
        CrsTransformationAdapterComposite average, median, firstSuccess, weightedAverage;
        CrsTransformationAdapterBaseLeaf dotSpatial, mightyLittleGeodesy, projNet4GeoAPI, projNet4GeoAPIWithDifferentConfiguration;

        private CrsTransformationAdapterWeightFactory weightFactory;

        [SetUp]
        public void SetUp() {
            weightFactory = CrsTransformationAdapterWeightFactory.Create();

            // Leaf adapters:
            dotSpatial = new CrsTransformationAdapterDotSpatial();
            mightyLittleGeodesy = new CrsTransformationAdapterMightyLittleGeodesy();
            // currently there are no configurations possibilities for the above two leafs
            // but for the below leaf it is possible to create instances with 
            // different configurations
            projNet4GeoAPI = new CrsTransformationAdapterProjNet4GeoAPI();
            projNet4GeoAPIWithDifferentConfiguration = new CrsTransformationAdapterProjNet4GeoAPI(new SridReader("somepath.csv"));

            // Composite adapters:
            crsTransformationAdapterCompositeFactory = CrsTransformationAdapterCompositeFactory.Create(
                new List<ICrsTransformationAdapter>{
                    dotSpatial, mightyLittleGeodesy, projNet4GeoAPI
                }
            );
            // Note that below list parameter is the same as the above but with the list items in reversed order
            crsTransformationAdapterCompositeFactoryWithLeafsInReversedOrder = CrsTransformationAdapterCompositeFactory.Create(
                new List<ICrsTransformationAdapter>{
                    projNet4GeoAPI, mightyLittleGeodesy, dotSpatial
                }
            );
            crsTransformationAdapterCompositeFactoryWithOneLeafDifferentlyConfigured = CrsTransformationAdapterCompositeFactory.Create(
                new List<ICrsTransformationAdapter>{dotSpatial, mightyLittleGeodesy, projNet4GeoAPIWithDifferentConfiguration}
            );

            average = crsTransformationAdapterCompositeFactory.CreateCrsTransformationAverage();
            median = crsTransformationAdapterCompositeFactory.CreateCrsTransformationMedian();
            firstSuccess = crsTransformationAdapterCompositeFactory.CreateCrsTransformationFirstSuccess();
            weightedAverage = crsTransformationAdapterCompositeFactory.CreateCrsTransformationWeightedAverage(new List<CrsTransformationAdapterWeight>{
                weightFactory.CreateFromInstance(dotSpatial, 1.0),
                weightFactory.CreateFromInstance(projNet4GeoAPI, 2.0),
                weightFactory.CreateFromInstance(mightyLittleGeodesy, 3.0)
            });
        }

        [Test]
        public void Composite_ShouldReturnLeafsInTheExpectedOrderAccordingToTheSetupMethod() {
            IList<ICrsTransformationAdapter> leafs1 = crsTransformationAdapterCompositeFactory.CreateCrsTransformationAverage().TransformationAdapterChildren;
            IList<ICrsTransformationAdapter> leafs2 = crsTransformationAdapterCompositeFactoryWithLeafsInReversedOrder.CreateCrsTransformationAverage().TransformationAdapterChildren;
            // Note that the list of leafs above should be in reversed order
            Assert.AreEqual(3, leafs1.Count);
            Assert.AreEqual(leafs1.Count, leafs2.Count);
            // There are three items in each list but since the elements 
            // should be the same but in reversed order, 
            // the below assertion is that item 0 should be item 2 in the other list
            Assert.AreEqual(leafs1[0].AdapteeType, leafs2[2].AdapteeType);
            Assert.AreEqual(leafs1[1].AdapteeType, leafs2[1].AdapteeType);
            Assert.AreEqual(leafs1[2].AdapteeType, leafs2[0].AdapteeType);
        }

        [Test]
        public void LeafAdapters_ShouldBeEqual_WhenTheSameTypeAndConfiguration() {
            var dotSpatial1 = new CrsTransformationAdapterDotSpatial();
            var dotSpatial2 = new CrsTransformationAdapterDotSpatial();
            Assert.AreEqual(
                dotSpatial1,
                dotSpatial2
            );
            Assert.AreEqual(
                dotSpatial1.GetHashCode(),
                dotSpatial2.GetHashCode()
            );

            Assert.AreEqual(
                dotSpatial, // created in the Setup method
                dotSpatial1
            );
            Assert.AreEqual(
                dotSpatial.GetHashCode(),
                dotSpatial1.GetHashCode()
            );

            var mightyLittleGeodesy2 = new CrsTransformationAdapterMightyLittleGeodesy();
            Assert.AreEqual(
                mightyLittleGeodesy, // created in the Setup method
                mightyLittleGeodesy2
            );
            Assert.AreEqual(
                mightyLittleGeodesy.GetHashCode(),
                mightyLittleGeodesy2.GetHashCode()
            );

            var projNet4GeoAPI2 = new CrsTransformationAdapterProjNet4GeoAPI();
            Assert.AreEqual(
                projNet4GeoAPI, // created in the Setup method
                projNet4GeoAPI2
            );
            Assert.AreEqual(
                projNet4GeoAPI.GetHashCode(),
                projNet4GeoAPI2.GetHashCode()
            );
        }
        
        [Test]
        public void LeafAdapters_ShouldNotBeEqual_WhenDifferentType() {
            Assert.AreNotEqual(
                projNet4GeoAPI,
                mightyLittleGeodesy
            );
            Assert.AreNotEqual(
                projNet4GeoAPI,
                dotSpatial
            );
            Assert.AreNotEqual(
                new CrsTransformationAdapterDotSpatial(),
                new CrsTransformationAdapterProjNet4GeoAPI()
            );
        }
        
        [Test]
        public void LeafAdapters_ShouldNotBeEqual_WhenTheSameTypeButDifferentConfiguration() {
            Assert.AreNotEqual(
                new CrsTransformationAdapterProjNet4GeoAPI(), // default configuration
                new CrsTransformationAdapterProjNet4GeoAPI(new SridReader("filepath1"))
            );

            Assert.AreNotEqual(
                new CrsTransformationAdapterProjNet4GeoAPI(
                    new SridReader(new List<EmbeddedResourceFileWithCRSdefinitions>{
                        EmbeddedResourceFileWithCRSdefinitions.SIX_SWEDISH_RT90_CRS_DEFINITIONS_COPIED_FROM_SharpMap_SpatialRefSys_xml}
                    )
                )
                ,
                new CrsTransformationAdapterProjNet4GeoAPI(
                    new SridReader(new List<EmbeddedResourceFileWithCRSdefinitions>{
                        EmbeddedResourceFileWithCRSdefinitions.STANDARD_FILE_SHIPPED_WITH_ProjNet4GeoAPI}
                    )
                )
            );
        }

        [Test]
        public void CompositeAdaptersForMedianAndAverage_ShouldBeEqual_WhenLeafsUseTheSameTypeAndConfigurationRegardlessOfTheOrder() {
            // Median
            var median1 = crsTransformationAdapterCompositeFactory.CreateCrsTransformationMedian();
            // the leafs created in reversed order:
            var median2 = crsTransformationAdapterCompositeFactoryWithLeafsInReversedOrder.CreateCrsTransformationMedian();
            Assert.AreEqual(
                median1,
                median2
            );
            Assert.AreEqual(
                median1.GetHashCode(),
                median2.GetHashCode()
            );

            // Average
            var average1 = crsTransformationAdapterCompositeFactory.CreateCrsTransformationAverage();
            // the leafs created in reversed order:
            var average2 = crsTransformationAdapterCompositeFactoryWithLeafsInReversedOrder.CreateCrsTransformationAverage();
            Assert.AreEqual(
                average1,
                average2
            );
            Assert.AreEqual(
                average1.GetHashCode(),
                average2.GetHashCode()
            );
        }

        [Test]
        public void CompositeAdapters_ShouldNotBeEqual_WhenDifferentNumberOfLeafs() {
            // Composite adapter factory with only two leafs:
            var crsTransformationAdapterCompositeFactoryWithTwoLeafs = CrsTransformationAdapterCompositeFactory.Create(
                new List<ICrsTransformationAdapter>{
                    dotSpatial, projNet4GeoAPI
                }
            );
            
            var crsTransformationAdapterCompositeFactoryWithThreeLeafs = crsTransformationAdapterCompositeFactory;

            // Average created with two differenf factories,
            // one with three leafs and one with two leafs
            Assert.AreNotEqual(
                crsTransformationAdapterCompositeFactoryWithThreeLeafs.CreateCrsTransformationAverage(),
                crsTransformationAdapterCompositeFactoryWithTwoLeafs.CreateCrsTransformationAverage()
            );            

            // Median created with two differenf factories,
            // one with three leafs and one with two leafs
            Assert.AreNotEqual(
                crsTransformationAdapterCompositeFactoryWithThreeLeafs.CreateCrsTransformationMedian(),
                crsTransformationAdapterCompositeFactoryWithTwoLeafs.CreateCrsTransformationMedian()
            );            

            // FirstSuccess created with two differenf factories,
            // one with three leafs and one with two leafs
            Assert.AreNotEqual(
                crsTransformationAdapterCompositeFactoryWithThreeLeafs.CreateCrsTransformationFirstSuccess(),
                crsTransformationAdapterCompositeFactoryWithTwoLeafs.CreateCrsTransformationFirstSuccess()
            );
        }
        
        [Test]
        public void CompositeAdapters_ShouldNotBeEqual_WhenDifferentType() {
            Assert.AreNotEqual(
                average,
                median
            );
            Assert.AreNotEqual(
                average,
                firstSuccess
            );
            Assert.AreNotEqual(
                average,
                weightedAverage
            );
            Assert.AreNotEqual(
                median,
                firstSuccess
            );
            Assert.AreNotEqual(
                median,
                weightedAverage
            );
            Assert.AreNotEqual(
                firstSuccess,
                weightedAverage
            );
        }

        [Test]
        public void CompositeAdapterFirstSuccess_ShouldBeEqual_WhenTheSameOrder() {
            Assert.AreEqual(
                crsTransformationAdapterCompositeFactory.CreateCrsTransformationFirstSuccess(),
                // new instance but leafs created in the same order with the same method as above:
                crsTransformationAdapterCompositeFactory.CreateCrsTransformationFirstSuccess()
            );
        }
        
        [Test]
        public void CompositeAdapterFirstSuccess_ShouldNotBeEqual_WhenDifferentOrder() {
            Assert.AreNotEqual(
                crsTransformationAdapterCompositeFactory.CreateCrsTransformationFirstSuccess(),
                // different order:
                crsTransformationAdapterCompositeFactoryWithLeafsInReversedOrder.CreateCrsTransformationFirstSuccess()
            );
        }

        [Test]
        public void CompositeAdapterWeightedAverage_ShouldBeEqual_WhenTheSameWeights() {
            // Two instances are created below but in different order 
            // but with the same weights
            var weightedAverage1 = crsTransformationAdapterCompositeFactory.CreateCrsTransformationWeightedAverage(new List<CrsTransformationAdapterWeight>{
                weightFactory.CreateFromInstance(new CrsTransformationAdapterDotSpatial(), 1.0),
                weightFactory.CreateFromInstance(new CrsTransformationAdapterProjNet4GeoAPI(), 2.0),
                weightFactory.CreateFromInstance(new CrsTransformationAdapterMightyLittleGeodesy(), 3.0)
            });
            // below the order is switched between the above first and second,
            // though their weights are the same
            var weightedAverage2 = crsTransformationAdapterCompositeFactory.CreateCrsTransformationWeightedAverage(new List<CrsTransformationAdapterWeight>{
                weightFactory.CreateFromInstance(new CrsTransformationAdapterProjNet4GeoAPI(), 2.0),
                weightFactory.CreateFromInstance(new CrsTransformationAdapterDotSpatial(), 1.0),
                weightFactory.CreateFromInstance(new CrsTransformationAdapterMightyLittleGeodesy(), 3.0)
            });
            Assert.AreEqual(
                weightedAverage1
                ,
                weightedAverage2
            );
            Assert.AreEqual(
                weightedAverage1.GetHashCode()
                ,
                weightedAverage2.GetHashCode()
            );
        }
        
        [Test]
        public void CompositeAdapterWeightedAverage_ShouldNotBeEqual_WhenDifferentWeights() {
            Assert.AreNotEqual(
                crsTransformationAdapterCompositeFactory.CreateCrsTransformationWeightedAverage(new List<CrsTransformationAdapterWeight>{
                    weightFactory.CreateFromInstance(new CrsTransformationAdapterDotSpatial(), 1.0),
                    weightFactory.CreateFromInstance(new CrsTransformationAdapterProjNet4GeoAPI(), 2.0),
                    weightFactory.CreateFromInstance(new CrsTransformationAdapterMightyLittleGeodesy(), 3.0)
                })
                ,
                // The second below has weight 2.01 instead of 2.0
                // and therefore they should be considered as Equal
                crsTransformationAdapterCompositeFactory.CreateCrsTransformationWeightedAverage(new List<CrsTransformationAdapterWeight>{
                    weightFactory.CreateFromInstance(new CrsTransformationAdapterDotSpatial(), 1.0),
                    weightFactory.CreateFromInstance(new CrsTransformationAdapterProjNet4GeoAPI(), 2.01),
                    weightFactory.CreateFromInstance(new CrsTransformationAdapterMightyLittleGeodesy(), 3.0)
                })
            );
        }

        [Test]
        public void CompositeAdapter_ShouldNotBeEqual_WhenOneLeafIsDifferentlyConfigured() {
            Assert.AreNotEqual(
                crsTransformationAdapterCompositeFactory.CreateCrsTransformationAverage(),
                crsTransformationAdapterCompositeFactoryWithOneLeafDifferentlyConfigured.CreateCrsTransformationAverage()
            );
        }

        [Test]
        public void LongNameOfImplementation_ShouldReturnFullClassName() {
            // Of course the first test below is fragile, but the class/package name will not change
            // often and if/when it does the test will fail but will be trivial to fix.
            // The purpose of this test is not only to "test" but rather to
            // illustrate what the method returns
            Assert.AreEqual(
                "Programmerare.CrsTransformations.Adapter.DotSpatial.CrsTransformationAdapterDotSpatial",
                dotSpatial.LongNameOfImplementation
            );

            Assert.AreEqual(
                typeof(CrsTransformationAdapterDotSpatial).FullName,
                dotSpatial.LongNameOfImplementation
            );

            Assert.AreEqual(
                typeof(CrsTransformationAdapterProjNet4GeoAPI).FullName,
                projNet4GeoAPI.LongNameOfImplementation
            );

            Assert.AreEqual(
                typeof(CrsTransformationAdapterMightyLittleGeodesy).FullName,
                mightyLittleGeodesy.LongNameOfImplementation
            );
            
            Assert.AreEqual(
                typeof(CrsTransformationAdapterCompositeAverage).FullName,
                average.LongNameOfImplementation
            );

            Assert.AreEqual(
                typeof(CrsTransformationAdapterCompositeMedian).FullName,
                median.LongNameOfImplementation
            );

            Assert.AreEqual(
                typeof(CrsTransformationAdapterCompositeFirstSuccess).FullName,
                firstSuccess.LongNameOfImplementation
            );

            Assert.AreEqual(
                typeof(CrsTransformationAdapterCompositeWeightedAverage).FullName,
                weightedAverage.LongNameOfImplementation
            );
        }

        [Test]
        public void ShortNameOfImplementation_ShouldReturnSuffixPartOfClassName() {
            
            Assert.AreEqual(
                "DotSpatial",
                dotSpatial.ShortNameOfImplementation
            );

            Assert.AreEqual(
                "ProjNet4GeoAPI",
                projNet4GeoAPI.ShortNameOfImplementation
            );

            Assert.AreEqual(
                "MightyLittleGeodesy",
                mightyLittleGeodesy.ShortNameOfImplementation
            );

            Assert.AreEqual(
                "Average",
                average.ShortNameOfImplementation
            );

            Assert.AreEqual(
                "Median",
                median.ShortNameOfImplementation
            );

            Assert.AreEqual(
                "FirstSuccess",
                firstSuccess.ShortNameOfImplementation
            );

            Assert.AreEqual(
                "WeightedAverage",
                weightedAverage.ShortNameOfImplementation
            );
        }
    }
}