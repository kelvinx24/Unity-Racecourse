using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public record ClosestSample(
  Vector3 Position,
  Vector3 Tangent,
  int ClosestIndex
);
