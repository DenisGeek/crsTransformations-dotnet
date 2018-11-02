package com.programmerare.crsTransformations

import com.programmerare.crsTransformations.coordinate.Coordinate
import com.programmerare.crsTransformations.crsIdentifier.CrsIdentifier

abstract class CrsTransformationAdapterBaseLeaf : CrsTransformationAdapterBase(), CrsTransformationAdapter {

    override final fun transform(inputCoordinate: Coordinate, crsIdentifierForOutputCoordinateSystem: CrsIdentifier): TransformResult {
        try {
            val outputCoordinate = transformHook(inputCoordinate, crsIdentifierForOutputCoordinateSystem)
            if(
                java.lang.Double.isNaN(outputCoordinate.yLatitude)
                ||
                java.lang.Double.isNaN(outputCoordinate.xLongitude)
            ) {
                return TransformResultImplementation(inputCoordinate, null, exception = null, isSuccess = false, crsTransformationAdapterResultSource = this)
            }
            else {
                return TransformResultImplementation(inputCoordinate, outputCoordinate, exception = null, isSuccess = outputCoordinate != null, crsTransformationAdapterResultSource = this)
            }
        }
        catch (e: Throwable) {
            return TransformResultImplementation(inputCoordinate, null, exception = e, isSuccess = false, crsTransformationAdapterResultSource = this)
        }
    }

}