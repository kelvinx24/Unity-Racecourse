using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public record SegmentSample(
  float Cumulative,
  int OveralIndex,
  int SegmentIndex,
  float SegmentT,
  Vector3 Position,
  Vector3 Tangent,
  Vector3 Normal
);
