# VRR GTFS Feed Transformer

This tool aims to to make the released GTFS feed more consistent and include the features of the [GTFS standard](https://developers.google.com/transit/gtfs/reference/). You can download the feed from the [open data portal](https://openvrr.de/dataset/gtfs).

## Build

|Branch|State|
|-|-|
|`develop`|[![Build Status](https://hypervtechnics-official.visualstudio.com/hypervtechnics-github/_apis/build/status/hypervtechnics.VrrGtfsTransform?branchName=develop)](https://hypervtechnics-official.visualstudio.com/hypervtechnics-github/_build/latest?definitionId=8&branchName=develop)|
|`master`|[![Build Status](https://hypervtechnics-official.visualstudio.com/hypervtechnics-github/_apis/build/status/hypervtechnics.VrrGtfsTransform?branchName=master)](https://hypervtechnics-official.visualstudio.com/hypervtechnics-github/_build/latest?definitionId=8&branchName=master)|

## Transformations

Currently there are only two transformations applied:

1. Group all stations which are the same with an appropiate `parent_station` to get better search results and avoid "duplicate" stops. A station is created for those.
2. The `platform_code` column is field from the information encoded in the `stop_id`. And terms like "Gleis" or similar are removed to be compliant with the GTFS standard.

## Usage

```
TODO
```

## Good to know

### `stop_id`

Format: `<1>:<2>:<3>(:<4>:<5>)`

TODO
