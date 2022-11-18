(**
---
title: Quantile
index: 19
category: Documentation
categoryindex: 0
---
*)

(*** hide ***)

(*** condition: prepare ***)
#I "../src/FSharp.Stats/bin/Release/netstandard2.0/"
#r "FSharp.Stats.dll"
#r "nuget: Plotly.NET, 2.0.0-preview.16"

(*** condition: ipynb ***)
#if IPYNB
#r "nuget: Plotly.NET, 2.0.0-preview.16"
#r "nuget: Plotly.NET.Interactive, 2.0.0-preview.16"
#r "nuget: FSharp.Stats"
#endif // IPYNB


open Plotly.NET
open Plotly.NET.StyleParam
open Plotly.NET.LayoutObjects

//some axis styling
module Chart = 
    let myAxis name = LinearAxis.init(Title=Title.init name,Mirror=StyleParam.Mirror.All,Ticks=StyleParam.TickOptions.Inside,ShowGrid=false,ShowLine=true)
    let myAxisRange name (min,max) = LinearAxis.init(Title=Title.init name,Range=Range.MinMax(min,max),Mirror=StyleParam.Mirror.All,Ticks=StyleParam.TickOptions.Inside,ShowGrid=false,ShowLine=true)
    let withAxisTitles x y chart = 
        chart 
        |> Chart.withTemplate ChartTemplates.lightMirrored
        |> Chart.withXAxis (myAxis x) 
        |> Chart.withYAxis (myAxis y)

