﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class RaycastController : MonoBehaviour {

    public LayerMask collisionMask;

    public const float skinWidth = 0.015f;
    public int horizontalRayCount = 4;
    public int verticalRayCount = 4;

    [HideInInspector]
    public float horizontalRaySpacing;
    [HideInInspector]
    public float verticalRaySpacing;

    [HideInInspector]
    public BoxCollider2D collider;
    public RaycastOrigins raycastOrigins;

    // Use this for initialization
    public virtual void Start() {
        collider = GetComponent<BoxCollider2D>();
        CalcRaySpacing();
    }

    public void UpdateRaycastOrigins() {
        Bounds bounds = collider.bounds; // Bounds of the collider.
        bounds.Expand(skinWidth * -2);   // Shrinks bounds by a skinWidth.

        /* Set Origin locations based on bounds. */
        raycastOrigins.bottomLeft = new Vector2(bounds.min.x, bounds.min.y);
        raycastOrigins.bottomRight = new Vector2(bounds.max.x, bounds.min.y);
        raycastOrigins.topLeft = new Vector2(bounds.min.x, bounds.max.y);
        raycastOrigins.topRight = new Vector2(bounds.max.x, bounds.max.y);
    }

    public void CalcRaySpacing() {
        Bounds bounds = collider.bounds; // Bounds of the collider.
        bounds.Expand(skinWidth * -2);   // Shrinks bounds by a skinWidth.

        /* Fire at least 2 rays in horizontal & vertical directions. */
        horizontalRayCount = Mathf.Clamp(horizontalRayCount, 2, int.MaxValue);
        verticalRayCount = Mathf.Clamp(horizontalRayCount, 2, int.MaxValue);

        /* Calc spacing between each ray */
        horizontalRaySpacing = bounds.size.y / (horizontalRayCount - 1);
        verticalRaySpacing = bounds.size.x / (verticalRayCount - 1);
    }

    public struct RaycastOrigins {
        public Vector2 topLeft, topRight;
        public Vector2 bottomLeft, bottomRight;
    }

}
