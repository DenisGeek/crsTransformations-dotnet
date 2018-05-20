package com.programmerare.crsTransformations;

import com.programmerare.crsTransformationFacadeGooberCTL.CRStransformationFacadeGooberCTL;
import com.programmerare.crsTransformationFacadeOrbisgisCTS.CRStransformationFacadeOrbisgisCTS;
import org.junit.jupiter.api.DisplayName;
import org.junit.jupiter.api.Test;

import static org.junit.jupiter.api.Assertions.assertEquals;

class CRStransformationFacadeBaseTest {

    @DisplayName("Goober Cts transformation from WGS84 to SWEREF99")
    @Test
    void gooberCtlTest() {
        testTransformationFromWgs84ToSweref99TM(new CRStransformationFacadeGooberCTL());
    }

    @DisplayName("Goober Cts transformation from RT90 to WGS84")
    @Test
    void gooberCtlTest2() {
        testTransformationFromRT90ToWgs84(new CRStransformationFacadeGooberCTL());
    }

    @DisplayName("Orbisgis Cts transformation from WGS84 to SWEREF99")
    @Test
    void orbisgisCtsTest() {
        testTransformationFromWgs84ToSweref99TM(new CRStransformationFacadeOrbisgisCTS());
    }

    @DisplayName("Orbisgis Cts transformation from RT90 to WGS84")
    @Test
    void orbisgisCtsTest2() {
        testTransformationFromRT90ToWgs84(new CRStransformationFacadeOrbisgisCTS());
    }

    private void testTransformationFromWgs84ToSweref99TM(CRStransformationFacade crsTransformationFacade) {
        // This test is using the coordinates of Stockholm Centralstation (Sweden)
        // https://kartor.eniro.se/m/03Yxp
        // WGS84 decimal (lat, lon)
        // 59.330231, 18.059196
        // SWEREF99 TM (nord, öst)
        // 6580822, 674032

        double wgs84Lat = 59.330231;
        double wgs84Lon = 18.059196;

        double sweref99_Y_expected = 6580822;
        double sweref99_X_expected = 674032;

        int epsgNumberForWgs84 = 4326;
        int epsgNumberForSweref99TM = 3006;
        Coordinate inputCoordinate = new Coordinate(wgs84Lon, wgs84Lat, epsgNumberForWgs84);
        Coordinate outputCoordinate = crsTransformationFacade.transform(inputCoordinate, epsgNumberForSweref99TM);
        assertEquals(sweref99_Y_expected, outputCoordinate.getYLatitude(), 0.5);
        assertEquals(sweref99_X_expected, outputCoordinate.getXLongitude(), 0.5);
    }

    private void testTransformationFromRT90ToWgs84(CRStransformationFacade crsTransformationFacade) {
        double wgs84Lat_expected = 59.330231;
        double wgs84Lon_expected = 18.059196;

        double rt90_Y = 6580994;
        double rt90_X = 1628294;

        int epsgNumberForWgs84 = 4326;
        int epsgNumberForRT90 = 3021;
        Coordinate inputCoordinate = new Coordinate(rt90_X, rt90_Y, epsgNumberForRT90);

        Coordinate outputCoordinate = crsTransformationFacade.transform(inputCoordinate, epsgNumberForWgs84);
        assertEquals(wgs84Lat_expected, outputCoordinate.getYLatitude(), 0.1);
        assertEquals(wgs84Lon_expected, outputCoordinate.getXLongitude(), 0.1);
    }
}