(**

# Quantile

[![Binder](https://mybinder.org/badge_logo.svg)](https://mybinder.org/v2/gh/fslaborg/FSharp.Stats/gh-pages?filepath=Quantile.ipynb)

_Summary:_ this tutorial demonstrates how to handle quantiles and QQ-Plots

### Table of contents

 - [Quantiles](#Quantiles)
 - [QQ plot](#QQ-plot)
   - [Comparing two sample distributions](#Comparing-two-sample-distributions)
   - [Comparing a sample against a distribution](#Comparing-a-sample-against-a-distribution)
     - [Normal distribution](#Normal-distribution)
     - [Uniform Distribution](#Uniform-Distribution)

## Quantiles

Quantiles are values that divide data into equally spaced groups. Percentiles are just quantiles that divide the data in 100 equally sized groups.
The median for example defines the 0.5 quantile or 0.5 percentile. You can calculate the quantile by what proportion of values are less than the value you are interested in.

_Note: There are many possibilities to handle ties or data that cannot be split equally. The default quantile method used here is `Quantile.mode`._

Let's sample 1000 data points from a normal distribution and calculate some percentiles.
*)
open System
open FSharp.Stats
open FSharp.Stats.Signal

let rng = Distributions.ContinuousDistribution.normal 3. 1.

let sample = Array.init 1000 (fun _ -> rng.Sample())

let quantile25  = Quantile.mode 0.25 sample
let quantile50  = Quantile.mode 0.50 sample
let quantile75  = Quantile.mode 0.75 sample
let quantile100 = Quantile.mode 1.00 sample


[|quantile25;quantile50;quantile75;quantile100|]
(***include-it-raw***)


(**

These special quantiles are also called quartiles since the divide the data into 4 sections.
Now we can divide the data into the ranges defined by the quantiles and plot them. Here the ranges defines half-open intervals:

*)

let range25  = sample |> Array.filter (fun x -> x < quantile25)
let range50  = sample |> Array.filter (fun x -> x > quantile25 && x < quantile50)
let range75  = sample |> Array.filter (fun x -> x > quantile50 && x < quantile75)
let range100 = sample |> Array.filter (fun x -> x > quantile75)

(*** hide ***)
let quartilePlot =
    [|
        Chart.Histogram(range25,"25",ShowLegend=false)   |> Chart.withTemplate ChartTemplates.lightMirrored |> Chart.withXAxisStyle("",MinMax=(0.,6.)) |> Chart.withYAxisStyle("Quartil 25")
        Chart.Histogram(range50,"50",ShowLegend=false)   |> Chart.withTemplate ChartTemplates.lightMirrored |> Chart.withXAxisStyle("",MinMax=(0.,6.)) |> Chart.withYAxisStyle("Quartil 50")
        Chart.Histogram(range75,"75",ShowLegend=false)   |> Chart.withTemplate ChartTemplates.lightMirrored |> Chart.withXAxisStyle("",MinMax=(0.,6.)) |> Chart.withYAxisStyle("Quartil 75")
        Chart.Histogram(range100,"100",ShowLegend=false) |> Chart.withTemplate ChartTemplates.lightMirrored |> Chart.withXAxisStyle("",MinMax=(0.,6.)) |> Chart.withYAxisStyle("Quartil 100")
    |]
    |> Chart.Grid(4,1)


(*** condition: ipynb ***)
#if IPYNB
quartilePlot
#endif // IPYNB

(***hide***)
quartilePlot |> GenericChart.toChartHTML
(***include-it-raw***)


(**

## QQ plot

QQ plots allow to compare sample distributions if:

  - the underlying population distribution is unknown or if
  - the relationship between two distributions should be evaluated in greater detail than just their estimated parameters.

When a sample is compared to a known distribution, every quantile can be calculated exactly by inverting their CDF. If you compare two samples, there is no uniquely defined CDF, 
so quantiles have to be interpolated. 

### Comparing two sample distributions

Two sample populations can be compared by QQ-plots where quantiles of the first sample are plotted against quantiles of the second sample. If the sample length is equal, both samples are ordered and plotted as pairs. 

$qq_i = X_i,Y_i$ with $X$ and $Y$ beeing ordered sample sequences of length $n$ and $(1 \le i \le n)$


If samples sizes are unequal the quantiles of the larger data set have to be interpolated from the quantiles of the smaller data set. 

**Lets create four samples of size 300 first:**

 - two that are drawn from a normal distribution of mean $3.0$ and standard deviation $0.5$

 - two that are drawn randomly between 0 and 1

*)


//create samples
let rnd = System.Random()
let norm = Distributions.ContinuousDistribution.normal 3.0 0.5

///Example 1: Aamples from a normal distribution
let normalDistA = Array.init 300 (fun _ -> norm.Sample())
let normalDistB = Array.init 300 (fun _ -> norm.Sample())

///Example 2: Random samples from values between 0 and 1
let evenRandomA = Array.init 300 (fun _ -> rnd.NextDouble())
let evenRandomB = Array.init 300 (fun _ -> rnd.NextDouble())

let exampleDistributions =
    [
        Chart.Histogram(normalDistA,Name="normalDistA") |> Chart.withTemplate ChartTemplates.lightMirrored
        Chart.Histogram(normalDistB,Name="normalDistB") |> Chart.withTemplate ChartTemplates.lightMirrored
        Chart.Histogram(evenRandomA,Name="evenRandomA") |> Chart.withTemplate ChartTemplates.lightMirrored
        Chart.Histogram(evenRandomB,Name="evenRandomB") |> Chart.withTemplate ChartTemplates.lightMirrored
    ]
    |> Chart.Grid(2,2)
    |> Chart.withSize(800.,700.)

(*** condition: ipynb ***)
#if IPYNB
exampleDistributions
#endif // IPYNB

(***hide***)
exampleDistributions |> GenericChart.toChartHTML
(***include-it-raw***)

(**

To compare if two distributions are equal or to identify ranges in which the distributions differ, a quantile pair from each of the two distributions can be calculated and plotted against each other.
If both distributions are similar, you would expect the quantiles to be identical and therefore are located on a straight line. If the samples are of different length $m$ and $n$ the number 
of quantiles is limited to $min$ $m$ $n$. For every data point of the smaller data set a corresponding quantile of the larger data set is determined.

Lets calculate the quantiles from _normalDistA_ vs _normalDistB_.
*)

// Here a tuple sequence is generated that pairwise contain the same quantiles from normalDistA and normalDistB
let qqData = QQPlot.fromTwoSamples normalDistA normalDistB

// Lets check out the first 5 elements in the sequence
Seq.take 5 qqData
(***include-it-raw***)

(**

You can use this tuple sequence and plot it against each other.

*)

open FSharp.Stats.Signal
open FSharp.Stats.Signal.QQPlot


//plots QQ plot from two sample populations
let plotFrom2Populations sampleA sampleB =

    //here the coordinates are calculated
    let qqCoordinates = QQPlot.fromTwoSamples sampleA sampleB

    Chart.Point (qqCoordinates,Name="QQ")
    |> Chart.withXAxisStyle "Quantiles sample A" 
    |> Chart.withYAxisStyle "Quantiles sample B"
    |> Chart.withTemplate ChartTemplates.lightMirrored

let myQQplot1 = plotFrom2Populations normalDistA normalDistB


(*** condition: ipynb ***)
#if IPYNB
myQQplot1
#endif // IPYNB

(***hide***)
myQQplot1 |> GenericChart.toChartHTML
(***include-it-raw***)


(**

Both samples were taken from the same distribution (here normal distribution) and therefore they match pretty well.

In the following plot you can see four comparisons of the four distributions defined in the beginning (2x normal + 2x uniform).

*)

let multipleQQPlots = 
    [
        plotFrom2Populations normalDistA normalDistB |> Chart.withXAxisStyle "normalA" |> Chart.withYAxisStyle "normalB"
        plotFrom2Populations normalDistA evenRandomB |> Chart.withXAxisStyle "normalA" |> Chart.withYAxisStyle "randomB"
        plotFrom2Populations evenRandomA normalDistB |> Chart.withXAxisStyle "randomA" |> Chart.withYAxisStyle "normalB"
        plotFrom2Populations evenRandomA evenRandomB |> Chart.withXAxisStyle "randomA" |> Chart.withYAxisStyle "randomB"
    ]
    |> Chart.Grid(2,2)
    |> Chart.withLegend false
    |> Chart.withSize(800.,700.)

(*** condition: ipynb ***)
#if IPYNB
multipleQQPlots
#endif // IPYNB

(***hide***)
multipleQQPlots |> GenericChart.toChartHTML
(***include-it-raw***)


(**

When QQ-plots are generated for pairwise comparisons, it is obvious, that the _random_-_random_ and _normal_-_normal_ samples fit nicely. The cross comparisons between normal and random samples do not match.
Its easy to see that the random samples are distributed between 0 and 1 while the samples from the normal distributions range from $1$ to ~$5$.


### Comparing a sample against a distribution

You can plot the quantiles from a sample versus a known distribution to check if your data follows the given distribution. 
There are various methods to determine quantiles that differ in handling ties and uneven spacing.


```
Quantile determination methods(rank,sampleLength):
  - Blom          -> (rank - 3. / 8.) / (sampleLength + 1. / 4.)
  - Rankit        -> (rank - 1. / 2.) / sampleLength
  - Tukey         -> (rank - 1. / 3.) / (sampleLength + 1. / 3.)
  - VanDerWerden  -> rank / (sampleLength + 1.)
```

_Note that this method does not replace a significance test wether the distributions differ statistically._

#### Normal distribution

The data can be z standardized prior to quantile determination to have zero mean and unit variance. If the data is zTransformed the bisector defines a perfect match.

*)

// The raw qq-plot data of a standard normal distribution and the sample distribution
// defaults: 
//   Method:     QuantileMethod.Rankit
//   ZTransform: false
let qq2Normal sample = QQPlot.toGauss(Method=QuantileMethod.Rankit,ZTransform=true) sample

// plots QQ plot from a sample population against a standard normal distribution. 
// if the data is zTransformed the bisector defines a perfect match.
let plotFromOneSampleGauss sample =
    
    //this is the main data plotted as x,y diagram
    let qqData = QQPlot.toGauss(Method=QuantileMethod.Rankit,ZTransform=true) sample

    let qqChart =
        Chart.Point qqData

    let expectedLine = 
        let minimum = qqData |> Seq.head |> snd
        let maximum = qqData |> Seq.last |> snd
        [
            minimum,minimum
            maximum,maximum
        ]
        |> Chart.Line
        |> Chart.withTraceName "expected"

    [
        qqChart
        expectedLine
    ]
    |> Chart.combine
    |> Chart.withXAxisStyle "Theoretical quantiles (normal)" 
    |> Chart.withYAxisStyle "Sample quantiles"
    |> Chart.withTemplate ChartTemplates.lightMirrored


let myQQPlotOneSampleGauss = plotFromOneSampleGauss normalDistA 

(*** condition: ipynb ***)
#if IPYNB
myQQPlotOneSampleGauss
#endif // IPYNB

(***hide***)
myQQPlotOneSampleGauss |> GenericChart.toChartHTML
(***include-it-raw***)



(**

As seen above the sample perfectly matches the expected quantiles from a normal distribution. This is expected because the sample was generated by sampling from an normal distribution.

*)

// compare the uniform sample against a normal distribution
let my2QQPlotOneSampleGauss = plotFromOneSampleGauss evenRandomA 


(*** condition: ipynb ***)
#if IPYNB
my2QQPlotOneSampleGauss
#endif // IPYNB

(***hide***)
my2QQPlotOneSampleGauss |> GenericChart.toChartHTML
(***include-it-raw***)


(**

As seen above the sample does not matches the expected quantiles from a normal distribution. The sample derives from an random sampling between 0 and 1 and therefore is overrepresented in the tails.


#### Uniform Distribution

You also can plot your data against a uniform distribution. Data can be standardized to lie between $0$ and $1$
*)

let uniform = 
    QQPlot.toUniform(Method=QuantileMethod.Rankit,Standardize=false) normalDistA
    |> Chart.Point
    |> Chart.withXAxisStyle "Theoretical quantiles (uniform)" 
    |> Chart.withYAxisStyle "Sample quantiles"
    |> Chart.withTemplate ChartTemplates.lightMirrored

(*** condition: ipynb ***)
#if IPYNB
uniform
#endif // IPYNB

(***hide***)
uniform |> GenericChart.toChartHTML
(***include-it-raw***)

(**

#### Any specified distribution

You also can plot your data against a distribution you can specify. You have to define the _inverse CDF_ or also called the _Quantile function_.

**LogNormal distribution**

*)

// generate a sample from a lognormal distriution
let sampleFromLogNormal =
    let d = Distributions.ContinuousDistribution.logNormal 0. 1.
    Array.init 500 (fun _ -> d.Sample())



// define the quantile function for the log normal distribution with parameters mu = 0 and sigma = 1
let quantileFunctionLogNormal p = 
    let mu = 0.
    let sigma = 1.
    Math.Exp (mu + Math.Sqrt(2. * (pown sigma 2)) * SpecialFunctions.Errorfunction.inverf(2. * p - 1.))

let logNormalNormalDist = QQPlot.toInvCDF(quantileFunctionLogNormal,Method=QuantileMethod.Rankit) normalDistA

let logNormalLogNormal  = QQPlot.toInvCDF(quantileFunctionLogNormal,Method=QuantileMethod.Rankit) sampleFromLogNormal

let logNormalChart = 
    [
        Chart.Point(logNormalNormalDist,Name="normal sample")
        Chart.Point(logNormalLogNormal,Name="log normal sample")
    ]
    |> Chart.combine
    |> Chart.withXAxisStyle "Theoretical quantiles Log Normal" 
    |> Chart.withYAxisStyle "Sample quantiles"
    |> Chart.withTemplate ChartTemplates.lightMirrored

(*** condition: ipynb ***)
#if IPYNB
logNormalChart
#endif // IPYNB

(***hide***)
logNormalChart |> GenericChart.toChartHTML
(***include-it-raw***)


(**

The log normal sample fits nicely to the bisector, but the sample from the normal distribution does not fit

*)