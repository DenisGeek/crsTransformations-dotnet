namespace Programmerare.CrsTransformations.CompositeTransformations
open System.Collections.Generic
open Programmerare.CrsTransformations
open Programmerare.CrsTransformations.Coordinate
open Programmerare.CrsTransformations.Identifier
(*
Copyright (c) Tomas Johansson , http://programmerare.com
The code in the "Core" project is licensed with MIT.
Other subprojects may be released with other licenses e.g. LGPL or Apache License 2.0.
Please find more information in the license file at the root directory of each subproject
(e.g. a subproject such as "Programmerare.CrsTransformations.Adapter.DotSpatial")
 *)
[<AbstractClass>]
type internal CompositeStrategyBase internal
    (
        crsTransformationAdapters : IList<ICrsTransformationAdapter>
    ) =
    class

        do
            if isNull crsTransformationAdapters then
                nullArg "crsTransformationAdapters"
            if crsTransformationAdapters.Count = 0 then
                invalidArg "crsTransformationAdapters" "'Composite' adapter can not be created with an empty list of 'leaf' adapters"

        abstract member _GetAllTransformationAdaptersInTheOrderTheyShouldBeInvoked : unit -> IList<ICrsTransformationAdapter>
        default this._GetAllTransformationAdaptersInTheOrderTheyShouldBeInvoked() = crsTransformationAdapters

        abstract member _GetAdapteeType : unit -> CrsTransformationAdapteeType
        default this._GetAdapteeType() = CrsTransformationAdapteeType.UNSPECIFIED_COMPOSITE

        (*
        * This base class method is reusable from both subclasses
        * that calculates the median or average which is provided with the last
        * function parameter of the method
        *)
        member this._CalculateAggregatedResultBase
            (
                allResults: IList<CrsTransformationResult>,
                inputCoordinate: CrsCoordinate,
                crsTransformationAdapterThatCreatedTheResult: ICrsTransformationAdapter,
                crsTransformationResultStatistic: CrsTransformationResultStatistic,
                medianOrAverage: CrsTransformationResultStatistic -> CrsCoordinate

            ) : CrsTransformationResult =
                if crsTransformationResultStatistic.IsStatisticsAvailable then
                    let coordRes: CrsCoordinate = medianOrAverage(crsTransformationResultStatistic)
                    CrsTransformationResult._CreateCrsTransformationResult(
                        inputCoordinate,
                        outputCoordinate = coordRes,
                        exceptionOrNull = null,
                        isSuccess = true,
                        crsTransformationAdapterResultSource = crsTransformationAdapterThatCreatedTheResult,
                        nullableCrsTransformationResultStatistic = crsTransformationResultStatistic
                    )
                else
                    CrsTransformationResult._CreateCrsTransformationResult(
                        inputCoordinate,
                        outputCoordinate = null,
                        exceptionOrNull = null,
                        isSuccess = false,
                        crsTransformationAdapterResultSource = crsTransformationAdapterThatCreatedTheResult,
                        nullableCrsTransformationResultStatistic = crsTransformationResultStatistic
                    )
        
        interface ICompositeStrategy with
    
            member this._GetAllTransformationAdaptersInTheOrderTheyShouldBeInvoked() =
                this._GetAllTransformationAdaptersInTheOrderTheyShouldBeInvoked()

            member this._GetAdapteeType() = 
                this._GetAdapteeType()

            member this._CalculateAggregatedResult
                (
                    allResults: IList<CrsTransformationResult>,
                    inputCoordinate: CrsCoordinate,
                    crsIdentifierForOutputCoordinateSystem: CrsIdentifier,
                    crsTransformationAdapterThatCreatedTheResult: ICrsTransformationAdapter
                ): CrsTransformationResult = raise (System.NotImplementedException())

            member this._ShouldContinueIterationOfAdaptersToInvoke(crsTransformationResult) = raise (System.NotImplementedException())

    end