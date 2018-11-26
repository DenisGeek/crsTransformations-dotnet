namespace Programmerare.CrsTransformations.CompositeTransformations {

using System;
using NUnit.Framework;
using Programmerare.CrsTransformations;
using Programmerare.CrsTransformations.Adapter.MightyLittleGeodesy;

class CrsTransformationAdapterWeightTest {

    private ICrsTransformationAdapter crsTransformationAdapterInstanceNotNull;
    
    [SetUp]
    public void setup() {
        crsTransformationAdapterInstanceNotNull = new CrsTransformationAdapterMightyLittleGeodesy();
    }
    
    [Test]
    public void createFromInstance_shouldThrowException_whenAdapterParameterIsNull() {
        ArgumentNullException exception = Assert.Throws<ArgumentNullException>(() => {
            CrsTransformationAdapterWeight.CreateFromInstance(
                null, // adapter
                123 // weight
            );
        });
    }

    [Test]
    public void createFromInstance_shouldThrowException_whenWeightParameterIsNegative() {
        ArgumentException exception = Assert.Throws<ArgumentException>(() => {
            CrsTransformationAdapterWeight.CreateFromInstance(
                crsTransformationAdapterInstanceNotNull,
                -1 // weight.  null weight leads to compiler error so that it imposslble
            );
        });
    }

    [Test]
    public void createFromInstance_shouldThrowException_whenWeightParameterIsZero() {
        ArgumentException exception = Assert.Throws<ArgumentException>(() => {
            CrsTransformationAdapterWeight.CreateFromInstance(
                crsTransformationAdapterInstanceNotNull,
                0.0 // weight.  null weight leads to compiler error so that it imposslble
            );
        });
    }

    [Test]
    public void createFromStringWithFullClassNameForImplementation_shouldThrowException_whenTheClassIsNotImplementingTheExpectedInterface() {
        string nameOfClassNotImplementingTheInterfaceCrsTransformationAdapter = this.GetType().FullName;
        ArgumentException exception = Assert.Throws<ArgumentException>(() => {
            CrsTransformationAdapterWeight.CreateFromStringWithFullClassNameForImplementation(
                nameOfClassNotImplementingTheInterfaceCrsTransformationAdapter,
                123 // weight
            );
        });
    }

    [Test]
    public void createFromStringWithFullClassNameForImplementation_shouldSucceed_whenTheClassIsImplementingTheExpectedInterface() {
        string nameOfClassImplementingTheInterfaceCrsTransformationAdapter = crsTransformationAdapterInstanceNotNull.GetType().FullName;
        double weightValue = 123;
        CrsTransformationAdapterWeight crsTransformationAdapterWeight = CrsTransformationAdapterWeight.CreateFromStringWithFullClassNameForImplementation(
            nameOfClassImplementingTheInterfaceCrsTransformationAdapter,
            weightValue
        );
        Assert.IsNotNull(crsTransformationAdapterWeight);
        Assert.AreEqual(
            nameOfClassImplementingTheInterfaceCrsTransformationAdapter,  
            crsTransformationAdapterWeight.CrsTransformationAdapter.GetType().FullName
        );
        Assert.AreEqual(
            weightValue, // expected
            crsTransformationAdapterWeight.Weight
        );
    }

}
}