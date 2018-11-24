namespace Programmerare.CrsTransformations.CompositeTransformations

open System.Collections.Generic
open Programmerare.CrsTransformations
open Programmerare.CrsTransformations.Coordinate
open Programmerare.CrsTransformations.Identifier

// TODO: rewrite comments below for .NET ...

(*
 * Base class for the 'composite' adapters.
 * @see CrsTransformationAdapterBase
 * @see CompositeStrategy
 * 
 * @author Tomas Johansson ( http://programmerare.com )
 * The code in the "crs-transformation-adapter-core" project is licensed with MIT.
 * Other subprojects may be released with other licenses e.g. LGPL or Apache License 2.0.
 * Please find more information in the license file at the root directory of each subproject
 * (e.g. the subprojects "crs-transformation-adapter-impl-geotools" , "crs-transformation-adapter-impl-proj4j" and so on)
*)
type CrsTransformationAdapterComposite internal
    (
    (*
     * Interface for calculating the resulting coordinate in different ways, 
     * e.g. one stratefy implementation calculates the median and another the average.
     *)        
    compositeStrategy: ICompositeStrategy
    ) =

    class
        inherit CrsTransformationAdapterBase()

        member this._GetCompositeStrategy() = compositeStrategy

        override this._TransformToCoordinateHook(inputCoordinate: CrsCoordinate, crsIdentifierForOutputCoordinateSystem: CrsIdentifier): CrsCoordinate =
            let transformResult = this._TransformHook(inputCoordinate, crsIdentifierForOutputCoordinateSystem)
            if(transformResult.IsSuccess) then
                transformResult.OutputCoordinate
            else
                failwith "Transformation failed"

        override this._TransformHook(inputCoordinate: CrsCoordinate, crsIdentifierForOutputCoordinateSystem: CrsIdentifier): CrsTransformationResult =
            let allCrsTransformationAdapters = this._GetCompositeStrategy()._GetAllTransformationAdaptersInTheOrderTheyShouldBeInvoked()
            let list = new List<CrsTransformationResult>()
            let mutable shouldContinue = true
            for crsTransformationAdapter in allCrsTransformationAdapters do
                if shouldContinue then
                    let res = crsTransformationAdapter.Transform(inputCoordinate, crsIdentifierForOutputCoordinateSystem)
                    list.Add(res)
                    if not(this._GetCompositeStrategy()._ShouldContinueIterationOfAdaptersToInvoke(res)) then
                        shouldContinue <- false
            this._GetCompositeStrategy()._CalculateAggregatedResult(list, inputCoordinate, crsIdentifierForOutputCoordinateSystem, this)

        override this.GetTransformationAdapterChildren(): IList<ICrsTransformationAdapter> =
            this._GetCompositeStrategy()._GetAllTransformationAdaptersInTheOrderTheyShouldBeInvoked()

        override this.IsComposite: bool = 
            true

        override this.AdapteeType : CrsTransformationAdapteeType =
            this._GetCompositeStrategy()._GetAdapteeType()

        static member _CreateCrsTransformationAdapterComposite
            (
                compositeStrategy: ICompositeStrategy
            ): CrsTransformationAdapterComposite =
                CrsTransformationAdapterComposite(compositeStrategy)

        member this.TransformToCoordinate(inputCoordinate, crsCode) =
            this._TransformToCoordinateHook(inputCoordinate, CrsIdentifierFactory.CreateFromCrsCode(crsCode))

        member this.TransformToCoordinate(inputCoordinate, epsgNumberForOutputCoordinateSystem) = 
            this._TransformToCoordinateHook(inputCoordinate, CrsIdentifierFactory.CreateFromEpsgNumber(epsgNumberForOutputCoordinateSystem))

        member this.TransformToCoordinate(inputCoordinate, crsIdentifier) = 
            this._TransformToCoordinateHook(inputCoordinate, crsIdentifier)


        member this.Transform(inputCoordinate, crsCode) =
            this._TransformHook(inputCoordinate, CrsIdentifierFactory.CreateFromCrsCode(crsCode))

        member this.Transform(inputCoordinate, epsgNumberForOutputCoordinateSystem) = 
            this._TransformHook(inputCoordinate, CrsIdentifierFactory.CreateFromEpsgNumber(epsgNumberForOutputCoordinateSystem))

        member this.Transform(inputCoordinate, crsIdentifier) = 
            this._TransformHook(inputCoordinate, crsIdentifier)
                

        interface ICrsTransformationAdapter with
            override this.GetTransformationAdapterChildren() =  this.GetTransformationAdapterChildren()

            override this.TransformToCoordinate(inputCoordinate, crsCode: string) =
                this.TransformToCoordinate(inputCoordinate, crsCode)

            override this.TransformToCoordinate(inputCoordinate, epsgNumberForOutputCoordinateSystem: int) = 
                this.TransformToCoordinate(inputCoordinate, epsgNumberForOutputCoordinateSystem)

            override this.TransformToCoordinate(inputCoordinate, crsIdentifier: CrsIdentifier) = 
                this.TransformToCoordinate(inputCoordinate, crsIdentifier)

            override this.Transform(inputCoordinate, crsCode: string) =
                this.Transform(inputCoordinate, crsCode) 

            override this.Transform(inputCoordinate, epsgNumberForOutputCoordinateSystem: int) = 
                this.Transform(inputCoordinate, epsgNumberForOutputCoordinateSystem)

            override this.Transform(inputCoordinate, crsIdentifier: CrsIdentifier) = 
                this.Transform(inputCoordinate, crsIdentifier)
    end