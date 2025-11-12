using Godot;
using System;

/// <summary>
/// Modifies the timeline object rendering
/// </summary>
public interface IObjectRenderModifier<T> : IMod
    where T : ITimelineObject
{
    /// <summary>
    /// Modifies the rendering of <see cref="ITimelineObject"/> for the <see cref="IObjectRenderModifier{T}"/>
    /// </summary>
    void ApplyRenderObject(T obj, Color color, float depth, Attempt attempt);
}
