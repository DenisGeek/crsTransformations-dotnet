package com.programmerare.crsTransformations.compositeTransformations

import com.programmerare.crsTransformations.*
import java.util.List

class CRStransformationFacadeAverage(crsTransformationFacades: List<CRStransformationFacade>) : CRStransformationFacadeBaseComposite(crsTransformationFacades) {

    override fun transformToResultObject(inputCoordinate: Coordinate, crsIdentifierForOutputCoordinateSystem: CrsIdentifier): TransformResult {
        var successCount = 0
        var sumLat = 0.0
        var sumLon = 0.0
        for (facade: CRStransformationFacade in crsTransformationFacades) {
            val res = facade.transformToResultObject(inputCoordinate, crsIdentifierForOutputCoordinateSystem)
            if(res.isSuccess) {
                successCount++
                val coord = res.outputCoordinate
                sumLat += coord.yLatitude
                sumLon += coord.xLongitude
            }
        }
        if(successCount > 0) {
            var avgLat = sumLat / successCount
            var avgLon = sumLon / successCount
            val coordRes = Coordinate.createFromYLatXLong(avgLat, avgLon, crsIdentifierForOutputCoordinateSystem)
            return TransformResultImplementation(inputCoordinate, outputCoordinate = coordRes, exception = null, isSuccess = true)
        }
        else {
            // TODO: aggregate mroe from the results e.g. exception messages
            return TransformResultImplementation(inputCoordinate, outputCoordinate = null, exception = null, isSuccess = false)
        }
    }
}